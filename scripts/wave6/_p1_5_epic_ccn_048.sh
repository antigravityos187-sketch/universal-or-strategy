#!/bin/bash
# V12.52 Phase 1.5 Template: Scope Boundary Validation
# Epic: EPIC-CCN-048
# Agent: sammy96
# Dependencies: Phase 1 (00-scope.md)
# Output: docs/brain/EPIC-CCN-048/01-scope-boundary.md

set -euo pipefail

EPIC_ID="EPIC-CCN-048"
sammy96="sammy96"
PHASE="1.5"

echo "=========================================="
echo "V12.52 Phase 1.5: Scope Boundary Validation"
echo "Epic: $EPIC_ID"
echo "Agent: $sammy96"
echo "=========================================="

# Step 1: V12.52 Verification Gate (Triple Verification)
echo ""
echo "Step 1: V12.52 Verification Gate"
echo "-----------------------------------"

python3 -c "
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import verify_can_execute

can_execute, reason = verify_can_execute('$EPIC_ID', '$PHASE', '$sammy96')
if not can_execute:
    print(f'❌ BLOCKED: {reason}')
    sys.exit(1)
print('✅ V12.52 verification passed')
"

if [ $? -ne 0 ]; then
    echo "❌ V12.52 verification failed - aborting"
    exit 1
fi

# Step 2: Start Phase Execution (Record Lamport Event)
echo ""
echo "Step 2: Starting Phase 1.5 Execution"
echo "-----------------------------------"

python3 -c "
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import start_phase_execution

started, reason = start_phase_execution('$EPIC_ID', '$PHASE', '$sammy96')
if not started:
    print(f'❌ Failed to start: {reason}')
    sys.exit(1)
print('✅ Phase 1.5 started (Lamport event recorded)')
"

if [ $? -ne 0 ]; then
    exit 1
fi

# Step 3: Execute Phase 1.5 Work (Scope Boundary Validation)
echo ""
echo "Step 3: Executing Phase 1.5 Work"
echo "-----------------------------------"

# Read scope definition
SCOPE_FILE="docs/brain/$EPIC_ID/00-scope.md"
if [ ! -f "$SCOPE_FILE" ]; then
    ERROR_MSG="Input file not found: $SCOPE_FILE"
    echo "❌ $ERROR_MSG"
    
    python3 -c "
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import fail_phase_execution

recorded, reason = fail_phase_execution('$EPIC_ID', '$PHASE', '$sammy96', '$ERROR_MSG')
print(f'Failure recorded: {reason}')
"
    exit 1
fi

# Validate scope boundary using Bob CLI (v12-phase1-5-boundary mode)
echo "Validating scope boundary for $EPIC_ID..."
OUTPUT_FILE="docs/brain/$EPIC_ID/01-scope-boundary.md"

# Bob CLI command for boundary validation (v1.0.4 syntax)
# CRITICAL: Use full path ~/.npm-global/bin/bob (not in PATH)
# Export API key from bashrc
export BOBSHELL_API_KEY=$(grep 'export BOBSHELL_API_KEY' ~/.bashrc | cut -d'=' -f2)
~/.npm-global/bin/bob \
    --chat-mode v12-phase1-5-boundary \
    --yolo \
    "Validate scope boundary for $EPIC_ID based on scope definition in $SCOPE_FILE. MANDATORY: Verify single-method boundary only. REJECT if scope exceeds single method. Output: $OUTPUT_FILE" \
    2>&1 | tee "logs/wave6/phase1_5/$EPIC_ID.log"

BOB_EXIT_CODE=${PIPESTATUS[0]}

if [ $BOB_EXIT_CODE -ne 0 ]; then
    ERROR_MSG="Bob CLI failed with exit code $BOB_EXIT_CODE"
    echo "❌ $ERROR_MSG"
    
    python3 -c "
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import fail_phase_execution

recorded, reason = fail_phase_execution('$EPIC_ID', '$PHASE', '$sammy96', '$ERROR_MSG')
print(f'Failure recorded: {reason}')
"
    exit 1
fi

# Verify output file was created
if [ ! -f "$OUTPUT_FILE" ]; then
    ERROR_MSG="Output file not created: $OUTPUT_FILE"
    echo "❌ $ERROR_MSG"
    
    python3 -c "
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import fail_phase_execution

recorded, reason = fail_phase_execution('$EPIC_ID', '$PHASE', '$sammy96', '$ERROR_MSG')
print(f'Failure recorded: {reason}')
"
    exit 1
fi

# Verify output file is non-empty
if [ ! -s "$OUTPUT_FILE" ]; then
    ERROR_MSG="Output file is empty: $OUTPUT_FILE"
    echo "❌ $ERROR_MSG"
    
    python3 -c "
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import fail_phase_execution

recorded, reason = fail_phase_execution('$EPIC_ID', '$PHASE', '$sammy96', '$ERROR_MSG')
print(f'Failure recorded: {reason}')
"
    exit 1
fi

echo "✅ Scope boundary validation complete: $OUTPUT_FILE"

# Step 4: Complete Phase Execution (Record Lamport Event)
echo ""
echo "Step 4: Completing Phase 1.5 Execution"
echo "-----------------------------------"

python3 -c "
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import complete_phase_execution

completed, reason = complete_phase_execution(
    '$EPIC_ID',
    '$PHASE',
    '$sammy96',
    ['$OUTPUT_FILE']
)
if not completed:
    print(f'❌ Failed to complete: {reason}')
    sys.exit(1)
print('✅ Phase 1.5 completed (Lamport event recorded)')
"

if [ $? -ne 0 ]; then
    exit 1
fi

echo ""
echo "=========================================="
echo "✅ Phase 1.5 SUCCESS: $EPIC_ID"
echo "Output: $OUTPUT_FILE"
echo "=========================================="

# Made with Bob