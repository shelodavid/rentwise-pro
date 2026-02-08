namespace RentWisePro.Etl.Core.Entities;

public class HudFairMarketRent
{
    public int Year { get; set; }
    public string GeoCode { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public decimal FmrMonthlyRent { get; set; }
    public string Source { get; set; } = "HUD";
    public DateTimeOffset ImportedAt { get; set; }
}
