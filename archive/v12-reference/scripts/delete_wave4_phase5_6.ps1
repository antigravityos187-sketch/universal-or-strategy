# Wave 4 Rollback - Delete Phase 5-6 Files for 78 Retry Epics
# Excludes: EPIC-CCN-024 (local execution), EPIC-CCN-027 (invalid)

Write-Host "=== Wave 4 Rollback: Phase 5-6 File Deletion ===" -ForegroundColor Cyan
Write-Host ""

# Build list of retry epics (all except 024, 027)
$retryEpics = @()
$retryEpics += 1..26 | ForEach-Object { "EPIC-CCN-{0:D3}" -f $_ }
$retryEpics += 28..80 | Where-Object { $_ -ne 24 } | ForEach-Object { "EPIC-CCN-{0:D3}" -f $_ }

Write-Host "Target: 78 retry epics (excluding 024, 027)" -ForegroundColor Yellow
Write-Host ""

$deletedCount = 0
$phase5Count = 0
$phase6Count = 0
$skippedCount = 0

foreach ($epic in $retryEpics) {
    $brainDir = "docs/brain/$epic"
    
    if (-not (Test-Path $brainDir)) {
        Write-Host "SKIP: $epic (brain directory not found)" -ForegroundColor Gray
        $skippedCount++
        continue
    }
    
    $epicDeleted = $false
    
    # Delete Phase 5 ticket files
    $phase5Files = Get-ChildItem "$brainDir/ticket-*-completion.md" -ErrorAction SilentlyContinue
    if ($phase5Files) {
        $phase5Files | Remove-Item -Force
        $phase5Count += $phase5Files.Count
        Write-Host "  Deleted $($phase5Files.Count) Phase 5 file(s) for $epic" -ForegroundColor Green
        $epicDeleted = $true
    }
    
    # Delete Phase 6 verification file
    $phase6File = "$brainDir/06-verification-report.md"
    if (Test-Path $phase6File) {
        Remove-Item $phase6File -Force
        $phase6Count++
        Write-Host "  Deleted Phase 6 file for $epic" -ForegroundColor Green
        $epicDeleted = $true
    }
    
    if ($epicDeleted) {
        $deletedCount++
    } else {
        Write-Host "  No Phase 5-6 files found for $epic" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "=== Rollback Complete ===" -ForegroundColor Cyan
Write-Host "Epics processed: $deletedCount" -ForegroundColor White
Write-Host "Epics skipped (no brain dir): $skippedCount" -ForegroundColor Gray
Write-Host "Phase 5 files deleted: $phase5Count" -ForegroundColor Green
Write-Host "Phase 6 files deleted: $phase6Count" -ForegroundColor Green
Write-Host "Total files deleted: $($phase5Count + $phase6Count)" -ForegroundColor Yellow
Write-Host ""

# Verification
Write-Host "=== Verification ===" -ForegroundColor Cyan
$remainingPhase5 = (Get-ChildItem -Path "docs/brain/EPIC-CCN-*/ticket-*-completion.md" -Recurse -ErrorAction SilentlyContinue).Count
$remainingPhase6 = (Get-ChildItem -Path "docs/brain/EPIC-CCN-*/06-verification-report.md" -Recurse -ErrorAction SilentlyContinue).Count

Write-Host "Remaining Phase 5 files: $remainingPhase5 (expected: 1 for EPIC-CCN-024)" -ForegroundColor $(if ($remainingPhase5 -eq 1) { "Green" } else { "Red" })
Write-Host "Remaining Phase 6 files: $remainingPhase6 (expected: 1 for EPIC-CCN-024)" -ForegroundColor $(if ($remainingPhase6 -eq 1) { "Green" } else { "Red" })

# Check EPIC-CCN-024 files still exist
$epic024Phase5 = Test-Path "docs/brain/EPIC-CCN-024/ticket-*-completion.md"
$epic024Phase6 = Test-Path "docs/brain/EPIC-CCN-024/06-verification-report.md"

Write-Host "EPIC-CCN-024 Phase 5 preserved: $epic024Phase5" -ForegroundColor $(if ($epic024Phase5) { "Green" } else { "Red" })
Write-Host "EPIC-CCN-024 Phase 6 preserved: $epic024Phase6" -ForegroundColor $(if ($epic024Phase6) { "Green" } else { "Red" })

Write-Host ""
if ($remainingPhase5 -eq 1 -and $remainingPhase6 -eq 1 -and $epic024Phase5 -and $epic024Phase6) {
    Write-Host "✅ Verification PASSED - Rollback successful" -ForegroundColor Green
} else {
    Write-Host "❌ Verification FAILED - Check counts above" -ForegroundColor Red
}

# Made with Bob
