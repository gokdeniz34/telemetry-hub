namespace TelemetryHub.Api.Application.Common.DTOs;

public class TelemetrySummaryDto
{
    public string DeviceId { get; set; } = null!;
    public int TotalSuccess { get; set; }
    public int TotalError { get; set; }
    public double AverageDuration { get; set; }
}