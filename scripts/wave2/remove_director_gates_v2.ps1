# Remove Director Approval Gates from Autonomous Refactor Commands
# Created: 2026-06-13
# Purpose: Enable true autonomous execution by removing manual approval gates

$ErrorActionPreference = "Stop"

Write-Host "=== Director Gate Removal Script ===" -ForegroundColor Cyan
Write-Host ""

# Commands used in autonomous-refactor workflow
$commands = @(
    "epic-intake",
    "epic-scope-boundary",
    "epic-plan",
    "epic-scan",
    "epic-tickets",
    "epic-validate",
    "epic-verify-ticket",
    "epic-review-final"
)

$totalFixed = 0
$backupDir = ".bob/commands/backups/gate-removal-$(Get-Date -Format 'yyyy-MM-dd-HHmmss')"

# Create backup directory
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
Write-Host "Backup directory: $backupDir" -ForegroundColor Green
Write-Host ""

foreach ($cmd in $commands) {
    $file = ".bob/commands/$cmd.md"
    
    if (-not (Test-Path $file)) {
        Write-Host "SKIP: $file not found" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "Processing: $cmd.md" -ForegroundColor White
    
    # Backup original
    Copy-Item $file "$backupDir/$cmd.md"
    
    # Read content
    $content = Get-Content $file -Raw
    $originalContent = $content
    
    # Remove gate sections and references
    $content = $content -replace '> You produce .* then STOP for Director approval\.', '> You produce planning artifacts then complete the phase.'
    $content = $content -replace '> You STOP and wait for Director confirmation before proceeding.*', '> You complete the phase and update the manifest.'
    $content = $content -replace '(?s)## !! .*?-GATE !!.*?(?=---|$)', ''
    $content = $content -replace 'Output: "\[.*?-GATE\].*?Awaiting.*?"', 'Output: "[PHASE-COMPLETE] Phase artifacts written and manifest updated."'
    $content = $content -replace '- Scope changes require Director approval\r?\n', ''
    $content = $content -replace '\*\*Gate:\*\* Wait for Director confirmation\.\r?\n', ''
    $content = $content -replace '(\r?\n){4,}', "`n`n`n"
    
    # Write if changed
    if ($content -ne $originalContent) {
        Set-Content -Path $file -Value $content -NoNewline
        Write-Host "  ✓ Gates removed" -ForegroundColor Green
        $totalFixed++
    } else {
        Write-Host "  ℹ No gates found" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Commands modified: $totalFixed / $($commands.Count)" -ForegroundColor Green
Write-Host ""

if ($totalFixed -gt 0) {
    Write-Host "✅ Success! Director gates removed." -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Review: git diff .bob/commands/" -ForegroundColor White
    Write-Host "2. Test: /epic-intake EPIC-TEST-001 'Test'" -ForegroundColor White
    Write-Host "3. Regenerate Wave 2 scripts" -ForegroundColor White
    Write-Host "4. Deploy and resume blocked epics" -ForegroundColor White
}

# Made with Bob
