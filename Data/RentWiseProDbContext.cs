using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Domain.Entities;
using RentWisePro.Web.Domain.Entities.Etl;
using RentWisePro.Web.Domain.Identity;

namespace RentWisePro.Web.Data
{
    public class RentWiseProDbContext : IdentityDbContext<ApplicationUser>
    {
        public RentWiseProDbContext(DbContextOptions<RentWiseProDbContext> options)
            : base(options)
        {
        }

        public DbSet<InvestmentProfile> InvestmentProfiles => Set<InvestmentProfile>();
        public DbSet<RentalListing> RentalListings => Set<RentalListing>();
        public DbSet<SavedPropertyProfile> SavedPropertyProfiles => Set<SavedPropertyProfile>();
        public DbSet<EtlListing> EtlListings => Set<EtlListing>();
        public DbSet<EtlProperty> EtlProperties => Set<EtlProperty>();
        public DbSet<EtlPropertyPhoto> EtlPropertyPhotos => Set<EtlPropertyPhoto>();
        public DbSet<EtlRun> EtlRuns => Set<EtlRun>();
        public DbSet<EtlAdminAction> EtlAdminActions => Set<EtlAdminAction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- InvestmentProfile ----
            modelBuilder.Entity<InvestmentProfile>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.InvestmentProfileName)
                      .HasMaxLength(255)
                      .IsRequired();

                entity.Property(e => e.IsDefault)
                      .HasDefaultValue(false);

                entity.Property(e => e.UserId)
                      .HasMaxLength(450)
                      .IsRequired();

                entity.HasIndex(e => e.UserId);

                entity.HasIndex(e => new { e.UserId, e.IsDefault })
                      .IsUnique()
                      .HasFilter("[IsDefault] = 1");

                // Seed the “ID=1 default profile” you mentioned
                entity.HasData(new InvestmentProfile
                {
                    Id = 1,
                    InvestmentProfileName = "Default",
                    UserId = "SYSTEM",
                    IsDefault = true,
                    DownpaymentPercentage = 20m,
                    TermYears = 30,
                    MortgageInterestRate = 6.50m,
                    PMIRate = 0m,
                    PropertyTaxRate = 0m,
                    HomeownersInsuranceAnnual = 0m
                });
            });

            // ---- RentalListing ----
            modelBuilder.Entity<RentalListing>(entity =>
            {
                entity.HasKey(e => e.RentalListingId);

                entity.HasIndex(e => e.Zpid)
                      .IsUnique();

                entity.Property(e => e.StreetAddress).HasMaxLength(255);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(2);
                entity.Property(e => e.ZipCode).HasMaxLength(10);
                entity.Property(e => e.County).HasMaxLength(100);
                entity.Property(e => e.PropertyType).HasMaxLength(50);
                entity.Property(e => e.ImgSrc).HasMaxLength(1000);
                entity.Property(e => e.SourceSystem).HasMaxLength(50);

                entity.Property(e => e.IngestedAtUtc)
                      .HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // ---- SavedPropertyProfile ----
            modelBuilder.Entity<SavedPropertyProfile>(entity =>
            {
                entity.HasKey(e => e.SavedPropertyProfileId);

                entity.HasOne(e => e.InvestmentProfile)
                      .WithMany(p => p.SavedPropertyProfiles)
                      .HasForeignKey(e => e.InvestmentProfileId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RentalListing)
                      .WithMany(l => l.SavedPropertyProfiles)
                      .HasForeignKey(e => e.RentalListingId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Prevent duplicates: one saved record per (profile, listing)
                entity.HasIndex(e => new { e.InvestmentProfileId, e.RentalListingId })
                      .IsUnique();

                entity.HasIndex(e => e.InvestmentProfileId);

                entity.Property(e => e.DownpaymentPercentage)
                      .HasColumnType("decimal(18,4)");

                entity.Property(e => e.MortgageInterestRate)
                      .HasColumnType("decimal(18,4)");

                entity.Property(e => e.SavedAtUtc)
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(e => e.UserId)
                      .HasMaxLength(450)
                      .IsRequired();

                entity.HasIndex(e => e.UserId);

                entity.HasIndex(e => new { e.UserId, e.SavedAtUtc });
            });

            // ---- ETL: Properties ----
            modelBuilder.Entity<EtlProperty>(entity =>
            {
                entity.ToTable("properties");
                entity.HasKey(e => e.PropertyId);
                entity.Property(e => e.Street).HasMaxLength(255);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(50);
                entity.Property(e => e.Zip).HasMaxLength(20);
            });

            // ---- ETL: Listings ----
            modelBuilder.Entity<EtlListing>(entity =>
            {
                entity.ToTable("listings");
                entity.HasKey(e => e.ListingId);
                entity.Property(e => e.Source).HasMaxLength(100).IsRequired();
                entity.Property(e => e.SourceListingId).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => new { e.Source, e.SourceListingId }).IsUnique();
                entity.HasIndex(e => e.PropertyId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.LastSeenAt);
            });

            // ---- ETL: Property Photos ----
            modelBuilder.Entity<EtlPropertyPhoto>(entity =>
            {
                entity.ToTable("property_photos");
                entity.HasKey(e => e.PhotoId);
                entity.Property(e => e.Source).HasMaxLength(100).IsRequired();
                entity.Property(e => e.UrlOriginal).HasMaxLength(1000);
                entity.Property(e => e.StoragePath).HasMaxLength(500);
                entity.HasIndex(e => new { e.PropertyId, e.Source, e.PhotoIndex }).IsUnique();
            });

            // ---- ETL: Runs ----
            modelBuilder.Entity<EtlRun>(entity =>
            {
                entity.ToTable("etl_runs");
                entity.HasKey(e => e.RunId);
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Notes);
            });

            modelBuilder.Entity<EtlRunSourceStat>(entity =>
            {
                entity.ToTable("etl_run_source_stats");
                entity.HasKey(e => new { e.RunId, e.Source });
                entity.Property(e => e.Source).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<WorkQueueItem>(entity =>
            {
                entity.ToTable("work_queue");
                entity.HasKey(e => e.WorkId);
                entity.Property(e => e.WorkType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            });

            // ---- ETL: Admin Actions ----
            modelBuilder.Entity<EtlAdminAction>(entity =>
            {
                entity.ToTable("etl_admin_actions");
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
}
