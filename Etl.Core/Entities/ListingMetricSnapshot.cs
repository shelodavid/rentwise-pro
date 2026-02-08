namespace RentWisePro.Etl.Core.Entities;

public class ListingMetricSnapshot
{
    public Guid MetricSnapshotId { get; set; }
    public Guid ListingId { get; set; }
    public DateTimeOffset AsOf { get; set; }
    public decimal? EstimatedRent { get; set; }
    public decimal? RprMonthly { get; set; }
    public decimal? Grm { get; set; }
    public decimal? EstimatedCashFlow { get; set; }
    public decimal? AffordabilityIndex { get; set; }
    public decimal? FmrUsed { get; set; }
    public decimal? VacancyRateUsed { get; set; }
    public decimal? Score { get; set; }
    public int? ScoreVersion { get; set; }

    public Listing? Listing { get; set; }
}
