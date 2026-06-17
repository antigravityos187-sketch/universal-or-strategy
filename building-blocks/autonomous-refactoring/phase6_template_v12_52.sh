#!/bin/bash
# V12.52 Phase 6 Template: Final Review
# Epic: {EPIC_ID}
# Agent: {AGENT_ID}
# Dependencies: Phase 5.V (all ticket-*-verification.md files)
# Output: docs/brain/{EPIC_ID}/05-completion-report.md

set -euo pipefail

EPIC_ID="{EPIC_ID}"
AGENT_ID="{AGENT_ID}"
PHASE="6"

echo "=========================================="
echo "V12.52 Phase 6: Final Review"
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

echo "✅ V12.52 verification passed - proceeding with Phase 6"

# Step 2: Start Phase Execution (Record Lamport Event)
echo ""
echo "Step 2: Starting Phase 6 Execution"
echo "-----------------------------------"
python3 scripts/epic_manifest.py start_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID"
if [ $? -ne 0 ]; then
    echo "❌ Failed to start phase execution"
    exit 1
fi
echo "✅ Phase 6 started (Lamport event recorded)"

# Step 3: Execute Phase 6 Work (Final Review)
echo ""
echo "Step 3: Executing Phase 6 Work"
echo "-----------------------------------"

# Collect all ticket verification files
VERIFICATION_FILES=$(ls docs/brain/$EPIC_ID/ticket-*-verification.md 2>/dev/null || true)
if [ -z "$VERIFICATION_FILES" ]; then
    ERROR_MSG="No ticket verification files found in docs/brain/$EPIC_ID/"
    echo "❌ $ERROR_MSG"
    python3 scripts/epic_manifest.py fail_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$ERROR_MSG"
    exit 1
fi

echo "Found verification files:"
echo "$VERIFICATION_FILES"

# Perform final review using Bob CLI (advanced mode - requires MCP tools)
echo "Performing final review for $EPIC_ID..."
OUTPUT_FILE="docs/brain/$EPIC_ID/05-completion-report.md"

# Bob CLI command for final review with Greptile audit
# Verify: (1) all tickets verified, (2) CYC ≤8 achieved, (3) tests passing, (4) 0 P0/P1 Greptile issues
bob --mode advanced --task "Perform final review for $EPIC_ID. Verify all ticket verifications in docs/brain/$EPIC_ID/. Run Greptile audit (expect 0 P0/P1 issues). Generate completion report. Output: $OUTPUT_FILE" \
    --context "docs/brain/$EPIC_ID/" \
    --output "$OUTPUT_FILE" \
    2>&1 | tee "logs/wave6/phase6/$EPIC_ID.log"

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

echo "✅ Final review complete: $OUTPUT_FILE"

# Step 4: Complete Phase Execution (Record Lamport Event)
echo ""
echo "Step 4: Completing Phase 6 Execution"
echo "-----------------------------------"
python3 scripts/epic_manifest.py complete_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$OUTPUT_FILE"
if [ $? -ne 0 ]; then
    echo "❌ Failed to complete phase execution"
    exit 1
fi
echo "✅ Phase 6 completed (Lamport event recorded)"

# Update roadmap with completion status
echo ""
echo "Step 5: Updating Roadmap"
echo "-----------------------------------"
python3 scripts/epic_manifest.py update_roadmap "$EPIC_ID" "completed"
if [ $? -ne 0 ]; then
    echo "⚠️ Warning: Failed to update roadmap (non-blocking)"
fi
echo "✅ Roadmap updated"

echo ""
echo "=========================================="
echo "✅ Phase 6 SUCCESS: $EPIC_ID"
echo "Output: $OUTPUT_FILE"
echo "Epic Status: COMPLETED"
echo "=========================================="

# Made with Bob
