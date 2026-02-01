using TelemetryHub.Api.Application.Telemetry.Queries;
using TelemetryHub.Api.Domain.Telemetry.Entities;
using TelemetryHub.Api.Domain.Telemetry.Repositories;

namespace TelemetryHub.Api.Application.Telemetry.Handlers;

public sealed class TelemetryQueryHandler
{
    private readonly ITelemetryRepository _repository;
    private readonly ILogger<TelemetryQueryHandler> _logger;

    public TelemetryQueryHandler(
        ITelemetryRepository repository,
        ILogger<TelemetryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<TelemetryEvent>>> HandleAsync(GetTelemetryQuery query, CancellationToken ct)
    {
        try
        {
            var telemetry = await _repository.QueryAsync(
                deviceId: query.DeviceId,
                level: query.Level,
                startDateUtc: query.StartDateUtc,
                endDateUtc: query.EndDateUtc,
                page: query.Page,
                pageSize: query.PageSize,
                ct: ct
            );

            return Result<List<TelemetryEvent>>.Ok(telemetry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Telemetry query failed for DeviceId={DeviceId}", query.DeviceId);
            return Result<List<TelemetryEvent>>.Fail("Telemetry sorgulaması sırasında bir hata oluştu.");
        }
    }
}
