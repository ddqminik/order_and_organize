$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
$propsFile = Join-Path $repoRoot "Directory.Build.props"

if (-not (Test-Path $propsFile)) {
    Write-Error "Directory.Build.props not found at $propsFile.`nCopy Directory.Build.props.example to Directory.Build.props and set GameDir to your Supermarket Together install path."
    exit 1
}

[xml]$props = Get-Content $propsFile
$gameDir = $props.Project.PropertyGroup.GameDir

if (-not $gameDir -or -not (Test-Path $gameDir)) {
    Write-Error "GameDir '$gameDir' does not exist. Update Directory.Build.props with the correct path."
    exit 1
}

$managedDir = Join-Path $gameDir "Supermarket Together_Data\Managed"
$assemblyCSharp = Join-Path $managedDir "Assembly-CSharp.dll"

if (-not (Test-Path $assemblyCSharp)) {
    Write-Error "Assembly-CSharp.dll not found at $managedDir. Verify your GameDir."
    exit 1
}

Write-Host "GameDir validated: $gameDir" -ForegroundColor Green
Write-Host ""

$csproj = Join-Path $repoRoot "src\OrderAndOrganize\OrderAndOrganize.csproj"

Write-Host "Restoring packages..."
dotnet restore $csproj
if ($LASTEXITCODE -ne 0) { Write-Error "Restore failed."; exit 1 }

Write-Host "Building..."
dotnet build $csproj --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed."; exit 1 }

$dllPath = Join-Path $repoRoot "src\OrderAndOrganize\bin\Release\OrderAndOrganize.dll"
if (Test-Path $dllPath) {
    Write-Host ""
    Write-Host "Build succeeded!" -ForegroundColor Green
    Write-Host "Output: $dllPath"
} else {
    Write-Error "Build appeared to succeed but DLL not found at expected path."
    exit 1
}
