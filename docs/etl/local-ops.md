# ETL Local Operations

## How to run locally

> These scripts are PowerShell-first and set `ASPNETCORE_ENVIRONMENT` + `ConnectionStrings__RentWiseProDb` before invoking the ETL project.

### Ingestion (run once)
```powershell
./scripts/etl/run-etl-once.ps1 -Environment Development -ConnectionString "Server=(localdb)\MSSQLLocalDB;Database=RentWisePro;Trusted_Connection=True;TrustServerCertificate=True;"
```

### Queue processing (run once)
```powershell
./scripts/etl/run-queue-once.ps1 -Environment Development -ConnectionString "Server=(localdb)\MSSQLLocalDB;Database=RentWisePro;Trusted_Connection=True;TrustServerCertificate=True;"
```

### Ingestion then queue (two commands)
```powershell
./scripts/etl/run-etl-and-queue.ps1 -Environment Development -ConnectionString "Server=(localdb)\MSSQLLocalDB;Database=RentWisePro;Trusted_Connection=True;TrustServerCertificate=True;"
```

### Optional filters for ingestion
```powershell
./scripts/etl/run-etl-once.ps1 -Environment Development -ConnectionString "..." -SourceFilter Zillow -PageSize 250 -Since "2024-01-01"
```

## How to schedule

### Register scheduled tasks
```powershell
./scripts/etl/register-tasks.ps1 -Environment Development -ConnectionString "Server=(localdb)\MSSQLLocalDB;Database=RentWisePro;Trusted_Connection=True;TrustServerCertificate=True;"
```

### Unregister scheduled tasks
```powershell
./scripts/etl/unregister-tasks.ps1
```

## How to check last run result

### Scheduled task last run status
```powershell
schtasks /Query /TN "RentWisePro-ETL-Ingestion" /V /FO LIST
schtasks /Query /TN "RentWisePro-ETL-Queue" /V /FO LIST
```

### Check SQL data recency
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d RentWisePro -Q "SELECT TOP 20 SourceName, MAX(LastUpdatedUtc) AS LastUpdatedUtc FROM Etl.Listings GROUP BY SourceName ORDER BY LastUpdatedUtc DESC;"
```

## Where logs are

- Console output is emitted by the ETL app when you run the scripts directly in PowerShell.
- Windows Task Scheduler keeps execution history; view it with `schtasks /Query /V` or via the Task Scheduler UI.
- If you need file-based logging, add a `Serilog`/`Microsoft.Extensions.Logging` sink in the ETL app (future enhancement).
