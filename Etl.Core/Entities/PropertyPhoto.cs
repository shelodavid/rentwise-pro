namespace RentWisePro.Etl.Core.Entities;

public class PropertyPhoto
{
    public Guid PhotoId { get; set; }
    public Guid PropertyId { get; set; }
    public string Source { get; set; } = string.Empty;
    public int PhotoIndex { get; set; }
    public string? UrlOriginal { get; set; }
    public string? StoragePath { get; set; }
    public string? Checksum { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Property? Property { get; set; }
}
