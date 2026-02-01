using System;
using System.Collections.Generic;
using System.Linq;
using RentWisePro.Web.Domain.Entities;
using RentWisePro.Web.Models.SavedAnalyses;
using RentWisePro.Web.Services.Finance;

namespace RentWisePro.Web.Services
{
    public class ClosingDisclosureCalculationService
    {
        private const decimal TransferTaxEstimate = 250m;
        private const decimal RecordingFeeEstimate = 100m;
        private const int DefaultEscrowMonths = 2;

        public ClosingDisclosureSummaryVm BuildSummary(
            RentalListing listing,
            SavedPropertyProfile snapshot,
            InvestmentProfile profile)
        {
            var price = listing.Price ?? 0m;
            var downpaymentPercentage = snapshot.DownpaymentPercentage > 0m
                ? snapshot.DownpaymentPercentage
                : profile.DownpaymentPercentage;
            var interestRate = snapshot.MortgageInterestRate > 0m
                ? snapshot.MortgageInterestRate
                : profile.MortgageInterestRate;
            var termYears = snapshot.TermYears > 0
                ? snapshot.TermYears
                : profile.TermYears;

            var downpaymentAmount = price * (downpaymentPercentage / 100m);
            var loanAmount = Math.Max(price - downpaymentAmount, 0m);
            var monthlyPrincipalAndInterest = FinanceMath.CalculateMonthlyPayment(loanAmount, interestRate, termYears);

            var normalizedTaxRate = NormalizePropertyTaxRate(profile.PropertyTaxRate);
            var monthlyPropertyTaxes = price * normalizedTaxRate / 12m;
            var monthlyInsurance = profile.HomeownersInsuranceAnnual / 12m;
            var monthlyEscrow = monthlyPropertyTaxes + monthlyInsurance;
            var totalMonthlyPayment = monthlyPrincipalAndInterest + monthlyEscrow;

            var originationFee = loanAmount * ((profile.LoanOriginationFeePct ?? 0m) / 100m);
            var originationCategory = BuildCategory("Origination Charges", new[]
            {
                new ClosingCostLineItemVm
                {
                    Label = "Loan origination fee",
                    Amount = originationFee,
                    Note = "Estimated as a % of loan amount"
                }
            });

            var cannotShopCategory = BuildCategory("Services You Cannot Shop For", new[]
            {
                new ClosingCostLineItemVm { Label = "Appraisal fee", Amount = profile.AppraisalFee ?? 0m },
                new ClosingCostLineItemVm { Label = "Credit report fee", Amount = profile.CreditReportFee ?? 0m },
                new ClosingCostLineItemVm { Label = "Flood inspection fee", Amount = profile.FloodInspectionFee ?? 0m }
            });

            var canShopCategory = BuildCategory("Services You Can Shop For", new[]
            {
                new ClosingCostLineItemVm { Label = "Title insurance", Amount = profile.TitleInsuranceCost ?? 0m },
                new ClosingCostLineItemVm { Label = "Title search", Amount = profile.TitleSearchFee ?? 0m },
                new ClosingCostLineItemVm { Label = "Escrow fee", Amount = profile.EscrowFee ?? 0m }
            });

            var taxesCategory = BuildCategory("Taxes & Government Fees", new[]
            {
                new ClosingCostLineItemVm { Label = "Transfer taxes (estimate)", Amount = TransferTaxEstimate },
                new ClosingCostLineItemVm { Label = "Recording fees (estimate)", Amount = RecordingFeeEstimate }
            });

            var prepaidCategory = BuildCategory("Prepaids & Escrow", new[]
            {
                new ClosingCostLineItemVm
                {
                    Label = $"Homeowners insurance ({DefaultEscrowMonths} mo.)",
                    Amount = monthlyInsurance * DefaultEscrowMonths
                },
                new ClosingCostLineItemVm
                {
                    Label = $"Property taxes ({DefaultEscrowMonths} mo.)",
                    Amount = monthlyPropertyTaxes * DefaultEscrowMonths
                }
            });

            var categories = new List<ClosingCostCategoryVm>
            {
                originationCategory,
                cannotShopCategory,
                canShopCategory,
                taxesCategory,
                prepaidCategory
            };

            var calculatedClosingCosts = categories.Sum(category => category.Total);
            var closingCostOverride = snapshot.ClosingCostOverride;
            var totalClosingCosts = closingCostOverride ?? calculatedClosingCosts;

            var renovationBudget = snapshot.RenovationBudget ?? 0m;
            var otherUpfrontCosts = snapshot.OtherUpfrontCosts ?? 0m;
            var totalOtherUpfront = renovationBudget + otherUpfrontCosts;
            var totalCashToClose = downpaymentAmount + totalClosingCosts + totalOtherUpfront;

            return new ClosingDisclosureSummaryVm
            {
                LoanTerms = new LoanTermsVm
                {
                    DownPaymentAmount = downpaymentAmount,
                    DownPaymentPercentage = downpaymentPercentage,
                    LoanAmount = loanAmount,
                    InterestRate = interestRate,
                    TermYears = termYears,
                    MonthlyPrincipalAndInterest = monthlyPrincipalAndInterest,
                    MonthlyEscrow = monthlyEscrow,
                    TotalMonthlyPayment = totalMonthlyPayment,
                    MonthlyEscrowNote = BuildEscrowNote(normalizedTaxRate, profile.HomeownersInsuranceAnnual)
                },
                ClosingCosts = new ClosingCostsSummaryVm
                {
                    TotalClosingCosts = totalClosingCosts,
                    CalculatedClosingCosts = calculatedClosingCosts,
                    ClosingCostOverrideApplied = closingCostOverride.HasValue,
                    ClosingCostOverride = closingCostOverride,
                    Categories = categories
                },
                CashToClose = new CashToCloseVm
                {
                    DownPaymentAmount = downpaymentAmount,
                    DownPaymentPercentage = downpaymentPercentage,
                    ClosingCostsTotal = totalClosingCosts,
                    CreditsAndAdjustments = 0m,
                    OtherUpfrontCosts = totalOtherUpfront,
                    TotalCashToClose = totalCashToClose
                }
            };
        }

        private static ClosingCostCategoryVm BuildCategory(string name, IEnumerable<ClosingCostLineItemVm> items)
        {
            var lineItems = items.ToList();
            var total = lineItems.Sum(item => item.Amount);
            return new ClosingCostCategoryVm
            {
                Name = name,
                Total = total,
                LineItems = lineItems
            };
        }

        private static decimal NormalizePropertyTaxRate(decimal rawRate)
        {
            if (rawRate <= 0m)
            {
                return 0m;
            }

            // If the rate is stored as 1.25, treat it as 1.25% (divide by 100).
            return rawRate > 1m ? rawRate / 100m : rawRate;
        }

        private static string BuildEscrowNote(decimal normalizedTaxRate, decimal annualInsurance)
        {
            if (normalizedTaxRate <= 0m && annualInsurance <= 0m)
            {
                return "Escrow uses default profile assumptions; taxes and insurance were not provided.";
            }

            if (normalizedTaxRate <= 0m)
            {
                return "Escrow includes insurance only (tax rate not provided).";
            }

            if (annualInsurance <= 0m)
            {
                return "Escrow includes taxes only (insurance not provided).";
            }

            return "Escrow includes estimated taxes and insurance from the scenario profile.";
        }
    }
}
