using System.Text.Json;

namespace TelemetryHub.Api.Domain.Audit.Entities;

public sealed class AuditLog
{
    public long Id { get; private set; }
    public string UserId { get; private set; } = default!;
    public string Action { get; private set; } = default!;
    public string? EntityId { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public string TraceId { get; private set; } = default!;
    public string? IpAddress { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        string action,
        string? entityId = null,
        string userId = "system",
        string traceId = "unknown",
        string? ipAddress = null)
    {
        return new AuditLog
        {
            Action = action,
            EntityId = entityId,
            UserId = string.IsNullOrWhiteSpace(userId) ? "system" : userId,
            TraceId = traceId,
            IpAddress = ipAddress,
            OccurredAtUtc = DateTime.UtcNow
        };
    }
}


