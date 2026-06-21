# Compare Jane Street violation files with complexity baseline files (NORMALIZED PATHS)
$ErrorActionPreference = "Stop"

Write-Host "=" * 80
Write-Host "FILE COMPARISON: Jane Street Violations vs Complexity Baseline (NORMALIZED)"
Write-Host "=" * 80

# Load Jane Street violations
$violations = Get-Content -Path "jane_street_p0_violations.json" -Raw | ConvertFrom-Json

# Load complexity baseline
$baseline = Get-Content -Path "baseline_180_methods.json" -Raw | ConvertFrom-Json

# Extract unique file paths and normalize (remove src\ prefix)
$violationFiles = $violations.violations | 
    Select-Object -ExpandProperty file -Unique | 
    ForEach-Object { $_ -replace '^src\\', '' } |
    Sort-Object -Unique

$baselineFiles = $baseline | 
    Select-Object -ExpandProperty file -Unique | 
    ForEach-Object { $_ -replace '^src\\', '' } |
    Sort-Object -Unique

Write-Host "`nJANE STREET VIOLATION FILES ($($violationFiles.Count) files):"
Write-Host "=" * 80
$violationFiles | ForEach-Object { Write-Host "  $_" }

Write-Host "`n`nCOMPLEXITY BASELINE FILES ($($baselineFiles.Count) files):"
Write-Host "=" * 80
$baselineFiles | ForEach-Object { Write-Host "  $_" }

# Find overlap
$overlap = $violationFiles | Where-Object { $baselineFiles -contains $_ }

Write-Host "`n`nOVERLAP ANALYSIS:"
Write-Host "=" * 80
Write-Host "Files with Jane Street violations: $($violationFiles.Count)"
Write-Host "Files with complexity methods: $($baselineFiles.Count)"
Write-Host "Files with BOTH: $($overlap.Count)"

if ($overlap.Count -gt 0) {
    $overlapPct = ($overlap.Count / $baselineFiles.Count) * 100
    Write-Host "Overlap percentage: $($overlapPct.ToString('F1'))%"
    
    Write-Host "`nFiles with BOTH violations AND complexity:"
    $overlap | ForEach-Object {
        $file = $_
        # Count violations (need to match with src\ prefix)
        $vCount = ($violations.violations | Where-Object { ($_.file -replace '^src\\', '') -eq $file }).Count
        # Count methods
        $mCount = ($baseline | Where-Object { ($_.file -replace '^src\\', '') -eq $file }).Count
        Write-Host "  $file"
        Write-Host "    - $mCount methods with CYC > 8"
        Write-Host "    - $vCount Jane Street violations"
    }
} else {
    Write-Host "`nNO OVERLAP - Files are completely different!"
}

# Find files only in violations
$violationOnly = $violationFiles | Where-Object { $baselineFiles -notcontains $_ }
Write-Host "`n`nFILES WITH ONLY JANE STREET VIOLATIONS ($($violationOnly.Count) files):"
Write-Host "=" * 80
$violationOnly | ForEach-Object { Write-Host "  $_" }

# Find files only in baseline
$baselineOnly = $baselineFiles | Where-Object { $violationFiles -notcontains $_ }
Write-Host "`n`nFILES WITH ONLY COMPLEXITY METHODS ($($baselineOnly.Count) files):"
Write-Host "=" * 80
$baselineOnly | ForEach-Object { Write-Host "  $_" }

Write-Host "`n" + ("=" * 80)
Write-Host "SUMMARY:"
Write-Host "  Total unique files: $(($violationFiles + $baselineFiles | Sort-Object -Unique).Count)"
Write-Host "  Files with both issues: $($overlap.Count)"
Write-Host "  Files with only violations: $($violationOnly.Count)"
Write-Host "  Files with only complexity: $($baselineOnly.Count)"
Write-Host "=" * 80

# Made with Bob
