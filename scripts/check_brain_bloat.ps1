# Check docs/brain/ context bloat
$files = Get-ChildItem -Path 'docs/brain' -File | Where-Object { 
    $_.Name -notlike 'WAVE[1-6]*' -and 
    $_.Name -notlike 'EPIC-CCN-*' -and 
    $_.Name -notlike 'EPIC-*' 
}

$count = $files.Count
$totalBytes = ($files | Measure-Object -Property Length -Sum).Sum
$totalMB = [math]::Round($totalBytes/1MB,2)

Write-Output "Files NOT excluded by .bobignore:"
Write-Output "Count: $count files"
Write-Output "Total: $totalMB MB"
Write-Output ""
Write-Output "Top 10 largest files:"
$files | Sort-Object Length -Descending | Select-Object -First 10 | ForEach-Object {
    $sizeMB = [math]::Round($_.Length/1MB,3)
    Write-Output "  $($_.Name): $sizeMB MB"
}

# Made with Bob
