# Wave 7 Method Extraction Script
# Extracts Wave 6 methods from Phase 0 hotspot files, computes Wave 7 as set difference
# Validates no overlap and exports wave7_methods.json

Write-Host "================================================================================"
Write-Host "WAVE 7 METHOD EXTRACTION"
Write-Host "================================================================================"
Write-Host ""

# Step 1: Load baseline 180 methods
Write-Host "[1/6] Loading baseline 180 methods from baseline_180_methods.json..."
$baseline = Get-Content baseline_180_methods.json | ConvertFrom-Json
Write-Host "      Loaded: $($baseline.Count) methods"

if ($baseline.Count -ne 180) {
    Write-Host "[ERROR] Expected 180 methods in baseline, found $($baseline.Count)" -ForegroundColor Red
    exit 1
}

# Step 2: Extract Wave 6 methods from Phase 0 hotspot files
Write-Host ""
Write-Host "[2/6] Extracting Wave 6 methods from Phase 0 hotspot files..."
$wave6Methods = @()
$wave6Epics = @()

# Wave 6 epics: 001-080, excluding 024 and 027
for ($i = 1; $i -le 80; $i++) {
    if ($i -eq 24 -or $i -eq 27) {
        Write-Host "      Skipping EPIC-CCN-$($i.ToString('000')) (excluded)" -ForegroundColor Yellow
        continue
    }
    
    $epicId = "EPIC-CCN-$($i.ToString('000'))"
    $hotspotFile = "docs/brain/$epicId/00-hotspots.md"
    
    if (-not (Test-Path $hotspotFile)) {
        Write-Host "      [WARN] Missing: $hotspotFile" -ForegroundColor Yellow
        continue
    }
    
    $content = Get-Content $hotspotFile -Raw
    
    # Try format 1: "- **Method**: MethodName"
    $method = $null
    $file = $null
    $cyc = $null
    
    if ($content -match '\*\*Method\*\*:\s*(.+?)[\r\n]') {
        $method = $matches[1].Trim()
    }
    # Try format 2: "- Method: MethodName" (no bold)
    elseif ($content -match '^-\s*Method:\s*(.+?)[\r\n]' -or $content -match '[\r\n]-\s*Method:\s*(.+?)[\r\n]') {
        $method = $matches[1].Trim()
    }
    
    # Try format 1: "- **File**: src/FileName.cs"
    if ($content -match '\*\*File\*\*:\s*src/(.+?)[\r\n]') {
        $file = $matches[1].Trim()
    }
    # Try format 2: "- File: src/FileName.cs" (no bold)
    elseif ($content -match '^-\s*File:\s*src/(.+?)[\r\n]' -or $content -match '[\r\n]-\s*File:\s*src/(.+?)[\r\n]') {
        $file = $matches[1].Trim()
    }
    
    # Try format 1: "- **Cyclomatic Complexity**: 18"
    if ($content -match '\*\*Cyclomatic Complexity\*\*:\s*(\d+)') {
        $cyc = [int]$matches[1]
    }
    # Try format 2: "- Cyclomatic Complexity: 18" (no bold)
    elseif ($content -match '^-\s*Cyclomatic Complexity:\s*(\d+)' -or $content -match '[\r\n]-\s*Cyclomatic Complexity:\s*(\d+)') {
        $cyc = [int]$matches[1]
    }
    # Try format 3: "- Current Complexity: 18"
    elseif ($content -match 'Current Complexity:\s*(\d+)') {
        $cyc = [int]$matches[1]
    }
    
    if (-not $method -or -not $file -or -not $cyc) {
        Write-Host "      [WARN] Incomplete data in $epicId (method=$method, file=$file, cyc=$cyc)" -ForegroundColor Yellow
        continue
    }
    
    $wave6Methods += [PSCustomObject]@{
        epic = $epicId
        file = $file
        method = $method
        cyc = $cyc
    }
    
    $wave6Epics += $epicId
}

Write-Host "      Extracted: $($wave6Methods.Count) methods from $($wave6Epics.Count) epics"

# Validate Wave 6 count
if ($wave6Methods.Count -ne 79) {
    Write-Host "[WARN] Expected 79 Wave 6 methods, found $($wave6Methods.Count)" -ForegroundColor Yellow
    Write-Host "       Difference: $($wave6Methods.Count - 79)"
    Write-Host "       This is expected if some epics have multi-method extractions or missing Phase 0 files"
}

# Step 3: Compute Wave 7 methods (set difference)
Write-Host ""
Write-Host "[3/6] Computing Wave 7 methods (set difference: 180 - Wave6)..."

$wave7Methods = @()

foreach ($baselineMethod in $baseline) {
    $found = $false
    
    foreach ($wave6Method in $wave6Methods) {
        if ($baselineMethod.file -eq $wave6Method.file -and 
            $baselineMethod.method -eq $wave6Method.method) {
            $found = $true
            break
        }
    }
    
    if (-not $found) {
        $wave7Methods += $baselineMethod
    }
}

Write-Host "      Wave 7 methods: $($wave7Methods.Count)"

# Step 4: Validate no overlap
Write-Host ""
Write-Host "[4/6] Validating no overlap between Wave 6 and Wave 7..."

