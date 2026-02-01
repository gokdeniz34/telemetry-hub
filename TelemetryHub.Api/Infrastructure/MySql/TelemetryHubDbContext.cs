using Microsoft.EntityFrameworkCore;
using TelemetryHub.Api.Domain.Audit.Entities;

namespace TelemetryHub.Api.Infrastructure.MySql;

public sealed class TelemetryHubDbContext : DbContext
{
    public TelemetryHubDbContext(DbContextOptions<TelemetryHubDbContext> options)
        : base(options)
    { }

    public DbSet<AuditLog> AuditLogs { get; set; } = default!;
}
