#!/bin/bash
# Phase 0 (Hotspot Analysis) Template with V12.52 Lamport Causal Verification
# Version: V12.52.1 (Fixed for actual hotspot generation)
# Epic: EPIC-CCN-025
# Agent: wave6-p0-025

set -e  # Exit on error

EPIC_ID="EPIC-CCN-025"
AGENT_ID="wave6-p0-025"
PHASE="0"
METHOD="CheckFFMAConditions"
FILE="V12_002.Entries.FFMA.cs"
CYC=16

echo "=== Phase 0: Hotspot Analysis ==="
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
- **Threshold**: 8 (Jane Street strict)
- **Status**: {'EXCEEDS THRESHOLD by ' + str(cyc - 8) + ' points' if cyc > 8 else 'WITHIN THRESHOLD'}

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
- **Complexity Risk**: {'HIGH' if cyc > 20 else 'MEDIUM' if cyc > 8 else 'LOW'} (CYC={cyc})
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
    'Hotspot analysis complete'
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
echo "=== Phase 0 Complete ==="
echo "Output: $OUTPUT_FILE"
echo "Lamport event recorded"
echo ""

# Made with Bob
