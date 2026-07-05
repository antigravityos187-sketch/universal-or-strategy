param (
    [string]$SrcPath = "C:\WSGTA\universal-or-strategy\src",
    [string]$NtPath = "C:\Users\Mohammed Khalid\Documents\NinjaTrader 8\bin\Custom\Strategies"
)

Write-Host "=== HARD LINK INTEGRITY AUDIT ===" -ForegroundColor Cyan
Write-Host "SRC : $SrcPath"
Write-Host "NT8 : $NtPath"
Write-Host ""

$desyncs = 0
$missing = 0
$ok = 0

# Files that live in src/ but are NOT NT8 strategy files (xUnit tests, pure-logic mirrors).
# These must never be hard-linked to NT8 -- exclude from audit.
$NtExcluded = @(
    "W7_061_SubmitAndRegisterTests.cs"
)

Get-ChildItem $SrcPath -Filter "*.cs" | Where-Object { $NtExcluded -notcontains $_.Name } | ForEach-Object {
    $srcFile = $_.FullName
    $ntFile  = Join-Path $NtPath $_.Name

    if (-not (Test-Path $ntFile)) {
        Write-Host "MISSING  : $($_.Name)" -ForegroundColor Red
        $missing++
        return
    }

    $srcHash = (Get-FileHash $srcFile -Algorithm MD5).Hash
    $ntHash  = (Get-FileHash $ntFile  -Algorithm MD5).Hash

    if ($srcHash -eq $ntHash) {
        Write-Host "OK       : $($_.Name)" -ForegroundColor Green
        $ok++
    } else {
        Write-Host "DESYNC   : $($_.Name)  [src=$srcHash] [nt=$ntHash]" -ForegroundColor Red
        $desyncs++
    }
}

Write-Host ""
Write-Host "=== SUMMARY ===" -ForegroundColor Cyan
Write-Host "OK      : $ok"      -ForegroundColor Green
Write-Host "DESYNC  : $desyncs" -ForegroundColor $(if ($desyncs -gt 0) { "Red" } else { "Green" })
Write-Host "MISSING : $missing" -ForegroundColor $(if ($missing -gt 0) { "Red" } else { "Green" })

if (($desyncs + $missing) -eq 0) {
    Write-Host ""
    Write-Host "PASS -- All source files match NinjaTrader. No stale DLL risk." -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "FAIL -- Run deploy-sync.ps1 immediately then F5 compile." -ForegroundColor Red
    exit 1
}
