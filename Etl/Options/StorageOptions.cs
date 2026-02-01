namespace RentWisePro.Etl.Options;

public class StorageOptions
{
    public string RawPayloadPath { get; set; } = ".local/raw";
    public string PhotoStoragePath { get; set; } = ".local/photos";
}
