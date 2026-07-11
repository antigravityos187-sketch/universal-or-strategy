# Compare Jane Street violation files with complexity baseline files
$ErrorActionPreference = "Stop"

Write-Host "=" * 80
Write-Host "FILE COMPARISON: Jane Street Violations vs Complexity Baseline"
Write-Host "=" * 80

# Load Jane Street violations
$violations = Get-Content -Path "jane_street_p0_violations.json" -Raw | ConvertFrom-Json

# Load complexity baseline
$baseline = Get-Content -Path "baseline_180_methods.json" -Raw | ConvertFrom-Json

# Extract unique file paths
$violationFiles = $violations.violations | Select-Object -ExpandProperty file -Unique | Sort-Object
$baselineFiles = $baseline | Select-Object -ExpandProperty file -Unique | Sort-Object

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
    Write-Host "`nFiles with BOTH violations AND complexity:"
    $overlap | ForEach-Object {
        $file = $_
        $vCount = ($violations.violations | Where-Object { $_.file -eq $file }).Count
        $mCount = ($baseline | Where-Object { $_.file -eq $file }).Count
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

# Made with Bob
