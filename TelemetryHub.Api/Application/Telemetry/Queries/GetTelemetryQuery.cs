namespace TelemetryHub.Api.Application.Telemetry.Queries;

public sealed class GetTelemetryQuery
{
    public string DeviceId { get; set; } = default!;
    public string? Level { get; set; }
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
