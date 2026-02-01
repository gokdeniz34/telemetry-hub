namespace TelemetryHub.Api;

public sealed class AuditLog
{
    public long Id { get; set; }

    public string UserId { get; set; } = default!;
    public string Action { get; set; } = default!;
    public string Path { get; set; } = default!;
    public string HttpMethod { get; set; } = default!;
    public DateTime OccurredAtUtc { get; set; }
    public string TraceId { get; set; } = default!;
    public string? IpAddress { get; set; }
}
