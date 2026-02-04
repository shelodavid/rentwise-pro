namespace RentWisePro.Web.Domain.Entities.Etl;

public class EtlRun
{
    public Guid RunId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
