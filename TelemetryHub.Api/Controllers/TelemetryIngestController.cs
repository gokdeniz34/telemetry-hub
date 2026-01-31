using Microsoft.AspNetCore.Mvc;
using TelemetryHub.Api.Domain.Telemetry;
using TelemetryHub.Api.Infrastructure.Mongo.Repositories;

namespace TelemetryHub.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryIngestController : ControllerBase
{
    private readonly TelemetryRepository _repository;

    public TelemetryIngestController(TelemetryRepository repository)
    {
        _repository = repository;
    }

    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest()
    {
        var telemetry = new TelemetryEvent
        {
            Source = "backend-api",
            Service = "auth",
            EventName = "login_success",
            Level = "Info",
            OccurredAt = DateTime.UtcNow,
            Payload = new Dictionary<string, object>
            {
                { "userId", 43 },
                { "ip", "127.0.0.1" },
                { "durationMs", 125 }
            }
        };

        await _repository.InsertAsync(telemetry);

        return Ok(new
        {
            message = "Telemetry inserted",
            id = telemetry.Id
        });
    }
}
