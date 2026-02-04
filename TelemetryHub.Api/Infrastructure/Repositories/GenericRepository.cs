using Microsoft.EntityFrameworkCore;
using TelemetryHub.Api.Infrastructure.MySql;
using TelemetryHub.Domain.Interfaces;

namespace TelemetryHub.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly TelemetryHubDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(TelemetryHubDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);
}