using Microsoft.AspNetCore.Mvc;
using TelemetryHub.Api.Application.Telemetry.Handlers;
using TelemetryHub.Api.Application.Telemetry.Queries;

namespace TelemetryHub.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public sealed class TelemetryQueryController(TelemetryQueryHandler handler) : ControllerBase
{
    [HttpGet("{deviceId}")]
    public async Task<IActionResult> GetByDevice(string deviceId, [FromQuery] int limit = 100)
    {
        var query = new GetTelemetryQuery(deviceId, limit);
        var result = await handler.HandleAsync(query);
        return Ok(result);
    }
}