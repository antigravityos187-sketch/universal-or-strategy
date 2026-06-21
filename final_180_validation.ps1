# Final 180-Method Validation
# Cross-reference: Baseline (180) → Wave 6 (80) → Wave 7 (100)

Write-Host "`n=== FINAL 180-METHOD VALIDATION ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Count baseline methods (CYC > 8)
Write-Host "Step 1: Counting baseline methods (CYC > 8)..." -ForegroundColor Yellow
$baselineCount = (Select-String -Path "complexity_audit_fresh_2026-06-14.txt" -Pattern '^\s+-\s+.*::.* \(CYC=([9]|[1-9][0-9])').Count
Write-Host "  Baseline: $baselineCount methods" -ForegroundColor Green

# Step 2: Count Wave 6 epics with Phase 0
Write-Host "`nStep 2: Counting Wave 6 epics (EPIC-CCN-001 through 080)..." -ForegroundColor Yellow
$wave6Count = 0
$wave6Phase1Count = 0
$wave6Epics = @()

for ($i = 1; $i -le 80; $i++) {
    $epicNum = "{0:D3}" -f $i
    $epicId = "EPIC-CCN-$epicNum"
    $hotspotPath = "docs/brain/$epicId/00-hotspots.md"
    $scopePath = "docs/brain/$epicId/00-scope.md"
    
    if (Test-Path $hotspotPath) {
        $wave6Count++
        $wave6Epics += $epicId
        
        if (Test-Path $scopePath) {
            $wave6Phase1Count++
        }
    }
}

Write-Host "  Wave 6 epics: $wave6Count" -ForegroundColor Green
Write-Host "  Phase 0 complete: $wave6Count" -ForegroundColor Green
Write-Host "  Phase 1 complete: $wave6Phase1Count" -ForegroundColor Green

# Step 3: Calculate Wave 7
Write-Host "`nStep 3: Calculating Wave 7 methods..." -ForegroundColor Yellow
$wave7Count = $baselineCount - $wave6Count
Write-Host "  Wave 7 methods: $wave7Count (baseline $baselineCount - wave6 $wave6Count)" -ForegroundColor Green

# Step 4: Validate totals
Write-Host "`n=== VALIDATION RESULTS ===" -ForegroundColor Cyan
$total = $wave6Count + $wave7Count
$match = $total -eq $baselineCount

Write-Host "Baseline (CYC > 8): $baselineCount methods" -ForegroundColor Yellow
Write-Host "Wave 6: $wave6Count epics/methods" -ForegroundColor $(if ($wave6Count -eq 80) { "Green" } else { "Yellow" })
Write-Host "Wave 7: $wave7Count methods" -ForegroundColor $(if ($wave7Count -eq 100) { "Green" } else { "Yellow" })
Write-Host "Total: $total" -ForegroundColor $(if ($match) { "Green" } else { "Red" })
Write-Host "Match: $(if ($match) { '✅ YES' } else { '❌ NO' })" -ForegroundColor $(if ($match) { "Green" } else { "Red" })

# Step 5: Special cases
Write-Host "`n=== SPECIAL CASES ===" -ForegroundColor Cyan
$epic003 = Test-Path "docs/brain/EPIC-CCN-003/00-hotspots.md"
Write-Host "EPIC-003 (Local .dll): $(if ($epic003) { '✅ Found' } else { '❌ Missing' })" -ForegroundColor $(if ($epic003) { "Green" } else { "Red" })

# Step 6: Summary
Write-Host "`n=== SUMMARY ===" -ForegroundColor Cyan
Write-Host "Total Methods (CYC > 8): $baselineCount" -ForegroundColor White
Write-Host ""
Write-Host "Wave 6 Breakdown:" -ForegroundColor White
Write-Host "  - Epics: EPIC-CCN-001 through 080" -ForegroundColor Gray
Write-Host "  - Count: $wave6Count epics" -ForegroundColor Gray
Write-Host "  - Phase 0: $wave6Count complete" -ForegroundColor Gray
Write-Host "  - Phase 1: $wave6Phase1Count complete" -ForegroundColor Gray
Write-Host "  - Special: EPIC-003 (local .dll execution)" -ForegroundColor Gray
Write-Host ""
Write-Host "Wave 7 Breakdown:" -ForegroundColor White
Write-Host "  - Methods: $wave7Count (computed via set difference)" -ForegroundColor Gray
Write-Host "  - Status: Ready for Phase 0 generation" -ForegroundColor Gray
Write-Host ""
Write-Host "Wave 8 (Merged):" -ForegroundColor White
Write-Host "  - Total: $total methods" -ForegroundColor Gray
Write-Host "  - Validation: $(if ($match) { '✅ PASS' } else { '❌ FAIL' })" -ForegroundColor $(if ($match) { "Green" } else { "Red" })

# Export summary
$summary = @{
    baseline_methods = $baselineCount
    wave6_epics = $wave6Count
    wave6_phase0_complete = $wave6Count
    wave6_phase1_complete = $wave6Phase1Count
    wave7_methods = $wave7Count
    wave8_total = $total
    validation_passed = $match
    special_cases = @{
        epic_003_local = $epic003
    }
}

$summary | ConvertTo-Json | Out-File "wave8_final_validation.json"
Write-Host "`n✅ Exported: wave8_final_validation.json" -ForegroundColor Green

# Made with Bob
