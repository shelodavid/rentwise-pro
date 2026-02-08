using Microsoft.EntityFrameworkCore;
using RentWisePro.Etl.Core.Entities;

namespace RentWisePro.Etl.Persistence.Contexts;

public class EtlDbContext : DbContext
{
    public EtlDbContext(DbContextOptions<EtlDbContext> options) : base(options)
    {
    }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<HudFairMarketRent> HudFairMarketRents => Set<HudFairMarketRent>();
    public DbSet<GeoMarketStat> GeoMarketStats => Set<GeoMarketStat>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingSnapshot> ListingSnapshots => Set<ListingSnapshot>();
    public DbSet<RawPayloadRef> RawPayloadRefs => Set<RawPayloadRef>();
    public DbSet<PropertyPhoto> PropertyPhotos => Set<PropertyPhoto>();
    public DbSet<RentForecast> RentForecasts => Set<RentForecast>();
    public DbSet<WorkQueueItem> WorkQueue => Set<WorkQueueItem>();
    public DbSet<EtlRun> EtlRuns => Set<EtlRun>();
    public DbSet<EtlRunSourceStat> EtlRunSourceStats => Set<EtlRunSourceStat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("properties");
            entity.HasKey(e => e.PropertyId);
            entity.Property(e => e.NormalizedAddress).HasMaxLength(500);
            entity.Property(e => e.NormalizedAddressHash).HasMaxLength(128).IsRequired();
            entity.Property(e => e.OriginalAddress).HasMaxLength(500);
            entity.Property(e => e.Street).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.Zip).HasMaxLength(20);
            entity.Property(e => e.PropertyType).HasMaxLength(100);
            entity.Property(e => e.Beds).HasPrecision(4, 1);
            entity.Property(e => e.Baths).HasPrecision(4, 1);
            entity.Property(e => e.Latitude).HasPrecision(9, 6);
            entity.Property(e => e.Longitude).HasPrecision(9, 6);
            entity.Property(e => e.EstimatedMonthlyRent).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RentEstimateSource).HasMaxLength(50);
            entity.HasIndex(e => e.NormalizedAddressHash).IsUnique();
        });

        modelBuilder.Entity<HudFairMarketRent>(entity =>
        {
            entity.ToTable("hud_fmr");
            entity.HasKey(e => new { e.GeoType, e.GeoKey, e.Year, e.Bedrooms });
            entity.Property(e => e.GeoKey).HasMaxLength(20).IsRequired();
            entity.Property(e => e.GeoType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Fmr).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Source).HasMaxLength(50).HasDefaultValue("HUD");
            entity.HasIndex(e => new { e.GeoType, e.GeoKey, e.Year });
            entity.HasIndex(e => new { e.GeoType, e.GeoKey, e.Year, e.Bedrooms });
        });

        modelBuilder.Entity<GeoMarketStat>(entity =>
        {
            entity.ToTable("geo_market_stats");
            entity.HasKey(e => new { e.GeoType, e.GeoKey, e.Year });
            entity.Property(e => e.GeoKey).HasMaxLength(20).IsRequired();
            entity.Property(e => e.GeoType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.VacancyRate).HasColumnType("decimal(6,3)");
            entity.Property(e => e.MedianHouseholdIncome).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Source).HasMaxLength(50).HasDefaultValue("ACS");
            entity.HasIndex(e => new { e.GeoType, e.GeoKey, e.Year });
        });

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.ToTable("listings");
            entity.HasKey(e => e.ListingId);
            entity.Property(e => e.Source).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SourceListingId).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(18,0)");
            entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("USD");
            entity.Property(e => e.MaterialHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(e => new { e.Source, e.SourceListingId }).IsUnique();
            entity.HasIndex(e => e.PropertyId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LastSeenAt);
        });

        modelBuilder.Entity<ListingSnapshot>(entity =>
        {
            entity.ToTable("listing_snapshots");
            entity.HasKey(e => e.SnapshotId);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18,0)");
            entity.Property(e => e.MaterialHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(e => new { e.ListingId, e.ScrapedAt });
        });

        modelBuilder.Entity<RawPayloadRef>(entity =>
        {
            entity.ToTable("raw_payload_refs");
            entity.HasKey(e => e.RawRef);
            entity.Property(e => e.RawRef).HasMaxLength(500);
            entity.Property(e => e.Source).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SourceListingId).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<PropertyPhoto>(entity =>
        {
            entity.ToTable("property_photos");
            entity.HasKey(e => e.PhotoId);
            entity.Property(e => e.Source).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UrlOriginal).HasMaxLength(1000);
            entity.Property(e => e.StoragePath).HasMaxLength(500);
            entity.Property(e => e.Checksum).HasMaxLength(128);
            entity.HasIndex(e => new { e.PropertyId, e.Source, e.PhotoIndex }).IsUnique();
        });

        modelBuilder.Entity<RentForecast>(entity =>
        {
            entity.ToTable("rent_forecasts");
            entity.HasKey(e => e.ForecastId);
            entity.Property(e => e.Source).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EstimatedRent).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => new { e.PropertyId, e.Source });
        });

        modelBuilder.Entity<WorkQueueItem>(entity =>
        {
            entity.ToTable("work_queue");
            entity.HasKey(e => e.WorkId);
            entity.Property(e => e.WorkType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PayloadJson).HasColumnType("nvarchar(max)");
            entity.HasIndex(e => new { e.Status, e.AvailableAt });
        });

        modelBuilder.Entity<EtlRun>(entity =>
        {
            entity.ToTable("etl_runs");
            entity.HasKey(e => e.RunId);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<EtlRunSourceStat>(entity =>
        {
            entity.ToTable("etl_run_source_stats");
            entity.HasKey(e => new { e.RunId, e.Source });
            entity.Property(e => e.Source).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Listing>()
            .HasOne(e => e.Property)
            .WithMany(p => p.Listings)
            .HasForeignKey(e => e.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ListingSnapshot>()
            .HasOne(e => e.Listing)
            .WithMany(l => l.Snapshots)
            .HasForeignKey(e => e.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PropertyPhoto>()
            .HasOne(e => e.Property)
            .WithMany(p => p.Photos)
            .HasForeignKey(e => e.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
