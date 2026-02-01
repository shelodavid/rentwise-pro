using RentWisePro.Etl.Core.Interfaces;
using Microsoft.Extensions.Options;
using RentWisePro.Etl.Options;

namespace RentWisePro.Etl.Storage;

public class LocalPhotoStorage : IPhotoStorage
{
    private readonly StorageOptions _options;

    public LocalPhotoStorage(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SaveAsync(Guid propertyId, string source, int photoIndex, byte[] content, CancellationToken cancellationToken)
    {
        var safeSource = source.Replace(' ', '_');
        var directory = Path.Combine(Directory.GetCurrentDirectory(), _options.PhotoStoragePath, propertyId.ToString(), safeSource);
        Directory.CreateDirectory(directory);

        var fileName = $"{photoIndex}.jpg";
        var path = Path.Combine(directory, fileName);
        await File.WriteAllBytesAsync(path, content, cancellationToken);
        return path;
    }
}
