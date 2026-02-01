using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentWisePro.Web.Domain.Entities
{
    [Table("SavedPropertyProfiles")]
    public class SavedPropertyProfile
    {
        // Single identity primary key (SQL Server allows only ONE identity column)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SavedPropertyProfileId { get; set; }

        // Relationships (FKs)
        [Required]
        public int InvestmentProfileId { get; set; }

        [Required]
        public int RentalListingId { get; set; }

        [Required, MaxLength(450)]
        public string UserId { get; set; } = "";

        // “Snapshot” of assumptions at save-time (so forecasts are stable)
        [Column(TypeName = "decimal(18,4)")]
        public decimal DownpaymentPercentage { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MortgageInterestRate { get; set; }

        [Required]
        public int TermYears { get; set; }

        // Optional overrides
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ClosingCostOverride { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RenovationBudget { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OtherUpfrontCosts { get; set; }

        // Forecast overrides (optional)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyRentOverride { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyOtherExpensesOverride { get; set; }

        [Required]
        public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(InvestmentProfileId))]
        public InvestmentProfile InvestmentProfile { get; set; } = null!;

        [ForeignKey(nameof(RentalListingId))]
        public RentalListing RentalListing { get; set; } = null!;
    }
}
