#!/bin/bash
# V12.52 Phase 5.V Template: Ticket Verification
# Epic: {EPIC_ID}
# Agent: {AGENT_ID}
# Ticket: {TICKET_ID}
# Dependencies: Phase 5 (ticket-{TICKET_ID}-completion.md)
# Output: docs/brain/{EPIC_ID}/ticket-{TICKET_ID}-verification.md

set -euo pipefail

EPIC_ID="{EPIC_ID}"
AGENT_ID="{AGENT_ID}"
TICKET_ID="{TICKET_ID}"
PHASE="5.V"

echo "=========================================="
echo "V12.52 Phase 5.V: Ticket Verification"
echo "Epic: $EPIC_ID"
echo "Ticket: $TICKET_ID"
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

echo "✅ V12.52 verification passed - proceeding with Phase 5.V"

# Step 2: Start Phase Execution (Record Lamport Event)
echo ""
echo "Step 2: Starting Phase 5.V Execution"
echo "-----------------------------------"
python3 scripts/epic_manifest.py start_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID"
if [ $? -ne 0 ]; then
    echo "❌ Failed to start phase execution"
    exit 1
fi
echo "✅ Phase 5.V started (Lamport event recorded)"

# Step 3: Execute Phase 5.V Work (Ticket Verification)
echo ""
echo "Step 3: Executing Phase 5.V Work"
echo "-----------------------------------"

# Read ticket completion file
COMPLETION_FILE="docs/brain/$EPIC_ID/ticket-$TICKET_ID-completion.md"
if [ ! -f "$COMPLETION_FILE" ]; then
    ERROR_MSG="Input file not found: $COMPLETION_FILE"
    echo "❌ $ERROR_MSG"
    python3 scripts/epic_manifest.py fail_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$ERROR_MSG"
    exit 1
fi

# Verify ticket execution using Bob CLI (advanced mode - requires MCP tools)
echo "Verifying ticket $TICKET_ID for $EPIC_ID..."
OUTPUT_FILE="docs/brain/$EPIC_ID/ticket-$TICKET_ID-verification.md"

# Bob CLI command for verification with jCodemunch complexity check
# CRITICAL: Use temp file + command substitution pattern (NEVER inline strings)
# CRITICAL: Use full path ~/.npm-global/bin/bob (not in PATH)
# Verify 5 checks: compilation, complexity (CYC ≤8), scope, tests, encoding

# Step 3.1: Create message file
cat > /tmp/phase5_v_msg_$EPIC_ID.txt << 'EOFMSG'
Verify ticket $TICKET_ID execution for $EPIC_ID based on completion file $COMPLETION_FILE. Check: (1) compilation passes, (2) CYC ≤8 achieved, (3) only target method modified, (4) xUnit tests passing, (5) UTF-8 encoding. Use jCodemunch for complexity verification. Output: $OUTPUT_FILE
EOFMSG

# Step 3.2: Execute Bob CLI with command substitution
~/.npm-global/bin/bob --mode advanced "$(cat /tmp/phase5_v_msg_$EPIC_ID.txt)" 2>&1 | tee "logs/wave6/phase5_v/$EPIC_ID-ticket-$TICKET_ID.log"

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

echo "✅ Ticket verification complete: $OUTPUT_FILE"

# Step 4: Complete Phase Execution (Record Lamport Event)
echo ""
echo "Step 4: Completing Phase 5.V Execution"
echo "-----------------------------------"
python3 scripts/epic_manifest.py complete_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$OUTPUT_FILE"
if [ $? -ne 0 ]; then
    echo "❌ Failed to complete phase execution"
    exit 1
fi
echo "✅ Phase 5.V completed (Lamport event recorded)"

echo ""
echo "=========================================="
echo "✅ Phase 5.V SUCCESS: $EPIC_ID (Ticket $TICKET_ID)"
echo "Output: $OUTPUT_FILE"
echo "=========================================="

# Made with Bob
