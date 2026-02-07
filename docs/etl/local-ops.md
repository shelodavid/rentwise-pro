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

## Admin role bootstrap (Development)

> The app seeds the `Admin` role in Development. If you enable the bootstrap settings, it will also create or update an admin user.

1. Enable the bootstrap settings (user-secrets recommended):

```bash
dotnet user-secrets set "AdminBootstrap:Enabled" "true"
dotnet user-secrets set "AdminBootstrap:Email" "admin@example.com"
dotnet user-secrets set "AdminBootstrap:Password" "<use-a-strong-password>"
```

Environment variable equivalents:

```bash
export AdminBootstrap__Enabled=true
export AdminBootstrap__Email=admin@example.com
export AdminBootstrap__Password="<use-a-strong-password>"
```

2. Start the web app in Development so the bootstrapper can create the role and assign the user.
3. Sign out and sign back in to refresh the auth cookie.

> If you omit `AdminBootstrap:Password`, the bootstrapper will only attach the `Admin` role to an existing user with the configured email.

## Manual role assignment (SQL fallback)

1. Register or identify the user account that needs ETL Ops access.
2. Look up the user and role IDs in SQL Server:

```sql
SELECT Id, Email FROM AspNetUsers WHERE Email = 'user@example.com';
SELECT Id, Name FROM AspNetRoles WHERE Name = 'Admin';
```

3. Insert the mapping in `AspNetUserRoles` (replace the GUIDs from step 2):

```sql
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('<user-id-guid>', '<role-id-guid>');
```

4. Sign out and sign back in to refresh the auth cookie.

## Where logs are

- Console output is emitted by the ETL app when you run the scripts directly in PowerShell.
- Windows Task Scheduler keeps execution history; view it with `schtasks /Query /V` or via the Task Scheduler UI.
- If you need file-based logging, add a `Serilog`/`Microsoft.Extensions.Logging` sink in the ETL app (future enhancement).
