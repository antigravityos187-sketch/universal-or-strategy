# Fix Phase 3 scripts: Add missing 'cd' command
# Root Cause: Bob Shell can't find .bob/mcp.linux.json without cd to project dir

Write-Host "Fixing Phase 3 scripts: Adding 'cd' command..." -ForegroundColor Cyan

$scriptsFixed = 0
$scriptsAlreadyFixed = 0

Get-ChildItem "scripts/wave4/_p3_*.sh" | ForEach-Object {
    $script = $_.FullName
    $content = Get-Content $script -Raw
    
    # Check if script already has 'cd' command
    if ($content -notmatch "cd /home/malhitticrypto/universal-or-strategy") {
        Write-Host "Fixing: $($_.Name)" -ForegroundColor Yellow
        
        # Insert 'cd' command after 'set -e'
        $newContent = $content -replace "(set -e)", "`$1`ncd /home/malhitticrypto/universal-or-strategy"
        
        # Write back to file
        $newContent | Set-Content $script -NoNewline
        $scriptsFixed++
    } else {
        Write-Host "Already fixed: $($_.Name)" -ForegroundColor Green
        $scriptsAlreadyFixed++
    }
}

Write-Host ""
Write-Host "✅ Phase 3 scripts fix complete!" -ForegroundColor Green
Write-Host "   Fixed: $scriptsFixed scripts" -ForegroundColor Cyan
Write-Host "   Already fixed: $scriptsAlreadyFixed scripts" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Upload fixed scripts to VM"
Write-Host "2. Re-run pilot test with EPIC-CCN-001, EPIC-CCN-002"
Write-Host "3. If pilot succeeds, re-run 62 failed epics"

# Made with Bob
