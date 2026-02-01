namespace RentWisePro.Etl.Core.Interfaces;

public interface IPhotoStorage
{
    Task<string> SaveAsync(Guid propertyId, string source, int photoIndex, byte[] content, CancellationToken cancellationToken);
}
