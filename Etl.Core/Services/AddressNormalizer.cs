using System.Text.RegularExpressions;

namespace RentWisePro.Etl.Core.Services;

public class AddressNormalizer
{
    private static readonly Regex MultiSpaceRegex = new("\\s+", RegexOptions.Compiled);

    public string Normalize(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return string.Empty;
        }

        var trimmed = address.Trim().ToUpperInvariant();
        var normalized = MultiSpaceRegex.Replace(trimmed, " ");
        return normalized;
    }
}
