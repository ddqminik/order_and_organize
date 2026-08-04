$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

# Build first
& "$scriptDir\build.ps1"
if ($LASTEXITCODE -ne 0) { exit 1 }

$dllPath = Join-Path $repoRoot "src\OrderAndOrganize\bin\Release\OrderAndOrganize.dll"
if (-not (Test-Path $dllPath)) {
    Write-Error "Build output not found at $dllPath"
    exit 1
}

# Read version from manifest
$manifest = Get-Content (Join-Path $repoRoot "manifest.json") | ConvertFrom-Json
$version = $manifest.version_number
$packageName = "OrderAndOrganize-$version"

# Create staging directory
$stagingDir = Join-Path $repoRoot "dist\$packageName"
if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null

# Copy required files
Copy-Item (Join-Path $repoRoot "manifest.json") -Destination $stagingDir
Copy-Item (Join-Path $repoRoot "README.md") -Destination $stagingDir
Copy-Item (Join-Path $repoRoot "CHANGELOG.md") -Destination $stagingDir

$iconPath = Join-Path $repoRoot "icon.png"
if (Test-Path $iconPath) {
    Copy-Item $iconPath -Destination $stagingDir
} else {
    Write-Warning "icon.png not found at $iconPath -- Thunderstore requires a 256x256 PNG icon."
}

# Create plugins subfolder with DLL
$pluginDir = Join-Path $stagingDir "plugins\OrderAndOrganize"
New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null
Copy-Item $dllPath -Destination $pluginDir

# Create zip
$zipPath = Join-Path $repoRoot "dist\$packageName.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($stagingDir, $zipPath)

# Clean up staging
Remove-Item $stagingDir -Recurse -Force

Write-Host ""
Write-Host "Thunderstore package created!" -ForegroundColor Green
Write-Host "  Location: $zipPath"
Write-Host "  Version:  $version"
Write-Host ""
Write-Host "Upload at: https://thunderstore.io/package/create/" -ForegroundColor Cyan
