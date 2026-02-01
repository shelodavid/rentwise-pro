# RentWise Pro ETL (v1)

## Overview
The ETL pipeline runs as a .NET Worker Service and pulls listing data from RapidAPI-based providers. It persists normalized property/listing data, raw payload references, listing snapshots, and work queue items for downstream processing.

## Prerequisites
- .NET SDK matching the repo TargetFramework (net8.0)
- SQL Server database configured for `RentWiseProDb`
- RapidAPI account + API key

## Configuration
### App settings template
Copy the template into your local appsettings for the ETL worker:

```bash
cp docs/etl/appsettings.Development.template.json Etl/appsettings.Development.json
```

> Do **not** commit secrets. Use user-secrets or environment variables for API keys.

### User-secrets (local dev)
From the ETL project directory:

```bash
dotnet user-secrets init --project Etl/RentWisePro.Etl.csproj
dotnet user-secrets set "RapidApi:ApiKey" "YOUR_RAPIDAPI_KEY" --project Etl/RentWisePro.Etl.csproj
```

### Environment variables (production)
Set these in your hosting environment:

- `ConnectionStrings__RentWiseProDb`
- `RapidApi__ApiKey`

## Running the ETL (run once)

```bash
dotnet run --project Etl/RentWisePro.Etl.csproj -- --runOnce
```

Optional filters:

```bash
dotnet run --project Etl/RentWisePro.Etl.csproj -- --runOnce --source="US Real Estate Listings" --since="2024-01-01T00:00:00Z"
```

## Running the queue worker only
The worker host includes both the orchestrator and work-queue processor. To run only the queue processor:

```bash
dotnet run --project Etl/RentWisePro.Etl.csproj -- --queueOnly
```

## Notes
- Raw payloads are stored on disk under `.local/raw` by default.
- Photos are stored on disk under `.local/photos/{propertyId}/{source}/{index}.jpg` by default.
- Work queue processing uses SQL Server locking (`UPDLOCK`, `READPAST`) to claim jobs safely.
- Default scheduling interval is 12 hours (configurable via `EtlExecution:Interval`).

## Implementation Notes
- **TargetFramework**: `net8.0`
- **DB Provider**: SQL Server (Entity Framework Core)
- **RapidAPI Adapter Implemented**: US Real Estate Listings (config-driven)
