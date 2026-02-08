using System.Globalization;

namespace RentWisePro.Web.Services
{
    public class CompositeScoreCalculator
    {
        public const string Version = "v1";

        private const decimal RprWeight = 30m;
        private const decimal RentVsFmrWeight = 20m;
        private const decimal VacancyWeight = 15m;
        private const decimal AffordabilityWeight = 15m;
        private const decimal PricePerSqftWeight = 10m;
        private const decimal PropertyTypeWeight = 10m;

        private static readonly Dictionary<string, decimal> PropertyTypeScores = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Single Family"] = 100m,
            ["SFR"] = 100m,
            ["Multi Family"] = 95m,
            ["Duplex"] = 90m,
            ["Triplex"] = 90m,
            ["Quadplex"] = 90m,
            ["Townhouse"] = 85m,
            ["Apartment"] = 80m,
            ["Condo"] = 75m,
            ["Manufactured"] = 60m
        };

        public CompositeScoreResult Calculate(CompositeScoreInputs inputs)
        {
            var breakdown = new CompositeScoreBreakdown();
            var totalWeight = 0m;
            var totalScore = 0m;

            var rprScore = NormalizeScore(inputs.RentToPriceRatioMonthly, 0.005m, 0.02m);
            var rentVsFmrScore = NormalizeRentVsFmr(inputs.EstimatedRent, inputs.FairMarketRent);

            var rentVsFmrWeight = rentVsFmrScore.HasValue ? RentVsFmrWeight : 0m;
            var rprWeight = RprWeight + (rentVsFmrScore.HasValue ? 0m : RentVsFmrWeight);
            if (rprScore.HasValue)
            {
                var contribution = rprScore.Value * rprWeight / 100m;
                breakdown = breakdown with { RprContribution = contribution, RprWeight = rprWeight };
                totalScore += contribution;
                totalWeight += rprWeight;
            }

            if (rentVsFmrScore.HasValue)
            {
                var contribution = rentVsFmrScore.Value * rentVsFmrWeight / 100m;
                breakdown = breakdown with { RentVsFmrContribution = contribution, RentVsFmrWeight = rentVsFmrWeight };
                totalScore += contribution;
                totalWeight += rentVsFmrWeight;
            }

            var vacancyScore = NormalizeVacancy(inputs.VacancyRate);
            if (vacancyScore.HasValue)
            {
                var contribution = vacancyScore.Value * VacancyWeight / 100m;
                breakdown = breakdown with { VacancyContribution = contribution, VacancyWeight = VacancyWeight };
                totalScore += contribution;
                totalWeight += VacancyWeight;
            }

            var affordabilityScore = NormalizeScore(inputs.AffordabilityIndex, 60m, 120m);
            if (affordabilityScore.HasValue)
            {
                var contribution = affordabilityScore.Value * AffordabilityWeight / 100m;
                breakdown = breakdown with { AffordabilityContribution = contribution, AffordabilityWeight = AffordabilityWeight };
                totalScore += contribution;
                totalWeight += AffordabilityWeight;
            }

            var pricePerSqftScore = NormalizePricePerSqft(inputs.PricePerSqft, inputs.MedianPricePerSqft);
            if (pricePerSqftScore.HasValue)
            {
                var contribution = pricePerSqftScore.Value * PricePerSqftWeight / 100m;
                breakdown = breakdown with { PricePerSqftContribution = contribution, PricePerSqftWeight = PricePerSqftWeight };
                totalScore += contribution;
                totalWeight += PricePerSqftWeight;
            }

            var propertyTypeScore = NormalizePropertyType(inputs.PropertyType);
            if (propertyTypeScore.HasValue)
            {
                var contribution = propertyTypeScore.Value * PropertyTypeWeight / 100m;
                breakdown = breakdown with { PropertyTypeContribution = contribution, PropertyTypeWeight = PropertyTypeWeight };
                totalScore += contribution;
                totalWeight += PropertyTypeWeight;
            }

            if (totalWeight == 0m)
            {
                return new CompositeScoreResult(null, breakdown, Version);
            }

            var finalScore = Math.Clamp(totalScore / totalWeight * 100m, 0m, 100m);
            return new CompositeScoreResult(decimal.Round(finalScore, 1), breakdown with { TotalWeight = totalWeight }, Version);
        }

