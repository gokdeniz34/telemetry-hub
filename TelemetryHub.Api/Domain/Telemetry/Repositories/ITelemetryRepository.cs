using TelemetryHub.Api.Application.Common.DTOs;
using TelemetryHub.Api.Domain.Telemetry.Entities;

namespace TelemetryHub.Api.Domain.Telemetry.Repositories;

public interface ITelemetryRepository
{
        Task BulkInsertAsync(IEnumerable<TelemetryEvent> events);
        Task<IEnumerable<TelemetryEvent>> GetByDeviceIdAsync(string deviceId, int limit);
        Task InsertAsync(TelemetryEvent telemetryEvent, CancellationToken ct = default);
        Task<List<TelemetryEvent>> QueryAsync(
        string deviceId,
        string? level = null,
        DateTime? startDateUtc = null,
        DateTime? endDateUtc = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default);
        Task<List<TelemetrySummaryDto>> GetHourlyStatsAsync();
}
