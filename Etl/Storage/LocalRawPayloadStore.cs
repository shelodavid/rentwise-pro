using RentWisePro.Etl.Core.Interfaces;
using Microsoft.Extensions.Options;
using RentWisePro.Etl.Options;

namespace RentWisePro.Etl.Storage;

public class LocalRawPayloadStore : IRawPayloadStore
{
    private readonly StorageOptions _options;

    public LocalRawPayloadStore(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SaveAsync(string source, string sourceListingId, string rawJson, CancellationToken cancellationToken)
    {
        var safeSource = source.Replace(' ', '_');
        var directory = Path.Combine(Directory.GetCurrentDirectory(), _options.RawPayloadPath, safeSource);
        Directory.CreateDirectory(directory);

        var fileName = $"{sourceListingId}-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.json";
        var path = Path.Combine(directory, fileName);

        await File.WriteAllTextAsync(path, rawJson, cancellationToken);
        return path;
    }
}