        public static string BuildTooltip(CompositeScoreResult result)
        {
            if (!result.Score.HasValue)
            {
                return "Composite score unavailable (missing metrics).";
            }

            var parts = new List<string>
            {
                FormatContribution("RPR", result.Breakdown.RprContribution, result.Breakdown.RprWeight),
                FormatContribution("Rent vs FMR", result.Breakdown.RentVsFmrContribution, result.Breakdown.RentVsFmrWeight),
                FormatContribution("Vacancy", result.Breakdown.VacancyContribution, result.Breakdown.VacancyWeight),
                FormatContribution("Affordability", result.Breakdown.AffordabilityContribution, result.Breakdown.AffordabilityWeight),
                FormatContribution("Price/Sqft", result.Breakdown.PricePerSqftContribution, result.Breakdown.PricePerSqftWeight),
                FormatContribution("Property Type", result.Breakdown.PropertyTypeContribution, result.Breakdown.PropertyTypeWeight)
            };

            return $"Composite score {result.Version}: {string.Join(", ", parts)}";
        }

        private static string FormatContribution(string label, decimal? contribution, decimal weight)
        {
            if (!contribution.HasValue || weight == 0m)
            {
                return $"{label} —";
            }

            var rounded = decimal.Round(contribution.Value, 1).ToString("0.0", CultureInfo.InvariantCulture);
            var weightFormatted = weight.ToString("0", CultureInfo.InvariantCulture);
            return $"{label} {rounded}/{weightFormatted}";
        }

        private static decimal? NormalizeScore(decimal? value, decimal min, decimal max)
        {
            if (!value.HasValue)
            {
                return null;
            }

            if (max <= min)
            {
                return 0m;
            }

            var normalized = (value.Value - min) / (max - min) * 100m;
            return Math.Clamp(normalized, 0m, 100m);
        }

        private static decimal? NormalizeRentVsFmr(decimal? estimatedRent, decimal? fairMarketRent)
        {
            if (!estimatedRent.HasValue || !fairMarketRent.HasValue || fairMarketRent.Value <= 0m)
            {
                return null;
            }

            var delta = (estimatedRent.Value - fairMarketRent.Value) / fairMarketRent.Value;
            return NormalizeScore(delta, -0.2m, 0.2m);
        }

        private static decimal? NormalizeVacancy(decimal? vacancyRate)
        {
            if (!vacancyRate.HasValue)
            {
                return null;
            }

            if (vacancyRate.Value <= 5m)
            {
                return 100m;
            }

            if (vacancyRate.Value >= 10m)
            {
                return 0m;
            }

            var normalized = 100m - ((vacancyRate.Value - 5m) / 5m * 100m);
            return Math.Clamp(normalized, 0m, 100m);
        }

        private static decimal? NormalizePricePerSqft(decimal? pricePerSqft, decimal? medianPricePerSqft)
        {
            if (!pricePerSqft.HasValue || !medianPricePerSqft.HasValue || medianPricePerSqft.Value <= 0m)
            {
                return null;
            }

            var ratio = pricePerSqft.Value / medianPricePerSqft.Value;
            if (ratio <= 0.9m)
            {
                return 100m;
            }

            if (ratio >= 1.3m)
            {
                return 0m;
            }

            var normalized = 100m - ((ratio - 0.9m) / 0.4m * 100m);
            return Math.Clamp(normalized, 0m, 100m);
        }

        private static decimal? NormalizePropertyType(string? propertyType)
        {
            if (string.IsNullOrWhiteSpace(propertyType))
            {
                return null;
            }

            if (PropertyTypeScores.TryGetValue(propertyType.Trim(), out var score))
            {
                return score;
            }

            return 70m;
        }
    }

    public record CompositeScoreInputs(
        decimal? RentToPriceRatioMonthly,
        decimal? EstimatedRent,
        decimal? FairMarketRent,
        decimal? VacancyRate,
        decimal? AffordabilityIndex,
        decimal? PricePerSqft,
        decimal? MedianPricePerSqft,
        string? PropertyType);

    public record CompositeScoreBreakdown(
        decimal? RprContribution = null,
        decimal? RentVsFmrContribution = null,
        decimal? VacancyContribution = null,
        decimal? AffordabilityContribution = null,
        decimal? PricePerSqftContribution = null,
        decimal? PropertyTypeContribution = null,
        decimal RprWeight = 0m,
        decimal RentVsFmrWeight = 0m,
        decimal VacancyWeight = 0m,
        decimal AffordabilityWeight = 0m,
        decimal PricePerSqftWeight = 0m,
        decimal PropertyTypeWeight = 0m,
        decimal TotalWeight = 0m);

    public record CompositeScoreResult(
        decimal? Score,
        CompositeScoreBreakdown Breakdown,
        string Version);
}
