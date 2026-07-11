# Complete 180-Method Mapping Validation
# Cross-references: Baseline audit → Wave 6 → Wave 7 → Wave 8

Write-Host "`n=== COMPLETE 180-METHOD MAPPING VALIDATION ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Parse baseline audit (180 methods from complexity_audit_fresh_2026-06-14.txt)
Write-Host "Step 1: Parsing baseline audit..." -ForegroundColor Yellow
$baselinePath = "complexity_audit_fresh_2026-06-14.txt"

if (-not (Test-Path $baselinePath)) {
    Write-Host "❌ ERROR: Baseline audit not found: $baselinePath" -ForegroundColor Red
    exit 1
}

$baselineMethods = @()
$content = Get-Content $baselinePath
$inMethodSection = $false

foreach ($line in $content) {
    # Start of methods section
    if ($line -match "Methods with CYC > 8:") {
        $inMethodSection = $true
        continue
    }
    
    # Parse method lines: "  - File.cs::MethodName (CYC=15, LOC=32)"
    if ($inMethodSection -and $line -match '^\s+-\s+(.+?)::(.+?)\s+\(CYC=(\d+)') {
        $file = $matches[1].Trim()
        $method = $matches[2].Trim()
        $cyc = [int]$matches[3]
        
        $baselineMethods += [PSCustomObject]@{
            File = $file
            Method = $method
            CYC = $cyc
            Source = "Baseline"
        }
    }
}

Write-Host "  Found: $($baselineMethods.Count) methods with CYC > 8" -ForegroundColor Green

# Step 2: Parse Wave 6 epics (Phase 0 hotspots)
Write-Host "`nStep 2: Parsing Wave 6 epics (EPIC-CCN-001 through 080)..." -ForegroundColor Yellow
$wave6Methods = @()
$wave6Epics = @()

for ($i = 1; $i -le 80; $i++) {
    $epicNum = "{0:D3}" -f $i
    $epicId = "EPIC-CCN-$epicNum"
    $hotspotPath = "docs/brain/$epicId/00-hotspots.md"
    $scopePath = "docs/brain/$epicId/00-scope.md"
    
    if (Test-Path $hotspotPath) {
        $hotspotContent = Get-Content $hotspotPath -Raw
        
        # Extract method name
        if ($hotspotContent -match '\*\*Method\*\*:\s*(.+?)[\r\n]') {
            $method = $matches[1].Trim()
        }
        
        # Extract file
        if ($hotspotContent -match '\*\*File\*\*:\s*(.+?)[\r\n]') {
            $file = $matches[1].Trim() -replace '^src/', ''
        }
        
        # Extract CYC
        if ($hotspotContent -match '\*\*Cyclomatic Complexity\*\*:\s*(\d+)') {
            $cyc = [int]$matches[1]
        }
        
        # Check Phase 1 completion
        $hasPhase1 = Test-Path $scopePath
        
        # Special case detection
        $special = ""
        if ($i -eq 3) { $special = "Local (.dll)" }
        
        $wave6Methods += [PSCustomObject]@{
            Epic = $epicId
            File = $file
            Method = $method
            CYC = $cyc
            Phase0 = $true
            Phase1 = $hasPhase1
            Special = $special
            Source = "Wave6"
        }
        
        $wave6Epics += $epicId
    }
}

Write-Host "  Found: $($wave6Methods.Count) Wave 6 methods" -ForegroundColor Green
Write-Host "  Phase 0 complete: $($wave6Methods.Count)" -ForegroundColor Green
Write-Host "  Phase 1 complete: $(($wave6Methods | Where-Object { $_.Phase1 }).Count)" -ForegroundColor Green

# Step 3: Compute Wave 7 methods (set difference)
Write-Host "`nStep 3: Computing Wave 7 methods (baseline - wave6)..." -ForegroundColor Yellow

$wave7Methods = @()
foreach ($baselineMethod in $baselineMethods) {
    $inWave6 = $wave6Methods | Where-Object { 
        $_.File -eq $baselineMethod.File -and $_.Method -eq $baselineMethod.Method 
    }
    
    if (-not $inWave6) {
        $wave7Methods += [PSCustomObject]@{
            File = $baselineMethod.File
            Method = $baselineMethod.Method
            CYC = $baselineMethod.CYC
            Source = "Wave7"
        }
    }
}

Write-Host "  Found: $($wave7Methods.Count) Wave 7 methods" -ForegroundColor Green

# Step 4: Validate totals
Write-Host "`n=== VALIDATION RESULTS ===" -ForegroundColor Cyan

$total = $wave6Methods.Count + $wave7Methods.Count
$match = $total -eq $baselineMethods.Count

