param(
    [string]$SourceFilter,
    [string]$Since,
    [int]$PageSize
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($env:ConnectionStrings__RentWiseProDb)) {
    $env:ConnectionStrings__RentWiseProDb = 'Server=(localdb)\\MSSQLLocalDB;Database=RentWisePro;Trusted_Connection=True;TrustServerCertificate=True;'
}

$projectPath = Resolve-Path (Join-Path $PSScriptRoot '..\..\Etl\RentWisePro.Etl.csproj')

$etlArgs = @('--runOnce')

if (-not [string]::IsNullOrWhiteSpace($SourceFilter)) {
    $etlArgs += @('--source', $SourceFilter)
}

if (-not [string]::IsNullOrWhiteSpace($Since)) {
    $etlArgs += @('--since', $Since)
}

if ($PageSize -gt 0) {
    $etlArgs += @('--page-size', $PageSize)
}

$dotnetArgs = @('run', '--project', $projectPath, '--') + $etlArgs

& dotnet @dotnetArgs

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
