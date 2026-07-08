# Analyze Jane Street P0 violations from JSON file
$ErrorActionPreference = "Stop"

Write-Host "=" * 80
Write-Host "JANE STREET VIOLATIONS ANALYSIS"
Write-Host "=" * 80

# Read JSON file
$json = Get-Content -Path "jane_street_p0_violations.json" -Raw | ConvertFrom-Json

# Print summary
Write-Host "`nSUMMARY:"
Write-Host "  Total violations: $($json.summary.total)"
Write-Host "  P0 (Critical): $($json.summary.P0)"
Write-Host "  P1 (High): $($json.summary.P1)"
Write-Host "  P2 (Medium): $($json.summary.P2)"

Write-Host "`nBY CATEGORY:"
$json.by_category.PSObject.Properties | ForEach-Object {
    Write-Host "  $($_.Name): $($_.Value)"
}

# Count violations by file
Write-Host "`n" + ("=" * 80)
Write-Host "TOP 20 FILES WITH MOST VIOLATIONS"
Write-Host "=" * 80
$fileGroups = $json.violations | Group-Object -Property file | Sort-Object Count -Descending | Select-Object -First 20
foreach ($group in $fileGroups) {
    Write-Host "  $($group.Name): $($group.Count) violations"
}

# Count violations by rule
Write-Host "`n" + ("=" * 80)
Write-Host "TOP 20 MOST COMMON VIOLATIONS"
Write-Host "=" * 80
$ruleGroups = $json.violations | Group-Object -Property rule_id | Sort-Object Count -Descending | Select-Object -First 20
foreach ($group in $ruleGroups) {
    Write-Host "  $($group.Name): $($group.Count) violations"
}

# Check overlap with 180 complexity methods
Write-Host "`n" + ("=" * 80)
Write-Host "CHECKING OVERLAP WITH 180 COMPLEXITY METHODS"
Write-Host "=" * 80

if (Test-Path "baseline_180_methods.json") {
    $baseline = Get-Content -Path "baseline_180_methods.json" -Raw | ConvertFrom-Json
    
    # Extract unique file paths
    $baselineFiles = $baseline | Select-Object -ExpandProperty file -Unique
    $violationFiles = $json.violations | Select-Object -ExpandProperty file -Unique
    
    # Find overlap
    $overlap = $baselineFiles | Where-Object { $violationFiles -contains $_ }
    
    Write-Host "`nBaseline complexity files: $($baselineFiles.Count)"
    Write-Host "Files with Jane Street violations: $($violationFiles.Count)"
    Write-Host "Files with BOTH complexity AND violations: $($overlap.Count)"
    
    if ($baselineFiles.Count -gt 0) {
        $overlapPct = ($overlap.Count / $baselineFiles.Count) * 100
        Write-Host "Overlap percentage: $($overlapPct.ToString('F1'))%"
    }
    
    if ($overlap.Count -gt 0) {
        Write-Host "`nFiles with both issues (showing first 10):"
        $overlap | Select-Object -First 10 | ForEach-Object {
            $file = $_
            $vCount = ($json.violations | Where-Object { $_.file -eq $file }).Count
            $mCount = ($baseline | Where-Object { $_.file -eq $file }).Count
            Write-Host "  - $file"
            Write-Host "    * $mCount methods with CYC > 8"
            Write-Host "    * $vCount Jane Street violations"
        }
    }
} else {
    Write-Host "`nbaseline_180_methods.json not found - skipping overlap analysis"
}

Write-Host "`n" + ("=" * 80)

# Made with Bob
