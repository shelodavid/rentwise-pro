# Architecture — RentWise Pro

## High-level
- ASP.NET MVC app reads from SQL Server.
- **SQL is read-oriented**: stored procedures/views may be used for retrieval, but **no** business computations.
- Financial math lives in C# services and is unit-testable.

## Modules
- Data Access Layer (EF Core / ADO.NET)
- Domain Models
- Calculation Services:
  - MortgageCalculator
  - ClosingCostCalculator
  - RoiForecastService
- MVC Controllers + Razor Views
- UI Partials for reusable components (PageHeader, StatCard, etc.)

## Data model (Phase 1: minimal baseline)
- InvestmentProfiles: assumption templates
- RentalListings: ETL-ingested property data
- SavedPropertyListings: user-saved properties snapshots

## Data model (Phase 2: optimized expansion, proposed)
Split raw ingest from normalized analytics:
- ListingsRaw (append-only, source + ingestion timestamp)
- Properties (stable identity: address normalization, geo)
- ListingSnapshots (time-series changes)
- RentEstimates (source, confidence)
- Taxes (jurisdiction, rate history)
- UserProfiles, SavedProperties (per user)
- ScenarioOverrides (per property + profile scenario)

Phase 1 will map existing data into a simpler schema, then migrate to Phase 2 once value is proven.
