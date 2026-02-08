using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Domain.Entities.Etl;

namespace RentWisePro.Web.Data;

public class EtlReadDbContext : DbContext
{
    public EtlReadDbContext(DbContextOptions<EtlReadDbContext> options) : base(options)
    {
    }

    public DbSet<EtlListing> EtlListings => Set<EtlListing>();
    public DbSet<EtlProperty> EtlProperties => Set<EtlProperty>();
    public DbSet<EtlPropertyPhoto> EtlPropertyPhotos => Set<EtlPropertyPhoto>();
    public DbSet<EtlRun> EtlRuns => Set<EtlRun>();
    public DbSet<EtlRunSourceStat> EtlRunSourceStats => Set<EtlRunSourceStat>();
    public DbSet<WorkQueueItem> EtlWorkQueueItems => Set<WorkQueueItem>();
    public DbSet<EtlAdminAction> EtlAdminActions => Set<EtlAdminAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EtlProperty>(entity =>
        {
            entity.ToTable("properties", table => table.ExcludeFromMigrations());
            entity.HasKey(e => e.PropertyId);
            entity.Property(e => e.Street).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.Zip).HasMaxLength(20);
        });

        modelBuilder.Entity<EtlListing>(entity =>
        {
            entity.ToTable("listings", table => table.ExcludeFromMigrations());
            entity.HasKey(e => e.ListingId);
            entity.Property(e => e.Source).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SourceListingId).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => new { e.Source, e.SourceListingId }).IsUnique();
            entity.HasIndex(e => e.PropertyId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LastSeenAt);
        });

        modelBuilder.Entity<EtlPropertyPhoto>(entity =>
        {
            entity.ToTable("property_photos", table => table.ExcludeFromMigrations());
            entity.HasKey(e => e.PhotoId);
            entity.Property(e => e.Source).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UrlOriginal).HasMaxLength(1000);
            entity.Property(e => e.StoragePath).HasMaxLength(500);
            entity.HasIndex(e => new { e.PropertyId, e.Source, e.PhotoIndex }).IsUnique();
        });

        modelBuilder.Entity<EtlRun>(entity =>
        {
            entity.ToTable("etl_runs", table => table.ExcludeFromMigrations());
            entity.HasKey(e => e.RunId);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Notes);
        });

        modelBuilder.Entity<EtlRunSourceStat>(entity =>
        {
            entity.ToTable("etl_run_source_stats", table => table.ExcludeFromMigrations());
            entity.HasKey(e => new { e.RunId, e.Source });
            entity.Property(e => e.Source).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<WorkQueueItem>(entity =>
        {
            entity.ToTable("work_queue", table => table.ExcludeFromMigrations());
            entity.HasKey(e => e.WorkId);
            entity.Property(e => e.WorkType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<EtlAdminAction>(entity =>
        {
            entity.ToTable("etl_admin_actions", table => table.ExcludeFromMigrations());
            entity.HasKey(e => e.ActionId);
            entity.Property(e => e.ActionType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RequestedByUserId).HasMaxLength(450);
        });

        modelBuilder.Entity<EtlListing>()
            .HasOne(e => e.Property)
            .WithMany(p => p.Listings)
            .HasForeignKey(e => e.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EtlPropertyPhoto>()
            .HasOne(e => e.Property)
            .WithMany(p => p.Photos)
            .HasForeignKey(e => e.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
