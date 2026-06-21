$epics = Get-ChildItem 'docs/brain' -Directory | Where-Object { $_.Name -match '^EPIC-CCN-\d+$' } | Where-Object { [int]($_.Name -replace 'EPIC-CCN-', '') -le 80 }

Write-Host "Total Wave 6 epics (001-080): $($epics.Count)"
Write-Host ""

$complete = 0
$missing = 0

foreach ($epic in $epics) {
    $id = $epic.Name
    $hasPhase0 = Test-Path "$($epic.FullName)/00-hotspots.md"
    
    if ($hasPhase0) {
        Write-Host "✅ $id"
        $complete++
    } else {
        Write-Host "❌ $id - missing Phase 0"
        $missing++
    }
}

Write-Host ""
Write-Host "Summary:"
Write-Host "  Complete: $complete"
Write-Host "  Missing: $missing"

# Made with Bob