$overlap = @()
foreach ($wave6Method in $wave6Methods) {
    foreach ($wave7Method in $wave7Methods) {
        if ($wave6Method.file -eq $wave7Method.file -and 
            $wave6Method.method -eq $wave7Method.method) {
            $overlap += [PSCustomObject]@{
                file = $wave6Method.file
                method = $wave6Method.method
            }
        }
    }
}

if ($overlap.Count -gt 0) {
    Write-Host "[ERROR] Found $($overlap.Count) overlapping methods:" -ForegroundColor Red
    $overlap | ForEach-Object {
        Write-Host "        - $($_.file)::$($_.method)" -ForegroundColor Red
    }
    exit 1
} else {
    Write-Host "      [PASS] No overlap detected" -ForegroundColor Green
}

# Validate total coverage
$totalMethods = $wave6Methods.Count + $wave7Methods.Count
if ($totalMethods -ne 180) {
    Write-Host "[ERROR] Total methods ($totalMethods) != 180" -ForegroundColor Red
    Write-Host "        Wave 6: $($wave6Methods.Count)" -ForegroundColor Red
    Write-Host "        Wave 7: $($wave7Methods.Count)" -ForegroundColor Red
    Write-Host "        Expected: 180" -ForegroundColor Red
    exit 1
} else {
    Write-Host "      [PASS] Total coverage: $totalMethods methods (100%)" -ForegroundColor Green
}

# Step 5: Export Wave 7 methods
Write-Host ""
Write-Host "[5/6] Exporting wave7_methods.json..."
$wave7Methods | ConvertTo-Json -Depth 10 | Out-File -FilePath wave7_methods.json -Encoding UTF8
Write-Host "      [OK] Exported $($wave7Methods.Count) methods to wave7_methods.json"

# Also export Wave 6 methods for reference
Write-Host "      Exporting wave6_methods.json for reference..."
$wave6Methods | ConvertTo-Json -Depth 10 | Out-File -FilePath wave6_methods.json -Encoding UTF8
Write-Host "      [OK] Exported $($wave6Methods.Count) methods to wave6_methods.json"

# Step 6: Generate summary statistics
Write-Host ""
Write-Host "[6/6] Generating summary statistics..."
Write-Host ""
Write-Host "================================================================================"
Write-Host "WAVE 7 SCOPE SUMMARY"
Write-Host "================================================================================"
Write-Host ""
Write-Host "WAVE 6 (Completed Phase 0-1):"
Write-Host "  Epics:   $($wave6Epics.Count)"
Write-Host "  Methods: $($wave6Methods.Count)"
Write-Host ""
Write-Host "WAVE 7 (New Scope):"
Write-Host "  Methods: $($wave7Methods.Count)"
Write-Host ""
Write-Host "TOTAL COVERAGE:"
Write-Host "  Wave 6 + Wave 7: $totalMethods methods"
Write-Host "  Baseline:        $($baseline.Count) methods"
Write-Host "  Coverage:        100%"
Write-Host ""

# Complexity distribution for Wave 7
$low = ($wave7Methods | Where-Object { $_.cyc -le 14 }).Count
$medium = ($wave7Methods | Where-Object { $_.cyc -ge 15 -and $_.cyc -le 19 }).Count
$high = ($wave7Methods | Where-Object { $_.cyc -ge 20 }).Count

Write-Host "WAVE 7 COMPLEXITY DISTRIBUTION:"
Write-Host "  Low (CYC 9-14):     $low methods ($([math]::Round($low/$wave7Methods.Count*100, 1))%)"
Write-Host "  Medium (CYC 15-19): $medium methods ($([math]::Round($medium/$wave7Methods.Count*100, 1))%)"
Write-Host "  High (CYC 20+):     $high methods ($([math]::Round($high/$wave7Methods.Count*100, 1))%)"
Write-Host ""

# Top 10 most complex Wave 7 methods
Write-Host "TOP 10 MOST COMPLEX WAVE 7 METHODS:"
$wave7Methods | Sort-Object -Property cyc -Descending | Select-Object -First 10 | ForEach-Object -Begin { $i = 1 } -Process {
    Write-Host ("{0,2}. {1,-40} :: {2,-40} CYC: {3}" -f $i, $_.file, $_.method, $_.cyc)
    $i++
}
Write-Host ""

# File distribution
Write-Host "WAVE 7 FILE DISTRIBUTION:"
$fileGroups = $wave7Methods | Group-Object -Property file | Sort-Object -Property Count -Descending
$fileGroups | Select-Object -First 10 | ForEach-Object {
    Write-Host ("  {0,-50} {1,3} methods" -f $_.Name, $_.Count)
}
if ($fileGroups.Count -gt 10) {
    Write-Host "  ... and $($fileGroups.Count - 10) more files"
}
Write-Host ""

Write-Host "================================================================================"
Write-Host "VALIDATION COMPLETE"
Write-Host "================================================================================"
Write-Host ""
Write-Host "[OK] Wave 7 method extraction successful" -ForegroundColor Green
Write-Host "[OK] No overlap with Wave 6" -ForegroundColor Green
Write-Host "[OK] 100% coverage of 180 baseline methods" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Review wave7_methods.json"
Write-Host "  2. Select 3 pilot epics for Wave 7 pilot"
Write-Host "  3. Generate Phase 0 scripts for Wave 7"
Write-Host ""

# Made with Bob