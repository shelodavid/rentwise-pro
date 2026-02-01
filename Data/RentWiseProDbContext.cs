using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Domain.Entities;

namespace RentWisePro.Web.Data
{
    public class RentWiseProDbContext : DbContext
    {
        public RentWiseProDbContext(DbContextOptions<RentWiseProDbContext> options)
            : base(options)
        {
        }

        public DbSet<InvestmentProfile> InvestmentProfiles => Set<InvestmentProfile>();
        public DbSet<RentalListing> RentalListings => Set<RentalListing>();
        public DbSet<SavedPropertyProfile> SavedPropertyProfiles => Set<SavedPropertyProfile>();

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

                // Seed the “ID=1 default profile” you mentioned
                entity.HasData(new InvestmentProfile
                {
                    Id = 1,
                    InvestmentProfileName = "Default",
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
            });
        }
    }
}
