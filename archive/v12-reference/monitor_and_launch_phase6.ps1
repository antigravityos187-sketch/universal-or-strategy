# Monitor EPIC-108 and auto-launch Phase 6 when complete
# Run this script to automate the remaining workflow

$ErrorActionPreference = "Continue"
$env:PYTHONIOENCODING = "utf-8"

Write-Host "=== Wave 2 Autonomous Monitor ===" -ForegroundColor Cyan
Write-Host "Started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host ""

$vaultPath = "C:\Users\Mohammed Khalid\Documents\V12-Agent-Vault"
$checkInterval = 300  # 5 minutes
$maxChecks = 20       # Max 100 minutes (20 * 5)
$checkCount = 0

function Update-Kanban {
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Updating Obsidian Kanban..." -ForegroundColor Yellow
    & python scripts/wave2/update_wave2_kanban.py --vault-path $vaultPath 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Kanban updated" -ForegroundColor Green
    }
}

function Check-Epic108Status {
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Checking EPIC-108 status..." -ForegroundColor Yellow
    
    # Check if completion log shows success
    $logCheck = gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="tail -5 /home/malhitticrypto/universal-or-strategy/logs/epic_108_completion.log 2>/dev/null" 2>$null
    
    if ($logCheck -match "EPIC-CCN-108 Phase 5 COMPLETE") {
        Write-Host "  EPIC-108 COMPLETE!" -ForegroundColor Green
        return $true
    }
    
    if ($logCheck -match "EPIC-108: BLOCKED") {
        Write-Host "  EPIC-108 BLOCKED - manual intervention needed" -ForegroundColor Red
        return "BLOCKED"
    }
    
    # Check screen sessions
    $screenCheck = gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls 2>/dev/null | grep epic108" 2>$null
    
    if ($screenCheck) {
        Write-Host "  Still running (screen session active)" -ForegroundColor Yellow
        
        # Show last few lines of progress
        $progress = gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="tail -3 /home/malhitticrypto/universal-or-strategy/logs/epic_108_completion.log 2>/dev/null" 2>$null
        Write-Host "  Progress: $($progress -join ' | ')" -ForegroundColor Gray
        
        return $false
    }
    
    # No screen session - check if complete or failed
    if ($logCheck -match "COMPLETE") {
        Write-Host "  EPIC-108 COMPLETE!" -ForegroundColor Green
        return $true
    }
    
    Write-Host "  Status unclear - checking again next cycle" -ForegroundColor Yellow
    return $false
}

