# Project Context — RentWise Pro

## Product goal
Make it easy for a user to answer two questions:
1) “How much will it **really** cost to buy this rental property?”
2) “If I buy it, what ROI should I expect under my assumptions?”

## Primary screens (Phase 1)
- Property Search (filter, sort, list)
- Saved Properties ("My Properties")
- Purchase Sheet / Data Generator (cash-to-close, closing costs)
- ROI Forecast (cashflow, cap rate, cash-on-cash, DSCR; horizons 6/12/60 months)

## Inputs & assumptions
- **Investment Profile** provides default assumptions (rates, fees, vacancy, maintenance, etc.)
- Per-property overrides are allowed in the UI (e.g., interest rate slider) and should feed the calculation services.

## Data sources (Phase 2 ingestion)
- Zillow-style listing/price/rent estimate data
- Rent estimate provider (Rentometer or alternatives)
- Property tax rates (state/county) or public datasets (HUD/ACS)
Phase 1 assumes ETL has already loaded the database.

## Success criteria
- A non-technical user can:
  - search, save, and evaluate a property
  - generate a purchase sheet
  - view ROI metrics and compare scenarios
