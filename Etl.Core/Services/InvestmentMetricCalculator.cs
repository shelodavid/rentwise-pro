using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Models;

namespace RentWisePro.Etl.Core.Services;

public class InvestmentMetricCalculator
{
    public ListingInvestmentMetrics Calculate(SourceListing listing, Property property, decimal? medianMonthlyIncome)
    {
        var price = listing.Price;
        var estimatedRent = listing.MonthlyRent;
        var squareFeet = listing.SquareFeet ?? property.SquareFeet;

        decimal? rprMonthly = null;
        decimal? grm = null;
        decimal? cashFlow = null;
        decimal? pricePerSqft = null;
        decimal? affordabilityIndex = null;

        if (estimatedRent.HasValue && price.HasValue && price.Value > 0m && estimatedRent.Value > 0m)
        {
            rprMonthly = estimatedRent.Value / price.Value;
            grm = price.Value / (estimatedRent.Value * 12m);

            var priceMonthlyCarry = price.Value * 0.01m / 12m;
            cashFlow = estimatedRent.Value
                       - priceMonthlyCarry
                       - priceMonthlyCarry
                       - (0.10m * estimatedRent.Value);
        }

        if (price.HasValue && price.Value > 0m && squareFeet.HasValue && squareFeet.Value > 0)
        {
            pricePerSqft = price.Value / squareFeet.Value;
        }

        if (estimatedRent.HasValue && estimatedRent.Value > 0m &&
            medianMonthlyIncome.HasValue && medianMonthlyIncome.Value > 0m)
        {
            affordabilityIndex = estimatedRent.Value / (medianMonthlyIncome.Value * 0.30m);
        }

        return new ListingInvestmentMetrics(
            estimatedRent,
            rprMonthly,
            grm,
            cashFlow,
            affordabilityIndex,
            pricePerSqft);
    }
}
