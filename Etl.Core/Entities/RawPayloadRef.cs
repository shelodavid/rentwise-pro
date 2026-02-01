namespace RentWisePro.Etl.Core.Entities;

public class RawPayloadRef
{
    public string RawRef { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string SourceListingId { get; set; } = string.Empty;
    public DateTimeOffset FetchedAt { get; set; }
}
