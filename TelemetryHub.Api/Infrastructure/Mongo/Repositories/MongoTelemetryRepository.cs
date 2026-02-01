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
        if (events == null || !events.Any()) return;

        // InsertManyAsync, MongoDB'nin toplu yazma protokolünü kullanır.
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

    public async Task<List<TelemetrySummaryDto>> GetHourlyStatsAsync()
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);

        var stats = await _collection.Aggregate()
            .Match(x => x.OccurredAt >= oneHourAgo)
            .Group(x => x.DeviceId, g => new
            {
                DeviceId = g.Key,
                SuccessCount = g.Count(x => x.Level != "Error" && x.Level != "Critical"),
                ErrorCount = g.Count(x => x.Level == "Error" || x.Level == "Critical"),
                Durations = g.Select(x => x.Payload["DurationMs"])
            })
            .ToListAsync();

        // Ortalama hesaplamasını C# tarafında (Memory'de) yapıyoruz (Çünkü gruplanmış veri artık çok küçüktür)
        return stats.Select(s => new TelemetrySummaryDto
        {
            DeviceId = s.DeviceId,
            TotalSuccess = s.SuccessCount,
            TotalError = s.ErrorCount,
            AverageDuration = s.Durations.Any() ? s.Durations.Select(d => Convert.ToDouble(d)).Average() : 0
        }).ToList();
    }
}