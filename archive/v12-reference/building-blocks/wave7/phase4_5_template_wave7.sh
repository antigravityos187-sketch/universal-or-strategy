#!/bin/bash
# V12.52 Phase 4.5 Template: Ticket Review (Jane Street Validation Gate)
# Epic: {EPIC_ID}
# Agent: {AGENT_ID}
# Dependencies: Phase 4 (04-tickets.md)
# Output: docs/brain/{EPIC_ID}/04-5-ticket-review.md

set -euo pipefail

EPIC_ID="{EPIC_ID}"
AGENT_ID="{AGENT_ID}"
PHASE="4.5"

echo "=========================================="
echo "V12.52 Phase 4.5: Ticket Review"
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

echo "✅ V12.52 verification passed - proceeding with Phase 4.5"

# Step 2: Start Phase Execution (Record Lamport Event)
echo ""
echo "Step 2: Starting Phase 4.5 Execution"
echo "-----------------------------------"
python3 scripts/epic_manifest.py start_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID"
if [ $? -ne 0 ]; then
    echo "❌ Failed to start phase execution"
    exit 1
fi
echo "✅ Phase 4.5 started (Lamport event recorded)"

# Step 3: Execute Phase 4.5 Work (Ticket Review)
echo ""
echo "Step 3: Executing Phase 4.5 Work"
echo "-----------------------------------"

# Read tickets file
TICKETS_FILE="docs/brain/$EPIC_ID/04-tickets.md"
if [ ! -f "$TICKETS_FILE" ]; then
    ERROR_MSG="Input file not found: $TICKETS_FILE"
    echo "❌ $ERROR_MSG"
    python3 scripts/epic_manifest.py fail_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$ERROR_MSG"
    exit 1
fi

# Review tickets using Bob CLI (v12-phase4-5-review mode)
echo "Reviewing tickets for $EPIC_ID..."
OUTPUT_FILE="docs/brain/$EPIC_ID/04-5-ticket-review.md"

# Bob CLI command for ticket review with Jane Street KB validation
# CRITICAL: Use temp file + command substitution pattern (NEVER inline strings)
# CRITICAL: Use full path ~/.npm-global/bin/bob (not in PATH)

# Step 3.1: Create message file
cat > /tmp/phase4_5_msg_$EPIC_ID.txt << 'EOFMSG'
Review tickets in $TICKETS_FILE for $EPIC_ID. Validate against Jane Street KB rules using Sequential Thinking MCP. Run 6 automated checks: 1) Single-method scope 2) Complexity target (CYC ≤8) 3) No scope creep 4) Ticket independence 5) Test coverage 6) UTF-8 encoding. Output: $OUTPUT_FILE with APPROVED/REJECTED status.
EOFMSG

# Step 3.2: Execute Bob CLI with command substitution
~/.npm-global/bin/bob --mode v12-phase4-5-review "$(cat /tmp/phase4_5_msg_$EPIC_ID.txt)" 2>&1 | tee "logs/wave7/phase4_5/$EPIC_ID.log"

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

# Check review status (APPROVED vs REJECTED)
if grep -q "Status: REJECTED" "$OUTPUT_FILE"; then
    ERROR_MSG="Tickets REJECTED - blocker issues found"
    echo "❌ $ERROR_MSG"
    python3 scripts/epic_manifest.py fail_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$ERROR_MSG"
    exit 1
fi

if ! grep -q "Status: APPROVED" "$OUTPUT_FILE"; then
    ERROR_MSG="Review status unclear - neither APPROVED nor REJECTED"
    echo "❌ $ERROR_MSG"
    python3 scripts/epic_manifest.py fail_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$ERROR_MSG"
    exit 1
fi

echo "✅ Ticket review complete: $OUTPUT_FILE"
echo "✅ Status: APPROVED (ready for Phase 5)"

# Step 4: Complete Phase Execution (Record Lamport Event)
echo ""
echo "Step 4: Completing Phase 4.5 Execution"
echo "-----------------------------------"
python3 scripts/epic_manifest.py complete_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$OUTPUT_FILE"
if [ $? -ne 0 ]; then
    echo "❌ Failed to complete phase execution"
    exit 1
fi
echo "✅ Phase 4.5 completed (Lamport event recorded)"

echo ""
echo "=========================================="
echo "✅ Phase 4.5 SUCCESS: $EPIC_ID"
echo "Output: $OUTPUT_FILE"
echo "Status: APPROVED - Ready for Phase 5"
echo "=========================================="

# Made with Bob