using System.ComponentModel.DataAnnotations;

namespace RentWisePro.Web.Models
{
    public class InvestmentProfileVm
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Profile name")]
        public string InvestmentProfileName { get; set; } = string.Empty;

        [Range(0, 100)]
        [Display(Name = "Down payment (%)")]
        public decimal DownpaymentPercentage { get; set; } = 20m;

        [Range(5, 40)]
        [Display(Name = "Term (years)")]
        public int TermYears { get; set; } = 30;

        [Range(0, 25)]
        [Display(Name = "Mortgage interest rate (%)")]
        public decimal MortgageInterestRate { get; set; } = 6.5m;

        [Range(0, 25)]
        [Display(Name = "PMI rate (%)")]
        public decimal PMIRate { get; set; } = 0m;

        [Range(0, 25)]
        [Display(Name = "Property tax rate (%)")]
        public decimal PropertyTaxRate { get; set; } = 0m;

        [Range(0, 1000000)]
        [Display(Name = "Homeowners insurance (annual $)")]
        public decimal HomeownersInsuranceAnnual { get; set; } = 0m;

        [Range(0, 100)]
        [Display(Name = "Vacancy rate (%)")]
        public decimal? VacancyRate { get; set; } = 0m;

        [Range(0, 100)]
        [Display(Name = "Property management fee (%)")]
        public decimal? PropertyManagementFeePct { get; set; } = 0m;

        [Range(0, 100000)]
        [Display(Name = "Monthly maintenance budget ($)")]
        public decimal? MonthlyMaintenanceBudget { get; set; } = 0m;

        [Range(0, 100000)]
        [Display(Name = "Monthly utilities ($)")]
        public decimal? MonthlyUtilitiesCost { get; set; } = 0m;

        [Range(0, 100)]
        [Display(Name = "Realtor closing fee (%)")]
        public decimal? RealtorClosingFeePercentage { get; set; } = 0m;

        [Range(0, 100)]
        [Display(Name = "Closing costs (%)")]
        public decimal? ClosingCostsPercentage { get; set; } = 0m;

        [Range(0, 100)]
        [Display(Name = "Loan origination fee (%)")]
        public decimal? LoanOriginationFeePct { get; set; } = 0m;

        [Range(0, 100000)]
        [Display(Name = "Appraisal fee ($)")]
        public decimal? AppraisalFee { get; set; } = 0m;

        [Range(0, 100000)]
        [Display(Name = "Credit report fee ($)")]
        public decimal? CreditReportFee { get; set; } = 0m;

        [Range(0, 100000)]
        [Display(Name = "Title insurance ($)")]
        public decimal? TitleInsuranceCost { get; set; } = 0m;

        [Range(0, 100000)]
        [Display(Name = "Title search fee ($)")]
        public decimal? TitleSearchFee { get; set; } = 0m;

        [Range(0, 100000)]
        [Display(Name = "Escrow fee ($)")]
        public decimal? EscrowFee { get; set; } = 0m;

        [Range(0, 100000)]
        [Display(Name = "Flood inspection fee ($)")]
        public decimal? FloodInspectionFee { get; set; } = 0m;

        [Range(0, 100000)]
        [Display(Name = "Miscellaneous fees ($)")]
        public decimal? MiscellaneousFees { get; set; } = 0m;

        [Range(0, 100000)]
        [Display(Name = "HOA estimate ($)")]
        public decimal? HOAEstimate { get; set; } = 0m;

        [Display(Name = "Set as default")]
        public bool IsDefault { get; set; }
    }
}
