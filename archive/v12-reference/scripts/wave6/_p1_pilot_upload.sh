#!/bin/bash
# V12.52 Phase 1 Pilot: Scope Definition with Agent Tracking
# Epic: EPIC-CCN-003
# Agent: wave6-p1-003
# Dependencies: Phase 0 (00-hotspots.md)
# Output: docs/brain/EPIC-CCN-003/00-scope.md

set -euo pipefail

EPIC_ID="EPIC-CCN-003"
AGENT_ID="wave6-p1-003"
PHASE="1"
START_TIME=$(date +%s)

echo "==========================================="
echo "V12.52 Phase 1: Scope Definition (PILOT)"
echo "Epic: $EPIC_ID"
echo "Agent: $AGENT_ID"
echo "==========================================="

# Step 1: V12.52 Verification Gate
echo ""
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

# Step 2: Start Phase Execution
echo ""
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

# Step 3: Execute Phase 1 Work with Agent Tracking
echo ""
echo "Step 3: Generate Scope Definition"
HOTSPOT_FILE="docs/brain/$EPIC_ID/00-hotspots.md"
OUTPUT_FILE="docs/brain/$EPIC_ID/00-scope.md"
LOG_FILE="logs/wave6/phase1/${EPIC_ID}_pilot.log"

mkdir -p logs/wave6/phase1

# Execute Bob CLI with verbose output to capture bobcoins
echo "Executing Bob CLI (v12-phase1-scope mode)..."
bob --mode v12-phase1-scope \
    --task "Define extraction scope for $EPIC_ID based on hotspot analysis. Read $HOTSPOT_FILE and create $OUTPUT_FILE with scope definition." \
    --verbose \
    2>&1 | tee "$LOG_FILE"

BOB_EXIT_CODE=${PIPESTATUS[0]}

if [ $BOB_EXIT_CODE -ne 0 ]; then
    ERROR_MSG="Bob CLI failed with exit code $BOB_EXIT_CODE"
    echo "❌ $ERROR_MSG"
    python3 -c "
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import fail_phase_execution

recorded, reason = fail_phase_execution('$EPIC_ID', '$PHASE', '$AGENT_ID', '$ERROR_MSG')
print(f'Failure recorded: {reason}')
"
    exit 1
fi

# Extract Agent Tracking from Bob CLI output
BOBCOINS_USED=$(grep -oP 'bobcoins.*?\K[0-9]+' "$LOG_FILE" | head -n 1 || echo "unknown")
API_KEY_NAME=$(grep -oP 'api.*?key.*?:\s*\K[a-zA-Z0-9_-]+' "$LOG_FILE" | head -n 1 || echo "unknown")
MODEL_NAME=$(grep -oP 'model.*?:\s*\K[a-zA-Z0-9_.-]+' "$LOG_FILE" | head -n 1 || echo "unknown")
END_TIME=$(date +%s)
EXECUTION_TIME=$((END_TIME - START_TIME))

# Verify output file
if [ ! -f "$OUTPUT_FILE" ] || [ ! -s "$OUTPUT_FILE" ]; then
    ERROR_MSG="Output file not created or empty: $OUTPUT_FILE"
    echo "❌ $ERROR_MSG"
    python3 -c "
import sys
sys.path.insert(0, 'scripts')
from epic_manifest import fail_phase_execution

recorded, reason = fail_phase_execution('$EPIC_ID', '$PHASE', '$AGENT_ID', '$ERROR_MSG')
print(f'Failure recorded: {reason}')
"
    exit 1
fi

# Inject Agent Tracking section into output file
echo "" >> "$OUTPUT_FILE"
echo "---" >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"
echo "## Agent Tracking" >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"
echo "- **Agent Name**: $AGENT_ID" >> "$OUTPUT_FILE"
echo "- **Mode**: v12-phase1-scope" >> "$OUTPUT_FILE"
echo "- **Bobcoins Used**: $BOBCOINS_USED" >> "$OUTPUT_FILE"
echo "- **API Key**: $API_KEY_NAME" >> "$OUTPUT_FILE"
echo "- **Model**: $MODEL_NAME" >> "$OUTPUT_FILE"
echo "- **Execution Time**: ${EXECUTION_TIME}s" >> "$OUTPUT_FILE"
echo "- **Timestamp**: $(date -u +%Y-%m-%dT%H:%M:%SZ)" >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"
echo "### MCP Tools Used" >> "$OUTPUT_FILE"
echo "" >> "$OUTPUT_FILE"
echo "- jcodemunch-mcp: get_file_outline, find_references, get_dependency_graph" >> "$OUTPUT_FILE"
echo "- sequential-thinking: sequentialthinking (scope boundary validation)" >> "$OUTPUT_FILE"
echo "- graphify: Codebase structure visualization" >> "$OUTPUT_FILE"

echo "✅ Scope definition generated: $OUTPUT_FILE"
echo "✅ Agent Tracking injected: bobcoins=$BOBCOINS_USED, api=$API_KEY_NAME, model=$MODEL_NAME, time=${EXECUTION_TIME}s"

# Step 4: Complete Phase Execution
echo ""
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
    'Scope definition complete'
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
echo "=== Phase 1 Complete ==="
echo "Output: $OUTPUT_FILE"
echo "Lamport event recorded"
echo ""

# Made with Bob