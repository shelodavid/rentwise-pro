using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentWisePro.Web.Domain.Entities
{
    [Table("SavedPropertyProfiles")]
    public class SavedPropertyProfile
    {
        [Key]
        public int SavedPropertyProfileId { get; set; }

        // Relationships
        [Required]
        public int InvestmentProfileId { get; set; }

        [Required]
        public int RentalListingId { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        // “Snapshot” of assumptions at save-time (so forecasts are stable)
        [Column(TypeName = "decimal(18,4)")]
        public decimal DownpaymentPercentage { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MortgageInterestRate { get; set; }

        public int TermYears { get; set; }

        // Closing cost overrides (optional)
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

        public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation
        public InvestmentProfile InvestmentProfile { get; set; } = null!;
        public RentalListing RentalListing { get; set; } = null!;
    }
}
