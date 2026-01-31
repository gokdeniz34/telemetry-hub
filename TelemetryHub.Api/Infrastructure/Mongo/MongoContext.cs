using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TelemetryHub.Api.Domain.Telemetry;

namespace TelemetryHub.Api;

public class MongoContext
{
    private readonly IMongoDatabase _database;

    public MongoContext(
        IMongoClient client,
        IOptions<MongoSettings> settings)
    {
        _database = client.GetDatabase(settings.Value.Database);
    }

    public IMongoCollection<TelemetryEvent> TelemetryEvents =>
        _database.GetCollection<TelemetryEvent>("telemetry_events");
}