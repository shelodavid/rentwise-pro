$ErrorActionPreference = 'Stop'

$tasks = @(
    'RentWisePro-ETL-Ingestion',
    'RentWisePro-ETL-Queue'
)

foreach ($task in $tasks) {
    & schtasks /Query /TN $task > $null 2>&1
    if ($LASTEXITCODE -eq 0) {
        & schtasks /Delete /TN $task /F
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }
    }
}
