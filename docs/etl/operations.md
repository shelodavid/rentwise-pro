# ETL Operations Metrics

This document describes the metrics shown on the Admin **ETL Ops** page and how each one is computed from the ETL schema.

## ETL Metrics

### Last 24 hours / Last 7 days
- **Total runs**: Count of rows in `etl_runs` where `StartedAt` is within the window.
- **Completed**: Count of rows in `etl_runs` where `StartedAt` is within the window and `Status = "Completed"`.
- **Failed**: Count of rows in `etl_runs` where `StartedAt` is within the window and `Status = "Failed"`.
- **Average run duration (ms)**: Average `DateDiffMillisecond(StartedAt, FinishedAt)` for runs in the window with a non-null `FinishedAt`.

### Per-source stats
Derived from `etl_run_source_stats` joined to `etl_runs` by `RunId`.

**Last run (per source)** uses the most recent `etl_runs.StartedAt` for each source:
- **Fetched count**: `ListingsFetched`
- **Upserted count**: `ListingsUpserted`
- **Snapshot count**: `SnapshotsCreated`
- **Missing count**: Count of `listings` rows where `Source` matches and `LastSeenAt < StartedAt` for that last run.
- **Errors**: `Errors`
- **Duration (ms)**: `DurationMs`

**Last 24h (per source)** aggregates over runs in the last 24 hours:
- **Fetched count**: Sum of `ListingsFetched`
- **Upserted count**: Sum of `ListingsUpserted`
- **Snapshot count**: Sum of `SnapshotsCreated`
- **Missing count**: Count of `listings` rows where `Source` matches and `LastSeenAt` is older than 24 hours.
- **Errors**: Sum of `Errors`
- **Average duration (ms)**: Average `DurationMs` for runs in the last 24 hours

### Work queue health
Derived from `work_queue`:
- **Queued count**: Count of rows where `Status = "queued"`.
- **Processing count**: Count of rows where `Status = "processing"`.
- **Failed count**: Count of rows where `Status = "failed"`.
- **Oldest queued item age**: `UtcNow - MIN(AvailableAt)` for queued items.

## Recent runs
The "Recent runs" table shows the 20 most recent runs from `etl_runs` ordered by `StartedAt` descending, including `StartedAt`, `FinishedAt`, `Status`, and `Notes`. Duration is computed with `DateDiffMillisecond(StartedAt, FinishedAt)` when `FinishedAt` is available.

## Run details
The "View details" page shows per-source stats for a single run based on `etl_run_source_stats`, including error counts. If no source stats are recorded, the UI displays a "No source stats recorded" message.
