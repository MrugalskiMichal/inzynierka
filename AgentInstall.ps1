# test

param(
    [Parameter(Mandatory= $true)]
    [string]$agentId,

    [Parameter(Mandatory= $true)]
    [string]$authToken,

    [Parameter(Mandatory= $true)]
    [string]$serverAddress,

    [int]$collectionIntervalSeconds = 5,

    [bool]$collectCpuData  = $true,
    [bool]$collectRamData  = $true,
    [bool]$collectGpuData  = $true,
    [bool]$collectDiskData = $true
)

# Directories
$InstallDir = Join-Path $env:ProgramFiles "Agent"
$TempZip = "$env:TEMP\agent-release.zip"

# zip url
$ReleaseUrl = "https://raw.githubusercontent.com/MrugalskiMichal/inzynierka/final-release/agent-release.zip"

if (-not ([Security.Principal.WindowsPrincipal] `
    [Security.Principal.WindowsIdentity]::GetCurrent()
    ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {

    Write-Error "Installer must be run as Administrator."
    exit 1
}
#Write-Host "Install dir: $InstallDir"
#Write-Host "Temp: $TempZip"

#Write-Host "url: $ReleaseUrl"

#Write-Host "Service name: $ServiceName"
#Write-Host "Service dipslay: $ServiceDisplayName"
#Write-Host "Service desc: $ServiceDescription"
#Write-Host "Service Exe Path: $ServiceExePath"

Write-Host "=== Installing Agent ===" -ForegroundColor Blue
# Downloading zip

Write-Host "Downloading agent-release.zip from $ReleaseUrl..."

# downloading agent-release.zip
Invoke-WebRequest -Uri $ReleaseUrl -OutFile $TempZip
Write-Host "agent-release.zip downloaded to: $TempZip"

# Prepare directory in ProgramFiles
Write-Host "== Creating Dircetory ==" -ForegroundColor Cyan

if (Test-Path $InstallDir) {
    Write-Host "Removing existing installation..."
    Remove-Item $InstallDir -Recurse -Force
}

New-Item -ItemType Directory -Path $InstallDir | Out-Null

# Unzip to new Folder
Write-Host "== Preparing Agent ==" -ForegroundColor Cyan

Write-Host "Extracting files from zip..." -ForegroundColor Yellow
Expand-Archive -Path $TempZip -DestinationPath $InstallDir -Force

# Move from agent-release\ to ProgramFiles\Agent
if (Test-Path "$InstallDir\agent-release") {
    Move-Item "$InstallDir\agent-release\*" $InstallDir
    Remove-Item "$InstallDir\agent-release" -Recurse -Force
}

Write-Host "Files extracted." -ForegroundColor Green

# Upload parameters into config.json
$configPath = Join-Path $InstallDir "config.json"

$config = [ordered]@{
    agentId = $agentId
    authToken = $authToken
    serverAddress = $serverAddress
    collectionIntervalSeconds = $collectionIntervalSeconds
    metrics = [ordered]@{
        collectCpuData  = $collectCpuData
        collectRamData  = $collectRamData
        collectGpuData  = $collectGpuData
        collectDiskData = $collectDiskData
    }
}

Write-Host "Writing to $configPath..." -ForegroundColor Yellow

$config | ConvertTo-Json -Depth 4 | Set-Content -Path $configPath -Encoding UTF8

Write-Host "Configuration updated." -ForegroundColor Green

# Install agent as Windows Service
Write-Host "== Installing Agent as Windows Service ==" -ForegroundColor Cyan

# Service info
$ServiceName        = "Agent"
$ServiceDisplayName = "System Metrics Agent"
$ServiceDescription = "Collects system metrics and sends them to a central server."
$ServiceExePath     = "`"$InstallDir\Agent.exe`""

$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "Existing Agent found. Removing..." -ForegroundColor Yellow
    sc.exe stop $ServiceName | Out-Null
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 3
    Write-Host "Old Agent Service Removed..." -ForegroundColor Green
}

Write-Host "Enlisting new Agent..." -ForegroundColor Yellow
sc.exe create $ServiceName `
    binPath= $ServiceExePath `
    start= demand `
    DisplayName= "`"$ServiceDisplayName`""

sc.exe description $ServiceName "`"$ServiceDescription`""

Write-Host "Agent Service installed, start manually..." -ForegroundColor Green

# Remove agent-release.zip
Remove-Item $TempZip -Force
Write-Host "agent-release.zip removed from $TempZip"

Write-Host "Agent installation complete!" -ForegroundColor Cyan
Read-Host "Press ENTER to continue..."

