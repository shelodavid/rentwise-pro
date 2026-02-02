using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RentWisePro.Etl.Persistence.Factories;

public class EtlDbContextFactory : IDesignTimeDbContextFactory<Contexts.EtlDbContext>
{
    public Contexts.EtlDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<Contexts.EtlDbContext>();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("Etl/appsettings.json", optional: true)
            .AddJsonFile("Etl/appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("RentWiseProDb") ??
                               configuration["ConnectionStrings:RentWiseProDb"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing connection string 'RentWiseProDb'.");
        }

        optionsBuilder.UseSqlServer(connectionString);
        return new Contexts.EtlDbContext(optionsBuilder.Options);
    }
}
