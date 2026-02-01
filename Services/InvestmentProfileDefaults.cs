using RentWisePro.Web.Domain.Entities;

namespace RentWisePro.Web.Services
{
    public static class InvestmentProfileDefaults
    {
        public const string DefaultProfileName = "Default Profile";

        public static InvestmentProfile CreateDefault()
        {
            return new InvestmentProfile
            {
                InvestmentProfileName = DefaultProfileName,
                IsDefault = true,
                DownpaymentPercentage = 20m,
                TermYears = 30,
                MortgageInterestRate = 6.50m,
                PMIRate = 0.8m,
                PropertyTaxRate = 1.0m,
                HomeownersInsuranceAnnual = 1200m,
                VacancyRate = 5m,
                PropertyManagementFeePct = 0m,
                MonthlyMaintenanceBudget = 0m,
                MonthlyUtilitiesCost = 0m,
                RealtorClosingFeePercentage = 3.0m,
                ClosingCostsPercentage = 2.0m,
                LoanOriginationFeePct = 0m,
                AppraisalFee = 0m,
                CreditReportFee = 0m,
                TitleInsuranceCost = 0m,
                TitleSearchFee = 0m,
                EscrowFee = 0m,
                FloodInspectionFee = 0m,
                MiscellaneousFees = 0m,
                HOAEstimate = 0m
            };
        }
    }
}
