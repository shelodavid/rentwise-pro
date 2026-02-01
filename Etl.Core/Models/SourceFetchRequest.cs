namespace RentWisePro.Etl.Core.Models;

public record SourceFetchRequest(int Page, int PageSize, DateTimeOffset? Since);
