using MongoDB.Driver;
using TelemetryHub.Api.Domain.Telemetry;

namespace TelemetryHub.Api.Infrastructure.Mongo.Repositories;

public class TelemetryRepository
{
    private readonly IMongoCollection<TelemetryEvent> _collection;

    public TelemetryRepository(MongoContext context)
    {
        _collection = context.TelemetryEvents;
    }

    public async Task InsertAsync(TelemetryEvent telemetry)
    {
        await _collection.InsertOneAsync(telemetry);
    }
}
