using RentWisePro.Web.Models.SavedAnalyses;

namespace RentWisePro.Web.Models.Forecast
{
    public class SavedAnalysisForecastVm
    {
        public int SavedPropertyProfileId { get; init; }
        public string AddressLine { get; init; } = string.Empty;
        public string LocationLine { get; init; } = string.Empty;
        public string InvestmentProfileName { get; init; } = string.Empty;
        public bool UsingDefaultInvestmentProfile { get; init; }
        public string InvestmentProfileNote { get; init; } = string.Empty;
        public ListingSummary Listing { get; init; } = new();
        public SavedAnalysisAssumptionsVm SnapshotAssumptions { get; init; } = new();
        public ForecastAssumptions Assumptions { get; init; } = new();
        public ForecastKpis Kpis { get; init; } = new();
        public IReadOnlyList<ForecastHorizonProjectionVm> Projections { get; init; } = Array.Empty<ForecastHorizonProjectionVm>();
    }

    public class ForecastHorizonProjectionVm
    {
        public string Label { get; init; } = string.Empty;
        public int Months { get; init; }
        public decimal NetCashflow { get; init; }
        public decimal CashOnCashReturnPercent { get; init; }
    }
}
