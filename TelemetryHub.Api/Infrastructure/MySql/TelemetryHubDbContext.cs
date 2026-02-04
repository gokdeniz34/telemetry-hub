using Microsoft.EntityFrameworkCore;
using TelemetryHub.Api.Domain.Audit.Entities;
using TelemetryHub.Api.Domain.Telemetry.Entities;
using TelemetryHub.Domain.Common;
using TelemetryHub.Domain.Entities;

namespace TelemetryHub.Api.Infrastructure.MySql;

public sealed class TelemetryHubDbContext : DbContext
{
    public TelemetryHubDbContext(DbContextOptions<TelemetryHubDbContext> options)
        : base(options)
    { }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<TelemetrySummary> TelemetrySummaries => Set<TelemetrySummary>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<TelemetryData> Telemetries => Set<TelemetryData>();

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

        // Fluent API ile İlişki Tanımlama
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ApiKey).IsRequired();

            // İlişki: Bir Device'ın (HasMany) çok Telemetry'si vardır.
            // Telemetry'nin (WithOne) tek Device'ı vardır.
            entity.HasMany(d => d.Telemetries)
                  .WithOne(t => t.Device)
                  .HasForeignKey(t => t.DeviceId)
                  .OnDelete(DeleteBehavior.Cascade); // Senin dediğin Cascade Delete
        });

        modelBuilder.Entity<TelemetryData>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }


    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                // Nesne yeni ekleniyorsa
                case EntityState.Added:
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    entry.Entity.CreatedBy = "System_Machine"; // İleride ICurrentUserService ile değiştireceğiz
                    break;

                // Nesne güncelleniyorsa
                case EntityState.Modified:
                    entry.Entity.LastModifiedDate = DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = "System_Machine";
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    // Not: Hem Async hem de normal SaveChanges'ı override etmek iyi bir pratiktir.
    public override int SaveChanges()
    {
        // Yukarıdaki mantığın aynısını buraya da yazabiliriz (Kod tekrarı olmasın diye genelde bir metoda çıkartılır)
        return base.SaveChanges();
    }
}