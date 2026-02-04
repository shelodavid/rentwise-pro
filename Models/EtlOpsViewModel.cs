using RentWisePro.Web.Services.Etl;

namespace RentWisePro.Web.Models;

public class EtlOpsViewModel
{
    public EtlRunnerStatus RunnerStatus { get; set; } = new(false, "Status unavailable", null, null);
    public IReadOnlyList<EtlAdminActionRow> RecentActions { get; set; } = Array.Empty<EtlAdminActionRow>();
}

public class EtlAdminActionRow
{
    public string ActionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public string? Message { get; set; }
    public string? RequestedByUserId { get; set; }
}
