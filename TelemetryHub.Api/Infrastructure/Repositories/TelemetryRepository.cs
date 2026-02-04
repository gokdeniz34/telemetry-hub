// Path: TelemetryHub.Infrastructure/Repositories/TelemetryRepository.cs
using Microsoft.EntityFrameworkCore;
using TelemetryHub.Api.Infrastructure.MySql;
using TelemetryHub.Domain.Entities;
using TelemetryHub.Domain.Interfaces;

namespace TelemetryHub.Infrastructure.Repositories;

public class TelemetryRepository : GenericRepository<TelemetryData>, ITelemetryRepository
{
    public TelemetryRepository(TelemetryHubDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TelemetryData>> GetLatestDataByDeviceIdAsync(int deviceId, int count)
    {
        return await _context.Telemetries
            .Where(x => x.DeviceId == deviceId)
            .OrderByDescending(x => x.CreatedDate)
            .Take(count)
            .ToListAsync();
    }
}