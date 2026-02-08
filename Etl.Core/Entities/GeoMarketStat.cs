namespace RentWisePro.Etl.Core.Entities;

public class GeoMarketStat
{
    public string GeoKey { get; set; } = string.Empty;
    public string GeoType { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal VacancyRate { get; set; }
    public decimal MedianHouseholdIncome { get; set; }
    public string Source { get; set; } = "ACS";
    public DateTimeOffset RetrievedAt { get; set; }
}
