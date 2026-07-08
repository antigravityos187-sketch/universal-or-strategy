#!/bin/bash
# V12.52 Phase 1 Template: Scope Definition
# Epic: EPIC-CCN-001
# Agent: alprofit
# Dependencies: Phase 0 (00-hotspots.md)
# Output: docs/brain/EPIC-CCN-001/00-scope.md

set -euo pipefail

EPIC_ID="EPIC-CCN-001"
AGENT_ID="alprofit"
PHASE="1"

echo "=========================================="
echo "V12.52 Phase 1: Scope Definition"
echo "Epic: $EPIC_ID"
echo "Agent: $AGENT_ID"
echo "=========================================="

# Step 1: V12.52 Verification Gate (Triple Verification)
echo ""
echo "Step 1: V12.52 Verification Gate"
echo "-----------------------------------"

# Gate 1: Dependencies (Manifest)
echo "Gate 1: Checking dependencies (manifest)..."
python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); result = module.verify_dependencies('$EPIC_ID', '$PHASE'); import sys; sys.exit(0 if result else 1)"
if [ $? -ne 0 ]; then
    echo "❌ BLOCKED: Dependencies not satisfied (manifest)"
    echo "❌ V12.52 verification failed - aborting"
    exit 1
fi
echo "✅ Dependencies satisfied"

# Gate 2: Causal Verification (Lamport)
echo "Gate 2: Checking causal verification (Lamport)..."
python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); can_exec, reason = module.verify_can_execute('$EPIC_ID', '$PHASE', '$AGENT_ID'); print(reason if not can_exec else 'OK'); import sys; sys.exit(0 if can_exec else 1)"
if [ $? -ne 0 ]; then
    echo "❌ BLOCKED: Causal verification failed"
    echo "❌ V12.52 verification failed - aborting"
    exit 1
fi
echo "✅ Causal verification passed"

# Gate 3: Filesystem State (Dual Verification)
echo "Gate 3: Checking filesystem state..."
python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); result = module.verify_filesystem_state('$EPIC_ID', '$PHASE'); import sys; sys.exit(0 if result else 1)"
if [ $? -ne 0 ]; then
    echo "❌ BLOCKED: State mismatch (filesystem)"
    echo "❌ V12.52 verification failed - aborting"
    exit 1
fi
echo "✅ Filesystem state verified"

echo "✅ V12.52 verification passed - proceeding with Phase 1"

# Step 2: Start Phase Execution (Record Lamport Event)
echo ""
echo "Step 2: Starting Phase 1 Execution"
echo "-----------------------------------"
python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); started, reason = module.start_phase_execution('$EPIC_ID', '$PHASE', '$AGENT_ID'); print(reason if not started else 'OK'); import sys; sys.exit(0 if started else 1)"
if [ $? -ne 0 ]; then
    echo "❌ Failed to start phase execution"
    exit 1
fi
echo "✅ Phase 1 started (Lamport event recorded)"

# Step 3: Execute Phase 1 Work (Scope Definition)
echo ""
echo "Step 3: Executing Phase 1 Work"
echo "-----------------------------------"

# Read hotspot analysis
HOTSPOT_FILE="docs/brain/$EPIC_ID/00-hotspots.md"
if [ ! -f "$HOTSPOT_FILE" ]; then
    ERROR_MSG="Input file not found: $HOTSPOT_FILE"
    echo "❌ $ERROR_MSG"
    python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); module.fail_phase_execution('$EPIC_ID', '$PHASE', '$AGENT_ID', '$ERROR_MSG')"
    exit 1
fi

# Define scope using Bob CLI (plan mode)
echo "Defining scope for $EPIC_ID..."
OUTPUT_FILE="docs/brain/$EPIC_ID/00-scope.md"

# Bob CLI command for scope definition (v1.0.4 syntax)
# CRITICAL: Use full path ~/.npm-global/bin/bob (not in PATH)
# Export API key from bashrc
export BOBSHELL_API_KEY=$(grep 'export BOBSHELL_API_KEY' ~/.bashrc | cut -d'=' -f2)
~/.npm-global/bin/bob \
    --chat-mode v12-phase1-scope \
    --yolo \
    "Define extraction scope for $EPIC_ID based on hotspot analysis in $HOTSPOT_FILE. Output: $OUTPUT_FILE" \
    2>&1 | tee "logs/wave6/phase1/$EPIC_ID.log"

BOB_EXIT_CODE=${PIPESTATUS[0]}

if [ $BOB_EXIT_CODE -ne 0 ]; then
    ERROR_MSG="Bob CLI failed with exit code $BOB_EXIT_CODE"
    echo "❌ $ERROR_MSG"
    python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); module.fail_phase_execution('$EPIC_ID', '$PHASE', '$AGENT_ID', '$ERROR_MSG')"
    exit 1
fi

# Verify output file was created
if [ ! -f "$OUTPUT_FILE" ]; then
    ERROR_MSG="Output file not created: $OUTPUT_FILE"
    echo "❌ $ERROR_MSG"
    python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); module.fail_phase_execution('$EPIC_ID', '$PHASE', '$AGENT_ID', '$ERROR_MSG')"
    exit 1
fi

# Verify output file is non-empty
if [ ! -s "$OUTPUT_FILE" ]; then
    ERROR_MSG="Output file is empty: $OUTPUT_FILE"
    echo "❌ $ERROR_MSG"
    python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); module.fail_phase_execution('$EPIC_ID', '$PHASE', '$AGENT_ID', '$ERROR_MSG')"
    exit 1
fi

echo "✅ Scope definition complete: $OUTPUT_FILE"

# Step 4: Complete Phase Execution (Record Lamport Event)
echo ""
echo "Step 4: Completing Phase 1 Execution"
echo "-----------------------------------"
python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); completed, reason = module.complete_phase_execution('$EPIC_ID', '$PHASE', '$AGENT_ID', ['$OUTPUT_FILE']); print(reason if not completed else 'OK'); import sys; sys.exit(0 if completed else 1)"
if [ $? -ne 0 ]; then
    echo "❌ Failed to complete phase execution"
    exit 1
fi
echo "✅ Phase 1 completed (Lamport event recorded)"

echo ""
echo "=========================================="
echo "✅ Phase 1 SUCCESS: $EPIC_ID"
echo "Output: $OUTPUT_FILE"
echo "=========================================="

# Made with Bob
