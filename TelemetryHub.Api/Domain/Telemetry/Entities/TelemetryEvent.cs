using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TelemetryHub.Api.Domain.Telemetry.Entities;

public class TelemetryEvent
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; private set; } = null!;
    public string Source { get; private set; } = null!;      // örn: "backend-api", "iot-gateway"
    public string Service { get; private set; } = null!;     // örn: "auth-service", "sensor-node"
    public string EventName { get; private set; } = null!;   // örn: "login_success", "temperature_read"
    public string Level { get; private set; } = "Info";      // Info, Warning, Error, Critical
    public DateTime OccurredAt { get; private set; }
    public string DeviceId { get; set; } = null!;
    public bool IsProcessed { get; set; } = false;
    public int DurationMs { get; set; }
    public Dictionary<string, object>? Payload { get; private set; }

    private TelemetryEvent() { }

    public static TelemetryEvent Create(
        string source,
        string service,
        string eventName,
        string deviceId,
        int durationMs,
        bool isProcessed = false,
        string level = "Info",
        Dictionary<string, object>? payload = null)
    {
        return new TelemetryEvent
        {
            Source = source,
            Service = service,
            EventName = eventName,
            Level = level,
            OccurredAt = DateTime.UtcNow,
            DeviceId = deviceId,
            IsProcessed = isProcessed,
            DurationMs = durationMs,
            Payload = payload
        };
    }
}