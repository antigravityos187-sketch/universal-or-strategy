<#
.SYNOPSIS
  Automates the creation of a milestone by archiving source code and updating the project brain.

.DESCRIPTION
  1. Creates a timestamped archive folder in ARCHIVE_SNAPSHOTS.
  2. Copies the primary strategy and panel files to the archive.
  3. Updates .claudeignore to ensure the archive doesn't cause context bloat.
  
.PARAMETER MilestoneName
  The name of the milestone (e.g., "RMA_Recovery").
#>

param (
    [Parameter(Mandatory = $true)]
    [string]$MilestoneName
)

$Timestamp = Get-Date -Format "yyyyMMdd_HHmm"
$SafeName = $MilestoneName -replace "[^\w]", "_"
$ArchiveFolder = "ARCHIVE_SNAPSHOTS\$Timestamp`_$SafeName"
$ProjectRoot = "C:\WSGTA\universal-or-strategy"

Write-Host "--- Creating Milestone Archive: $MilestoneName ---" -ForegroundColor Cyan

# 1. Create Archive Directory
if (-not (Test-Path "$ProjectRoot\$ArchiveFolder")) {
    New-Item -ItemType Directory -Path "$ProjectRoot\$ArchiveFolder" -Force | Out-Null
}

# 2. Identify and Copy Files
$FilesToArchive = @(
    "UniversalORStrategyV12_Dev.cs",
    "V12StandardPanel_V12_001_Dev.cs",
    "UniversalORStrategyV12.cs",
    "V12StandardPanel_V12_001.cs"
)

foreach ($file in $FilesToArchive) {
    if (Test-Path "$ProjectRoot\$file") {
        Copy-Item "$ProjectRoot\$file" "$ProjectRoot\$ArchiveFolder\$file" -Force
        Write-Host "Archived: $file" -ForegroundColor Gray
    }
}

# 3. Harden .claudeignore (Anti-Bloat Protection)
$IgnorePath = "$ProjectRoot\.claudeignore"
$IgnoreEntry = "ARCHIVE_SNAPSHOTS/"

if (Test-Path $IgnorePath) {
    $currentIgnore = Get-Content $IgnorePath
    if ($currentIgnore -notcontains $IgnoreEntry) {
        Add-Content $IgnorePath "`n# Auto-archive protection`n$IgnoreEntry"
        Write-Host "Hardened .claudeignore: Archived snapshots now hidden from AI context." -ForegroundColor Green
    }
}
else {
    Set-Content $IgnorePath $IgnoreEntry
    Write-Host "Created .claudeignore: Hiding archives." -ForegroundColor Yellow
}

Write-Host "--- Milestone Successfully Archived in $ArchiveFolder ---" -ForegroundColor Cyan
