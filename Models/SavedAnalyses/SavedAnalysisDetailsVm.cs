using System;

namespace RentWisePro.Web.Models.SavedAnalyses
{
    public class SavedAnalysisDetailsVm
    {
        public int SavedPropertyProfileId { get; init; }
        public string AddressLine { get; init; } = string.Empty;
        public string LocationLine { get; init; } = string.Empty;
        public decimal? Price { get; init; }
        public DateTime SavedAtUtc { get; init; }
        public string SnapshotInvestmentProfileName { get; init; } = string.Empty;
        public string ScenarioInvestmentProfileName { get; init; } = string.Empty;
        public int ScenarioInvestmentProfileId { get; init; }
        public bool IsScenarioProfileDifferent { get; init; }
        public IReadOnlyList<InvestmentProfileOptionVm> ScenarioProfiles { get; init; } = Array.Empty<InvestmentProfileOptionVm>();
        public SavedAnalysisAssumptionsVm Assumptions { get; init; } = new();
        public ClosingDisclosureSummaryVm ClosingDisclosure { get; init; } = new();
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

    public class InvestmentProfileOptionVm
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public bool IsDefault { get; init; }
    }

    public class ClosingDisclosureSummaryVm
    {
        public LoanTermsVm LoanTerms { get; init; } = new();
        public ClosingCostsSummaryVm ClosingCosts { get; init; } = new();
        public CashToCloseVm CashToClose { get; init; } = new();
    }

    public class LoanTermsVm
    {
        public decimal DownPaymentAmount { get; init; }
        public decimal DownPaymentPercentage { get; init; }
        public decimal LoanAmount { get; init; }
        public decimal InterestRate { get; init; }
        public int TermYears { get; init; }
        public decimal MonthlyPrincipalAndInterest { get; init; }
        public decimal MonthlyEscrow { get; init; }
        public decimal TotalMonthlyPayment { get; init; }
        public string MonthlyEscrowNote { get; init; } = string.Empty;
    }

    public class ClosingCostsSummaryVm
    {
        public decimal TotalClosingCosts { get; init; }
        public decimal CalculatedClosingCosts { get; init; }
        public bool ClosingCostOverrideApplied { get; init; }
        public decimal? ClosingCostOverride { get; init; }
        public IReadOnlyList<ClosingCostCategoryVm> Categories { get; init; } = Array.Empty<ClosingCostCategoryVm>();
    }

    public class ClosingCostCategoryVm
    {
        public string Name { get; init; } = string.Empty;
        public decimal Total { get; init; }
        public IReadOnlyList<ClosingCostLineItemVm> LineItems { get; init; } = Array.Empty<ClosingCostLineItemVm>();
    }

    public class ClosingCostLineItemVm
    {
        public string Label { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string? Note { get; init; }
    }

    public class CashToCloseVm
    {
        public decimal DownPaymentAmount { get; init; }
        public decimal DownPaymentPercentage { get; init; }
        public decimal ClosingCostsTotal { get; init; }
        public decimal CreditsAndAdjustments { get; init; }
        public decimal OtherUpfrontCosts { get; init; }
        public decimal TotalCashToClose { get; init; }
    }
}
