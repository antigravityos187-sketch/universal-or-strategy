# Launch 3 Worker Orchestrator Windows
# Purpose: Open 3 VS Code windows for parallel epic execution
# Usage: powershell -File .\scripts\launch_worker_orchestrators.ps1

Write-Host "🚀 Launching 3 Worker Orchestrator Windows..." -ForegroundColor Cyan
Write-Host ""

# Find Bob IDE executable
$bobPath = "$env:LOCALAPPDATA\Programs\IBM Bob\IBM Bob.exe"

if (-not (Test-Path $bobPath)) {
    Write-Host "ERROR: Bob IDE not found at: $bobPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please ensure Bob IDE is installed" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found Bob IDE at: $bobPath" -ForegroundColor Green
Write-Host ""

# Define worker directories
$workers = @(
    @{
        Name = "Worker 1"
        Path = "C:\WSGTA\universal-or-epic-cluster-1"
        Epic = "EPIC-CCN-21"
    },
    @{
        Name = "Worker 2"
        Path = "C:\WSGTA\universal-or-epic-cluster-2"
        Epic = "EPIC-CCN-23"
    },
    @{
        Name = "Worker 3"
        Path = "C:\WSGTA\universal-or-epic-cluster-3"
        Epic = "EPIC-CCN-24"
    }
)

# Verify all worker directories exist
$allExist = $true
foreach ($worker in $workers) {
    if (-not (Test-Path $worker.Path)) {
        Write-Host "❌ ERROR: $($worker.Name) directory not found: $($worker.Path)" -ForegroundColor Red
        $allExist = $false
    }
}

if (-not $allExist) {
    Write-Host ""
    Write-Host "Please create worker directories using git worktree:" -ForegroundColor Yellow
    Write-Host "  git worktree add ..\universal-or-epic-cluster-1 gitbutler/workspace" -ForegroundColor Yellow
    Write-Host "  git worktree add ..\universal-or-epic-cluster-2 gitbutler/workspace" -ForegroundColor Yellow
    Write-Host "  git worktree add ..\universal-or-epic-cluster-3 gitbutler/workspace" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ All worker directories verified" -ForegroundColor Green
Write-Host ""

# Launch VS Code windows
foreach ($worker in $workers) {
    Write-Host "🪟 Opening $($worker.Name): $($worker.Path)" -ForegroundColor Cyan
    
    # Launch Bob IDE in new window
    Start-Process -FilePath $bobPath -ArgumentList "--new-window", $worker.Path
    
    # Small delay to avoid overwhelming the system
    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "✅ All 3 worker windows launched!" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Wait for all 3 VS Code windows to open (~5 seconds)" -ForegroundColor White
Write-Host "  2. In each window, click the Bob IDE icon in the left sidebar" -ForegroundColor White
Write-Host "  3. Start orchestrators:" -ForegroundColor White
Write-Host ""
Write-Host "     Window 2 (cluster-1): /epic-orchestrate EPIC-CCN-21" -ForegroundColor Cyan
Write-Host "     Window 3 (cluster-2): /epic-orchestrate EPIC-CCN-23" -ForegroundColor Cyan
Write-Host "     Window 4 (cluster-3): /epic-orchestrate EPIC-CCN-24" -ForegroundColor Cyan
Write-Host ""
Write-Host "  4. Return to Window 1 (this window) to monitor progress" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Umbrella Orchestrator ready in Window 1!" -ForegroundColor Green

# Made with Bob
