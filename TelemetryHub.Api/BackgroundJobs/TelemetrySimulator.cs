using TelemetryHub.Api.Application.Telemetry.Handlers;
using TelemetryHub.Api.Application.Telemetry.Commands;

namespace TelemetryHub.Api.BackgroundJobs;

public class TelemetrySimulator(
    IServiceProvider serviceProvider,
    ILogger<TelemetrySimulator> logger) : BackgroundService
{
    private readonly string[] _devices = ["iot-therm-01", "iot-therm-02", "gateway-main", "sensor-hub-v9"];
    private readonly string[] _services = ["ClimateControl", "EdgeProcessor", "HealthMonitor"];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool test = false;
        if (!test)
        {
            return;
        }
        // Altyapı servislerinin (DB, Elastic) ayağa kalkması için kısa bir güvenli bekleme
        await Task.Delay(5000, stoppingToken);

        logger.LogInformation("🚀 Simülatör ateşlendi! Veri üretimi başlıyor...");

        var random = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IngestTelemetryHandler>();

                // Tek seferde 10 adet farklı telemetri paketi gönderiyoruz
                for (int i = 0; i < 10; i++)
                {
                    var deviceId = _devices[random.Next(_devices.Length)];
                    int duration;
                    string level = "Info";

                    // Cihaz bazlı davranış simülasyonu
                    if (deviceId == "gateway-main")
                    {
                        duration = random.Next(10, 100); // Gateway her zaman hızlıdır
                    }
                    else if (deviceId == "sensor-hub-v9")
                    {
                        duration = random.Next(1000, 3000); // Bu sensör yavaş ve sorunlu
                        if (duration > 2500) level = "Critical";
                    }
                    else
                    {
                        duration = random.Next(100, 800);
                        level = random.Next(1, 100) > 95 ? "Critical" : "Info";
                    }

                    var command = new IngestTelemetryCommand(
                        Source: "EdgeGateway_v2",
                        Service: _services[random.Next(_services.Length)],
                        EventName: level == "Critical" ? "system_failure" : "status_check",
                        DeviceId: deviceId,
                        DurationMs: duration,
                        Level: level,
                        Payload: new Dictionary<string, object> {
        { "Version", "2.1.0" },
        { "RetryCount", level == "Critical" ? 3 : 0 }
                        }
                    );

                    // Handler'ı direkt çağırıyoruz (HTTP üzerinden değil, kod içinden)
                    await handler.HandleAsync(command, stoppingToken);
                }

                logger.LogInformation("{Time}: 10 paket başarıyla kuyruğa iletildi.", DateTime.Now.ToLongTimeString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Simülatör veri gönderirken bir aksilik yaşadı!");
            }

            // Sistemi boğmamak için 3 saniye soluklan
            await Task.Delay(3000, stoppingToken);
        }
    }
}