using Microsoft.EntityFrameworkCore;
using TelemetryHub.Api.Domain.Audit;
using TelemetryHub.Api.Domain.Audit.Entities;
using TelemetryHub.Api.Domain.Audit.Repositories;
using TelemetryHub.Api.Infrastructure.MySql;

namespace TelemetryHub.Api.Infrastructure.Audit.Repositories;

public sealed class MySqlAuditLogRepository : IAuditLogRepository
{
    private readonly TelemetryHubDbContext _dbContext;

    public MySqlAuditLogRepository(TelemetryHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InsertAsync(AuditLog auditLog, CancellationToken ct = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, ct); // Büyük nesneler için AddAsync daha iyidir
        await _dbContext.SaveChangesAsync(ct);
    }
}
