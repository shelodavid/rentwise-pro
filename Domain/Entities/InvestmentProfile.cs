using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentWisePro.Web.Domain.Entities
{
    [Table("InvestmentProfiles")]
    public class InvestmentProfile
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string InvestmentProfileName { get; set; } = "";

        // Purchase assumptions
        [Column(TypeName = "decimal(18,2)")]
        public decimal DownpaymentPercentage { get; set; } = 20m;

        public int TermYears { get; set; } = 30;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MortgageInterestRate { get; set; } = 6.5m;

        [Column(TypeName = "decimal(18,4)")]
        public decimal PMIRate { get; set; } = 0m;

        // Recurring costs / forecast assumptions
        [Column(TypeName = "decimal(18,4)")]
        public decimal PropertyTaxRate { get; set; } = 0m;

        [Column(TypeName = "decimal(18,4)")]
        public decimal HomeownersInsuranceAnnual { get; set; } = 0m;

        [Column(TypeName = "decimal(18,4)")]
        public decimal? VacancyRate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? PropertyManagementFeePct { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? MonthlyMaintenanceBudget { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? MonthlyUtilitiesCost { get; set; }

        // Closing cost assumptions
        [Column(TypeName = "decimal(18,4)")]
        public decimal? RealtorClosingFeePercentage { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? ClosingCostsPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? LoanOriginationFeePct { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AppraisalFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CreditReportFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TitleInsuranceCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TitleSearchFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EscrowFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? FloodInspectionFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MiscellaneousFees { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? HOAEstimate { get; set; }

        // Navigation
        public ICollection<SavedPropertyProfile> SavedPropertyProfiles { get; set; } = new List<SavedPropertyProfile>();
    }
}
