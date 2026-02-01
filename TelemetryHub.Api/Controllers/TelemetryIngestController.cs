using Microsoft.AspNetCore.Mvc;
using TelemetryHub.Api.Application.Telemetry.Commands;
using TelemetryHub.Api.Application.Telemetry.Handlers;
using TelemetryHub.Api.Domain.Audit;

namespace TelemetryHub.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public sealed class TelemetryIngestController : ControllerBase
{
    private readonly TelemetryIngestHandler _handler;

    public TelemetryIngestController(TelemetryIngestHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("ingest")]
    // [Auditable("TelemetryIngest")]
    public async Task<IActionResult> Ingest([FromBody] IngestTelemetryCommand command, CancellationToken ct)
    {
        var result = await _handler.HandleAsync(command, ct);

        if (!result.Success)
        {
            return BadRequest(new
            {
                success = false,
                error = result.Error
            });
        }

        return Ok(new
        {
            success = true,
            id = result.Data
        });
    }
}
