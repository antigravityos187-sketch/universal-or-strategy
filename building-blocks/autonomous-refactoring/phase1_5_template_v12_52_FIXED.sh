#!/bin/bash
# V12.52 Phase 1.5 Template: Scope Boundary Validation
# Epic: {EPIC_ID}
# Agent: {AGENT_ID}
# Dependencies: Phase 1 (00-scope.md)
# Output: docs/brain/{EPIC_ID}/01-scope-boundary.md

set -euo pipefail

EPIC_ID="{EPIC_ID}"
AGENT_ID="{AGENT_ID}"
PHASE="1.5"

echo "=========================================="
echo "V12.52 Phase 1.5: Scope Boundary Validation"
echo "Epic: $EPIC_ID"
echo "Agent: $AGENT_ID"
echo "=========================================="

# Step 1: V12.52 Verification Gate (Triple Verification)
echo ""
echo "Step 1: V12.52 Verification Gate"
echo "-----------------------------------"

# Gate 1: Dependencies (Manifest)
echo "Gate 1: Checking dependencies (manifest)..."
python3 scripts/epic_manifest.py verify_dependencies "$EPIC_ID" "$PHASE"
if [ $? -ne 0 ]; then
    echo "❌ BLOCKED: Dependencies not satisfied (manifest)"
    echo "❌ V12.52 verification failed - aborting"
    exit 1
fi
echo "✅ Dependencies satisfied"

# Gate 2: Causal Verification (Lamport)
echo "Gate 2: Checking causal verification (Lamport)..."
python3 scripts/epic_manifest.py verify_can_execute "$EPIC_ID" "$PHASE"
if [ $? -ne 0 ]; then
    echo "❌ BLOCKED: Causal verification failed"
    echo "❌ V12.52 verification failed - aborting"
    exit 1
fi
echo "✅ Causal verification passed"

# Gate 3: Filesystem State (Dual Verification)
echo "Gate 3: Checking filesystem state..."
python3 scripts/epic_manifest.py verify_filesystem_state "$EPIC_ID" "$PHASE"
if [ $? -ne 0 ]; then
    echo "❌ BLOCKED: State mismatch (filesystem)"
    echo "❌ V12.52 verification failed - aborting"
    exit 1
fi
echo "✅ Filesystem state verified"

echo "✅ V12.52 verification passed - proceeding with Phase 1.5"

# Step 2: Start Phase Execution (Record Lamport Event)
echo ""
echo "Step 2: Starting Phase 1.5 Execution"
echo "-----------------------------------"
python3 scripts/epic_manifest.py start_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID"
if [ $? -ne 0 ]; then
    echo "❌ Failed to start phase execution"
    exit 1
fi
echo "✅ Phase 1.5 started (Lamport event recorded)"

# Step 3: Execute Phase 1.5 Work (Scope Boundary Validation)
echo ""
echo "Step 3: Executing Phase 1.5 Work"
echo "-----------------------------------"

# Read scope definition
SCOPE_FILE="docs/brain/$EPIC_ID/00-scope.md"
if [ ! -f "$SCOPE_FILE" ]; then
    ERROR_MSG="Input file not found: $SCOPE_FILE"
    echo "❌ $ERROR_MSG"
    python3 scripts/epic_manifest.py fail_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$ERROR_MSG"
    exit 1
fi

# Validate scope boundary using Bob CLI (Wave 4 two-step pattern)
echo "Validating scope boundary for $EPIC_ID..."
OUTPUT_FILE="docs/brain/$EPIC_ID/01-scope-boundary.md"

# Step 1: Create message file (Wave 4 pattern - prevents freeze)
cat > /tmp/phase1_5_msg_$EPIC_ID.txt << 'EOFMSG'
Execute Phase 1.5 (Scope Boundary Validation) for $EPIC_ID.

**Input**: Read docs/brain/$EPIC_ID/00-scope.md

**Task**: Validate that the extraction scope is limited to a SINGLE METHOD only.

**MANDATORY Boundary Checks**:
1. ✅ Scope limited to single method (no multi-method extraction)
2. ✅ No changes to callers
3. ✅ No changes to callees
4. ✅ No changes to other methods in same file

**Output**: Create docs/brain/$EPIC_ID/01-scope-boundary.md with:
- Boundary validation results
- Approval status (APPROVED/REJECTED)
- Rationale

Use execute_command with cat > file << 'EOF' pattern for file creation.
EOFMSG

# Step 2: Run Bob with --yolo (Wave 4 pattern)
export BOBSHELL_API_KEY=$(grep 'export BOBSHELL_API_KEY' ~/.bashrc | cut -d'=' -f2)
bob --yolo --chat-mode v12-phase1-5-boundary "$(cat /tmp/phase1_5_msg_$EPIC_ID.txt)" 2>&1 | tee "logs/wave6/phase1_5/$EPIC_ID.log"

BOB_EXIT_CODE=${PIPESTATUS[0]}

if [ $BOB_EXIT_CODE -ne 0 ]; then
    ERROR_MSG="Bob CLI failed with exit code $BOB_EXIT_CODE"
    echo "❌ $ERROR_MSG"
    python3 scripts/epic_manifest.py fail_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$ERROR_MSG"
    exit 1
fi

# Verify output file was created
if [ ! -f "$OUTPUT_FILE" ]; then
    ERROR_MSG="Output file not created: $OUTPUT_FILE"
    echo "❌ $ERROR_MSG"
    python3 scripts/epic_manifest.py fail_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$ERROR_MSG"
    exit 1
fi

# Verify output file is non-empty
if [ ! -s "$OUTPUT_FILE" ]; then
    ERROR_MSG="Output file is empty: $OUTPUT_FILE"
    echo "❌ $ERROR_MSG"
    python3 scripts/epic_manifest.py fail_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$ERROR_MSG"
    exit 1
fi

echo "✅ Scope boundary validation complete: $OUTPUT_FILE"

# Step 4: Complete Phase Execution (Record Lamport Event)
echo ""
echo "Step 4: Completing Phase 1.5 Execution"
echo "-----------------------------------"
python3 scripts/epic_manifest.py complete_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$OUTPUT_FILE"
if [ $? -ne 0 ]; then
    echo "❌ Failed to complete phase execution"
    exit 1
fi
echo "✅ Phase 1.5 completed (Lamport event recorded)"

echo ""
echo "=========================================="
echo "✅ Phase 1.5 SUCCESS: $EPIC_ID"
echo "Output: $OUTPUT_FILE"
echo "=========================================="

# Made with Bob