// Path: TelemetryHub.Domain/Interfaces/ITelemetryRepository.cs
using TelemetryHub.Domain.Entities;

namespace TelemetryHub.Domain.Interfaces;

public interface ITelemetryRepository : IGenericRepository<TelemetryData>
{
    Task<IEnumerable<TelemetryData>> GetLatestDataByDeviceIdAsync(int deviceId, int count);
}