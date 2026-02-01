using TelemetryHub.Api.Domain.Audit.Entities;

namespace TelemetryHub.Api.Domain.Audit.Repositories;

public interface IAuditLogRepository
{
    Task InsertAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

