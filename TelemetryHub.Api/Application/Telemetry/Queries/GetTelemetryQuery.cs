namespace TelemetryHub.Api.Application.Telemetry.Queries;

public record GetTelemetryQuery(string DeviceId, int Limit = 100);