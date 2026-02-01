namespace TelemetryHub.Api;

public interface IAuditLogRepository
{
    Task InsertAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

