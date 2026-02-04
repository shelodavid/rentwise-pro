using Microsoft.AspNetCore.Mvc.Rendering;

namespace RentWisePro.Web.Models;

public class EtlListingsIndexVm
{
    public List<EtlListingRowVm> Listings { get; set; } = new();
    public List<SelectListItem> Sources { get; set; } = new();
    public List<SelectListItem> Statuses { get; set; } = new();
    public List<string> StatusOptions { get; set; } = new();
    public string? Status { get; set; }
    public string? Source { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? SortBy { get; set; }
    public EtlHealthVm Health { get; set; } = new();
}

public class EtlListingRowVm
{
    public Guid ListingId { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public decimal? Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? Beds { get; set; }
    public decimal? Baths { get; set; }
    public int? SquareFeet { get; set; }
    public string Source { get; set; } = string.Empty;
    public string SourceListingId { get; set; } = string.Empty;
    public DateTimeOffset LastSeenAt { get; set; }
    public List<string> PhotoUrls { get; set; } = new();
}

public class EtlHealthVm
{
    public EtlRunVm? LatestRun { get; set; }
    public Dictionary<string, int> ListingCountsByStatus { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class EtlRunVm
{
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
