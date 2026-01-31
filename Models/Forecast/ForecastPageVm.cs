namespace RentWisePro.Web.Models.Forecast
{
    public class ForecastPageVm
    {
        public ListingSummary Listing { get; set; } = new();
        public ForecastAssumptions Assumptions { get; set; } = new();
        public ForecastKpis Kpis { get; set; } = new();
    }

    public class ListingSummary
    {
        public long Zpid { get; set; }
        public string? AddressLine { get; set; }
        public string? LocationLine { get; set; }
        public string? PropertyType { get; set; }
        public string? ImgSrc { get; set; }
        public decimal Price { get; set; }
        public decimal EstimatedRent { get; set; }
        public int? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }
    }

    public class ForecastAssumptions
    {
        public decimal DownpaymentPercentage { get; set; }
        public decimal MortgageInterestRate { get; set; }
        public int TermYears { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal MonthlyMortgage { get; set; }
        public decimal MonthlyPmi { get; set; }
        public decimal MonthlyPropertyTaxes { get; set; }
        public decimal MonthlyInsurance { get; set; }
        public decimal MonthlyVacancy { get; set; }
        public decimal MonthlyPropertyManagement { get; set; }
        public decimal MonthlyMaintenance { get; set; }
        public decimal MonthlyUtilities { get; set; }
        public decimal MonthlyOtherExpenses { get; set; }
        public decimal MonthlyNonMortgageExpenses { get; set; }
        public decimal TotalMonthlyExpenses { get; set; }
        public decimal TotalCashInvested { get; set; }
    }

    public class ForecastKpis
    {
        public decimal MonthlyCashflow { get; set; }
        public decimal CashOnCashReturnPercent { get; set; }
        public decimal CapRatePercent { get; set; }
        public decimal Dscr { get; set; }
    }

    public class ForecastCalculationResult
    {
        public ForecastAssumptions Assumptions { get; set; } = new();
        public ForecastKpis Kpis { get; set; } = new();
    }
}
