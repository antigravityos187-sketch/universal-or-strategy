# Count Jane Street P0 violations
$json = Get-Content 'jane_street_p0_violations.json' -Raw -Encoding UTF8 | ConvertFrom-Json
Write-Host "Total Jane Street P0 violations: $($json.Count)"

# Group by file
$byFile = $json | Group-Object -Property file | Sort-Object -Property Count -Descending
Write-Host ""
Write-Host "Top 10 files with most violations:"
$byFile | Select-Object -First 10 | ForEach-Object {
    Write-Host "  $($_.Name): $($_.Count) violations"
}

# Group by rule
$byRule = $json | Group-Object -Property rule_id | Sort-Object -Property Count -Descending
Write-Host ""
Write-Host "Top 10 most common violations:"
$byRule | Select-Object -First 10 | ForEach-Object {
    Write-Host "  $($_.Name): $($_.Count) violations"
}

# Made with Bob
