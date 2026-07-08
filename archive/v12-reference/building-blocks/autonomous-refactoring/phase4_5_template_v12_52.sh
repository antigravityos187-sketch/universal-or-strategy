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
echo "Jane Street Validation Gate"
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
echo "Reviewing tickets for $EPIC_ID against Jane Street KB..."
OUTPUT_FILE="docs/brain/$EPIC_ID/04-5-ticket-review.md"

# Step 3a: Query Jane Street KB for ticket validation rules
echo "Querying Jane Street KB for validation rules..."
python3 scripts/query_kb.py "complexity reduction" > /tmp/jane_street_complexity.txt
python3 scripts/query_kb.py "FSM extraction" > /tmp/jane_street_fsm.txt
python3 scripts/query_kb.py "testing patterns" > /tmp/jane_street_testing.txt

# Step 3b: Create message file for Bob CLI (temp file pattern - MANDATORY)
cat > /tmp/phase4_5_msg_$EPIC_ID.txt << 'EOFMSG'
You are the V12 Ticket Reviewer for Phase 4.5 (Jane Street Validation Gate).

**Task**: Review tickets in {TICKETS_FILE} against Jane Street KB rules.

**Validation Checklist**:
1. Each ticket targets CYC ≤ 8 (Jane Street strict standard)
2. Extraction scope is single-method only (no scope creep)
3. xUnit test generation is specified (NEVER NUnit/MSTest)
4. UTF-8 encoding compliance is mentioned
5. Complexity reduction is measurable
6. Jane Street patterns are followed (FSM/Actor, lock-free, etc.)

**Jane Street KB Context**:
- Complexity rules: See /tmp/jane_street_complexity.txt
- FSM patterns: See /tmp/jane_street_fsm.txt
- Testing standards: See /tmp/jane_street_testing.txt

**Output**: {OUTPUT_FILE}

**Required Sections**:
1. Ticket-by-Ticket Review (PASS/FAIL for each ticket)
2. Jane Street Compliance Summary
3. Recommendations (if any tickets fail)
4. Agent Tracking (agent name, bobcoins, API key, execution time)

**Decision**: APPROVE (all tickets pass) or REJECT (any ticket fails)
EOFMSG

# Replace placeholders in message file
sed -i "s|{TICKETS_FILE}|$TICKETS_FILE|g" /tmp/phase4_5_msg_$EPIC_ID.txt
sed -i "s|{OUTPUT_FILE}|$OUTPUT_FILE|g" /tmp/phase4_5_msg_$EPIC_ID.txt

# Step 3c: Invoke Bob CLI with command substitution (MANDATORY pattern)
~/.npm-global/bin/bob --yolo --chat-mode v12-phase4-5-review "$(cat /tmp/phase4_5_msg_$EPIC_ID.txt)" \
    2>&1 | tee "logs/wave7/phase4_5/$EPIC_ID.log"

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

# Check if tickets were APPROVED or REJECTED
if grep -q "Decision: REJECT" "$OUTPUT_FILE"; then
    ERROR_MSG="Tickets REJECTED by Jane Street validation gate"
    echo "❌ $ERROR_MSG"
    python3 scripts/epic_manifest.py fail_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$ERROR_MSG"
    exit 1
fi

if ! grep -q "Decision: APPROVE" "$OUTPUT_FILE"; then
    ERROR_MSG="No clear APPROVE/REJECT decision in review"
    echo "❌ $ERROR_MSG"
    python3 scripts/epic_manifest.py fail_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$ERROR_MSG"
    exit 1
fi

echo "✅ Ticket review complete: $OUTPUT_FILE"
echo "✅ Tickets APPROVED by Jane Street validation gate"

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
echo "Jane Street Validation: PASSED"
echo "=========================================="

# Made with Bob