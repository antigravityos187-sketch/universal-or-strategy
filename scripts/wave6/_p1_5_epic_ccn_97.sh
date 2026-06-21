#!/bin/bash
# V12.52 Phase 1.5 Template: Scope Boundary Validation
# Epic: EPIC-CCN-97
# Agent: alprofit
# Dependencies: Phase 1 (00-scope.md)
# Output: docs/brain/EPIC-CCN-97/01-scope-boundary.md

set -euo pipefail

EPIC_ID="EPIC-CCN-97"
AGENT_ID="alprofit"
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