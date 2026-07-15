param (
    [string]$SrcPath = "C:\WSGTA\universal-or-strategy\src\PropTraderTools",
    [string]$NtPath  = "C:\Users\Mohammed Khalid\Documents\NinjaTrader 8\bin\Custom\AddOns\PropTraderTools",
    [switch]$Fix
)

# Files that are NEVER deployed to NT8 (test files, build artefacts).
# Presence of these in SrcPath does not constitute a MISSING violation.
$DeployExcludes = @("CopyEngineTests.cs")

Write-Host "=== NT8 HARD LINK INTEGRITY AUDIT ===" -ForegroundColor Cyan
Write-Host "SRC : $SrcPath"
Write-Host "NT8 : $NtPath"
if ($Fix) { Write-Host "MODE: AUTO-FIX (hard link repair enabled)" -ForegroundColor Yellow }
Write-Host ""

if (-not (Test-Path $NtPath)) {
    Write-Host "ERROR: NT8 path not found: $NtPath" -ForegroundColor Red
    Write-Host "       Verify NinjaTrader 8 is installed and the AddOns folder exists." -ForegroundColor Red
    exit 1
}

$desyncs  = 0
$missing  = 0
$ok       = 0
$skipped  = 0
$fixed    = 0

Get-ChildItem $SrcPath -Filter "*.cs" | ForEach-Object {
    $srcFile = $_.FullName
    $ntFile  = Join-Path $NtPath $_.Name

    # Skip non-deployable files (test files etc.)
    if ($DeployExcludes -contains $_.Name) {
        Write-Host "SKIP     : $($_.Name)  (test file -- not deployed to NT8)" -ForegroundColor DarkGray
        $skipped++
        return
    }

    if (-not (Test-Path $ntFile)) {
        if ($Fix) {
            # Create hard link
            New-Item -ItemType HardLink -Path $ntFile -Value $srcFile | Out-Null
            $linkCount = (fsutil hardlink list $ntFile 2>$null | Measure-Object -Line).Lines
            Write-Host "FIXED    : $($_.Name)  (hard link created, count=$linkCount)" -ForegroundColor Yellow
            $fixed++
        } else {
            Write-Host "MISSING  : $($_.Name)" -ForegroundColor Red
            $missing++
        }
        return
    }

    $srcHash = (Get-FileHash $srcFile -Algorithm SHA256).Hash
    $ntHash  = (Get-FileHash $ntFile  -Algorithm SHA256).Hash

    if ($srcHash -eq $ntHash) {
        # Check whether it is a hard link (link count > 1) or just a matching copy
        $linkCount = (fsutil hardlink list $ntFile 2>$null | Measure-Object -Line).Lines
        $linkStatus = if ($linkCount -ge 2) { "hard-linked" } else { "copy-only -- run -Fix" }
        Write-Host "OK       : $($_.Name)  ($linkStatus)" -ForegroundColor Green
        $ok++
    } else {
        if ($Fix) {
            # Replace NT8 copy with hard link to Wave source
            Remove-Item $ntFile -Force
            New-Item -ItemType HardLink -Path $ntFile -Value $srcFile | Out-Null
            $linkCount = (fsutil hardlink list $ntFile 2>$null | Measure-Object -Line).Lines
            Write-Host "FIXED    : $($_.Name)  (hash mismatch repaired -- hard link created, count=$linkCount)" -ForegroundColor Yellow
            $fixed++
        } else {
            Write-Host "DESYNC   : $($_.Name)" -ForegroundColor Red
            $desyncs++
        }
    }
}

Write-Host ""
Write-Host "=== SUMMARY ===" -ForegroundColor Cyan
Write-Host "OK      : $ok"       -ForegroundColor Green
Write-Host "DESYNC  : $desyncs"  -ForegroundColor $(if ($desyncs  -gt 0) { "Red" } else { "Green" })
Write-Host "MISSING : $missing"  -ForegroundColor $(if ($missing  -gt 0) { "Red" } else { "Green" })
Write-Host "FIXED   : $fixed"    -ForegroundColor $(if ($fixed    -gt 0) { "Yellow" } else { "Green" })
Write-Host "SKIPPED : $skipped"  -ForegroundColor DarkGray

if (($desyncs + $missing) -eq 0) {
    Write-Host ""
    Write-Host "PASS -- All deployable source files match NinjaTrader. No stale deploy risk." -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "FAIL -- Run: powershell -File scripts\verify_links.ps1 -Fix" -ForegroundColor Red
    Write-Host "       Then F5 compile in NinjaTrader to pick up the updated source." -ForegroundColor Red
    exit 1
}
