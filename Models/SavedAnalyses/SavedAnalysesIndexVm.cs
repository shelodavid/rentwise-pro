namespace RentWisePro.Web.Models.SavedAnalyses
{
    public class SavedAnalysesIndexVm
    {
        public IReadOnlyList<SavedAnalysisSummaryVm> Analyses { get; init; } = Array.Empty<SavedAnalysisSummaryVm>();
    }

    public class SavedAnalysisSummaryVm
    {
        public int SavedPropertyProfileId { get; init; }
        public string? StreetAddress { get; init; }
        public string? City { get; init; }
        public string? State { get; init; }
        public string? ZipCode { get; init; }
        public decimal? Price { get; init; }
        public DateTime SavedAtUtc { get; init; }
        public string InvestmentProfileName { get; init; } = string.Empty;
    }
}
