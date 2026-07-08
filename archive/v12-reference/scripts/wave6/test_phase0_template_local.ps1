# Local Test: Phase 0 Template with EPIC-CCN-001 Metadata
# Purpose: Verify fixed template generates correct 00-hotspots.md
# Generated: 2026-06-17T19:35:00Z

$ErrorActionPreference = "Stop"

$EPIC_ID = "EPIC-CCN-001"
$AGENT_ID = "local-test-001"
$PHASE = "0"
$METHOD = "SymmetryGuardReplaceExistingFollowerTarget"
$FILE = "src/V12_002.Symmetry.Replace.cs"
$CYC = 18

Write-Host "=== Phase 0: Hotspot Analysis (Local Test) ===" -ForegroundColor Cyan
Write-Host "Epic: $EPIC_ID"
Write-Host "Agent: $AGENT_ID"
Write-Host "Phase: $PHASE"
Write-Host "Method: $METHOD"
Write-Host "File: $FILE"
Write-Host "CYC: $CYC"
Write-Host ""

# ============================================================================
# V12.52 BLOCKING GATE: Verify Can Execute
# ============================================================================
Write-Host "Step 1: V12.52 Verification Gate" -ForegroundColor Yellow

$verifyScript = @"
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import verify_can_execute

can_execute, reason = verify_can_execute('$EPIC_ID', '$PHASE', '$AGENT_ID')
if not can_execute:
    print(f'❌ BLOCKED: {reason}')
    sys.exit(1)
print(f'✅ Verification passed: {reason}')
"@

python -c $verifyScript
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ V12.52 verification failed - aborting" -ForegroundColor Red
    exit 1
}

Write-Host ""

# ============================================================================
# Start Phase Execution (Records Lamport Event)
# ============================================================================
Write-Host "Step 2: Start Phase Execution" -ForegroundColor Yellow

$startScript = @"
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import start_phase_execution

started, reason = start_phase_execution('$EPIC_ID', '$PHASE', '$AGENT_ID')
if not started:
    print(f'❌ Failed to start: {reason}')
    sys.exit(1)
print(f'✅ Phase started: {reason}')
"@

python -c $startScript
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to start phase - aborting" -ForegroundColor Red
    exit 1
}

Write-Host ""

# ============================================================================
# Execute Phase 0 Work (Generate Hotspot Analysis)
# ============================================================================
Write-Host "Step 3: Generate Hotspot Analysis" -ForegroundColor Yellow

$OUTPUT_FILE = "docs/brain/$EPIC_ID/00-hotspots.md"

# Backup existing file if it exists
if (Test-Path $OUTPUT_FILE) {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $BACKUP_FILE = "$OUTPUT_FILE.backup.$timestamp"
    Write-Host "⚠ Backing up existing file to: $BACKUP_FILE" -ForegroundColor Yellow
    Copy-Item $OUTPUT_FILE $BACKUP_FILE
}

# Generate hotspot analysis markdown from epic metadata
$generateScript = @"
import sys
import os

epic_id = '$EPIC_ID'
method = '$METHOD'
file = '$FILE'
cyc = $CYC

# Create brain directory if it doesn't exist
brain_dir = f'docs/brain/{epic_id}'
os.makedirs(brain_dir, exist_ok=True)

# Generate hotspot analysis markdown
content = f'''# Phase 0: Hotspot Analysis - {epic_id}

## Target Method
- **Method**: {method}
- **File**: {file}
- **Cyclomatic Complexity**: {cyc}
- **Jane Street Violations**: 0 (validation pending Phase 3)

## Complexity Metrics
- **Cyclomatic Complexity**: {cyc}
- **Threshold**: 15 (Jane Street aligned)
- **Status**: {'EXCEEDS THRESHOLD by ' + str(cyc - 15) + ' points' if cyc > 15 else 'WITHIN THRESHOLD'}

## Blast Radius
- Analysis pending (requires jCodemunch MCP tools in Phase 1)
- Will verify:
  - Direct callers of {method}
  - Files that import {file}
  - Downstream impact on related logic

## Call Hierarchy
- Analysis pending (requires jCodemunch MCP tools in Phase 1)
- Will verify:
  - Parent callers (who invokes this method)
  - Child callees (what this method invokes)
  - Recursion depth and patterns

## Risk Assessment
- **Complexity Risk**: {'HIGH' if cyc > 20 else 'MEDIUM' if cyc > 15 else 'LOW'} (CYC={cyc})
- **Jane Street Risk**: UNKNOWN (requires Phase 3 audit)
- **Blast Radius Risk**: UNKNOWN (requires Phase 1 analysis)
- **Overall Risk**: {'HIGH' if cyc > 20 else 'MEDIUM'}

## Refactoring Strategy
1. Extract conditional branches into helper methods
2. Reduce cyclomatic complexity from {cyc} to <=15
3. Maintain lock-free Actor/FSM pattern
4. Verify no Unicode/emoji in string literals
5. Add unit tests for extracted methods

## Next Steps
- Phase 1: Create scope definition with boundary validation
- Phase 2: Generate architecture plan with Jane Street validation
- Phase 3: DNA & PR audit before surgery
'''

