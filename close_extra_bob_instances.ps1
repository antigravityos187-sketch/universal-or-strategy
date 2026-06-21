# Close Extra Bob Instances Script
# Keeps only the 2 most recent Bob processes, closes the rest

Write-Host "=== CLOSING EXTRA BOB INSTANCES ===" -ForegroundColor Cyan
Write-Host ""

# Get all Bob processes
$bobProcesses = Get-Process | Where-Object {$_.Name -like "*Bob*"} | Sort-Object StartTime -Descending

Write-Host "Found $($bobProcesses.Count) Bob instances:" -ForegroundColor Yellow
$bobProcesses | Select-Object Id, Name, @{Name="CPU(s)";Expression={[math]::Round($_.CPU,2)}}, @{Name="Memory(MB)";Expression={[math]::Round($_.WorkingSet/1MB,2)}}, StartTime | Format-Table -AutoSize

if ($bobProcesses.Count -le 2) {
    Write-Host "`nOnly 2 or fewer Bob instances running. No action needed." -ForegroundColor Green
    exit 0
}

Write-Host "`nKeeping the 2 most recent Bob instances..." -ForegroundColor Yellow
$toKeep = $bobProcesses | Select-Object -First 2
$toClose = $bobProcesses | Select-Object -Skip 2

Write-Host "`nClosing $($toClose.Count) older Bob instances:" -ForegroundColor Yellow
foreach ($process in $toClose) {
    try {
        $cpuRounded = [math]::Round($process.CPU,2)
        $memRounded = [math]::Round($process.WorkingSet/1MB,2)
        Write-Host "  Closing Bob (PID: $($process.Id), CPU: ${cpuRounded}s, Memory: ${memRounded}MB)" -ForegroundColor Red
        Stop-Process -Id $process.Id -Force
        Write-Host "    Success: Closed" -ForegroundColor Green
    } catch {
        Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=== CLEANUP COMPLETE ===" -ForegroundColor Cyan
Write-Host "`nRemaining Bob instances:" -ForegroundColor Yellow
Get-Process | Where-Object {$_.Name -like "*Bob*"} | Select-Object Id, Name, @{Name="CPU(s)";Expression={[math]::Round($_.CPU,2)}}, @{Name="Memory(MB)";Expression={[math]::Round($_.WorkingSet/1MB,2)}} | Format-Table -AutoSize

Write-Host "`nExpected improvement:" -ForegroundColor Green
Write-Host "  - CPU usage should drop significantly" -ForegroundColor White
Write-Host "  - Memory freed: approximately 4-5 GB" -ForegroundColor White
Write-Host "  - Fan noise should reduce within 1-2 minutes" -ForegroundColor White

# Made with Bob
