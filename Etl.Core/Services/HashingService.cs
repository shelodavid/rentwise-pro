using System.Security.Cryptography;
using System.Text;

namespace RentWisePro.Etl.Core.Services;

public class HashingService
{
    public string ComputeSha256(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
