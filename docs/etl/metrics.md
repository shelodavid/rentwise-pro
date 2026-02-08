# Rental Listing Metrics (ETL + Web v1)

The Rental Listings page surfaces ETL-derived metrics alongside a **Composite Rental Suitability Score (v1)**. These metrics are intended to provide quick, directional insights only—**they are rough estimates and should not be used as final underwriting numbers**.

## Metrics shown on listings

Each listing card includes a compact **Metrics** row:

- **RPR (Rent-to-Price Ratio)** — Monthly rent divided by listing price.
- **GRM (Gross Rent Multiplier)** — Listing price divided by annualized rent.
- **Estimated Rent** — ETL-estimated monthly rent.
- **Cash Flow (rough)** — ETL-estimated monthly cash flow (preliminary).
- **Composite Score (v1)** — 0–100 weighted score (hover for breakdown).

Additional details can appear when available:

- **Vacancy badge** (Low/Medium/High)
- **Affordability index** (as provided by geo market data)
- **Rent vs FMR delta** — Percent difference between estimated rent and HUD Fair Market Rent (if present)

## Composite Rental Suitability Score v1

The v1 score is calculated at query/render time (not persisted). Weights are transparent and shown in the tooltip:

- **RPR** — 30 points (40 if FMR data is missing)
- **Rent vs FMR** — 20 points (omitted if FMR is missing)
- **Vacancy pressure** — 15 points
- **Affordability index** — 15 points
- **Price per sqft vs local median** — 10 points (currently **N/A** until ETL provides medians)
- **Property type factor** — 10 points

If required market data is missing, components are omitted and weights are rebalanced where specified. Listings will display “—” instead of zero when data is absent.

## Disclaimers

- These metrics are **directional** and can change as ETL improves.
- Scores are **not** persisted; they are computed per request in the web app.
- Vacancy/affordability data is optional and will appear only when geo market data tables are available.
