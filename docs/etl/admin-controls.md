# ETL Admin Controls (Local Dev)

## Overview
The ETL Ops admin page lets administrators trigger local ETL jobs and toggle local scheduling without running long tasks in the web process. The page delegates execution to external processes and records audit entries in the web database (`etl_admin_actions`).

## Supported Actions
- **Run ETL (once)**: Starts the ETL ingestion pipeline with `--runOnce`.
- **Run Queue (once)**: Runs the work queue with `--queue-only --queue-once`.
- **Disable Schedule**: Unregisters Windows Task Scheduler entries using `scripts/etl/unregister-tasks.ps1`.
- **Enable Schedule**: Registers Windows Task Scheduler entries using `scripts/etl/register-tasks.ps1`.

## Local Execution Details
- The web app spawns an external process (`RentWisePro.Etl.exe` if present under `Etl/bin/Release/net8.0`, otherwise `dotnet run --project Etl/RentWisePro.Etl.csproj`).
- Output and errors are captured and stored in `etl_admin_actions` for visibility in the UI.
- A 30-minute lock window prevents concurrent runs to avoid overlapping ingestion.
- Schedule state is best-effort: on Windows the app queries `schtasks` for `RentWisePro-ETL-Ingestion` and `RentWisePro-ETL-Queue`.

## AWS Mapping (Future)
| Local Dev Action | Proposed AWS Replacement |
| --- | --- |
| Run ETL (once) | EventBridge rule triggers ECS task/Lambda for ingestion |
| Run Queue (once) | EventBridge rule triggers ECS task/Lambda for queue processing |
| Disable Schedule | Disable EventBridge rules |
| Enable Schedule | Enable EventBridge rules |
| Status Checks | CloudWatch metrics or ECS task status |

## Notes & Limitations
- Schedule toggles require Windows Task Scheduler; non-Windows hosts will show an informational status.
- Ensure the ETL project path exists (`Etl/RentWisePro.Etl.csproj`) or actions will fail with a clear error.
