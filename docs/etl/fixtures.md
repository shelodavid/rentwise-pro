# ETL Fixture Mode

Fixture mode is intended for deterministic, repeatable ETL runs in development. It loads JSON
responses from disk instead of calling RapidAPI.

## Directory structure

Each fixture source has one or more scenario folders. A scenario contains a `search-results.json`
file and optional listing-detail JSON overrides:

```
Etl.Sources/Fixtures/{source-name}/{scenario}/search-results.json
Etl.Sources/Fixtures/{source-name}/{scenario}/listing-details/{listing-id}.json
```

The JSON format matches the `DevFixtureListingSource` schema. You can mix nested `address` objects
with flattened address fields (`address_line`, `city`, `state`, `postal_code`, `zip`).

## Adding or updating fixtures

1. Choose a source folder name (for example `fixture-listings`).
2. Create a new scenario folder (for example `baseline`).
3. Add `search-results.json` with an array of listings.
4. (Optional) Add `listing-details/{listing-id}.json` files for richer details.

Keep listing IDs stable between scenarios to exercise snapshot/change detection logic.

## Running baseline vs. changed scenarios

Set the ETL options in your local `appsettings.Development.json` (or user secrets):

```json
"Etl": {
  "UseFixtures": true,
  "FixtureScenario": "baseline",
  "FixtureSources": [ "Fixture Listings" ]
}
```

Switch `FixtureScenario` to `changed` to simulate price/status updates or removed listings:

```json
"Etl": {
  "UseFixtures": true,
  "FixtureScenario": "changed",
  "FixtureSources": [ "Fixture Listings" ]
}
```

`FixtureSources` maps to the fixture source folder names, using the same normalization rules as
`DevFixtureListingSource` (lowercase and dash-delimited).
