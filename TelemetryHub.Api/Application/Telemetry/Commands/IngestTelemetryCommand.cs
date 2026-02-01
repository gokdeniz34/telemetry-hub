namespace TelemetryHub.Api.Application.Telemetry.Commands;

public record IngestTelemetryCommand(
    string DeviceId,
    string? Level,
    int DurationMs,
    Dictionary<string, object>? Payload);