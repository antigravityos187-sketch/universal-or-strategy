# Wave 3 Phase 2 Status Checker
# Monitors Phase 2 (Architecture Planning) execution on GCP VM

param(
    [switch]$Detailed,
    [switch]$Logs,
    [string]$Epic = ""
)

Write-Host "`n=== Wave 3 Phase 2 Status Check ===" -ForegroundColor Cyan
Write-Host "Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss UTC')`n" -ForegroundColor Gray

# Check screen sessions
Write-Host "[1/4] Checking screen sessions..." -ForegroundColor Yellow
$screenOutput = gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls" 2>&1
$screenCount = ($screenOutput | Select-String "p2-" | Measure-Object).Count

if ($screenCount -eq 0) {
    Write-Host "  [OK] All sessions complete (No Sockets found)" -ForegroundColor Green
    $allComplete = $true
} else {
    Write-Host "  [RUNNING] $screenCount sessions still active" -ForegroundColor Yellow
    $allComplete = $false
    
    if ($Detailed) {
        Write-Host "`n  Active sessions:" -ForegroundColor Gray
        $screenOutput | Select-String "p2-" | ForEach-Object {
            Write-Host "    $_" -ForegroundColor Gray
        }
    }
}

# Check files created
Write-Host "`n[2/4] Checking architecture plan files..." -ForegroundColor Yellow
$fileCount = gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-{116..125}/02-architecture-plan.md 2>/dev/null | wc -l" 2>&1
$fileCount = [int]$fileCount.Trim()

Write-Host "  Files created: $fileCount/10" -ForegroundColor $(if ($fileCount -eq 10) { "Green" } elseif ($fileCount -gt 0) { "Yellow" } else { "Red" })

if ($fileCount -gt 0 -and $Detailed) {
    Write-Host "`n  Created files:" -ForegroundColor Gray
    $files = gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls -lh /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-{116..125}/02-architecture-plan.md 2>/dev/null" 2>&1
    $files | ForEach-Object {
        Write-Host "    $_" -ForegroundColor Gray
    }
}

# Check bobcoin usage
Write-Host "`n[3/4] Checking bobcoin usage..." -ForegroundColor Yellow
$bobcoinOutput = gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' /home/malhitticrypto/universal-or-strategy/logs/phase2/EPIC-CCN-*.log 2>/dev/null" 2>&1

if ($bobcoinOutput) {
    $costs = $bobcoinOutput | Select-String "Cost: (\d+\.\d+)" | ForEach-Object { [double]$_.Matches.Groups[1].Value }
    $totalCost = ($costs | Measure-Object -Sum).Sum
    
    Write-Host "  Epics reported: $($costs.Count)/10" -ForegroundColor $(if ($costs.Count -eq 10) { "Green" } elseif ($costs.Count -gt 0) { "Yellow" } else { "Red" })
    
    if ($costs.Count -gt 0) {
        Write-Host "  Total cost: $([math]::Round($totalCost, 2)) bobcoins" -ForegroundColor Cyan
        Write-Host "  Average: $([math]::Round($totalCost / $costs.Count, 2)) bobcoins/epic" -ForegroundColor Gray
        
        if ($Detailed) {
            Write-Host "`n  Per-epic costs:" -ForegroundColor Gray
            $bobcoinOutput | ForEach-Object {
                Write-Host "    $_" -ForegroundColor Gray
            }
        }
    }
} else {
    Write-Host "  [PENDING] No bobcoin reports yet" -ForegroundColor Yellow
}

# Check for errors
Write-Host "`n[4/4] Checking for errors..." -ForegroundColor Yellow
$errors = gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -i 'error\|failed\|exception' /home/malhitticrypto/universal-or-strategy/logs/phase2/EPIC-CCN-*.log 2>/dev/null | head -20" 2>&1

if ($errors -and $errors -notmatch "No such file") {
    Write-Host "  [WARNING] Errors detected in logs" -ForegroundColor Red
    if ($Detailed) {
        Write-Host "`n  Error samples:" -ForegroundColor Gray
        $errors | Select-Object -First 10 | ForEach-Object {
            Write-Host "    $_" -ForegroundColor Red
        }
    }
} else {
    Write-Host "  [OK] No errors detected" -ForegroundColor Green
}

# Summary
Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "Status: $(if ($allComplete) { 'COMPLETE' } else { 'IN PROGRESS' })" -ForegroundColor $(if ($allComplete) { "Green" } else { "Yellow" })
Write-Host "Files: $fileCount/10" -ForegroundColor $(if ($fileCount -eq 10) { "Green" } elseif ($fileCount -gt 0) { "Yellow" } else { "Red" })
Write-Host "Sessions: $(if ($allComplete) { '0 (all done)' } else { "$screenCount active" })" -ForegroundColor $(if ($allComplete) { "Green" } else { "Yellow" })

if ($allComplete -and $fileCount -eq 10) {
    Write-Host "`n[SUCCESS] Phase 2 complete! Ready to sync files and create report." -ForegroundColor Green
    Write-Host "`nNext steps:" -ForegroundColor Cyan
    Write-Host "  1. Run: gcloud compute scp --recurse v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-{116..125} docs/brain/ --zone=us-central1-a" -ForegroundColor Gray
    Write-Host "  2. Create Phase 2 completion report" -ForegroundColor Gray
    Write-Host "  3. Prepare Phase 3 (Audit) scripts" -ForegroundColor Gray
} elseif ($allComplete -and $fileCount -lt 10) {
    Write-Host "`n[WARNING] Sessions complete but missing files. Check logs for failures." -ForegroundColor Yellow
} else {
    $elapsed = [math]::Round(((Get-Date) - (Get-Date "2026-06-13 23:50:10")).TotalMinutes, 1)
    $remaining = [math]::Max(0, 25 - $elapsed)
    Write-Host "`n[IN PROGRESS] Estimated time remaining: $remaining minutes" -ForegroundColor Yellow
    Write-Host "Check again in 5-10 minutes." -ForegroundColor Gray
}

# View specific log if requested
if ($Logs -and $Epic) {
    Write-Host "`n=== Log for EPIC-CCN-$Epic ===" -ForegroundColor Cyan
    gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="tail -50 /home/malhitticrypto/universal-or-strategy/logs/phase2/EPIC-CCN-$Epic.log"
}

Write-Host ""

# Made with Bob
