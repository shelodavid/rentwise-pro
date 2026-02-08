using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RentWisePro.Etl.Persistence.Contexts;
using RentWisePro.Etl.ReferenceData;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("RentWiseProDb") ??
                       builder.Configuration["ConnectionStrings:RentWiseProDb"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Missing connection string 'RentWiseProDb'.");
}

builder.Services.AddDbContext<EtlDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ReferenceDataPaths>();
builder.Services.AddScoped<ReferenceDataDownloader>();
builder.Services.AddScoped<HudFmrReferenceImporter>();
builder.Services.AddScoped<GeoMarketStatsImporter>();
builder.Services.AddScoped<ReferenceDataCommandHandler>();

var host = builder.Build();
var handler = host.Services.GetRequiredService<ReferenceDataCommandHandler>();
var command = ReferenceDataCommand.Parse(args);

if (!command.IsValid)
{
    Console.WriteLine(ReferenceDataCommand.Usage);
    return;
}

await handler.HandleAsync(command, CancellationToken.None);
