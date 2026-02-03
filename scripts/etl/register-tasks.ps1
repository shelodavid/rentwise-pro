param(
    [string]$RunAsUser,
    [string]$RunAsPassword
)

$ErrorActionPreference = 'Stop'

function Get-UserIdentity {
    return [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
}

$resolvedUser = if ([string]::IsNullOrWhiteSpace($RunAsUser)) { Get-UserIdentity } else { $RunAsUser }
$logonType = if ([string]::IsNullOrWhiteSpace($RunAsPassword)) { 'InteractiveToken' } else { 'Password' }

$runEtlScript = Resolve-Path (Join-Path $PSScriptRoot 'run-etl-once.ps1')
$runQueueScript = Resolve-Path (Join-Path $PSScriptRoot 'run-queue-once.ps1')

$etlArguments = [System.Security.SecurityElement]::Escape("-NoProfile -ExecutionPolicy Bypass -File `"$runEtlScript`"")
$queueCommand = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$runQueueScript`""

$today = Get-Date -Format 'yyyy-MM-dd'
$registeredAt = (Get-Date).ToString('yyyy-MM-ddTHH:mm:ss')

$etlXml = @"
<?xml version="1.0" encoding="UTF-16"?>
<Task version="1.4" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <RegistrationInfo>
    <Date>$registeredAt</Date>
    <Author>$resolvedUser</Author>
  </RegistrationInfo>
  <Triggers>
    <CalendarTrigger>
      <StartBoundary>${today}T06:00:00</StartBoundary>
      <ScheduleByDay>
        <DaysInterval>1</DaysInterval>
      </ScheduleByDay>
    </CalendarTrigger>
    <CalendarTrigger>
      <StartBoundary>${today}T12:00:00</StartBoundary>
      <ScheduleByDay>
        <DaysInterval>1</DaysInterval>
      </ScheduleByDay>
    </CalendarTrigger>
    <CalendarTrigger>
      <StartBoundary>${today}T18:00:00</StartBoundary>
      <ScheduleByDay>
        <DaysInterval>1</DaysInterval>
      </ScheduleByDay>
    </CalendarTrigger>
    <CalendarTrigger>
      <StartBoundary>${today}T23:00:00</StartBoundary>
      <ScheduleByDay>
        <DaysInterval>1</DaysInterval>
      </ScheduleByDay>
    </CalendarTrigger>
  </Triggers>
  <Principals>
    <Principal id="Author">
      <UserId>$resolvedUser</UserId>
      <LogonType>$logonType</LogonType>
      <RunLevel>LeastPrivilege</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <AllowHardTerminate>true</AllowHardTerminate>
    <StartWhenAvailable>true</StartWhenAvailable>
    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
    <IdleSettings>
      <StopOnIdleEnd>false</StopOnIdleEnd>
      <RestartOnIdle>false</RestartOnIdle>
    </IdleSettings>
    <AllowStartOnDemand>true</AllowStartOnDemand>
    <Enabled>true</Enabled>
    <Hidden>false</Hidden>
    <RunOnlyIfIdle>false</RunOnlyIfIdle>
    <WakeToRun>false</WakeToRun>
    <ExecutionTimeLimit>PT2H</ExecutionTimeLimit>
    <Priority>7</Priority>
  </Settings>
  <Actions Context="Author">
    <Exec>
      <Command>powershell.exe</Command>
      <Arguments>$etlArguments</Arguments>
    </Exec>
  </Actions>
</Task>
"@

$tempFile = [System.IO.Path]::GetTempFileName()
Set-Content -Path $tempFile -Value $etlXml -Encoding Unicode

$authArgs = @('/RU', $resolvedUser)
if ([string]::IsNullOrWhiteSpace($RunAsPassword)) {
    $authArgs += '/IT'
} else {
    $authArgs += @('/RP', $RunAsPassword)
}

& schtasks /Create /TN 'RentWisePro-ETL-Ingestion' /XML $tempFile /F @authArgs
if ($LASTEXITCODE -ne 0) {
    Remove-Item $tempFile -ErrorAction SilentlyContinue
    exit $LASTEXITCODE
}

& schtasks /Create /TN 'RentWisePro-ETL-Queue' /TR $queueCommand /SC MINUTE /MO 15 /F @authArgs
if ($LASTEXITCODE -ne 0) {
    Remove-Item $tempFile -ErrorAction SilentlyContinue
    exit $LASTEXITCODE
}

Remove-Item $tempFile -ErrorAction SilentlyContinue
