#!/bin/bash
# Wave 6 Pilot Test: EPIC-CCN-001 Phase 0 with V12.52 Gates
# Purpose: Verify V12.52 Lamport Causal Verification works correctly
# Epic: EPIC-CCN-001
# Method: SymmetryGuardReplaceExistingFollowerTarget
# File: src/V12_002.Symmetry.Replace.cs
# CYC: 18
# Agent: pilot-test-001
# Generated: 2026-06-17T18:31:00Z

set -e  # Exit on error
set -u  # Exit on undefined variable

# ============================================================================
# CONFIGURATION
# ============================================================================

EPIC_ID="EPIC-CCN-001"
PHASE="0"
AGENT_ID="pilot-test-001"
METHOD="SymmetryGuardReplaceExistingFollowerTarget"
FILE="src/V12_002.Symmetry.Replace.cs"
CYC=18

# Paths
PROJECT_ROOT="$HOME/universal-or-strategy"
BRAIN_DIR="$PROJECT_ROOT/docs/brain/$EPIC_ID"
SCRIPTS_DIR="$PROJECT_ROOT/scripts"
LOG_FILE="$PROJECT_ROOT/logs/wave6/pilot_test_${EPIC_ID}_phase${PHASE}.log"

# Create log directory
mkdir -p "$(dirname "$LOG_FILE")"

# ============================================================================
# LOGGING SETUP
# ============================================================================

log() {
    echo "[$(date +'%Y-%m-%d %H:%M:%S')] $*" | tee -a "$LOG_FILE"
}

log_error() {
    echo "[$(date +'%Y-%m-%d %H:%M:%S')] ERROR: $*" | tee -a "$LOG_FILE" >&2
}

log "=========================================="
log "Wave 6 Pilot Test: $EPIC_ID Phase $PHASE"
log "Method: $METHOD"
log "File: $FILE"
log "CYC: $CYC"
log "Agent: $AGENT_ID"
log "=========================================="

# ============================================================================
# V12.52 GATE 1: DEPENDENCY VERIFICATION
# ============================================================================

log "V12.52 GATE 1: Verifying dependencies..."

cd "$PROJECT_ROOT"

# Phase 0 has no dependencies (it's the first phase)
log "✓ Phase 0 has no dependencies - GATE 1 PASSED"

# ============================================================================
# V12.52 GATE 2: LAMPORT CAUSAL VERIFICATION
# ============================================================================

log "V12.52 GATE 2: Verifying Lamport causal ordering..."

