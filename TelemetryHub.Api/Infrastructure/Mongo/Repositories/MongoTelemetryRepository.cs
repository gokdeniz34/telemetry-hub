using TelemetryHub.Api.Domain.Telemetry.Entities;
using TelemetryHub.Api.Domain.Telemetry.Repositories;
using MongoDB.Driver;
using TelemetryHub.Api.Application.Common.DTOs;

namespace TelemetryHub.Api.Infrastructure.Mongo.Repositories;

public class MongoTelemetryRepository : ITelemetryRepository
{
    private readonly IMongoCollection<TelemetryEvent> _collection;

    public MongoTelemetryRepository(MongoContext context)
    {
        _collection = context.TelemetryEvents;
    }
    public async Task<IEnumerable<TelemetryEvent>> GetByDeviceIdAsync(string deviceId, int limit)
    {
        return await _collection.Find(x => x.DeviceId == deviceId)
        .SortByDescending(x => x.OccurredAt)
        .Limit(limit)
        .ToListAsync();
    }
    public async Task BulkInsertAsync(IEnumerable<TelemetryEvent> events)
    {
        if (events == null || !events.Any())
            return;

        await _collection.InsertManyAsync(events);
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
    public async Task<List<TelemetrySummaryDto>> GetHourlyStatsAndMarkAsProcessedAsync()
    {
        // 1. Henüz işlenmemiş verileri bul
        var unprocessedEvents = await _collection.Find(x => !x.IsProcessed).ToListAsync();

        if (!unprocessedEvents.Any()) return new List<TelemetrySummaryDto>();

        // 2. Gruplama ve Ortalama (Memory'de yapmak veriler çok değilse en kolayı)
        var stats = unprocessedEvents
            .GroupBy(x => x.DeviceId)
            .Select(g => new TelemetrySummaryDto
            {
                DeviceId = g.Key,
                TotalSuccess = g.Count(x => x.Level != "Error" && x.Level != "Critical"),
                TotalError = g.Count(x => x.Level == "Error" || x.Level == "Critical"),
                // Artık DurationMs Entity içinde olduğu için direkt erişiyoruz!
                AverageDuration = g.Average(x => x.DurationMs)
            }).ToList();

        // 3. İŞLENDİ OLARAK İŞARETLE (Çok Kritik!)
        var ids = unprocessedEvents.Select(x => x.Id).ToList();
        await _collection.UpdateManyAsync(
            Builders<TelemetryEvent>.Filter.In(x => x.Id, ids),
            Builders<TelemetryEvent>.Update.Set(x => x.IsProcessed, true)
        );

        return stats;
    }
}