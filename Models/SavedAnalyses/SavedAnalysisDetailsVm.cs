namespace RentWisePro.Web.Models.SavedAnalyses
{
    public class SavedAnalysisDetailsVm
    {
        public int SavedPropertyProfileId { get; init; }
        public string AddressLine { get; init; } = string.Empty;
        public string LocationLine { get; init; } = string.Empty;
        public decimal? Price { get; init; }
        public DateTime SavedAtUtc { get; init; }
        public string InvestmentProfileName { get; init; } = string.Empty;
        public SavedAnalysisAssumptionsVm Assumptions { get; init; } = new();
    }

    public class SavedAnalysisAssumptionsVm
    {
        public decimal DownpaymentPercentage { get; init; }
        public decimal MortgageInterestRate { get; init; }
        public int TermYears { get; init; }
        public decimal? ClosingCostOverride { get; init; }
        public decimal? RenovationBudget { get; init; }
        public decimal? OtherUpfrontCosts { get; init; }
        public decimal? MonthlyRentOverride { get; init; }
        public decimal? MonthlyOtherExpensesOverride { get; init; }
    }
}
