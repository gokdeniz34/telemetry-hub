// Infrastructure/Mongo/IndexBuilders/TelemetryIndexes.cs
using MongoDB.Driver;
using TelemetryHub.Api.Domain.Telemetry.Entities;

public static class TelemetryIndexes
{
    public static async Task CreateIndexes(IMongoCollection<TelemetryEvent> collection)
    {
        // En çok kullanılan sorgu alanları: DeviceId ve OccurredAt
        var deviceIndex = Builders<TelemetryEvent>.IndexKeys.Ascending(x => x.DeviceId);
        var dateIndex = Builders<TelemetryEvent>.IndexKeys.Descending(x => x.OccurredAt);

        await collection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<TelemetryEvent>(deviceIndex),
            new CreateIndexModel<TelemetryEvent>(dateIndex)
        });
    }
}