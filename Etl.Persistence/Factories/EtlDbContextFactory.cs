using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RentWisePro.Etl.Persistence.Factories;

public class EtlDbContextFactory : IDesignTimeDbContextFactory<Contexts.EtlDbContext>
{
    public Contexts.EtlDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<Contexts.EtlDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("RENTWISEPRO_CONNECTIONSTRING") ??
                               "Server=.;Database=RentWisePro;Trusted_Connection=True;TrustServerCertificate=True;";
        optionsBuilder.UseSqlServer(connectionString);
        return new Contexts.EtlDbContext(optionsBuilder.Options);
    }
}