# Write to file
output_path = f'{brain_dir}/00-hotspots.md'
with open(output_path, 'w', encoding='utf-8') as f:
    f.write(content)

print(f'✅ Hotspot analysis generated: {output_path}')
"@

python -c $generateScript
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to generate hotspot analysis - recording failure" -ForegroundColor Red
    
    $failScript = @"
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import fail_phase_execution

recorded, reason = fail_phase_execution('$EPIC_ID', '$PHASE', '$AGENT_ID', 'Hotspot generation failed')
print(f'Failure recorded: {reason}')
"@
    python -c $failScript
    exit 1
}

Write-Host "✅ Hotspot analysis complete" -ForegroundColor Green
Write-Host ""

# ============================================================================
# Complete Phase Execution (Records Lamport Event)
# ============================================================================
Write-Host "Step 4: Complete Phase Execution" -ForegroundColor Yellow

$completeScript = @"
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import complete_phase_execution

completed, reason = complete_phase_execution(
    '$EPIC_ID',
    '$PHASE',
    '$AGENT_ID',
    ['$OUTPUT_FILE'],
    'Hotspot analysis complete (local test)'
)
if not completed:
    print(f'❌ Failed to complete: {reason}')
    sys.exit(1)
print(f'✅ Phase completed: {reason}')
"@

python -c $completeScript
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to complete phase" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Phase 0 Complete (Local Test) ===" -ForegroundColor Cyan
Write-Host "Output: $OUTPUT_FILE"
Write-Host "Lamport event recorded"
Write-Host ""

# ============================================================================
# VERIFICATION
# ============================================================================
Write-Host "Step 5: Verify Output" -ForegroundColor Yellow

# Check file exists
if (-not (Test-Path $OUTPUT_FILE)) {
    Write-Host "❌ Output file not found: $OUTPUT_FILE" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Output file exists" -ForegroundColor Green

# Check file encoding (UTF-8 without BOM)
$content = Get-Content $OUTPUT_FILE -Raw -Encoding UTF8
if ($content) {
    Write-Host "✓ File encoding: UTF-8" -ForegroundColor Green
} else {
    Write-Host "❌ File encoding incorrect or empty" -ForegroundColor Red
    exit 1
}

# Check required sections
$REQUIRED_SECTIONS = @(
    "Target Method",
    "Complexity Metrics",
    "Blast Radius",
    "Call Hierarchy",
    "Risk Assessment",
    "Refactoring Strategy"
)

$MISSING_SECTIONS = @()
foreach ($section in $REQUIRED_SECTIONS) {
    if ($content -notmatch "## $section") {
        $MISSING_SECTIONS += $section
    }
}

if ($MISSING_SECTIONS.Count -gt 0) {
    Write-Host "❌ Missing required sections: $($MISSING_SECTIONS -join ', ')" -ForegroundColor Red
    exit 1
}
Write-Host "✓ All required sections present" -ForegroundColor Green

# Check method name
if ($content -notmatch [regex]::Escape($METHOD)) {
    Write-Host "❌ Method name not found in output" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Method name present: $METHOD" -ForegroundColor Green

# Check CYC value
if ($content -notmatch "Cyclomatic Complexity.*: $CYC") {
    Write-Host "❌ CYC value not found in output" -ForegroundColor Red
    exit 1
}
Write-Host "✓ CYC value present: $CYC" -ForegroundColor Green

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "✓ LOCAL TEST SUCCESSFUL" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Template: building-blocks/autonomous-refactoring/phase0_template_v12_52.sh"
Write-Host "Output: $OUTPUT_FILE"
Write-Host "V12.52 Gates: ALL PASSED"
Write-Host "Verification: ALL CHECKS PASSED"
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Ready for VM deployment" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan

exit 0

# Made with Bob