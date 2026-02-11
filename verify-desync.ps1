# verify-desync.ps1
# WSGTA Infrastructure: Desync Diagnostic
# Compares Repo files with NinjaTrader files to ensure One Source of Truth.

$RepoRoot = "C:\WSGTA\universal-or-strategy"
$NtCustomDir = "C:\Users\Mohammed Khalid\Documents\NinjaTrader 8\bin\Custom"

$Files = @(
    "Indicators\V12StandardPanel_V12_001_Dev.cs",
    "Strategies\UniversalORStrategyV12_002_Dev.cs",
    "Strategies\UniversalORStrategyV12_002_Dev.Entries.cs",
    "Strategies\UniversalORStrategyV12_002_Dev.Orders.cs",
    "Strategies\UniversalORStrategyV12_002_Dev.SIMA.cs",
    "Strategies\UniversalORStrategyV12_002_Dev.REAPER.cs",
    "Strategies\UniversalORStrategyV12_002_Dev.UI.cs",
    "Strategies\SignalBroadcaster.cs"
)

Write-Host "`n--- WSGTA DESYNC AUDIT ---" -ForegroundColor Cyan
$DesyncedCount = 0

foreach ($relPath in $Files) {
    $repoPath = Join-Path $RepoRoot (Split-Path $relPath -Leaf)
    $ntPath = Join-Path $NtCustomDir $relPath
    
    if (!(Test-Path $repoPath)) { continue }
    if (!(Test-Path $ntPath)) {
        Write-Host "FAIL: Missing in NT8 -> $relPath" -ForegroundColor Red
        $DesyncedCount++
        continue
    }

    $ntItem = Get-Item $ntPath
    if ($ntItem.Attributes -match "ReparsePoint") {
        Write-Host "PASS: [LINKED] $relPath" -ForegroundColor Green
    }
    else {
        # Compare content if not linked
        $repoHash = (Get-FileHash $repoPath).Hash
        $ntHash = (Get-FileHash $ntPath).Hash
        
        if ($repoHash -eq $ntHash) {
            Write-Host "PASS: [SYNCED] $relPath" -ForegroundColor Green
        }
        else {
            Write-Host "FAIL: [DRIFTED] $relPath (Contents Different!)" -ForegroundColor Red
            $DesyncedCount++
        }
    }
}

if ($DesyncedCount -eq 0) {
    Write-Host "`nSUCCESS: All V12 Files are Secure." -ForegroundColor Cyan
}
else {
    Write-Host "`nWARNING: $DesyncedCount files found in Desync state!" -ForegroundColor Yellow
    Write-Host "Run .\deploy-sync.ps1 to fix." -ForegroundColor Gray
}
