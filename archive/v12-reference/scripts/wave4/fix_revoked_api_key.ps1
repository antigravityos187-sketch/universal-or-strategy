# Fix Revoked API Key in Phase 6 Scripts
# Replaces revoked API key with working key from EPIC-CCN-001

$REVOKED_KEY = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
$WORKING_KEY = "bob_prod_bob-admin_yN7cbWSG9B926LkYPex4pXBGgTbZdN7Xg1ihASxzGdFGz7N8Z5WWDiqeWGUvsXiTWMzag9Hur9EA53BtXQRr2E4_4Z2YTW686zBchNH8KMgN69E3YGDzeRYcWMYxtKkxooeR"

$scripts = @(
    "scripts/wave4/_p6_015.sh",
    "scripts/wave4/_p6_030.sh",
    "scripts/wave4/_p6_045.sh",
    "scripts/wave4/_p6_060.sh",
    "scripts/wave4/_p6_075.sh"
)

Write-Host "=== API Key Replacement ===" -ForegroundColor Cyan
Write-Host "Revoked key (first 30 chars): $($REVOKED_KEY.Substring(0,30))..." -ForegroundColor Red
Write-Host "Working key (first 30 chars): $($WORKING_KEY.Substring(0,30))..." -ForegroundColor Green
Write-Host ""

$fixed = 0
foreach ($script in $scripts) {
    if (Test-Path $script) {
        Write-Host "Processing: $script" -ForegroundColor Yellow
        $content = Get-Content $script -Raw
        
        if ($content -match [regex]::Escape($REVOKED_KEY)) {
            $content = $content -replace [regex]::Escape($REVOKED_KEY), $WORKING_KEY
            Set-Content $script -Value $content -NoNewline
            Write-Host "  [FIXED] API key replaced" -ForegroundColor Green
            $fixed++
        } else {
            Write-Host "  [SKIP] Revoked key not found (already fixed or different key)" -ForegroundColor Gray
        }
    } else {
        Write-Host "  [ERROR] File not found: $script" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Scripts fixed: $fixed/5" -ForegroundColor $(if ($fixed -eq 5) { "Green" } else { "Yellow" })
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Upload fixed scripts to VM (gcloud compute scp)" -ForegroundColor White
Write-Host "2. Verify upload count matches (V12.27 Protocol)" -ForegroundColor White
Write-Host "3. Re-execute Phase 6 for affected epics" -ForegroundColor White

# Made with Bob
