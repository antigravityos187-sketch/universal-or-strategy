$json = Get-Content 'jane_street_p0_violations.json' -Raw | ConvertFrom-Json

$categories = @('Philosophy', 'Type Safety', 'Concurrency', 'Performance')

foreach ($cat in $categories) {
    Write-Host "`n=== $cat Examples ==="
    $examples = $json.violations | Where-Object { $_.category -eq $cat } | Select-Object -First 3
    
    foreach ($ex in $examples) {
        Write-Host "`nRule: $($ex.rule_id)"
        Write-Host "Message: $($ex.message)"
        Write-Host "Fix: $($ex.fix_suggestion)"
    }
}

Write-Host "`n=== Summary ==="
Write-Host "Total P0 Violations: $($json.summary.P0)"
Write-Host "Philosophy: $($json.by_category.Philosophy) (74.6%)"
Write-Host "Type Safety: $($json.by_category.'Type Safety') (23.1%)"
Write-Host "Concurrency: $($json.by_category.Concurrency) (1.7%)"
Write-Host "Performance: $($json.by_category.Performance) (0.7%)"

# Made with Bob
