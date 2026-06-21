# Fix Wave 7 Bob CLI Pattern
# Adds temp file + command substitution pattern to all 7 Bob CLI templates
# Version: 1.0
# Date: 2026-06-21

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Wave 7 Bob CLI Pattern Fix" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$templatesDir = "building-blocks/wave7"
$fixedCount = 0
$errors = @()

# Template configurations
$templates = @(
    @{
        File = "phase1_template_wave7.sh"
        Phase = "1"
        Mode = "v12-phase1-scope"
        OldPattern = '~/.npm-global/bin/bob \s+--chat-mode v12-phase1-scope \s+--yolo \s+"Define extraction scope.*?" \s+2>&1'
        MessageVar = 'Define extraction scope for $EPIC_ID based on hotspot analysis in $HOTSPOT_FILE. Output: $OUTPUT_FILE'
    },
    @{
        File = "phase1_5_template_wave7.sh"
        Phase = "1.5"
        Mode = "v12-phase1-5-boundary"
        OldPattern = 'bob --yolo --chat-mode v12-phase1-5-boundary "\$\(cat /tmp/phase1_5_msg_\$EPIC_ID\.txt\)"'
        MessageVar = 'Validate scope boundary for $EPIC_ID based on scope definition in $SCOPE_FILE. Ensure no scope creep. Output: $OUTPUT_FILE'
    },
    @{
        File = "phase2_template_wave7.sh"
        Phase = "2"
        Mode = "plan"
        OldPattern = 'bob --mode plan --task "Create detailed extraction architecture.*?" \s+--context'
        MessageVar = 'Create detailed extraction architecture for $EPIC_ID based on validated scope in $BOUNDARY_FILE. Query Jane Street KB for extraction patterns. Output: $OUTPUT_FILE'
    },
    @{
        File = "phase4_template_wave7.sh"
        Phase = "4"
        Mode = "plan"
        OldPattern = 'bob --mode plan --task "Generate surgical extraction tickets.*?" \s+--context'
        MessageVar = 'Generate surgical extraction tickets for $EPIC_ID based on audit report in $AUDIT_FILE. Use jCodemunch to analyze method complexity. Each ticket must target CYC ≤8 (Jane Street strict). Output: $OUTPUT_FILE'
    },
    @{
        File = "phase5_template_wave7.sh"
        Phase = "5"
        Mode = "v12-engineer"
        OldPattern = 'bob --mode v12-engineer --yolo --task "Execute ticket.*?" \s+--context'
        MessageVar = 'Execute ticket $TICKET_ID for $EPIC_ID based on tickets file $TICKETS_FILE. Target CYC ≤8 (Jane Street strict). Generate xUnit tests. Output: $OUTPUT_FILE'
    },
    @{
        File = "phase5_v_template_wave7.sh"
        Phase = "5.V"
        Mode = "advanced"
        OldPattern = 'bob --mode advanced --task "Verify ticket.*?" \s+--context'
        MessageVar = 'Verify ticket $TICKET_ID execution for $EPIC_ID based on completion file $COMPLETION_FILE. Check: (1) compilation passes, (2) CYC ≤8 achieved, (3) only target method modified, (4) xUnit tests passing, (5) UTF-8 encoding. Use jCodemunch for complexity verification. Output: $OUTPUT_FILE'
    },
    @{
        File = "phase6_template_wave7.sh"
        Phase = "6"
        Mode = "advanced"
        OldPattern = 'bob --mode advanced --task "Perform final review.*?" \s+--context'
        MessageVar = 'Perform final review for $EPIC_ID. Verify all ticket verifications in docs/brain/$EPIC_ID/. Run Greptile audit (expect 0 P0/P1 issues). Generate completion report. Output: $OUTPUT_FILE'
    }
)

foreach ($template in $templates) {
    $filePath = Join-Path $templatesDir $template.File
    
    Write-Host "Processing: $($template.File)" -ForegroundColor Yellow
    
    if (-not (Test-Path $filePath)) {
        $errors += "File not found: $filePath"
        Write-Host "  ✗ File not found" -ForegroundColor Red
        continue
    }
    
    try {
        $content = Get-Content $filePath -Raw
        
        # Determine the correct pattern based on phase
        if ($template.Phase -eq "1") {
            # Phase 1: Has full path, needs temp file pattern
            $newPattern = @"
# Step 3.1: Create message file for Bob CLI
cat > /tmp/phase1_msg_`$EPIC_ID.txt << 'EOFMSG'
$($template.MessageVar)
EOFMSG

# Step 3.2: Execute Bob CLI with command substitution
~/.npm-global/bin/bob --yolo --chat-mode $($template.Mode) "`$(cat /tmp/phase1_msg_`$EPIC_ID.txt)" 2>&1 | tee "logs/wave6/phase1/`$EPIC_ID.log"
"@
            
            # Replace the old inline pattern
            $content = $content -replace '(?s)# Bob CLI command for scope definition.*?2>&1 \| tee "logs/wave6/phase1/\$EPIC_ID\.log"', $newPattern
            
        } elseif ($template.Phase -eq "1.5") {
            # Phase 1.5: Has temp file pattern, needs full path
            $content = $content -replace 'bob --yolo --chat-mode', '~/.npm-global/bin/bob --yolo --chat-mode'
            
        } else {
            # Phases 2, 4, 5, 5.V, 6: Need both full path and temp file pattern
            $phaseNum = $template.Phase -replace '\.', '_'
            $logDir = switch ($template.Phase) {
                "2" { "phase2" }
                "4" { "phase4" }
                "5" { "phase5" }
                "5.V" { "phase5_v" }
                "6" { "phase6" }
            }
            
            $modeFlag = if ($template.Mode -eq "v12-engineer") { "--mode $($template.Mode) --yolo" } else { "--mode $($template.Mode)" }
            
            $newPattern = @"
# Step 3.1: Create message file for Bob CLI
cat > /tmp/phase${phaseNum}_msg_`$EPIC_ID.txt << 'EOFMSG'
$($template.MessageVar)
EOFMSG

# Step 3.2: Execute Bob CLI with command substitution
~/.npm-global/bin/bob $modeFlag "`$(cat /tmp/phase${phaseNum}_msg_`$EPIC_ID.txt)" 2>&1 | tee "logs/wave6/$logDir/`$EPIC_ID.log"
"@
            
            # Replace the old inline pattern (more flexible regex)
            $content = $content -replace "(?s)# Bob CLI command.*?2>&1 \| tee ""logs/wave6/$logDir/\`$EPIC_ID\.log""", $newPattern
        }
        
        # Write the updated content
        Set-Content -Path $filePath -Value $content -NoNewline -Encoding UTF8
        
        Write-Host "  ✓ Fixed successfully" -ForegroundColor Green
        $fixedCount++
        
    } catch {
        $errors += "Error processing $($template.File): $_"
        Write-Host "  ✗ Error: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Fixed: $fixedCount / $($templates.Count)" -ForegroundColor $(if ($fixedCount -eq $templates.Count) { "Green" } else { "Yellow" })

if ($errors.Count -gt 0) {
    Write-Host ""
    Write-Host "Errors:" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host "  - $error" -ForegroundColor Red
    }
    exit 1
}

Write-Host ""
Write-Host "✓ All templates fixed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next step: Run verification script" -ForegroundColor Cyan
Write-Host "  python scripts/verify_wave7_templates.py" -ForegroundColor White

# Made with Bob
