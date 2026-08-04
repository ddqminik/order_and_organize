$ErrorActionPreference = "Stop"

# Check if the game is running
$gameProcess = Get-Process "Supermarket Together" -ErrorAction SilentlyContinue
if ($gameProcess) {
    Write-Error "Supermarket Together is currently running. Please close the game before backing up saves."
    exit 1
}

$saveDir = Join-Path $env:USERPROFILE "AppData\LocalLow\DDTNL\Supermarket Together"
if (-not (Test-Path $saveDir)) {
    Write-Error "Save directory not found at $saveDir"
    exit 1
}

$backupRoot = Join-Path $env:USERPROFILE "Documents\Supermarket Together Backups"
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$backupDir = Join-Path $backupRoot $timestamp

# Create backup directory
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null

# Copy save data recursively
Write-Host "Backing up save data..."
Copy-Item -Path "$saveDir\*" -Destination $backupDir -Recurse -Force

# Verify backup contains .es3 files
$es3Files = Get-ChildItem -Path $backupDir -Filter "*.es3" -Recurse
if ($es3Files.Count -eq 0) {
    Write-Warning "No .es3 save files found in backup. The backup may be incomplete."
} else {
    Write-Host "Verified: $($es3Files.Count) .es3 file(s) in backup." -ForegroundColor Green
}

Write-Host ""
Write-Host "Backup complete!" -ForegroundColor Green
Write-Host "Location: $backupDir"
