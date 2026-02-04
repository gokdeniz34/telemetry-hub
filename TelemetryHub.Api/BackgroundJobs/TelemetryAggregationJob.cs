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
        bool test = false;
        if (!test)
        {
            return;
        }
        logger.LogInformation("Telemetry Aggregation Job is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var mongoRepo = scope.ServiceProvider.GetRequiredService<ITelemetryRepository>();
                var dbContext = scope.ServiceProvider.GetRequiredService<TelemetryHubDbContext>();

                var hourlyStats = await mongoRepo.GetHourlyStatsAndMarkAsProcessedAsync();

                if (hourlyStats.Count > 0)
                {
                    var currentHour = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0);

                    foreach (var stat in hourlyStats)
                    {
                        var existing = await dbContext.Set<TelemetrySummary>()
                        .FirstOrDefaultAsync(x => x.DeviceId == stat.DeviceId && x.Date == currentHour, stoppingToken);

                        // Yeni gelen verinin toplam adet sayısı
                        int newBatchCount = stat.TotalSuccess + stat.TotalError;

                        if (existing != null)
                        {
                            // AĞIRLIKLI ORTALAMA HESABI:
                            // (Eski Ortalama * Eski Adet + Yeni Ortalama * Yeni Adet) / (Eski Adet + Yeni Adet)
                            double totalOldDuration = existing.AverageDuration * existing.TotalCount;
                            double totalNewDuration = stat.AverageDuration * newBatchCount;

                            existing.TotalCount += newBatchCount;
                            existing.AverageDuration = (totalOldDuration + totalNewDuration) / existing.TotalCount;

                            existing.TotalSuccess += stat.TotalSuccess;
                            existing.TotalError += stat.TotalError;
                        }
                        else
                        {
                            await dbContext.Set<TelemetrySummary>().AddAsync(new TelemetrySummary
                            {
                                DeviceId = stat.DeviceId,
                                Date = currentHour,
                                TotalSuccess = stat.TotalSuccess,
                                TotalError = stat.TotalError,
                                AverageDuration = stat.AverageDuration,
                                TotalCount = newBatchCount // İlk defa ekleniyor
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

            // await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}