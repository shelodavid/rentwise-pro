namespace RentWisePro.Etl.Core.Entities;

public class WorkQueueItem
{
    public Guid WorkId { get; set; }
    public string WorkType { get; set; } = string.Empty;
    public Guid PropertyId { get; set; }
    public Guid? ListingId { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public DateTimeOffset AvailableAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
