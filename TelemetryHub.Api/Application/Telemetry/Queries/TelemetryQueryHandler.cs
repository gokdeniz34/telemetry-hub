using TelemetryHub.Api.Application.Telemetry.Queries;
using TelemetryHub.Api.Domain.Telemetry.Entities;
using TelemetryHub.Api.Domain.Telemetry.Repositories;

namespace TelemetryHub.Api.Application.Telemetry.Handlers;

public sealed class TelemetryQueryHandler(ITelemetryRepository repository)
{
    public async Task<Result<IEnumerable<TelemetryEvent>>> HandleAsync(GetTelemetryQuery query)
    {
        var data = await repository.GetByDeviceIdAsync(query.DeviceId, query.Limit);
        return Result<IEnumerable<TelemetryEvent>>.Ok(data);
    }
}