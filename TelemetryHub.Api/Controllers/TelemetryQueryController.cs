using Microsoft.AspNetCore.Mvc;
using TelemetryHub.Api.Application.Telemetry.Handlers;
using TelemetryHub.Api.Application.Telemetry.Queries;

namespace TelemetryHub.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public sealed class TelemetryQueryController : ControllerBase
{
    private readonly TelemetryQueryHandler _handler;

    public TelemetryQueryController(TelemetryQueryHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("{deviceId}")]
    public async Task<IActionResult> GetTelemetry(
        string deviceId,
        [FromQuery] string? level,
        [FromQuery] DateTime? startDateUtc,
        [FromQuery] DateTime? endDateUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var query = new GetTelemetryQuery
        {
            DeviceId = deviceId,
            Level = level,
            StartDateUtc = startDateUtc,
            EndDateUtc = endDateUtc,
            Page = page,
            PageSize = pageSize
        };

        var result = await _handler.HandleAsync(query, ct);

        if (!result.Success)
            return NotFound(new
            {
                success = false,
                error = result.Error
            });

        return Ok(new
        {
            success = true,
            data = result.Data
        });
    }
}