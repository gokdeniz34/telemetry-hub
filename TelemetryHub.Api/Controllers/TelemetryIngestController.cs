using Microsoft.AspNetCore.Mvc;
using TelemetryHub.Api.Application.Telemetry.Commands;
using TelemetryHub.Api.Application.Telemetry.Handlers;

namespace TelemetryHub.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public sealed class TelemetryIngestController(IngestTelemetryHandler handler) : ControllerBase
{
    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest([FromBody] IngestTelemetryCommand command, CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Accepted(result);
    }
}