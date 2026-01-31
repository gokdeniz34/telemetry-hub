using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TelemetryHub.Api.Domain.Telemetry;

public class TelemetryEvent
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string Source { get; set; } = null!;   // mobile-app, backend, iot
    public string Service { get; set; } = null!;  // auth, payment, search
    public string EventName { get; set; } = null!;

    public string Level { get; set; } = "Info";   // Info, Warning, Error

    public DateTime OccurredAt { get; set; }

    public Dictionary<string, object>? Payload { get; set; }
}