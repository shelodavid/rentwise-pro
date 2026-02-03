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

## Notes

- `--source` filters to a specific listing source (case-insensitive).
- `--since` expects an ISO-8601 timestamp (e.g., `2024-01-01T00:00:00Z`).
- `--pageSize` controls the listing page size for each source.
