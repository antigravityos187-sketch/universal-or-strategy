# Analyze Epic to Wave/Phase Mapping
# Shows which epic belongs to which wave and what phase it's in

$epicDirs = Get-ChildItem -Path "docs/brain" -Directory | Where-Object { $_.Name -match "^EPIC-CCN-" }

$results = @()

foreach ($dir in $epicDirs) {
    $epicId = $dir.Name
    $epicPath = $dir.FullName
    
    # Check for Lamport events (Wave 6 indicator)
    $lamportPath = Join-Path $epicPath ".lamport"
    $hasLamport = Test-Path $lamportPath
    
    # Check for manifest
    $manifestPath = Join-Path $epicPath "manifest.json"
    $hasManifest = Test-Path $manifestPath
    
    # Check which phase files exist
    $phase0 = Test-Path (Join-Path $epicPath "00-hotspots.md")
    $phase1 = Test-Path (Join-Path $epicPath "00-scope.md")
    $phase1_5 = Test-Path (Join-Path $epicPath "01-scope-boundary.md")
    $phase2 = Test-Path (Join-Path $epicPath "02-architecture-plan.md")
    $phase3 = Test-Path (Join-Path $epicPath "03-audit-report.md")
    $phase4 = Test-Path (Join-Path $epicPath "04-tickets.md")
    
    # Determine wave
    $wave = if ($hasLamport) { "Wave 6" } 
            elseif ($phase0 -or $phase1) { "Wave 4/5" }
            else { "Unknown" }
    
    # Determine highest completed phase
    $highestPhase = if ($phase4) { "Phase 4" }
                    elseif ($phase3) { "Phase 3" }
                    elseif ($phase2) { "Phase 2" }
                    elseif ($phase1_5) { "Phase 1.5" }
                    elseif ($phase1) { "Phase 1" }
                    elseif ($phase0) { "Phase 0" }
                    else { "None" }
    
    $results += [PSCustomObject]@{
        Epic = $epicId
        Wave = $wave
        HighestPhase = $highestPhase
        HasLamport = $hasLamport
        HasManifest = $hasManifest
        Phase0 = $phase0
        Phase1 = $phase1
        Phase1_5 = $phase1_5
        Phase2 = $phase2
        Phase3 = $phase3
        Phase4 = $phase4
    }
}

# Group by wave
Write-Host "`n=== EPIC WAVE MAPPING ===" -ForegroundColor Cyan
Write-Host ""

$wave6 = $results | Where-Object { $_.Wave -eq "Wave 6" }
$wave45 = $results | Where-Object { $_.Wave -eq "Wave 4/5" }
$unknown = $results | Where-Object { $_.Wave -eq "Unknown" }

Write-Host "Wave 6 (Lamport-based): $($wave6.Count) epics" -ForegroundColor Green
Write-Host "Wave 4/5 (Pre-Lamport): $($wave45.Count) epics" -ForegroundColor Yellow
Write-Host "Unknown: $($unknown.Count) epics" -ForegroundColor Red
Write-Host ""

# Show Wave 6 phase distribution
Write-Host "`n=== WAVE 6 PHASE DISTRIBUTION ===" -ForegroundColor Cyan
$wave6 | Group-Object HighestPhase | Sort-Object Name | ForEach-Object {
    Write-Host "$($_.Name): $($_.Count) epics"
}

# Show Wave 4/5 epics
if ($wave45.Count -gt 0) {
    Write-Host "`n=== WAVE 4/5 EPICS (Pre-Lamport) ===" -ForegroundColor Yellow
    $wave45 | Sort-Object Epic | ForEach-Object {
        Write-Host "$($_.Epic) - $($_.HighestPhase)"
    }
}

# Show unknown epics
if ($unknown.Count -gt 0) {
    Write-Host "`n=== UNKNOWN EPICS ===" -ForegroundColor Red
    $unknown | Sort-Object Epic | ForEach-Object {
        Write-Host "$($_.Epic) - $($_.HighestPhase)"
    }
}

# Export detailed CSV
$csvPath = "epic_wave_mapping.csv"
$results | Export-Csv -Path $csvPath -NoTypeInformation
Write-Host "`nDetailed mapping exported to: $csvPath" -ForegroundColor Green

# Summary
Write-Host "`n=== SUMMARY ===" -ForegroundColor Cyan
Write-Host "Total Epics: $($results.Count)"
Write-Host "Wave 6: $($wave6.Count)"
Write-Host "Wave 4/5: $($wave45.Count)"
Write-Host "Unknown: $($unknown.Count)"

# Made with Bob
