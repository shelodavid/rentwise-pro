using RentWisePro.Etl.Core.Entities;
using RentWisePro.Etl.Core.Models;
using RentWisePro.Etl.Core.Services;
using Xunit;

namespace RentWisePro.Etl.Tests.Services;

public class InvestmentMetricCalculatorTests
{
    [Fact]
    public void Calculate_ComputesRentPriceMetrics()
    {
        var calculator = new InvestmentMetricCalculator();
        var listing = new SourceListing
        {
            Price = 120000m,
            MonthlyRent = 1200m
        };
        var property = new Property();

        var metrics = calculator.Calculate(listing, property, null);

        Assert.Equal(1200m, metrics.EstimatedRent);
        Assert.Equal(0.01m, metrics.RprMonthly);
        Assert.NotNull(metrics.Grm);
        Assert.InRange(metrics.Grm.Value, 8.333332m, 8.333334m);

        var expectedCashFlow = 1200m
                               - (120000m * 0.01m / 12m)
                               - (120000m * 0.01m / 12m)
                               - (0.10m * 1200m);
        Assert.Equal(expectedCashFlow, metrics.EstimatedCashFlow);
    }

    [Fact]
    public void Calculate_SkipsMetricsWhenMissingRentOrPrice()
    {
        var calculator = new InvestmentMetricCalculator();
        var listing = new SourceListing
        {
            Price = 200000m
        };
        var property = new Property();

        var metrics = calculator.Calculate(listing, property, null);

        Assert.Null(metrics.RprMonthly);
        Assert.Null(metrics.Grm);
        Assert.Null(metrics.EstimatedCashFlow);
        Assert.Null(metrics.EstimatedRent);
    }

    [Fact]
    public void Calculate_ComputesAffordabilityAndPricePerSqft()
    {
        var calculator = new InvestmentMetricCalculator();
        var listing = new SourceListing
        {
            Price = 200000m,
            MonthlyRent = 1500m,
            SquareFeet = 1000
        };
        var property = new Property();

        var metrics = calculator.Calculate(listing, property, 5000m);

        Assert.Equal(200m, metrics.PricePerSqft);
        Assert.Equal(1.0m, metrics.AffordabilityIndex);
    }
}
