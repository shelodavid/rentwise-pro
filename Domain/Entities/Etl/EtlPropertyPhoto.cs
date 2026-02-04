namespace RentWisePro.Web.Domain.Entities.Etl;

public class EtlPropertyPhoto
{
    public Guid PhotoId { get; set; }
    public Guid PropertyId { get; set; }
    public string Source { get; set; } = string.Empty;
    public int PhotoIndex { get; set; }
    public string? UrlOriginal { get; set; }
    public string? StoragePath { get; set; }

    public EtlProperty? Property { get; set; }
}
