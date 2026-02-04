param(
    [string]$Environment = 'Development',
    [string]$ConnectionString,
    [string]$ProjectPath = 'Etl/RentWisePro.Etl.csproj',
    [string]$SourceFilter,
    [string]$Since,
    [int]$PageSize
)

$ErrorActionPreference = 'Stop'

if (-not [string]::IsNullOrWhiteSpace($ConnectionString)) {
    $env:ConnectionStrings__RentWiseProDb = $ConnectionString
}

if ([string]::IsNullOrWhiteSpace($env:ConnectionStrings__RentWiseProDb)) {
    Write-Error 'Missing connection string. Provide -ConnectionString or set ConnectionStrings__RentWiseProDb.'
    exit 1
}

$env:ASPNETCORE_ENVIRONMENT = $Environment

if ([System.IO.Path]::IsPathRooted($ProjectPath)) {
    $resolvedProjectPath = Resolve-Path $ProjectPath
} else {
    $resolvedProjectPath = Resolve-Path (Join-Path $PSScriptRoot ('..\..\' + $ProjectPath))
}

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

$dotnetArgs = @('run', '--project', $resolvedProjectPath, '--') + $etlArgs

& dotnet @dotnetArgs

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$queueArgs = @('--queue-only', '--queue-once')
$queueDotnetArgs = @('run', '--project', $resolvedProjectPath, '--') + $queueArgs

& dotnet @queueDotnetArgs

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
