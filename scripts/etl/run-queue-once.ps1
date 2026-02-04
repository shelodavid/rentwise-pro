param(
    [string]$Environment = 'Development',
    [string]$ConnectionString,
    [string]$ProjectPath = 'Etl/RentWisePro.Etl.csproj'
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

$etlArgs = @('--queue-only', '--queue-once')

$dotnetArgs = @('run', '--project', $resolvedProjectPath, '--') + $etlArgs

& dotnet @dotnetArgs

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
