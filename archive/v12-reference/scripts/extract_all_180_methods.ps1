# Extract all 180 methods from complexity audit
# Outputs JSON for Wave 7 extraction

$methods = @()
$lines = Get-Content complexity_audit_fresh_2026-06-14.txt | Select-String '- .+::.+ \(CYC=(\d+)'

foreach ($line in $lines) {
    if ($line -match '-\s+(.+?)::(.+?)\s+\(CYC=(\d+)') {
        $file = $matches[1].Trim()
        $method = $matches[2].Trim()
        $cyc = [int]$matches[3]
        
        if ($cyc -gt 8) {
            $methods += [PSCustomObject]@{
                file = $file
                method = $method
                cyc = $cyc
            }
        }
    }
}

Write-Host "================================================================================"
Write-Host "180 METHOD VALIDATION"
Write-Host "================================================================================"
Write-Host ""
Write-Host "Total methods with CYC > 8: $($methods.Count)"

if ($methods.Count -eq 180) {
    Write-Host "[PASS] Validation PASSED: 180 methods confirmed" -ForegroundColor Green
} else {
    Write-Host "[FAIL] Validation FAILED: Expected 180, found $($methods.Count)" -ForegroundColor Red
    Write-Host "       Difference: $($methods.Count - 180)"
}

Write-Host ""
Write-Host "COMPLEXITY DISTRIBUTION:"
$low = ($methods | Where-Object { $_.cyc -le 14 }).Count
$medium = ($methods | Where-Object { $_.cyc -ge 15 -and $_.cyc -le 19 }).Count
$high = ($methods | Where-Object { $_.cyc -ge 20 }).Count

Write-Host "  Low (CYC 9-14):     $low methods ($([math]::Round($low/$methods.Count*100, 1))%)"
Write-Host "  Medium (CYC 15-19): $medium methods ($([math]::Round($medium/$methods.Count*100, 1))%)"
Write-Host "  High (CYC 20+):     $high methods ($([math]::Round($high/$methods.Count*100, 1))%)"

Write-Host ""
Write-Host "TOP 10 MOST COMPLEX METHODS:"
$methods | Sort-Object -Property cyc -Descending | Select-Object -First 10 | ForEach-Object -Begin { $i = 1 } -Process {
    Write-Host ("{0,2}. {1,-40} :: {2,-40} CYC: {3}" -f $i, $_.file, $_.method, $_.cyc)
    $i++
}

Write-Host ""
Write-Host "Exporting to baseline_180_methods.json..."
$methods | ConvertTo-Json -Depth 10 | Out-File -FilePath baseline_180_methods.json -Encoding UTF8
Write-Host "[OK] Export complete"
Write-Host ""

# Made with Bob
