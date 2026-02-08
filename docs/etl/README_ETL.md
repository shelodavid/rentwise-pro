# RentWise Pro ETL Runner

## Environment setup

Set the SQL connection string (PowerShell):

```powershell
$env:ConnectionStrings__RentWiseProDb = 'Server=DESKTOP-FRU391A\\SQLEXPRESS;Database=RentWisePro;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True'
```

Set ETL API credentials (placeholder):

```powershell
dotnet user-secrets set "RapidApi:ApiKey" "..." --project Etl\\RentWisePro.Etl.csproj
```

## Use fixtures (no paid API keys)

Set one of the following in `appsettings.Development.json` (or environment variables):

```json
{
  "Etl": {
    "UseFixtures": true,
    "FixtureRootPath": "Etl.Sources/Fixtures"
  }
}
```

The fixture dataset lives under `Etl.Sources/Fixtures/fixture-listings`. To simulate price or status changes,
replace `search-results.json` with `search-results.updated.json` and run the ETL again to trigger snapshots.
If you keep `RapidApi:Sources` configured, ensure the `Name` matches the fixture folder (for example, set it
to `Fixture Listings` so it maps to `fixture-listings`).

## Run ingestion once

```powershell
dotnet run --project Etl\\RentWisePro.Etl.csproj -- --runOnce --source=<optional> --since=<optional> --pageSize=<optional>
```

## Run queue once

```powershell
dotnet run --project Etl\\RentWisePro.Etl.csproj -- --workQueue --runOnce
```

## Run queue loop

```powershell
dotnet run --project Etl\\RentWisePro.Etl.csproj -- --workQueue
```

## Apply ETL migrations locally

```powershell
dotnet ef migrations add FixDecimalPrecision --project Etl.Persistence\\RentWisePro.Etl.Persistence.csproj --startup-project Etl\\RentWisePro.Etl.csproj --context RentWisePro.Etl.Persistence.Contexts.EtlDbContext
dotnet ef database update --project Etl.Persistence\\RentWisePro.Etl.Persistence.csproj --startup-project Etl\\RentWisePro.Etl.csproj --context RentWisePro.Etl.Persistence.Contexts.EtlDbContext
```

## Notes

- `--source` filters to a specific listing source (case-insensitive).
- `--since` expects an ISO-8601 timestamp (e.g., `2024-01-01T00:00:00Z`).
- `--pageSize` controls the listing page size for each source.

## Listing investment metrics

ETL persists a baseline set of investment metrics directly on `listings` for fast sorting/filtering:

- **EstimatedRent** — monthly rent from the source payload (or `rent` in fixtures).
- **RprMonthly** — rent-to-price ratio (`EstimatedRent / Price`).
- **Grm** — gross rent multiplier (`Price / (EstimatedRent * 12)`).
- **EstimatedCashFlow** — rough monthly cash flow: `EstimatedRent - (1% price / 12) - (1% price / 12) - (10% rent)`.
- **AffordabilityIndex** — rent compared to 30% of median monthly income (when income data is available).
- **PricePerSqft** — `Price / SquareFeet` when sqft is known.

`listing_metric_snapshots` exists for future composite-score snapshots (includes optional FMR/vacancy inputs and score
fields), but the current UI relies on the listing-level metrics above.
