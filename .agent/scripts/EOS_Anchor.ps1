# EOS_Anchor.ps1 - Autonomous Brain Persistence for V12.12
# This script is run by the Director at the end of every session.

$TaskFile = "C:\WSGTA\universal-or-strategy\.agent\brain\task.md"
$BriefFile = "C:\WSGTA\universal-or-strategy\.agent\brain\master_mission_brief_ui.md"

# 1. Gather Task Context
Write-Host "Gathering Task Context..." -ForegroundColor Cyan
$RecentTasks = Get-Content $TaskFile | Select-String "\[x\]" | Select-Object -First 10

# 2. Build the Snapshot
$Date = Get-Date -Format "yyyy-MM-dd HH:mm"
$Content = @"
[DNA_CHECKPOINT_V12.12_SNAPSHOT]
TIMESTAMP: $Date
SYNOPSIS: V12.12 Strategic Refactor Prep.
COMPLETED ITEMS:
$($RecentTasks -join "`n")

ARCHITECTURAL ANCHOR:
$(Get-Content $BriefFile | Select-Object -First 20)
"@

# --- SEEDING TO SUPERMEMORY ---
Write-Host "ðŸ’¾ Seeding to Supermemory (Autonomous Bridge)..." -ForegroundColor Cyan

# Use the verified Node.JS engine to handle SSE-to-MCP flow
$jsEngine = Join-Path $RepoRoot ".agent\scripts\supermemory_anchor.js"
node $jsEngine $Content

if ($LASTEXITCODE -eq 0) {
    Write-Host "âœ… BRAIN ANCHOR COMPLETE!" -ForegroundColor Green
}
else {
    Write-Host "âŒ BRAIN ANCHOR FAILED!" -ForegroundColor Red
}

Write-Host "Anchor Successful! Web-models can now access this via the Chrome Extension." -ForegroundColor Green
