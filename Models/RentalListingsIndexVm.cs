using Microsoft.AspNetCore.Mvc.Rendering;

namespace RentWisePro.Web.Models
{
    public class RentalListingsIndexVm
    {
        public List<RentalListingCardVm> Listings { get; set; } = new();
        public List<SelectListItem> PropertyTypes { get; set; } = new();
        public string? Search { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinBedrooms { get; set; }
        public decimal? MinBathrooms { get; set; }
        public decimal? MinRpr { get; set; }
        public string? PropertyType { get; set; }
        public string? VacancyBucket { get; set; }
        public string? AffordabilityBucket { get; set; }
        public string? SortBy { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    public class RentalListingCardVm
    {
        public int RentalListingId { get; set; }
        public long Zpid { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? PropertyType { get; set; }
        public decimal? Price { get; set; }
        public decimal? EstimatedMonthlyRent { get; set; }
        public decimal? Rpr { get; set; }
        public decimal? RentToPriceRatioMonthly { get; set; }
        public string? RentToPriceRatioMonthlyPct { get; set; }
        public decimal? Grm { get; set; }
        public decimal? CashFlow { get; set; }
        public decimal? PricePerSqft { get; set; }
        public decimal? VacancyRate { get; set; }
        public string? VacancyBucket { get; set; }
        public string? VacancyBucketLabel { get; set; }
        public string? VacancyBadgeClass { get; set; }
        public decimal? AffordabilityIndex { get; set; }
        public string? AffordabilityBucket { get; set; }
        public string? AffordabilityBucketLabel { get; set; }
        public decimal? FairMarketRent { get; set; }
        public decimal? RentVsFmrDeltaPct { get; set; }
        public decimal? CompositeScore { get; set; }
        public string? CompositeScoreVersion { get; set; }
        public Services.CompositeScoreBreakdown? CompositeScoreBreakdown { get; set; }
        public string? CompositeScoreTooltip { get; set; }
        public int? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }
        public string? ImgSrc { get; set; }
        public DateTime IngestedAtUtc { get; set; }
    }
}
