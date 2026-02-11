<#
.SYNOPSIS
  Automates the deployment of NinjaTrader 8 strategy files from the project repository to the NinjaTrader bin folder.

.DESCRIPTION
  This script performs the following actions:
  1. Verifies the source file exists in the project repository.
  2. Ensures the class name in the .cs file matches the filename.
  3. Copies the file to the NinjaTrader Custom Strategies folder.
  4. Optionally updates the version string displayed in the UI.

.PARAMETER SourceFileName
  The name of the .cs file in the project root to deploy.

.EXAMPLE
  .\ninja_deploy.ps1 -SourceFileName "UniversalORStrategyV8_2.cs"
#>

param (
  [Parameter(Mandatory = $true)]
  [string]$SourceFileName,
  [switch]$IsProduction # V12.12: Allow production deployment
)

$ErrorActionPreference = "Stop"

# Configuration
# V12.12: Infrastructure Hardening - Switched to local C:\WSGTA root
$ProjectRoot = "C:\WSGTA\universal-or-strategy"

# 1. Component Type Detection & Path Setup
$SourcePath = Join-Path $ProjectRoot $SourceFileName
if (-not (Test-Path $SourcePath)) {
  Write-Error "Source file not found at: $SourcePath"
}

# 1a. Security Guardrail: Enforce _Dev suffix (unless -IsProduction is used)
if (-not $IsProduction -and $SourceFileName -notmatch "_Dev\.cs$") {
  Write-Host "CRITICAL ERROR: Production files cannot be deployed directly without -IsProduction." -ForegroundColor Red
  Write-Host "Rule: Enforce '_Dev' suffix for all development work." -ForegroundColor Red
  Write-Error "DEPLOYMENT HALTED: File $SourceFileName lacks mandatory '_Dev' suffix."
}

$Content = Get-Content $SourcePath
$LineCount = $Content.Count
$ContentText = $Content -join [Environment]::NewLine

# 1b. Integrity Guardrail: Anti-Truncation Check
if ($LineCount -gt 1000) {
  Write-Host "Performing integrity audit for large file ($LineCount lines)..." -ForegroundColor Gray
  # Heuristic: If it has zero methods or regions, it might be truncated
  if ($ContentText -notmatch "OnBarUpdate" -and $ContentText -notmatch "OnStateChange" -and $ContentText -notmatch "#region") {
    Write-Host "WARNING: File appears suspiciously empty or truncated despite high line count." -ForegroundColor Yellow
    Write-Error "INTEGRITY CHECK FAILED: Potential truncation detected (missing State/Region anchors). Aborting deployment."
  }
}
elseif ($LineCount -lt 100) {
  Write-Host "WARNING: File is unexpectedly small ($LineCount lines)." -ForegroundColor Yellow
  Write-Error "INTEGRITY CHECK FAILED: File too small. Aborting deployment."
}

$TargetFolder = "Strategies"

if ($ContentText -match "public class .* : Indicator") {
  $TargetFolder = "Indicators"
  Write-Host "Detected Indicator component type." -ForegroundColor Cyan
}
elseif ($ContentText -match "public( partial)? class .* : Strategy") {
  $TargetFolder = "Strategies"
  Write-Host "Detected Strategy component type." -ForegroundColor Cyan
}
else {
  Write-Host "Unknown component type. Defaulting to Strategies." -ForegroundColor Yellow
}

$NinjaTraderPath = "C:\Users\Mohammed Khalid\Documents\NinjaTrader 8\bin\Custom\$TargetFolder"

$ClassName = [System.IO.Path]::GetFileNameWithoutExtension($SourceFileName)
$DestinationPath = Join-Path $NinjaTraderPath $SourceFileName

Write-Host "--- Starting Deployment of $SourceFileName ---" -ForegroundColor Cyan

# 2. Class Name Synchronization
Write-Host "Verifying class name synchronization..." -ForegroundColor Gray
# Update: public [partial] class [Something] : [BaseType]
# IMPORTANT: Preserve 'partial' keyword if present (V12.12 fix)
$Pattern = "public(\s+partial)?\s+class\s+\w+\s*:\s*(Strategy|Indicator)"
$NewContent = $ContentText -replace $Pattern, "public`$1 class $ClassName : `$2"

if ($ContentText -ne $NewContent) {
  Write-Host "Updating class name to match filename: $ClassName" -ForegroundColor Yellow
  $NewContent | Set-Content $SourcePath -Encoding UTF8 -NoNewline
}
else {
  Write-Host "Class name already matches filename." -ForegroundColor Green
}

# 3. Deployment
Write-Host "Wiping target deployment folder to prevent 'Ghost Files'..." -ForegroundColor Gray
# V12.12 Wipe-on-Deploy Protocol: Remove existing _Dev files to prevent duplicate definitions
$FilesToWipe = Get-ChildItem -Path $NinjaTraderPath -Filter "$ClassName*"
foreach ($file in $FilesToWipe) {
  Remove-Item $file.FullName -Force
  Write-Host "Purged: $($file.Name)" -ForegroundColor DarkGray
}

Write-Host "Deploying to NinjaTrader folder..." -ForegroundColor Gray
Copy-Item $SourcePath $DestinationPath -Force

# 3a. Partial Class Support (e.g. .Sima.cs, .Reaper.cs)
if ($ContentText -match "partial class") {
  Write-Host "Modular classes detected. Searching for partial files..." -ForegroundColor Gray
  $BaseFileName = $SourceFileName -replace "\.cs$", ""
  $Partials = Get-ChildItem -Path $ProjectRoot -Filter "$BaseFileName.*.cs" | Where-Object { $_.Name -ne $SourceFileName }
    
  foreach ($Partial in $Partials) {
    $PartialDest = Join-Path $NinjaTraderPath $Partial.Name
    Write-Host "Deploying partial: $($Partial.Name)" -ForegroundColor Gray
    Copy-Item $Partial.FullName $PartialDest -Force
  }
}

if (Test-Path $DestinationPath) {
  Write-Host "SUCCESS: File(s) deployed to $DestinationPath" -ForegroundColor Green
}
else {
  Write-Error "FAILED: Deployment failed to $DestinationPath"
}

Write-Host "--- Deployment Complete ---" -ForegroundColor Cyan
Write-Host "Please switch to NinjaTrader and press F5 to compile." -ForegroundColor Magenta
