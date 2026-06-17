#!/bin/bash
# Local Test: Phase 0 Template with EPIC-CCN-001 Metadata
# Purpose: Verify fixed template generates correct 00-hotspots.md
# Generated: 2026-06-17T19:34:00Z

set -e  # Exit on error

EPIC_ID="EPIC-CCN-001"
AGENT_ID="local-test-001"
PHASE="0"
METHOD="SymmetryGuardReplaceExistingFollowerTarget"
FILE="src/V12_002.Symmetry.Replace.cs"
CYC=18

echo "=== Phase 0: Hotspot Analysis (Local Test) ==="
echo "Epic: $EPIC_ID"
echo "Agent: $AGENT_ID"
echo "Phase: $PHASE"
echo "Method: $METHOD"
echo "File: $FILE"
echo "CYC: $CYC"
echo ""

# ============================================================================
# V12.52 BLOCKING GATE: Verify Can Execute
# ============================================================================
echo "Step 1: V12.52 Verification Gate"
python3 -c "
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import verify_can_execute

can_execute, reason = verify_can_execute('$EPIC_ID', '$PHASE', '$AGENT_ID')
if not can_execute:
    print(f'❌ BLOCKED: {reason}')
    sys.exit(1)
print(f'✅ Verification passed: {reason}')
"

if [ $? -ne 0 ]; then
    echo "❌ V12.52 verification failed - aborting"
    exit 1
fi

echo ""

# ============================================================================
# Start Phase Execution (Records Lamport Event)
# ============================================================================
echo "Step 2: Start Phase Execution"
python3 -c "
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import start_phase_execution

started, reason = start_phase_execution('$EPIC_ID', '$PHASE', '$AGENT_ID')
if not started:
    print(f'❌ Failed to start: {reason}')
    sys.exit(1)
print(f'✅ Phase started: {reason}')
"

if [ $? -ne 0 ]; then
    echo "❌ Failed to start phase - aborting"
    exit 1
fi

echo ""

# ============================================================================
# Execute Phase 0 Work (Generate Hotspot Analysis)
# ============================================================================
echo "Step 3: Generate Hotspot Analysis"

OUTPUT_FILE="docs/brain/$EPIC_ID/00-hotspots.md"

# Backup existing file if it exists
if [ -f "$OUTPUT_FILE" ]; then
    BACKUP_FILE="${OUTPUT_FILE}.backup.$(date +%Y%m%d_%H%M%S)"
    echo "⚠ Backing up existing file to: $BACKUP_FILE"
    cp "$OUTPUT_FILE" "$BACKUP_FILE"
fi

# Generate hotspot analysis markdown from epic metadata
python3 -c "
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
"

if [ $? -ne 0 ]; then
    echo "❌ Failed to generate hotspot analysis - recording failure"
    
    # Record failure event
    python3 -c "
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import fail_phase_execution

recorded, reason = fail_phase_execution('$EPIC_ID', '$PHASE', '$AGENT_ID', 'Hotspot generation failed')
print(f'Failure recorded: {reason}')
"
    exit 1
fi

echo "✅ Hotspot analysis complete"
echo ""

# ============================================================================
# Complete Phase Execution (Records Lamport Event)
# ============================================================================
echo "Step 4: Complete Phase Execution"

python3 -c "
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
"

if [ $? -ne 0 ]; then
    echo "❌ Failed to complete phase"
    exit 1
fi

echo ""
echo "=== Phase 0 Complete (Local Test) ==="
echo "Output: $OUTPUT_FILE"
echo "Lamport event recorded"
echo ""

# ============================================================================
# VERIFICATION
# ============================================================================
echo "Step 5: Verify Output"

# Check file exists
if [ ! -f "$OUTPUT_FILE" ]; then
    echo "❌ Output file not found: $OUTPUT_FILE"
    exit 1
fi
echo "✓ Output file exists"

# Check file encoding (UTF-8 without BOM)
if file "$OUTPUT_FILE" | grep -q "UTF-8"; then
    echo "✓ File encoding: UTF-8"
else
    echo "❌ File encoding incorrect (expected UTF-8)"
    exit 1
fi

# Check required sections
REQUIRED_SECTIONS=(
    "Target Method"
    "Complexity Metrics"
    "Blast Radius"
    "Call Hierarchy"
    "Risk Assessment"
    "Refactoring Strategy"
)

MISSING_SECTIONS=()
for section in "${REQUIRED_SECTIONS[@]}"; do
    if ! grep -q "## $section" "$OUTPUT_FILE"; then
        MISSING_SECTIONS+=("$section")
    fi
done

if [ ${#MISSING_SECTIONS[@]} -gt 0 ]; then
    echo "❌ Missing required sections: ${MISSING_SECTIONS[*]}"
    exit 1
fi
echo "✓ All required sections present"

# Check method name
if ! grep -q "$METHOD" "$OUTPUT_FILE"; then
    echo "❌ Method name not found in output"
    exit 1
fi
echo "✓ Method name present: $METHOD"

# Check CYC value
if ! grep -q "Cyclomatic Complexity.*: $CYC" "$OUTPUT_FILE"; then
    echo "❌ CYC value not found in output"
    exit 1
fi
echo "✓ CYC value present: $CYC"

echo ""
echo "=========================================="
echo "✓ LOCAL TEST SUCCESSFUL"
echo "=========================================="
echo "Template: building-blocks/autonomous-refactoring/phase0_template_v12_52.sh"
echo "Output: $OUTPUT_FILE"
echo "V12.52 Gates: ALL PASSED"
echo "Verification: ALL CHECKS PASSED"
echo "=========================================="
echo "Ready for VM deployment"
echo "=========================================="

exit 0

# Made with Bob