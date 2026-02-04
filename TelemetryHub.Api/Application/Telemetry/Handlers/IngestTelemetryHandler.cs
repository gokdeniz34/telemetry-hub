using System.Text.Json;
using TelemetryHub.Api.Application.Telemetry.Commands;
using TelemetryHub.Api.Domain.Audit.Repositories;
using TelemetryHub.Api.Domain.Telemetry.Entities;
using TelemetryHub.Api.Domain.Audit.Entities;
using TelemetryHub.Api.Infrastructure.Common;

namespace TelemetryHub.Api.Application.Telemetry.Handlers;

public sealed class IngestTelemetryHandler(
    TelemetryQueue queue,
    IAuditLogRepository auditLogRepository,
    ILogger<IngestTelemetryHandler> logger)
{
    public async Task<Result<string>> HandleAsync(IngestTelemetryCommand command, CancellationToken ct)
    {
        try
        {
            var cleanPayload = SanitizePayload(command.Payload);

            var telemetry = TelemetryEvent.Create(
                source: command.Source,
                service: command.Service,
                eventName: command.EventName,
                deviceId: command.DeviceId,
                durationMs: command.DurationMs,
                level: command.Level,
                payload: cleanPayload
            );

            await queue.Writer.WriteAsync(telemetry, ct);
            await HandleCriticalAuditAsync(telemetry, ct);

            return Result<string>.Ok(telemetry.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ingest failed for Device: {DeviceId}", command.DeviceId);
            return Result<string>.Fail("Veri kuyruğa alınırken bir hata oluştu.");
        }
    }

    private async Task HandleCriticalAuditAsync(TelemetryEvent telemetry, CancellationToken ct)
    {
        if (telemetry.Level == "Critical" || telemetry.DurationMs > 2000)
        {
            string actionType = telemetry.DurationMs > 2000 ? "PERFORMANCE_DEGRADATION" : "CRITICAL_ERROR";

            var audit = AuditLog.Create(
                action: actionType,
                entityId: telemetry.Id, // MongoDB Id'si ile ilişkilendirdik
                userId: "SystemMonitor",
                traceId: Guid.NewGuid().ToString(), // İstek izleme için benzersiz ID
                ipAddress: "127.0.0.1"
            );

            await auditLogRepository.InsertAsync(audit, ct);

            logger.LogWarning("Audit log created for {ActionType} on Device: {DeviceId}", actionType, telemetry.DeviceId);
        }
    }

    private static Dictionary<string, object>? SanitizePayload(Dictionary<string, object>? rawPayload)
    {
        if (rawPayload == null) return null;

        var result = new Dictionary<string, object>();
        foreach (var (key, value) in rawPayload)
        {
            result[key] = value is JsonElement element ? ExtractValue(element) : value;
        }
        return result;
    }

    private static object ExtractValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString() ?? "",
        JsonValueKind.Number => element.TryGetInt32(out int i) ? i : element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => element.GetRawText()
    };
}