namespace RentWisePro.Etl.Core.Models;

public sealed record RentEstimate(decimal MonthlyRent, string Source, DateTimeOffset AsOf);
