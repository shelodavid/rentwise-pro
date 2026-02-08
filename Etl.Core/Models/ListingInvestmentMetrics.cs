namespace RentWisePro.Etl.Core.Models;

public sealed record ListingInvestmentMetrics(
    decimal? EstimatedRent,
    decimal? RprMonthly,
    decimal? Grm,
    decimal? EstimatedCashFlow,
    decimal? AffordabilityIndex,
    decimal? PricePerSqft);
