using TelemetryHub.Api.Domain.Telemetry.Entities;
using TelemetryHub.Api.Domain.Telemetry.Repositories;
using MongoDB.Driver;

namespace TelemetryHub.Api.Infrastructure.Mongo.Repositories;

public class MongoTelemetryRepository : ITelemetryRepository
{
    private readonly IMongoCollection<TelemetryEvent> _collection;

    public MongoTelemetryRepository(MongoContext context)
    {
        _collection = context.TelemetryEvents;
    }

    public async Task InsertAsync(
        TelemetryEvent telemetryEvent,
        CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(telemetryEvent, cancellationToken: ct);
    }
    public async Task<List<TelemetryEvent>> QueryAsync(
        string deviceId,
        string? level = null,
        DateTime? startDateUtc = null,
        DateTime? endDateUtc = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default)
    {
        var filterBuilder = Builders<TelemetryEvent>.Filter;
        var filter = filterBuilder.Eq(x => x.DeviceId, deviceId);

        if (!string.IsNullOrEmpty(level))
            filter &= filterBuilder.Eq(x => x.Level, level);

        if (startDateUtc.HasValue)
            filter &= filterBuilder.Gte(x => x.OccurredAt, startDateUtc.Value);

        if (endDateUtc.HasValue)
            filter &= filterBuilder.Lte(x => x.OccurredAt, endDateUtc.Value);

        return await _collection
            .Find(filter)
            .SortByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);
    }
}
