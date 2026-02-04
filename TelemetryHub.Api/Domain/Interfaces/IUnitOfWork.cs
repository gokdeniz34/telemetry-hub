// Path: TelemetryHub.Domain/Interfaces/IUnitOfWork.cs
namespace TelemetryHub.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repository'lere buradan ulaşıyoruz
    ITelemetryRepository Telemetries { get; }
    // IDeviceRepository Devices { get; } // Eğer oluşturursan

    // Tek bir merkezden SaveChanges yönetimi
    Task<int> SaveChangesAsync();
}