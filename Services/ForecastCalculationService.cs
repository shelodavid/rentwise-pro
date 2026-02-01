using RentWisePro.Web.Domain.Entities;
using RentWisePro.Web.Models.Forecast;
using RentWisePro.Web.Services.Finance;

namespace RentWisePro.Web.Services
{
    public class ForecastCalculationService
    {
        public ForecastCalculationResult Calculate(RentalListing listing, InvestmentProfile profile, SavedPropertyProfile? overrides)
        {
            var price = listing.Price ?? 0m;
            var downpaymentPercentage = overrides?.DownpaymentPercentage ?? profile.DownpaymentPercentage;
            var interestRate = overrides?.MortgageInterestRate ?? profile.MortgageInterestRate;
            var termYears = overrides?.TermYears ?? profile.TermYears;
            var monthlyRent = overrides?.MonthlyRentOverride ?? listing.EstimatedRent ?? 0m;

            var downpaymentAmount = price * (downpaymentPercentage / 100m);
            var loanAmount = price - downpaymentAmount;
            var monthlyMortgage = FinanceMath.CalculateMonthlyPayment(loanAmount, interestRate, termYears);
            var monthlyPmi = loanAmount * (profile.PMIRate / 100m) / 12m;

            var monthlyPropertyTaxes = price * (profile.PropertyTaxRate / 100m) / 12m;
            var monthlyInsurance = profile.HomeownersInsuranceAnnual / 12m;
            var monthlyVacancy = monthlyRent * ((profile.VacancyRate ?? 0m) / 100m);
            var monthlyPropertyManagement = monthlyRent * ((profile.PropertyManagementFeePct ?? 0m) / 100m);
            var monthlyMaintenance = profile.MonthlyMaintenanceBudget ?? 0m;
            var monthlyUtilities = profile.MonthlyUtilitiesCost ?? 0m;
            var monthlyOtherExpenses = overrides?.MonthlyOtherExpensesOverride ?? 0m;

            var monthlyNonMortgageExpenses = monthlyPropertyTaxes + monthlyInsurance + monthlyVacancy +
                                             monthlyPropertyManagement + monthlyMaintenance + monthlyUtilities + monthlyOtherExpenses;
            var totalMonthlyExpenses = monthlyNonMortgageExpenses + monthlyMortgage + monthlyPmi;
            var monthlyCashflow = monthlyRent - totalMonthlyExpenses;

            var annualNoi = (monthlyRent - monthlyNonMortgageExpenses) * 12m;
            var annualDebtService = (monthlyMortgage + monthlyPmi) * 12m;

            var closingCosts = overrides?.ClosingCostOverride ?? EstimateClosingCosts(price, loanAmount, profile);
            var renovationBudget = overrides?.RenovationBudget ?? 0m;
            var otherUpfrontCosts = overrides?.OtherUpfrontCosts ?? 0m;
            var totalCashInvested = downpaymentAmount + closingCosts + renovationBudget + otherUpfrontCosts;

            var cashOnCashReturnPercent = totalCashInvested > 0m
                ? (monthlyCashflow * 12m / totalCashInvested) * 100m
                : 0m;

            var capRatePercent = price > 0m
                ? (annualNoi / price) * 100m
                : 0m;

            var dscr = annualDebtService > 0m
                ? annualNoi / annualDebtService
                : 0m;

            return new ForecastCalculationResult
            {
                Assumptions = new ForecastAssumptions
                {
                    DownpaymentPercentage = downpaymentPercentage,
                    MortgageInterestRate = interestRate,
                    TermYears = termYears,
                    LoanAmount = loanAmount,
                    MonthlyRent = monthlyRent,
                    MonthlyMortgage = monthlyMortgage,
                    MonthlyPmi = monthlyPmi,
                    MonthlyPropertyTaxes = monthlyPropertyTaxes,
                    MonthlyInsurance = monthlyInsurance,
                    MonthlyVacancy = monthlyVacancy,
                    MonthlyPropertyManagement = monthlyPropertyManagement,
                    MonthlyMaintenance = monthlyMaintenance,
                    MonthlyUtilities = monthlyUtilities,
                    MonthlyOtherExpenses = monthlyOtherExpenses,
                    MonthlyNonMortgageExpenses = monthlyNonMortgageExpenses,
                    TotalMonthlyExpenses = totalMonthlyExpenses,
                    TotalCashInvested = totalCashInvested
                },
                Kpis = new ForecastKpis
                {
                    MonthlyCashflow = monthlyCashflow,
                    CashOnCashReturnPercent = cashOnCashReturnPercent,
                    CapRatePercent = capRatePercent,
                    Dscr = dscr
                }
            };
        }

        public IReadOnlyList<ForecastHorizonProjectionVm> BuildHorizonProjections(
            ForecastCalculationResult calculation)
        {
            var horizons = new[]
            {
                new ForecastHorizonProjectionVm { Label = "6 months", Months = 6 },
                new ForecastHorizonProjectionVm { Label = "12 months", Months = 12 },
                new ForecastHorizonProjectionVm { Label = "1 year", Months = 12 },
                new ForecastHorizonProjectionVm { Label = "5 years", Months = 60 }
            };

            var monthlyCashflow = calculation.Kpis.MonthlyCashflow;
            var totalCashInvested = calculation.Assumptions.TotalCashInvested;

            return horizons.Select(horizon =>
            {
                var netCashflow = monthlyCashflow * horizon.Months;
                var cashOnCash = totalCashInvested > 0m
                    ? (netCashflow / totalCashInvested) * 100m
                    : 0m;

                return new ForecastHorizonProjectionVm
                {
                    Label = horizon.Label,
                    Months = horizon.Months,
                    NetCashflow = netCashflow,
                    CashOnCashReturnPercent = cashOnCash
                };
            }).ToList();
        }

        private static decimal EstimateClosingCosts(decimal price, decimal loanAmount, InvestmentProfile profile)
        {
            var percentCosts = price * ((profile.ClosingCostsPercentage ?? 0m) / 100m);
            var realtorCosts = price * ((profile.RealtorClosingFeePercentage ?? 0m) / 100m);
            var originationCosts = loanAmount * ((profile.LoanOriginationFeePct ?? 0m) / 100m);

            var flatFees = (profile.AppraisalFee ?? 0m) + (profile.CreditReportFee ?? 0m) +
                           (profile.TitleInsuranceCost ?? 0m) + (profile.TitleSearchFee ?? 0m) +
                           (profile.EscrowFee ?? 0m) + (profile.FloodInspectionFee ?? 0m) +
                           (profile.MiscellaneousFees ?? 0m);

            return percentCosts + realtorCosts + originationCosts + flatFees;
        }
    }
}