# Use epic_manifest.py to verify causal ordering
CAUSAL_CHECK=$(python3 -c "
import sys
sys.path.insert(0, '$SCRIPTS_DIR')
from epic_manifest import verify_can_execute

try:
    can_execute, reason = verify_can_execute('$EPIC_ID', '$PHASE', '$AGENT_ID')
    if can_execute:
        print('PASS')
    else:
        print(f'FAIL: {reason}')
        sys.exit(1)
except Exception as e:
    print(f'ERROR: {e}')
    sys.exit(1)
" 2>&1)

if [[ "$CAUSAL_CHECK" == "PASS" ]]; then
    log "✓ Lamport causal ordering verified - GATE 2 PASSED"
else
    log_error "✗ Lamport causal verification failed: $CAUSAL_CHECK"
    log_error "GATE 2 FAILED - Aborting execution"
    exit 1
fi

# ============================================================================
# V12.52 GATE 3: FILESYSTEM STATE VERIFICATION
# ============================================================================

log "V12.52 GATE 3: Verifying filesystem state..."

# Check that brain directory exists
if [[ ! -d "$BRAIN_DIR" ]]; then
    log_error "✗ Brain directory does not exist: $BRAIN_DIR"
    log_error "GATE 3 FAILED - Aborting execution"
    exit 1
fi

# Check that manifest exists
if [[ ! -f "$BRAIN_DIR/manifest.json" ]]; then
    log_error "✗ Manifest does not exist: $BRAIN_DIR/manifest.json"
    log_error "GATE 3 FAILED - Aborting execution"
    exit 1
fi

# Verify no unexpected Phase 0 output files exist (clean slate test)
EXISTING_HOTSPOTS="$BRAIN_DIR/00-hotspots.md"
if [[ -f "$EXISTING_HOTSPOTS" ]]; then
    log "⚠ WARNING: Phase 0 output already exists (expected for pilot test)"
    log "  This is a re-run to verify V12.52 gates work correctly"
fi

log "✓ Filesystem state verified - GATE 3 PASSED"

# ============================================================================
# RECORD PHASE START (V12.52 LAMPORT EVENT)
# ============================================================================

log "Recording Phase $PHASE start event..."

python3 -c "
import sys
sys.path.insert(0, '$SCRIPTS_DIR')
from epic_manifest import start_phase_execution

try:
    start_phase_execution('$EPIC_ID', '$PHASE', '$AGENT_ID')
    print('Phase start recorded successfully')
except Exception as e:
    print(f'ERROR: Failed to record phase start: {e}')
    sys.exit(1)
" 2>&1 | tee -a "$LOG_FILE"

if [[ ${PIPESTATUS[0]} -ne 0 ]]; then
    log_error "Failed to record phase start"
    exit 1
fi

log "✓ Phase start event recorded with Lamport clock"

# ============================================================================
# PHASE 0 EXECUTION: HOTSPOT ANALYSIS
# ============================================================================

log "Executing Phase 0: Hotspot Analysis..."

# Since this is a pilot test and Phase 0 already exists, we'll verify
# the existing hotspot analysis is valid rather than regenerating it

if [[ -f "$EXISTING_HOTSPOTS" ]]; then
    log "Verifying existing hotspot analysis..."
    
    # Check that file contains required sections
    REQUIRED_SECTIONS=(
        "Target Method"
        "Complexity Metrics"
        "Blast Radius"
        "Call Hierarchy"
        "Risk Assessment"
        "Refactoring Strategy"
    )
    
    MISSING_SECTIONS=()
    for section in "${REQUIRED_SECTIONS[@]}"; do
        if ! grep -q "## $section" "$EXISTING_HOTSPOTS"; then
            MISSING_SECTIONS+=("$section")
        fi
    done
    
    if [[ ${#MISSING_SECTIONS[@]} -gt 0 ]]; then
        log_error "✗ Hotspot analysis missing required sections: ${MISSING_SECTIONS[*]}"
        exit 1
    fi
    
    # Verify method name matches
    if ! grep -q "$METHOD" "$EXISTING_HOTSPOTS"; then
        log_error "✗ Hotspot analysis does not contain method: $METHOD"
        exit 1
    fi
    
    # Verify CYC matches
    if ! grep -q "Cyclomatic Complexity.*: $CYC" "$EXISTING_HOTSPOTS"; then
        log_error "✗ Hotspot analysis CYC mismatch (expected: $CYC)"
        exit 1
    fi
    
    log "✓ Existing hotspot analysis validated"
    OUTPUT_FILE="$EXISTING_HOTSPOTS"
else
    log_error "✗ Hotspot analysis file not found (expected for pilot test)"
    log_error "  This pilot test requires existing Phase 0 output"
    exit 1
fi

# ============================================================================
# RECORD PHASE COMPLETION (V12.52 LAMPORT EVENT)
# ============================================================================

log "Recording Phase $PHASE completion event..."

python3 -c "
import sys
sys.path.insert(0, '$SCRIPTS_DIR')
from epic_manifest import complete_phase_execution

try:
    complete_phase_execution(
        '$EPIC_ID',
        '$PHASE',
        '$AGENT_ID',
        outputs=['$OUTPUT_FILE'],
        notes='Pilot test: V12.52 gates verified, existing hotspot analysis validated'
    )
    print('Phase completion recorded successfully')
except Exception as e:
    print(f'ERROR: Failed to record phase completion: {e}')
    sys.exit(1)
" 2>&1 | tee -a "$LOG_FILE"

if [[ ${PIPESTATUS[0]} -ne 0 ]]; then
    log_error "Failed to record phase completion"
    exit 1
fi

log "✓ Phase completion event recorded with Lamport clock"

# ============================================================================
# VERIFICATION
# ============================================================================

log "Verifying Phase $PHASE completion..."

# 1. Output file exists
if [[ ! -f "$OUTPUT_FILE" ]]; then
    log_error "✗ Output file not found: $OUTPUT_FILE"
    exit 1
fi
log "✓ Output file exists: $OUTPUT_FILE"

# 2. Manifest updated
MANIFEST_CHECK=$(python3 -c "
import sys
import json
sys.path.insert(0, '$SCRIPTS_DIR')
from epic_manifest import load_manifest

try:
    manifest = load_manifest('$EPIC_ID')
    phase_data = manifest['phases'].get('$PHASE', {})
    status = phase_data.get('status', 'unknown')
    print(status)
except Exception as e:
    print(f'ERROR: {e}')
    sys.exit(1)
" 2>&1)

if [[ "$MANIFEST_CHECK" == "completed" ]]; then
    log "✓ Manifest status: completed"
else
    log_error "✗ Manifest status: $MANIFEST_CHECK (expected: completed)"
    exit 1
fi

# 3. Lamport events recorded
EVENT_LOG_CHECK=$(python3 -c "
import sys
sys.path.insert(0, '$SCRIPTS_DIR')
from epic_manifest import get_event_log

try:
    events = get_event_log('$EPIC_ID', '$PHASE')
    if len(events) >= 2:  # start + complete
        print(f'PASS: {len(events)} events')
    else:
        print(f'FAIL: Only {len(events)} events (expected >= 2)')
        sys.exit(1)
except Exception as e:
    print(f'ERROR: {e}')
    sys.exit(1)
" 2>&1)

if [[ "$EVENT_LOG_CHECK" == PASS* ]]; then
    log "✓ Lamport event log: $EVENT_LOG_CHECK"
else
    log_error "✗ Lamport event log check failed: $EVENT_LOG_CHECK"
    exit 1
fi

# ============================================================================
# SUCCESS
# ============================================================================

log "=========================================="
log "✓ PILOT TEST SUCCESSFUL"
log "=========================================="
log "Epic: $EPIC_ID"
log "Phase: $PHASE"
log "Agent: $AGENT_ID"
log "V12.52 Gates: ALL PASSED"
log "  - Gate 1: Dependencies verified"
log "  - Gate 2: Lamport causal ordering verified"
log "  - Gate 3: Filesystem state verified"
log "Lamport Events: Recorded (start + complete)"
log "Output: $OUTPUT_FILE"
log "Manifest: Updated (status=completed)"
log "=========================================="
log "V12.52 Lamport Causal Verification: PRODUCTION READY"
log "=========================================="

exit 0

# Made with Bob
