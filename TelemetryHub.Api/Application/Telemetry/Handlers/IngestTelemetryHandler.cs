using TelemetryHub.Api.Application.Telemetry.Commands;
using TelemetryHub.Api.Domain.Audit.Repositories;
using TelemetryHub.Api.Domain.Telemetry.Entities;
using TelemetryHub.Api.Domain.Audit.Entities;
using TelemetryHub.Api.Domain.Telemetry.Repositories;

namespace TelemetryHub.Api.Application.Telemetry.Handlers;

public sealed class TelemetryIngestHandler
{
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<TelemetryIngestHandler> _logger;

    public TelemetryIngestHandler(
        ITelemetryRepository telemetryRepository,
        IAuditLogRepository auditLogRepository,
        ILogger<TelemetryIngestHandler> logger)
    {
        _telemetryRepository = telemetryRepository;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task<Result<string>> HandleAsync(IngestTelemetryCommand command, CancellationToken ct)
    {
        // Kaynak belirtilmemişse varsayılan atayabiliriz
        var source = "device-gateway";
        var service = $"device-{command.DeviceId}";

        try
        {
            // Komut içinden gelen özel verileri Payload içine paketliyoruz
            var payload = new Dictionary<string, object>
        {
            { "DurationMs", command.DurationMs }
        };
            var telemetry = TelemetryEvent.Create(
                source: source,
                service: service,
                eventName: "telemetry_received",
                deviceId: command.DeviceId,
                level: command.Level ?? "Info",
                payload: payload
            );

            // Telemetry (Mongo)
            await _telemetryRepository.InsertAsync(telemetry, ct);

            // Audit Log (MySQL)
            var audit = AuditLog.Create(
                action: "TelemetryIngested",
                entityId: telemetry.Id,
                userId: "anonymous", // gerçek kullanıcı verisini al, yoksa anonymous
                traceId: Guid.NewGuid().ToString()
            );

            await _auditLogRepository.InsertAsync(audit, ct);

            _logger.LogInformation("Event {Event} ingested from {Service}", telemetry.EventName, telemetry.Service);

            return Result<string>.Ok(telemetry.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ingest failed for Service: {Service}", service);
            return Result<string>.Fail("Veri işlenirken bir hata oluştu.");
        }
    }

}
