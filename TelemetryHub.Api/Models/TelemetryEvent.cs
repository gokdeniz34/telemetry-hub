using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TelemetryHub.Api.Models;

public class TelemetryEvent
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string DeviceId { get; set; } = null!;
    public string Type { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}
