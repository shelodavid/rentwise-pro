using Microsoft.Extensions.Configuration;

namespace RentWisePro.Etl.ReferenceData;

public class ReferenceDataPaths
{
    public string GetFixtureRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "Etl.Sources", "Fixtures", "ReferenceData");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), "Etl.Sources", "Fixtures", "ReferenceData");
    }

    public string GetStorageRoot(IConfiguration configuration)
    {
        var value = configuration["Storage:RawPayloadPath"];
        return string.IsNullOrWhiteSpace(value) ? ".local/raw" : value;
    }
}
