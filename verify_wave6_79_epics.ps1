# Verify Wave 6 79 Epics (Confirmed Scope)
# Based on docs/brain/WAVE6_WAVE7_MASTER_PLAN.md

Write-Host "`n=== WAVE 6 EPIC VERIFICATION ===" -ForegroundColor Cyan
Write-Host "Expected: 79 epics (78 VM + 1 Local)" -ForegroundColor Yellow
Write-Host "Range: EPIC-CCN-001 through 080"
Write-Host "Exclusions: EPIC-024 (missing Phase 0), EPIC-027 (user excluded)"
Write-Host "Special: EPIC-003 (local .dll execution)"
Write-Host ""

$wave6Epics = @()
$found = @()
$missing = @()

# Generate expected epic list (001-080, excluding 024 and 027)
for ($i = 1; $i -le 80; $i++) {
    $epicNum = "{0:D3}" -f $i
    $epicId = "EPIC-CCN-$epicNum"
    
    # Skip exclusions
    if ($i -eq 24 -or $i -eq 27) {
        continue
    }
    
    $wave6Epics += $epicId
    
    # Check if epic directory exists
    $epicPath = "docs/brain/$epicId"
    if (Test-Path $epicPath) {
        # Check for Phase 0 (hotspots)
        $phase0 = Test-Path (Join-Path $epicPath "00-hotspots.md")
        
        if ($phase0) {
            $found += [PSCustomObject]@{
                Epic = $epicId
                Path = $epicPath
                Phase0 = $phase0
                Special = if ($i -eq 3) { "Local (.dll)" } else { "" }
            }
        } else {
            $missing += [PSCustomObject]@{
                Epic = $epicId
                Reason = "Missing Phase 0 (00-hotspots.md)"
            }
        }
    } else {
        $missing += [PSCustomObject]@{
            Epic = $epicId
            Reason = "Epic directory not found"
        }
    }
}

# Results
Write-Host "=== RESULTS ===" -ForegroundColor Cyan
Write-Host "Expected: $($wave6Epics.Count) epics" -ForegroundColor Yellow
Write-Host "Found: $($found.Count) epics" -ForegroundColor $(if ($found.Count -eq 78) { "Green" } else { "Red" })
Write-Host "Missing: $($missing.Count) epics" -ForegroundColor $(if ($missing.Count -eq 0) { "Green" } else { "Red" })
Write-Host ""

if ($found.Count -eq 78) {
    Write-Host "✅ SUCCESS: All 78 Wave 6 epics found!" -ForegroundColor Green
} else {
    Write-Host "❌ FAILURE: Expected 78, found $($found.Count)" -ForegroundColor Red
}

# Show special cases
Write-Host "`n=== SPECIAL CASES ===" -ForegroundColor Cyan
Write-Host "EPIC-003: Local execution (due to .dll) - $(if ($found | Where-Object { $_.Epic -eq 'EPIC-CCN-003' }) { '✅ Found' } else { '❌ Missing' })"
Write-Host "EPIC-024: Excluded (missing Phase 0) - $(if (-not (Test-Path 'docs/brain/EPIC-CCN-024')) { '✅ Correctly excluded' } else { '⚠️  Directory exists' })"
Write-Host "EPIC-027: Excluded (user confirmed) - $(if (-not (Test-Path 'docs/brain/EPIC-CCN-027')) { '✅ Correctly excluded' } else { '⚠️  Directory exists' })"

# Show missing epics if any
if ($missing.Count -gt 0) {
    Write-Host "`n=== MISSING EPICS ===" -ForegroundColor Red
    $missing | Format-Table -AutoSize
}

# Export found epics
$csvPath = "wave6_79_epics_verified.csv"
$found | Export-Csv -Path $csvPath -NoTypeInformation
Write-Host "`nVerified epics exported to: $csvPath" -ForegroundColor Green

# Summary for user
Write-Host "`n=== SUMMARY ===" -ForegroundColor Cyan
Write-Host "Wave 6 Scope: EPIC-CCN-001 through 080 (excluding 024, 027)"
Write-Host "Total Expected: 78 epics"
Write-Host "Total Found: $($found.Count) epics"
Write-Host "Match: $(if ($found.Count -eq 78) { '✅ YES' } else { '❌ NO' })"

# Made with Bob
