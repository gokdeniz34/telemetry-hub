// Path: TelemetryHub.Infrastructure/UnitOfWork/UnitOfWork.cs
using TelemetryHub.Api.Infrastructure.MySql;
using TelemetryHub.Domain.Interfaces;
using TelemetryHub.Infrastructure.Repositories;

namespace TelemetryHub.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly TelemetryHubDbContext _context;
    private ITelemetryRepository? _telemetryRepository;

    public UnitOfWork(TelemetryHubDbContext context)
    {
        _context = context;
    }

    // Repository'i sadece ihtiyaç duyulduğunda (Lazy Loading) oluşturuyoruz
    public ITelemetryRepository Telemetries =>
        _telemetryRepository ??= new TelemetryRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}