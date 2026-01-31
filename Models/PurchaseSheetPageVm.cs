using System.ComponentModel.DataAnnotations;

namespace RentWisePro.Web.Models
{
    public class PurchaseSheetPageVm
    {
        public long Zpid { get; set; }
        public int RentalListingId { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? PropertyType { get; set; }
        public decimal? Price { get; set; }
        public decimal? EstimatedRent { get; set; }
        public int? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }
        public string? ImgSrc { get; set; }

        [Display(Name = "Downpayment (%)")]
        [Range(0, 100)]
        public decimal DownpaymentPercentage { get; set; }

        [Display(Name = "Interest Rate (%)")]
        [Range(0, 100)]
        public decimal MortgageInterestRate { get; set; }

        [Display(Name = "Loan Term (Years)")]
        [Range(1, 50)]
        public int TermYears { get; set; }

        [Display(Name = "Closing Cost Override")]
        [Range(0, 100000000)]
        public decimal? ClosingCostOverride { get; set; }

        [Display(Name = "Renovation Budget")]
        [Range(0, 100000000)]
        public decimal? RenovationBudget { get; set; }

        [Display(Name = "Other Upfront Costs")]
        [Range(0, 100000000)]
        public decimal? OtherUpfrontCosts { get; set; }

        public PurchaseSheetOutputsVm Outputs { get; set; } = new();
        public PurchaseSheetClosingCostsVm ClosingCosts { get; set; } = new();
    }

    public class PurchaseSheetOutputsVm
    {
        public decimal DownpaymentAmount { get; set; }
        public decimal MortgageAmount { get; set; }
        public decimal MonthlyPrincipalAndInterest { get; set; }
        public decimal TotalClosingCosts { get; set; }
        public decimal CalculatedClosingCosts { get; set; }
        public decimal CashToClose { get; set; }
    }

    public class PurchaseSheetClosingCostsVm
    {
        public decimal ClosingCostsPercentage { get; set; }
        public decimal RealtorFees { get; set; }
        public decimal LoanOriginationFee { get; set; }
        public decimal AppraisalFee { get; set; }
        public decimal CreditReportFee { get; set; }
        public decimal TitleInsurance { get; set; }
        public decimal TitleSearch { get; set; }
        public decimal EscrowFee { get; set; }
        public decimal FloodInspectionFee { get; set; }
        public decimal MiscellaneousFees { get; set; }
        public decimal HoaEstimate { get; set; }
    }

    public class HomeIndexVm
    {
        public List<PurchaseSheetListingVm> Listings { get; set; } = new();
    }

    public class PurchaseSheetListingVm
    {
        public long Zpid { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public decimal? Price { get; set; }
        public int? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }
        public string? ImgSrc { get; set; }
    }
}
