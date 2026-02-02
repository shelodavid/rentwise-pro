-- Latest ETL runs
SELECT TOP (10)
    RunId,
    Status,
    StartedAt,
    CompletedAt,
    Notes
FROM etl_runs
ORDER BY StartedAt DESC;

-- Entity counts
SELECT 'properties' AS entity, COUNT(*) AS total FROM properties
UNION ALL
SELECT 'listings', COUNT(*) FROM listings
UNION ALL
SELECT 'listing_snapshots', COUNT(*) FROM listing_snapshots
UNION ALL
SELECT 'raw_payload_refs', COUNT(*) FROM raw_payload_refs
UNION ALL
SELECT 'work_queue', COUNT(*) FROM work_queue;

-- Work queue status distribution
SELECT Status, COUNT(*) AS total
FROM work_queue
GROUP BY Status
ORDER BY Status;
