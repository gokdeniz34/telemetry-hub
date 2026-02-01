namespace TelemetryHub.Api.Application.Telemetry.Commands;

public sealed class IngestTelemetryCommand
{
    public string DeviceId { get; init; } = null!;
    public double DurationMs { get; init; }
    public string Level { get; init; } = default!;
    public Dictionary<string, object>? Payload { get; init; }

}
