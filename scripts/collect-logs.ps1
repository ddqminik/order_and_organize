$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

[xml]$props = Get-Content (Join-Path $repoRoot "Directory.Build.props")
$gameDir = $props.Project.PropertyGroup.GameDir

$logFile = Join-Path $gameDir "BepInEx\LogOutput.log"

if (-not (Test-Path $logFile)) {
    Write-Error "BepInEx log not found at $logFile"
    exit 1
}

$logsDir = Join-Path $repoRoot "logs"
if (-not (Test-Path $logsDir)) {
    New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
}

$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$destFile = Join-Path $logsDir "LogOutput_$timestamp.log"

Copy-Item $logFile -Destination $destFile -Force

Write-Host "Log collected!" -ForegroundColor Green
Write-Host "Location: $destFile"
Write-Host ""
Write-Host "Order & Organize entries:"
Select-String -Path $destFile -Pattern "Order & Organize|OrderAndOrganize" | ForEach-Object { Write-Host $_.Line }
