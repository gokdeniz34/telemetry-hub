namespace TelemetryHub.Api.Domain.Telemetry.Entities;

public class TelemetrySummary
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string DeviceId { get; set; } = null!;
    public int TotalSuccess { get; set; }
    public int TotalError { get; set; }
    public int TotalCount { get; set; }
    public double AverageDuration { get; set; }
}