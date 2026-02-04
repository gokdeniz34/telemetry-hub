// Path: TelemetryHub.Domain/Common/BaseEntity.cs
namespace TelemetryHub.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "System";
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedBy { get; set; }
}