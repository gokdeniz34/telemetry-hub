// Path: TelemetryHub.Domain/Entities/Device.cs
using TelemetryHub.Domain.Common;

namespace TelemetryHub.Domain.Entities;

public class Device : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = Guid.NewGuid().ToString(); // Cihazın sisteme giriş anahtarı
    public bool IsActive { get; set; } = true;

    // Navigation Property: EF Core bu liste üzerinden cihazın verilerine ulaşır.
    public ICollection<TelemetryData> Telemetries { get; set; } = new HashSet<TelemetryData>();
}