$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

# Check if the game is running
$gameProcess = Get-Process "Supermarket Together" -ErrorAction SilentlyContinue
if ($gameProcess) {
    Write-Error "Supermarket Together is currently running. Please close the game before deploying."
    exit 1
}

# Build first
Write-Host "Building project..."
& (Join-Path $scriptDir "build.ps1")
if ($LASTEXITCODE -ne 0) { exit 1 }

# Read GameDir
[xml]$props = Get-Content (Join-Path $repoRoot "Directory.Build.props")
$gameDir = $props.Project.PropertyGroup.GameDir

$pluginDir = Join-Path $gameDir "BepInEx\plugins\OrderAndOrganize"
$sourceDll = Join-Path $repoRoot "src\OrderAndOrganize\bin\Release\OrderAndOrganize.dll"
$sourcePdb = Join-Path $repoRoot "src\OrderAndOrganize\bin\Release\OrderAndOrganize.pdb"

# Create plugin directory
if (-not (Test-Path $pluginDir)) {
    New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null
    Write-Host "Created plugin directory: $pluginDir" -ForegroundColor Yellow
}

# Copy DLL
Copy-Item $sourceDll -Destination $pluginDir -Force
Write-Host "Copied OrderAndOrganize.dll" -ForegroundColor Green

# Copy PDB if it exists (for debugging)
if (Test-Path $sourcePdb) {
    Copy-Item $sourcePdb -Destination $pluginDir -Force
    Write-Host "Copied OrderAndOrganize.pdb" -ForegroundColor Green
}

Write-Host ""
Write-Host "Deployment complete!" -ForegroundColor Green
Write-Host "Location: $pluginDir"
Write-Host ""
Write-Host "Start the game and check BepInEx\LogOutput.log for Order & Organize startup messages."
