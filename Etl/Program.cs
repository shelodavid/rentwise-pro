using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RentWisePro.Etl.Core.Interfaces;
using RentWisePro.Etl.Core.Options;
using RentWisePro.Etl.Core.Services;
using RentWisePro.Etl.Options;
using RentWisePro.Etl.Persistence.Contexts;
using RentWisePro.Etl.Persistence.Repositories;
using RentWisePro.Etl.Services;
using RentWisePro.Etl.Sources.Clients;
using RentWisePro.Etl.Sources.Sources;
using RentWisePro.Etl.Storage;
using RentWisePro.Etl.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly(), optional: true);
}

builder.Services.Configure<EtlOptions>(builder.Configuration.GetSection("Etl"));
builder.Services.Configure<RapidApiOptions>(builder.Configuration.GetSection("RapidApi"));
builder.Services.Configure<RentometerOptions>(builder.Configuration.GetSection("Rentometer"));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<EtlExecutionOptions>(options =>
{
    builder.Configuration.GetSection("EtlExecution").Bind(options);
    ApplyExecutionOptions(options, args);
});

var connectionString = builder.Configuration.GetConnectionString("RentWiseProDb") ??
                       builder.Configuration["ConnectionStrings:RentWiseProDb"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Missing connection string 'RentWiseProDb'.");
}

builder.Services.AddDbContext<EtlDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddSingleton<AddressNormalizer>();
builder.Services.AddSingleton<HashingService>();
builder.Services.AddSingleton<MaterialHashBuilder>();
builder.Services.AddSingleton<SnapshotDecider>();

builder.Services.AddScoped<IEtlRepository, EtlRepository>();

builder.Services.AddHttpClient<RapidApiClient>();
var rapidApiOptions = builder.Configuration.GetSection("RapidApi").Get<RapidApiOptions>() ?? new RapidApiOptions();
foreach (var source in rapidApiOptions.Sources)
{
    builder.Services.AddSingleton<IListingSource>(sp =>
        new RapidApiListingSource(
            sp.GetRequiredService<RapidApiClient>(),
            source,
            rapidApiOptions.ApiKey,
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RapidApiListingSource>>()));
}

builder.Services.AddSingleton<IRawPayloadStore, LocalRawPayloadStore>();
builder.Services.AddSingleton<IPhotoStorage, LocalPhotoStorage>();

builder.Services.AddHttpClient<PhotoDownloadService>();
builder.Services.AddScoped<RentForecastService>();

builder.Services.AddScoped<IEtlOrchestrator, EtlOrchestrator>();
builder.Services.AddHostedService<EtlWorker>();
builder.Services.AddHostedService<WorkQueueWorker>();

var host = builder.Build();
await host.RunAsync();

static void ApplyExecutionOptions(EtlExecutionOptions options, string[] args)
{
    for (var index = 0; index < args.Length; index++)
    {
        var arg = args[index];
        if (string.Equals(arg, "--runOnce", StringComparison.OrdinalIgnoreCase))
        {
            options.RunOnce = true;
        }
        else if (string.Equals(arg, "--run-once", StringComparison.OrdinalIgnoreCase))
        {
            options.RunOnce = true;
        }
        else if (string.Equals(arg, "--queueOnly", StringComparison.OrdinalIgnoreCase))
        {
            options.QueueOnly = true;
        }
        else if (string.Equals(arg, "--queue-only", StringComparison.OrdinalIgnoreCase))
        {
            options.QueueOnly = true;
        }
        else if (string.Equals(arg, "--workQueue", StringComparison.OrdinalIgnoreCase))
        {
            options.QueueOnly = true;
        }
        else if (string.Equals(arg, "--work-queue", StringComparison.OrdinalIgnoreCase))
        {
            options.QueueOnly = true;
        }
        else if (string.Equals(arg, "--queue-once", StringComparison.OrdinalIgnoreCase))
        {
            options.QueueOnly = true;
            options.QueueRunOnce = true;
        }
        else if (arg.StartsWith("--source=", StringComparison.OrdinalIgnoreCase))
        {
            options.SourceFilter = arg.Split('=', 2)[1];
        }
        else if (string.Equals(arg, "--source", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
        {
            options.SourceFilter = args[index + 1];
        }
        else if (arg.StartsWith("--since=", StringComparison.OrdinalIgnoreCase))
        {
            if (DateTimeOffset.TryParse(arg.Split('=', 2)[1], out var since))
            {
                options.Since = since;
            }
        }
        else if (string.Equals(arg, "--since", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
        {
            if (DateTimeOffset.TryParse(args[index + 1], out var since))
            {
                options.Since = since;
            }
        }
        else if (arg.StartsWith("--pageSize=", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(arg.Split('=', 2)[1], out var pageSize))
            {
                options.PageSize = pageSize;
            }
        }
        else if (string.Equals(arg, "--pageSize", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
        {
            if (int.TryParse(args[index + 1], out var pageSize))
            {
                options.PageSize = pageSize;
            }
        }
        else if (arg.StartsWith("--page-size=", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(arg.Split('=', 2)[1], out var pageSize))
            {
                options.PageSize = pageSize;
            }
        }
        else if (string.Equals(arg, "--page-size", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
        {
            if (int.TryParse(args[index + 1], out var pageSize))
            {
                options.PageSize = pageSize;
            }
        }
    }

    if (options.QueueOnly && options.RunOnce && !options.QueueRunOnce)
    {
        options.QueueRunOnce = true;
    }
}
