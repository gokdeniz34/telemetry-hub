using Microsoft.EntityFrameworkCore;
using TelemetryHub.Api.Domain.Audit.Entities;
using TelemetryHub.Api.Domain.Telemetry.Entities; // Eklendi

namespace TelemetryHub.Api.Infrastructure.MySql;

public sealed class TelemetryHubDbContext : DbContext
{
    public TelemetryHubDbContext(DbContextOptions<TelemetryHubDbContext> options)
        : base(options)
    { }

    public DbSet<AuditLog> AuditLogs { get; set; } = default!;
    public DbSet<TelemetrySummary> TelemetrySummaries { get; set; } = default!; // Eklendi

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AuditLog yapılandırması (Opsiyonel)
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
        });

        // TelemetrySummary yapılandırması (Kritik!)
        modelBuilder.Entity<TelemetrySummary>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Aynı gün aynı cihaz için sadece 1 satır olabilir (Upsert mantığı için şart)
            entity.HasIndex(x => new { x.DeviceId, x.Date }).IsUnique();

            // MySQL veri tipi optimizasyonu
            entity.Property(e => e.DeviceId).HasMaxLength(50).IsRequired();
        });
    }
}