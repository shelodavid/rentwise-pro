namespace RentWisePro.Etl.Core.Models;

public sealed record EtlRunRequest(
    string? SourceFilter,
    DateTimeOffset? Since,
    int? PageSize
);
