// Path: TelemetryHub.Domain/Entities/TelemetryData.cs
using TelemetryHub.Domain.Common;

namespace TelemetryHub.Domain.Entities;

public class TelemetryData : BaseEntity
{
    public double Temperature { get; set; }
    public double Humidity { get; set; }

    // Foreign Key: Verinin hangi cihaza ait olduğunun nüfus kaydı.
    public int DeviceId { get; set; }
    
    // Navigation Property: Bu veri üzerinden cihazın bilgilerine (örn. adına) ulaşmamızı sağlar.
    public Device Device { get; set; } = null!; 
}