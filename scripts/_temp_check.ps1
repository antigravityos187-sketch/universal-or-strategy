# Temp script to check for ghost NinjaTrader folders and files
Write-Host "=== NinjaTrader folders in Documents ==="
Get-ChildItem 'c:\Users\Mohammed Khalid\Documents\' -Directory | Where-Object { $_.Name -like '*Ninja*' } | ForEach-Object { Write-Host $_.FullName }

Write-Host ""
Write-Host "=== Checking for UniversalORStrategyV12.cs (production monolith) in all NT paths ==="
$searchPaths = @(
    'c:\Users\Mohammed Khalid\Documents\NinjaTrader 8\bin\Custom\',
    'c:\Users\Mohammed Khalid\Documents\'
)
foreach ($p in $searchPaths) {
    if (Test-Path $p) {
        Get-ChildItem $p -Recurse -Filter 'UniversalORStrategyV12.cs' -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "FOUND: $($_.FullName)" }
    }
}

Write-Host ""
Write-Host "=== Templates folder check ==="
$templatePath = 'c:\Users\Mohammed Khalid\Documents\NinjaTrader 8\templates'
if (Test-Path $templatePath) {
    Get-ChildItem $templatePath -Recurse -Filter '*UniversalOR*' -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "FOUND: $($_.FullName)" }
} else {
    Write-Host "No templates folder found"
}

Write-Host ""
Write-Host "=== Deployed Dev files verification ==="
$stratPath = 'c:\Users\Mohammed Khalid\Documents\NinjaTrader 8\bin\Custom\Strategies'
Get-ChildItem $stratPath -Filter '*UniversalOR*' | ForEach-Object { Write-Host "$($_.Name) - $($_.LastWriteTime)" }
Get-ChildItem $stratPath -Filter '*Signal*' | ForEach-Object { Write-Host "$($_.Name) - $($_.LastWriteTime)" }

Write-Host ""
Write-Host "=== Line 47 of deployed Dev.cs ==="
$line = (Get-Content "$stratPath\UniversalORStrategyV12_Dev.cs")[46]
Write-Host "LINE 47: $line"
