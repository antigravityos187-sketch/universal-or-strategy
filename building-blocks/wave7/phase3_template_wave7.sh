#!/bin/bash
# V12.52 Phase 3 Template: DNA & PR Audit
# Epic: {EPIC_ID}
# Agent: {AGENT_ID}
# Dependencies: Phase 2 (02-architecture-plan.md)
# Output: docs/brain/{EPIC_ID}/03-audit-report.md

set -euo pipefail

EPIC_ID="{EPIC_ID}"
AGENT_ID="{AGENT_ID}"
PHASE="3"

echo "=========================================="
echo "V12.52 Phase 3: DNA & PR Audit"
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

echo "✅ V12.52 verification passed - proceeding with Phase 3"

# Step 2: Start Phase Execution (Record Lamport Event)
echo ""
echo "Step 2: Starting Phase 3 Execution"
echo "-----------------------------------"
python3 scripts/epic_manifest.py start_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID"
if [ $? -ne 0 ]; then
    echo "❌ Failed to start phase execution"
    exit 1
fi
echo "✅ Phase 3 started (Lamport event recorded)"

# Step 3: Execute Phase 3 Work (DNA & PR Audit)
echo ""
echo "Step 3: Executing Phase 3 Work"
echo "-----------------------------------"

# Read architecture plan
ARCH_FILE="docs/brain/$EPIC_ID/02-architecture-plan.md"
if [ ! -f "$ARCH_FILE" ]; then
    ERROR_MSG="Input file not found: $ARCH_FILE"
    echo "❌ $ERROR_MSG"
    python3 scripts/epic_manifest.py fail_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$ERROR_MSG"
    exit 1
fi

# Run DNA & PR audit using Bob CLI (v12-phase3-audit custom mode)
echo "Running DNA & PR audit for $EPIC_ID..."
OUTPUT_FILE="docs/brain/$EPIC_ID/03-audit-report.md"

# Create message file (MANDATORY: temp file + command substitution pattern)
cat > /tmp/phase3_msg_$EPIC_ID.txt << 'EOFMSG'
Run V12 DNA compliance checks and PR hygiene validation.

Context:
- Epic: $EPIC_ID
- Architecture Plan: docs/brain/$EPIC_ID/02-architecture-plan.md

Tasks:
1. Read architecture plan from 02-architecture-plan.md
2. Use jCodemunch MCP tools for code compliance verification
3. Use Sequential Thinking MCP for audit decisions
4. Run V12 DNA compliance checks
5. Verify no scope creep
6. Write audit report to docs/brain/$EPIC_ID/03-audit-report.md

Required MCP Tools:
- jCodemunch: search_ast, get_layer_violations, get_dependency_cycles
- Sequential Thinking: sequentialthinking (validate compliance)

Agent Tracking (MANDATORY):
- Include Agent Tracking section in 03-audit-report.md
- Agent Name: v12-phase3-audit
- Bobcoins Used: [extract from logs]
- API Key: [which API was used]
- Execution Time: [duration]
EOFMSG

# Invoke Bob with command substitution (MANDATORY pattern)
~/.npm-global/bin/bob --yolo --chat-mode v12-phase3-audit "$(cat /tmp/phase3_msg_$EPIC_ID.txt)" \
    2>&1 | tee "logs/wave7/phase3/$EPIC_ID.log"

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

echo "✅ DNA & PR audit complete: $OUTPUT_FILE"

# Step 4: Complete Phase Execution (Record Lamport Event)
echo ""
echo "Step 4: Completing Phase 3 Execution"
echo "-----------------------------------"
python3 scripts/epic_manifest.py complete_phase_execution "$EPIC_ID" "$PHASE" "$AGENT_ID" "$OUTPUT_FILE"
if [ $? -ne 0 ]; then
    echo "❌ Failed to complete phase execution"
    exit 1
fi
echo "✅ Phase 3 completed (Lamport event recorded)"

echo ""
echo "=========================================="
echo "✅ Phase 3 SUCCESS: $EPIC_ID"
echo "Output: $OUTPUT_FILE"
echo "=========================================="

# Made with Bob
