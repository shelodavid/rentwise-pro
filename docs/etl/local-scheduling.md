# Local Windows Scheduling (ETL + Work Queue)

This guide documents how to run the RentWise Pro ETL and work queue locally on Windows using Task Scheduler and the PowerShell helpers in `scripts/etl`.

## Prerequisites

- **.NET SDK** (for `dotnet run`)
- **SQL Server** (LocalDB or SQL Server Express works)
- **PowerShell** and **Task Scheduler** (built into Windows)

## Configure the connection string

The ETL requires `ConnectionStrings__RentWiseProDb`. Set it once per session or persist it in your user environment.

### Option 1: Environment variable (PowerShell)

```powershell
$env:ConnectionStrings__RentWiseProDb = 'Server=(localdb)\MSSQLLocalDB;Database=RentWisePro;Trusted_Connection=True;TrustServerCertificate=True;'
```

### Option 2: User secrets (development only)

```powershell
dotnet user-secrets set "ConnectionStrings:RentWiseProDb" "Server=(localdb)\MSSQLLocalDB;Database=RentWisePro;Trusted_Connection=True;TrustServerCertificate=True;" --project Etl\RentWisePro.Etl.csproj
```

> The scripts default to a LocalDB connection string if the environment variable is missing. Override it for your environment.

## Run manually (one-off)

```powershell
# ETL ingestion (single run)
.\scripts\etl\run-etl-once.ps1 -SourceFilter "Zillow" -Since "2024-01-01T00:00:00Z" -PageSize 50

# Work queue processing (single run)
.\scripts\etl\run-queue-once.ps1

# ETL + queue (single run)
.\scripts\etl\run-etl-and-queue.ps1
```

## Register scheduled tasks

```powershell
.\scripts\etl\register-tasks.ps1
```

This registers:
- **RentWisePro-ETL-Ingestion** — runs at 06:00, 12:00, 18:00, 23:00 daily.
- **RentWisePro-ETL-Queue** — runs every 15 minutes.

### Optional: run whether user is logged on or not

Provide explicit credentials to store a password with the task:

```powershell
.\scripts\etl\register-tasks.ps1 -RunAsUser "DOMAIN\username" -RunAsPassword "<password>"
```

> This is optional. The default behavior runs the tasks only when the current user is logged on.

## Unregister scheduled tasks

```powershell
.\scripts\etl\unregister-tasks.ps1
```

## Logs and validation

- Check **Task Scheduler > Task History** for exit codes and execution history.
- Validate ETL runs and queue status using `sqlcmd` and the existing smoke check script:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d RentWisePro -E -i docs\etl\SmokeCheck.sql
```

Replace the server and auth flags as needed for your SQL Server instance.
