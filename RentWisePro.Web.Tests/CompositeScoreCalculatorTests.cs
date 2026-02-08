using RentWisePro.Web.Services;
using Xunit;

namespace RentWisePro.Web.Tests
{
    public class CompositeScoreCalculatorTests
    {
        [Fact]
        public void Calculate_ReturnsNullScore_WhenInputsMissing()
        {
            var calculator = new CompositeScoreCalculator();
            var result = calculator.Calculate(new CompositeScoreInputs(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null));

            Assert.Null(result.Score);
            Assert.Equal(0m, result.Breakdown.TotalWeight);
        }

        [Fact]
        public void Calculate_ProducesStableScore()
        {
            var calculator = new CompositeScoreCalculator();
            var inputs = new CompositeScoreInputs(
                RentToPriceRatioMonthly: 0.015m,
                EstimatedRent: 2200m,
                FairMarketRent: 2000m,
                VacancyRate: 4m,
                AffordabilityIndex: 105m,
                PricePerSqft: 180m,
                MedianPricePerSqft: 200m,
                PropertyType: "Single Family");

            var result = calculator.Calculate(inputs);

            Assert.NotNull(result.Score);
            Assert.Equal(100m, result.Breakdown.TotalWeight);
            Assert.Equal(81.3m, result.Score);
        }

        [Fact]
        public void Calculate_ClampsScoresBetweenZeroAndHundred()
        {
            var calculator = new CompositeScoreCalculator();
            var inputs = new CompositeScoreInputs(
                RentToPriceRatioMonthly: 0.1m,
                EstimatedRent: 5000m,
                FairMarketRent: 1000m,
                VacancyRate: 25m,
                AffordabilityIndex: 200m,
                PricePerSqft: 500m,
                MedianPricePerSqft: 100m,
                PropertyType: "Single Family");

            var result = calculator.Calculate(inputs);

            Assert.NotNull(result.Score);
            Assert.InRange(result.Score.Value, 0m, 100m);
        }
    }
}