Write-Host "Baseline methods (CYC > 8): $($baselineMethods.Count)" -ForegroundColor Yellow
Write-Host "Wave 6 methods: $($wave6Methods.Count)" -ForegroundColor $(if ($wave6Methods.Count -eq 80) { "Green" } else { "Red" })
Write-Host "Wave 7 methods: $($wave7Methods.Count)" -ForegroundColor $(if ($wave7Methods.Count -eq 100) { "Green" } else { "Red" })
Write-Host "Total (Wave 6 + Wave 7): $total" -ForegroundColor $(if ($match) { "Green" } else { "Red" })
Write-Host "Match: $(if ($match) { '✅ YES' } else { '❌ NO' })" -ForegroundColor $(if ($match) { "Green" } else { "Red" })

# Step 5: Check for overlaps
Write-Host "`n=== OVERLAP CHECK ===" -ForegroundColor Cyan
$overlaps = @()

foreach ($wave6Method in $wave6Methods) {
    $inWave7 = $wave7Methods | Where-Object { 
        $_.File -eq $wave6Method.File -and $_.Method -eq $wave6Method.Method 
    }
    
    if ($inWave7) {
        $overlaps += [PSCustomObject]@{
            File = $wave6Method.File
            Method = $wave6Method.Method
            Wave6Epic = $wave6Method.Epic
        }
    }
}

if ($overlaps.Count -eq 0) {
    Write-Host "✅ No overlaps detected" -ForegroundColor Green
} else {
    Write-Host "❌ OVERLAPS DETECTED: $($overlaps.Count)" -ForegroundColor Red
    $overlaps | Format-Table -AutoSize
}

# Step 6: Special cases summary
Write-Host "`n=== SPECIAL CASES ===" -ForegroundColor Cyan
$specialCases = $wave6Methods | Where-Object { $_.Special -ne "" }

if ($specialCases.Count -gt 0) {
    Write-Host "Found $($specialCases.Count) special case(s):" -ForegroundColor Yellow
    $specialCases | Format-Table Epic, Method, Special -AutoSize
} else {
    Write-Host "No special cases found" -ForegroundColor Green
}

# Step 7: Export results
Write-Host "`n=== EXPORT ===" -ForegroundColor Cyan

$wave6Methods | Export-Csv -Path "wave6_80_methods_verified.csv" -NoTypeInformation
$wave7Methods | Export-Csv -Path "wave7_100_methods_computed.csv" -NoTypeInformation

# Create combined manifest
$combinedManifest = @{
    generated_date = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    baseline_methods = $baselineMethods.Count
    wave6_methods = $wave6Methods.Count
    wave7_methods = $wave7Methods.Count
    total_methods = $total
    validation_passed = $match
    overlaps_detected = $overlaps.Count
    special_cases = $specialCases.Count
    wave6_epics = $wave6Epics
    wave6_phase0_complete = $wave6Methods.Count
    wave6_phase1_complete = ($wave6Methods | Where-Object { $_.Phase1 }).Count
}

$combinedManifest | ConvertTo-Json -Depth 10 | Out-File "wave8_180_method_manifest.json"

Write-Host "✅ wave6_80_methods_verified.csv" -ForegroundColor Green
Write-Host "✅ wave7_100_methods_computed.csv" -ForegroundColor Green
Write-Host "✅ wave8_180_method_manifest.json" -ForegroundColor Green

# Step 8: Final summary
Write-Host "`n=== FINAL SUMMARY ===" -ForegroundColor Cyan
Write-Host "Baseline: $($baselineMethods.Count) methods (CYC > 8)" -ForegroundColor Yellow
Write-Host "Wave 6: $($wave6Methods.Count) methods (EPIC-CCN-001 through 080)" -ForegroundColor Yellow
Write-Host "  - Phase 0 complete: $($wave6Methods.Count)" -ForegroundColor Green
Write-Host "  - Phase 1 complete: $(($wave6Methods | Where-Object { $_.Phase1 }).Count)" -ForegroundColor Green
Write-Host "  - Special cases: $($specialCases.Count)" -ForegroundColor Yellow
Write-Host "Wave 7: $($wave7Methods.Count) methods (computed via set difference)" -ForegroundColor Yellow
Write-Host "Wave 8: $total methods (Wave 6 + Wave 7)" -ForegroundColor Yellow
Write-Host ""
Write-Host "Validation: $(if ($match) { '✅ PASS' } else { '❌ FAIL' })" -ForegroundColor $(if ($match) { "Green" } else { "Red" })
Write-Host "Overlaps: $(if ($overlaps.Count -eq 0) { '✅ NONE' } else { "❌ $($overlaps.Count)" })" -ForegroundColor $(if ($overlaps.Count -eq 0) { "Green" } else { "Red" })

# Made with Bob
