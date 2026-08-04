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

$manifest = Get-Content (Join-Path $repoRoot "manifest.json") | ConvertFrom-Json
$version = $manifest.version_number
$packageName = "OrderAndOrganize-$version-NexusMods"

$stagingDir = Join-Path $repoRoot "dist\$packageName"
if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null

Copy-Item (Join-Path $repoRoot "README.md") -Destination $stagingDir

$pluginDir = Join-Path $stagingDir "plugins\OrderAndOrganize"
New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null
Copy-Item $dllPath -Destination $pluginDir

$zipPath = Join-Path $repoRoot "dist\$packageName.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($stagingDir, $zipPath)

Remove-Item $stagingDir -Recurse -Force

Write-Host ""
Write-Host "NexusMods package created!" -ForegroundColor Green
Write-Host "  Location: $zipPath"
Write-Host "  Version:  $version"
Write-Host ""
Write-Host "Contents:"
Write-Host "  plugins/OrderAndOrganize/OrderAndOrganize.dll"
Write-Host "  README.md"
Write-Host ""
Write-Host "Upload at: https://www.nexusmods.com/supermarkettogether/mods/" -ForegroundColor Cyan
