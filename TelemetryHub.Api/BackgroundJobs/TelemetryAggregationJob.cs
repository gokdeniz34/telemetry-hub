using Microsoft.EntityFrameworkCore;
using TelemetryHub.Api.Domain.Telemetry.Entities;
using TelemetryHub.Api.Domain.Telemetry.Repositories;
using TelemetryHub.Api.Infrastructure.MySql;

namespace TelemetryHub.Api.BackgroundJobs;

public class TelemetryAggregationJob(
    IServiceScopeFactory scopeFactory,
    ILogger<TelemetryAggregationJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Telemetry Aggregation Job is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var mongoRepo = scope.ServiceProvider.GetRequiredService<ITelemetryRepository>();
                var dbContext = scope.ServiceProvider.GetRequiredService<TelemetryHubDbContext>();

                var hourlyStats = await mongoRepo.GetHourlyStatsAsync();

                if (hourlyStats.Any())
                {
                    var today = DateTime.UtcNow.Date;

                    foreach (var stat in hourlyStats)
                    {
                        // MySQL'de "Upsert" işlemi: Varsa güncelle, yoksa ekle
                        var existing = await dbContext.Set<TelemetrySummary>()
                            .FirstOrDefaultAsync(x => x.DeviceId == stat.DeviceId && x.Date == today, stoppingToken);

                        if (existing != null)
                        {
                            existing.TotalSuccess += stat.TotalSuccess;
                            existing.TotalError += stat.TotalError;
                            // Ağırlıklı ortalama mantığına girmeden basit güncelleme:
                            existing.AverageDuration = stat.AverageDuration;
                        }
                        else
                        {
                            await dbContext.Set<TelemetrySummary>().AddAsync(new TelemetrySummary
                            {
                                DeviceId = stat.DeviceId,
                                Date = today,
                                TotalSuccess = stat.TotalSuccess,
                                TotalError = stat.TotalError,
                                AverageDuration = stat.AverageDuration
                            }, stoppingToken);
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                    logger.LogInformation("Successfully aggregated {Count} devices to MySQL.", hourlyStats.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during telemetry aggregation.");
            }

            // Her saat başı çalış (Test aşamasında burayı TimeSpan.FromMinutes(1) yapabilirsin)
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}