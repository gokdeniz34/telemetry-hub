namespace TelemetryHub.Api.Application.Telemetry.Commands;

public record IngestTelemetryCommand(
    string Source,
    string Service,
    string EventName,
    string DeviceId,
    int DurationMs,
    string Level,
    Dictionary<string, object>? Payload);