using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TelemetryHub.Api;
using TelemetryHub.Api.Domain.Telemetry.Entities;

public class MongoContext
{
    private readonly IMongoDatabase _db;

    public MongoContext(IMongoClient client, IOptions<MongoSettings> settings)
    {
        _db = client.GetDatabase(settings.Value.Database);
    }

    public IMongoCollection<TelemetryEvent> TelemetryEvents => _db.GetCollection<TelemetryEvent>("TelemetryEvents");
}
