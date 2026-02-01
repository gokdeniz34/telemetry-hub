using System.Text.Json; // Bunu eklemeyi unutma
using TelemetryHub.Api.Application.Telemetry.Commands;
using TelemetryHub.Api.Domain.Audit.Repositories;
using TelemetryHub.Api.Domain.Telemetry.Entities;
using TelemetryHub.Api.Domain.Audit.Entities;
using TelemetryHub.Api.Infrastructure.Common;

namespace TelemetryHub.Api.Application.Telemetry.Handlers;

public sealed class TelemetryIngestHandler(
    TelemetryQueue queue,
    IAuditLogRepository auditLogRepository,
    ILogger<TelemetryIngestHandler> logger)
{
    public async Task<Result<string>> HandleAsync(IngestTelemetryCommand command, CancellationToken ct)
    {
        var source = "device-gateway";
        var service = $"device-{command.DeviceId}";

        try
        {
            // Payload Dönüştürme Mantığı
            var processedPayload = new Dictionary<string, object>();

            if (command.Payload != null)
            {
                foreach (var item in command.Payload)
                {
                    // JsonElement gelirse içindeki gerçek değeri çıkarıyoruz
                    if (item.Value is JsonElement element)
                    {
                        processedPayload[item.Key] = ExtractValue(element);
                    }
                    else
                    {
                        processedPayload[item.Key] = item.Value;
                    }
                }
            }

            // DurationMs'i de payload içine sayı olarak ekliyoruz (Agregasyon için önemli)
            processedPayload["DurationMs"] = command.DurationMs;

            var telemetry = TelemetryEvent.Create(
                source: source,
                service: service,
                eventName: "telemetry_received",
                deviceId: command.DeviceId,
                level: command.Level ?? "Info",
                payload: processedPayload // Temizlenmiş payload
            );

            await queue.Writer.WriteAsync(telemetry, ct);

            if (telemetry.Level == "Critical")
            {
                var audit = AuditLog.Create("CriticalTelemetryIngested", telemetry.Id, "system", Guid.NewGuid().ToString());
                await auditLogRepository.InsertAsync(audit, ct);
            }

            return Result<string>.Ok(telemetry.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ingest failed for Device: {DeviceId}", command.DeviceId);
            return Result<string>.Fail("Veri işleme kuyruğuna alınamadı.");
        }
    }

    // JsonElement'ten C# tipine güvenli dönüşüm yapan yardımcı metod
    private static object ExtractValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "",
            JsonValueKind.Number => element.TryGetInt32(out int i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            _ => element.GetRawText() // Obje veya Array ise string olarak sakla
        };
    }
}