function Launch-Phase6 {
    Write-Host ""
    Write-Host "=== Launching Phase 6 ===" -ForegroundColor Cyan
    Write-Host "Uploading script..." -ForegroundColor Yellow
    
    gcloud compute scp launch_phase6_all_epics.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a 2>$null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Script uploaded" -ForegroundColor Green
        
        Write-Host "Executing Phase 6..." -ForegroundColor Yellow
        gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && chmod +x launch_phase6_all_epics.sh && screen -dmS phase6 bash -c './launch_phase6_all_epics.sh 2>&1 | tee logs/phase6_all_epics.log'" 2>$null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  Phase 6 launched in screen session 'phase6'" -ForegroundColor Green
            Write-Host ""
            Write-Host "Monitor with: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command=`"tail -f /home/malhitticrypto/universal-or-strategy/logs/phase6_all_epics.log`"" -ForegroundColor Cyan
            return $true
        }
    }
    
    Write-Host "  Failed to launch Phase 6" -ForegroundColor Red
    return $false
}

# Initial status
Write-Host "Monitoring EPIC-108 completion..." -ForegroundColor Cyan
Write-Host "Will check every $checkInterval seconds (max $maxChecks checks)" -ForegroundColor Gray
Write-Host ""

# Update Kanban initially
Update-Kanban
Write-Host ""

# Monitoring loop
while ($checkCount -lt $maxChecks) {
    $checkCount++
    Write-Host "--- Check $checkCount/$maxChecks ---" -ForegroundColor Cyan
    
    $status = Check-Epic108Status
    
    if ($status -eq $true) {
        # EPIC-108 complete - launch Phase 6
        Write-Host ""
        Write-Host "EPIC-108 COMPLETE - Launching Phase 6..." -ForegroundColor Green
        
        Update-Kanban
        
        if (Launch-Phase6) {
            Write-Host ""
            Write-Host "=== Phase 6 Launched Successfully ===" -ForegroundColor Green
            Write-Host "Phase 6 will take approximately 2-3 hours" -ForegroundColor Yellow
            Write-Host "Check progress with:" -ForegroundColor Cyan
            Write-Host "  gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command=`"tail -30 /home/malhitticrypto/universal-or-strategy/logs/phase6_all_epics.log`"" -ForegroundColor Gray
            Write-Host ""
            Write-Host "Monitor will continue checking Phase 6 progress..." -ForegroundColor Yellow
            
            # Now monitor Phase 6
            $phase6Checks = 0
            $maxPhase6Checks = 36  # 3 hours at 5 min intervals
            
            while ($phase6Checks -lt $maxPhase6Checks) {
                Start-Sleep -Seconds $checkInterval
                $phase6Checks++
                
                Write-Host ""
                Write-Host "--- Phase 6 Check $phase6Checks/$maxPhase6Checks ---" -ForegroundColor Cyan
                
                $phase6Log = gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="tail -10 /home/malhitticrypto/universal-or-strategy/logs/phase6_all_epics.log 2>/dev/null" 2>$null
                
                if ($phase6Log -match "Phase 6 Complete") {
                    Write-Host "PHASE 6 COMPLETE!" -ForegroundColor Green
                    Update-Kanban
                    
                    Write-Host ""
                    Write-Host "=== WAVE 2 COMPLETE ===" -ForegroundColor Green
                    Write-Host "All epics processed through Phase 6" -ForegroundColor Green
                    Write-Host "Check completion reports in: docs/brain/EPIC-CCN-*/05-completion-report.md" -ForegroundColor Cyan
                    exit 0
                }
                
                Write-Host "  Phase 6 still running..." -ForegroundColor Yellow
                Write-Host "  Last lines: $($phase6Log[-2..-1] -join ' | ')" -ForegroundColor Gray
                
                # Update Kanban every 3 checks (15 minutes)
                if ($phase6Checks % 3 -eq 0) {
                    Update-Kanban
                }
            }
            
            Write-Host ""
            Write-Host "Phase 6 monitoring timeout reached (3 hours)" -ForegroundColor Yellow
            Write-Host "Check status manually" -ForegroundColor Yellow
            exit 0
        }
        
        Write-Host "Failed to launch Phase 6 - manual intervention required" -ForegroundColor Red
        exit 1
    }
    elseif ($status -eq "BLOCKED") {
        Write-Host ""
        Write-Host "EPIC-108 BLOCKED - Manual intervention required" -ForegroundColor Red
        Write-Host "Check: /home/malhitticrypto/universal-or-strategy/logs/epic_108_completion.log" -ForegroundColor Yellow
        exit 1
    }
    
    # Update Kanban every 3 checks (15 minutes)
    if ($checkCount % 3 -eq 0) {
        Write-Host ""
        Update-Kanban
    }
    
    # Wait before next check
    if ($checkCount -lt $maxChecks) {
        Write-Host "  Waiting $checkInterval seconds..." -ForegroundColor Gray
        Start-Sleep -Seconds $checkInterval
        Write-Host ""
    }
}

Write-Host ""
Write-Host "Monitoring timeout reached ($($maxChecks * $checkInterval / 60) minutes)" -ForegroundColor Yellow
Write-Host "EPIC-108 did not complete in expected time" -ForegroundColor Yellow
Write-Host "Check status manually:" -ForegroundColor Cyan
Write-Host "  gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command=`"tail -50 /home/malhitticrypto/universal-or-strategy/logs/epic_108_completion.log`"" -ForegroundColor Gray

# Made with Bob
