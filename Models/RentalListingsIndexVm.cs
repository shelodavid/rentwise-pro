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
        public string? PropertyType { get; set; }
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
        public int? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }
        public string? ImgSrc { get; set; }
        public DateTime IngestedAtUtc { get; set; }
    }
}
