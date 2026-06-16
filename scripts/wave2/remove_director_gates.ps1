# Remove Director Approval Gates from Autonomous Refactor Commands
# Created: 2026-06-13
# Purpose: Enable true autonomous execution by removing manual approval gates

$ErrorActionPreference = "Stop"

Write-Host "=== Director Gate Removal Script ===" -ForegroundColor Cyan
Write-Host "Target: Commands used in autonomous-refactor workflow" -ForegroundColor Cyan
Write-Host ""

# Commands used in autonomous-refactor workflow (from /epic-run)
$commands = @(
    "epic-intake",           # Phase 0 & 1
    "epic-scope-boundary",   # Phase 1.5
    "epic-plan",             # Phase 2
    "epic-scan",             # Phase 3 (Sentinel audit)
    "epic-tickets",          # Phase 4
    "epic-validate",         # Phase 5 (per ticket)
    "epic-verify-ticket",    # Phase 5.V (per ticket)
    "epic-review-final"      # Phase 6
)

$totalFixed = 0
$backupDir = ".bob/commands/backups/gate-removal-$(Get-Date -Format 'yyyy-MM-dd-HHmmss')"

# Create backup directory
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
Write-Host "Created backup directory: $backupDir" -ForegroundColor Green
Write-Host ""

foreach ($cmd in $commands) {
    $file = ".bob/commands/$cmd.md"
    
    if (-not (Test-Path $file)) {
        Write-Host "⚠️  SKIP: $file not found" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "Processing: $file" -ForegroundColor White
    
    # Backup original
    $backupFile = "$backupDir/$cmd.md"
    Copy-Item $file $backupFile
    Write-Host "  ✓ Backed up to: $backupFile" -ForegroundColor Gray
    
    # Read content
    $content = Get-Content $file -Raw
    $originalContent = $content
    
    # Pattern 1: Remove "STOP for Director approval" from role description
    $content = $content -replace '> You produce .* then STOP for Director approval\.', '> You produce planning artifacts then complete the phase.'
    
    # Pattern 2: Remove "Awaiting Director confirmation" from role description
    $content = $content -replace '> You STOP and wait for Director confirmation before proceeding.*', '> You complete the phase and update the manifest.'
    
    # Pattern 3: Remove entire gate sections (## !! GATE !! through end of section)
    # Match from "## !! [NAME]-GATE !!" to the next "---" or end of file
    $content = $content -replace '(?s)## !! .*?-GATE !!.*?(?=---|$)', ''
    
    # Pattern 4: Remove gate output messages
    $content = $content -replace 'Output: "\[.*?-GATE\].*?Awaiting.*?"', 'Output: "[PHASE-COMPLETE] Phase artifacts written and manifest updated."'
    
    # Pattern 5: Remove "Scope changes require Director approval" from philosophy
    $content = $content -replace '- Scope changes require Director approval\r?\n', ''
    
    # Pattern 6: Remove "Wait for Director confirmation" from pr-loop
    $content = $content -replace '\*\*Gate:\*\* Wait for Director confirmation\.\r?\n', ''
    
    # Pattern 7: Clean up multiple consecutive blank lines (max 2)
    $content = $content -replace '(\r?\n){4,}', "`n`n`n"
    
    # Check if changes were made
    if ($content -ne $originalContent) {
        # Write updated content
        Set-Content -Path $file -Value $content -NoNewline
        Write-Host "  ✓ Removed Director gates" -ForegroundColor Green
        $totalFixed++
    } else {
        Write-Host "  ℹ️  No gates found" -ForegroundColor Gray
    }
    
    Write-Host ""
}

Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Commands processed: $($commands.Count)" -ForegroundColor White
Write-Host "Commands modified: $totalFixed" -ForegroundColor Green
Write-Host "Backups saved to: $backupDir" -ForegroundColor Gray
Write-Host ""

if ($totalFixed -gt 0) {
    Write-Host "✅ Director gates removed from $totalFixed commands" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Review changes: git diff .bob/commands/" -ForegroundColor White
    Write-Host "2. Test on single epic: /epic-intake EPIC-TEST-001 'Test epic'" -ForegroundColor White
    Write-Host "3. If successful, regenerate Wave 2 scripts" -ForegroundColor White
    Write-Host "4. Deploy to VM and resume blocked epics" -ForegroundColor White
} else {
    Write-Host "ℹ️  No changes needed - gates already removed or not present" -ForegroundColor Gray
}

# Made with Bob
