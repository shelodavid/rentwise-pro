# ETL Fixture Listings

This folder contains deterministic JSON fixtures for the ETL listing sources. The fixture source expects the
following structure:

```
Etl.Sources/Fixtures/{source-name}/search-results.json
Etl.Sources/Fixtures/{source-name}/listing-details/{listing-id}.json (optional)
```

## Schema

Each `search-results.json` file contains an array of listings with the following fields:

- `listing_id` (string, required): stable identifier for the listing.
- `source_listing_id` (string, optional): explicit source listing ID when it differs from `listing_id`.
- `address` (object, optional):
  - `line`, `city`, `state`, `postal_code`
- `address_line`, `city`, `state`, `postal_code`, `zip` (string, optional): flattened address fields.
- `latitude`, `longitude` (number, optional)
- `price` (number, optional)
- `beds`, `baths` (number, optional)
- `sqft` (number, optional)
- `status` (string, optional)
- `property_type` (string, optional)
- `year_built` (number, optional)
- `lot_size` (number, optional)
- `photos` (array of string URLs, optional)
- `updated_at` (ISO-8601 timestamp, optional)

The fixture source normalizes addresses and derives a stable ID when no `listing_id` is provided.

## Updating fixtures

To simulate change detection (snapshots), replace `search-results.json` with
`search-results.updated.json` in the same fixture folder. The updated file contains price changes
and a status change to trigger snapshots.
