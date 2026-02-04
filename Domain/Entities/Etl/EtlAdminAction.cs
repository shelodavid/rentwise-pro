namespace RentWisePro.Web.Domain.Entities.Etl;

public class EtlAdminAction
{
    public Guid ActionId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? RequestedByUserId { get; set; }
    public string? Command { get; set; }
}
