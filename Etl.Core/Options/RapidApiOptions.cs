namespace RentWisePro.Etl.Core.Options;

public class RapidApiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public List<RapidApiSourceOptions> Sources { get; set; } = new();
}

public class RapidApiSourceOptions
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string EndpointTemplate { get; set; } = string.Empty;
    public int MaxRequestsPerMinute { get; set; } = 60;
    public int MaxConcurrency { get; set; } = 2;
    public int PageSize { get; set; } = 50;
}
