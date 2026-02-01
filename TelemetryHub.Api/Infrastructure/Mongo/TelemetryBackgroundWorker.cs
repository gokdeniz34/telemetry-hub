using Microsoft.Extensions.Hosting;
using TelemetryHub.Api.Domain.Telemetry.Entities;
using TelemetryHub.Api.Domain.Telemetry.Repositories;
using TelemetryHub.Api.Infrastructure.Common;

namespace TelemetryHub.Api.Infrastructure.Mongo;

public class TelemetryBackgroundWorker(
    TelemetryQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<TelemetryBackgroundWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var batch = new List<TelemetryEvent>();

            // Batching: 500 kayıt veya 2 saniye kuralı
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(2));

            try
            {
                while (batch.Count < 500 && !stoppingToken.IsCancellationRequested)
                {
                    if (await queue.Reader.WaitToReadAsync(timeoutCts.Token))
                    {
                        if (queue.Reader.TryRead(out var item)) batch.Add(item);
                    }
                }
            }
            catch (OperationCanceledException) { }

            if (batch.Count > 0)
            {
                using var scope = scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<ITelemetryRepository>();
                await repo.BulkInsertAsync(batch);
                logger.LogInformation("{Count} events written to MongoDB.", batch.Count);
            }
        }
    }
}