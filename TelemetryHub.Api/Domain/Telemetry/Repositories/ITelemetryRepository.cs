using TelemetryHub.Api.Domain.Telemetry.Entities;

namespace TelemetryHub.Api.Domain.Telemetry.Repositories;

public interface ITelemetryRepository
{
        Task InsertAsync(TelemetryEvent telemetryEvent, CancellationToken ct = default);
        Task<List<TelemetryEvent>> QueryAsync(
        string deviceId,
        string? level = null,
        DateTime? startDateUtc = null,
        DateTime? endDateUtc = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default);
}
