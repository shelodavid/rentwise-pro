# Reference Data Import (HUD FMR + Census/ACS)

This guide covers the **one-time/sample import** flow for HUD Fair Market Rent (FMR) and Census/ACS market stats,
along with a scalable pattern for future scheduled ingestion.

## Tables

The ETL persistence layer stores reference data in:

| Table | Purpose | Key columns |
| --- | --- | --- |
| `hud_fmr` | HUD Fair Market Rent by geo + bedroom | `GeoType`, `GeoKey`, `Year`, `Bedrooms` |
| `geo_market_stats` | Vacancy + median income by geo | `GeoType`, `GeoKey`, `Year` |

Both tables include `Source` and `RetrievedAt` metadata for provenance and caching.

## Sample import (recommended for dev)

Use fixture CSVs checked into the repo:

```
dotnet run --project Etl.ReferenceData -- --import hud-fmr --year 2024 --geo zip --sample
dotnet run --project Etl.ReferenceData -- --import acs --year 2023 --geo zip --sample
```

Fixtures live in `Etl.Sources/Fixtures/ReferenceData`.

## Manual or cached downloads

If you provide a `--source-path`, the importer reads from that file:

```
dotnet run --project Etl.ReferenceData -- --import hud-fmr --year 2024 --geo zip --source-path C:\data\hud_fmr.csv
```

If no source path is provided, the importer caches downloads to:

```
{Storage:RawPayloadPath}/reference/hud/hud_fmr_{year}_{geo}.csv
{Storage:RawPayloadPath}/reference/acs/geo_market_stats_{year}_{geo}.csv
```

You can override the download URL with `--download-url`. If you need a manual drop,
use `--manual` to skip downloads and log the expected path.

## Geo strategy (v1)

- ZIP is the primary `GeoType` for lookups.
- ZIP missing → caller should fall back to `CITY_STATE` keys (example: `CITY_STATE:TX:Austin`) or handle nulls.

## Lookup service

Use `IGeoMarketDataLookup` from `Etl.Core` with an implementation in `Etl.Persistence`:

- `GetHudFmrAsync(zip, bedrooms, year)`
- `GetVacancyRateAsync(zip, year)`
- `GetMedianIncomeAsync(zip, year)`

These methods are null-safe and return `null` when data is missing.
