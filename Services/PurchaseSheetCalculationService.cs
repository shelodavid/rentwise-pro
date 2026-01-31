using RentWisePro.Web.Domain.Entities;

namespace RentWisePro.Web.Services
{
    public class PurchaseSheetCalculationService
    {
        public PurchaseSheetCalculationResult Calculate(
            RentalListing listing,
            InvestmentProfile profileDefaults,
            SavedPropertyProfile overrides)
        {
            var purchasePrice = listing.Price ?? 0m;
            var downpaymentPercentage = overrides.DownpaymentPercentage;
            var downpaymentAmount = purchasePrice * (downpaymentPercentage / 100m);
            var mortgageAmount = Math.Max(purchasePrice - downpaymentAmount, 0m);

            var termMonths = Math.Max(overrides.TermYears * 12, 0);
            var monthlyInterestRate = (overrides.MortgageInterestRate / 100m) / 12m;
            var monthlyPrincipalAndInterest = CalculateMonthlyPrincipalAndInterest(
                mortgageAmount,
                monthlyInterestRate,
                termMonths);

            var closingCostsBreakdown = new PurchaseSheetClosingCostsBreakdown
            {
                ClosingCostsPercentage = purchasePrice * ((profileDefaults.ClosingCostsPercentage ?? 0m) / 100m),
                RealtorFees = purchasePrice * ((profileDefaults.RealtorClosingFeePercentage ?? 0m) / 100m),
                LoanOriginationFee = mortgageAmount * ((profileDefaults.LoanOriginationFeePct ?? 0m) / 100m),
                AppraisalFee = profileDefaults.AppraisalFee ?? 0m,
                CreditReportFee = profileDefaults.CreditReportFee ?? 0m,
                TitleInsurance = profileDefaults.TitleInsuranceCost ?? 0m,
                TitleSearch = profileDefaults.TitleSearchFee ?? 0m,
                EscrowFee = profileDefaults.EscrowFee ?? 0m,
                FloodInspectionFee = profileDefaults.FloodInspectionFee ?? 0m,
                MiscellaneousFees = profileDefaults.MiscellaneousFees ?? 0m,
                HoaEstimate = profileDefaults.HOAEstimate ?? 0m
            };

            var calculatedClosingCosts = closingCostsBreakdown.Total;
            var totalClosingCosts = overrides.ClosingCostOverride ?? calculatedClosingCosts;

            var renovationBudget = overrides.RenovationBudget ?? 0m;
            var otherUpfrontCosts = overrides.OtherUpfrontCosts ?? 0m;

            var cashToClose = downpaymentAmount + totalClosingCosts + renovationBudget + otherUpfrontCosts;

            return new PurchaseSheetCalculationResult
            {
                DownpaymentAmount = downpaymentAmount,
                MortgageAmount = mortgageAmount,
                MonthlyPrincipalAndInterest = monthlyPrincipalAndInterest,
                ClosingCosts = closingCostsBreakdown,
                TotalClosingCosts = totalClosingCosts,
                CashToClose = cashToClose,
                CalculatedClosingCosts = calculatedClosingCosts
            };
        }

        private static decimal CalculateMonthlyPrincipalAndInterest(
            decimal mortgageAmount,
            decimal monthlyInterestRate,
            int termMonths)
        {
            if (mortgageAmount <= 0m || termMonths <= 0)
            {
                return 0m;
            }

            if (monthlyInterestRate == 0m)
            {
                return mortgageAmount / termMonths;
            }

            var rateAsDouble = (double)monthlyInterestRate;
            var pow = Math.Pow(1 + rateAsDouble, termMonths);
            var numerator = (double)mortgageAmount * rateAsDouble * pow;
            var denominator = pow - 1;

            return denominator == 0
                ? 0m
                : (decimal)(numerator / denominator);
        }
    }

    public class PurchaseSheetCalculationResult
    {
        public decimal DownpaymentAmount { get; set; }
        public decimal MortgageAmount { get; set; }
        public decimal MonthlyPrincipalAndInterest { get; set; }
        public PurchaseSheetClosingCostsBreakdown ClosingCosts { get; set; } = new();
        public decimal TotalClosingCosts { get; set; }
        public decimal CalculatedClosingCosts { get; set; }
        public decimal CashToClose { get; set; }
    }

    public class PurchaseSheetClosingCostsBreakdown
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

        public decimal Total => ClosingCostsPercentage
            + RealtorFees
            + LoanOriginationFee
            + AppraisalFee
            + CreditReportFee
            + TitleInsurance
            + TitleSearch
            + EscrowFee
            + FloodInspectionFee
            + MiscellaneousFees
            + HoaEstimate;
    }
}
