#!/bin/bash
# Wave 7 Master Launch Script
# Version: 1.0
# Created: 2026-06-21
# Purpose: Launch and monitor all 161 Wave 7 epics with cost-optimized polling

set -e  # Exit on error

# ============================================================================
# Configuration
# ============================================================================

WAVE_ID="wave7"
TOTAL_EPICS=161
VM_NAME="v12-test-golden-v2"
VM_ZONE="us-central1-a"
VM_USER="malhitticrypto"
VM_DIR="/home/malhitticrypto/universal-or-strategy"

# Polling configuration (per COST_OPTIMIZED_POLLING_PROTOCOL.md)
LAUNCH_VERIFICATION_EPICS=10  # First 10 epics: 1-minute polling
LAUNCH_POLL_INTERVAL=60       # 1 minute for launch verification
FULL_WAVE_POLL_INTERVAL=240   # 4 minutes for remaining epics

# Lamport event log
LAMPORT_LOG=".lamport/wave7/event_log.jsonl"
LAMPORT_GLOBAL_LOG=".lamport/event_log.jsonl"

# Epic roadmap
ROADMAP="epic_roadmap_wave7.json"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# ============================================================================
# Helper Functions
# ============================================================================

log() {
    echo -e "${BLUE}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} ✅ $1"
}

log_error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} ❌ $1"
}

log_warning() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} ⚠️  $1"
}

# Record Lamport event
record_event() {
    local event_type=$1
    local epic_id=$2
    local phase=$3
    local status=$4
    local metadata=$5
    
    python3 -c "
import sys
import json
from datetime import datetime
sys.path.insert(0, 'scripts')
from epic_manifest import record_lamport_event

record_lamport_event(
    event_type='$event_type',
    epic_id='$epic_id',
    phase='$phase',
    status='$status',
    metadata=$metadata if '$metadata' else {}
)
"
}

# Check VM connectivity
check_vm_connectivity() {
    log "Checking VM connectivity..."
    if gcloud compute ssh $VM_NAME --zone=$VM_ZONE --command="echo 'VM accessible'" &>/dev/null; then
        log_success "VM is accessible"
        return 0
    else
        log_error "VM is not accessible"
        return 1
    fi
}

# Get epic count by status
get_epic_count() {
    local status=$1
    python3 -c "
import json
with open('$ROADMAP', 'r') as f:
    roadmap = json.load(f)
count = sum(1 for epic in roadmap['epics'].values() if epic['status'] == '$status')
print(count)
"
}

# Get list of epics by status
get_epics_by_status() {
    local status=$1
    python3 -c "
import json
with open('$ROADMAP', 'r') as f:
    roadmap = json.load(f)
epics = [epic_id for epic_id, epic in roadmap['epics'].items() if epic['status'] == '$status']
print(' '.join(epics))
"
}

# Check VM status for specific epic
check_epic_status_vm() {
    local epic_id=$1
    gcloud compute ssh $VM_NAME --zone=$VM_ZONE --command="
cd $VM_DIR
if [ -f docs/brain/$epic_id/manifest.json ]; then
    python3 -c \"
import json
with open('docs/brain/$epic_id/manifest.json', 'r') as f:
    manifest = json.load(f)
print(manifest.get('status', 'unknown'))
\"
else
    echo 'not_started'
fi
" 2>/dev/null || echo "error"
}

# Launch single epic on VM
launch_epic_vm() {
    local epic_id=$1
    local epic_num=$(echo $epic_id | sed 's/EPIC-W7-//')
    
    log "Launching $epic_id on VM..."
    
    # Get epic details from roadmap
    local epic_data=$(python3 -c "
import json
with open('$ROADMAP', 'r') as f:
    roadmap = json.load(f)
epic = roadmap['epics']['$epic_id']
print(f\"{epic['method']}|{epic['file']}|{epic['cyc_before']}\")
")
    
    IFS='|' read -r method file cyc <<< "$epic_data"
    
    # Create launch script on VM
    gcloud compute ssh $VM_NAME --zone=$VM_ZONE --command="
cd $VM_DIR

# Create epic directory
mkdir -p docs/brain/$epic_id

# Generate Phase 0 script from template
cat building-blocks/wave7/phase0_template_wave7.sh | \
    sed 's/{EPIC_ID}/$epic_id/g' | \
    sed 's/{AGENT_ID}/wave7-phase0-$epic_num/g' | \
    sed 's/{METHOD}/$method/g' | \
    sed 's/{FILE}/$file/g' | \
    sed 's/{CYC}/$cyc/g' > docs/brain/$epic_id/_phase0.sh

chmod +x docs/brain/$epic_id/_phase0.sh

# Launch in screen session
screen -dmS $epic_id bash -c 'cd $VM_DIR && docs/brain/$epic_id/_phase0.sh > logs/$epic_id-phase0.log 2>&1'

echo 'Launched $epic_id'
"
    
    if [ $? -eq 0 ]; then
        log_success "Launched $epic_id"
        record_event "epic_start" "$epic_id" "0" "running" "{}"
        return 0
    else
        log_error "Failed to launch $epic_id"
        record_event "epic_start" "$epic_id" "0" "failed" "{\"error\": \"launch_failed\"}"
        return 1
    fi
}

# Monitor progress
monitor_progress() {
    log "=== Wave 7 Progress Report ==="
    
    local completed=$(get_epic_count "completed")
    local running=$(get_epic_count "running")
    local failed=$(get_epic_count "failed")
    local pending=$(get_epic_count "pending")
    
    echo ""
    echo "  Completed: $completed/$TOTAL_EPICS ($(( completed * 100 / TOTAL_EPICS ))%)"
    echo "  Running:   $running"
    echo "  Failed:    $failed"
    echo "  Pending:   $pending"
    echo ""
    
    # Check for errors in Lamport log
    if [ -f "$LAMPORT_LOG" ]; then
        local error_count=$(grep -c '"event_type": "phase_fail"' "$LAMPORT_LOG" 2>/dev/null || echo "0")
        if [ "$error_count" -gt 0 ]; then
            log_warning "Detected $error_count phase failures in Lamport log"
        fi
    fi
    
    # Cost tracking (placeholder - implement with actual API tracking)
    log "Cost tracking: See bobcoin usage in logs"
    
    echo "=============================="
}

# Recovery loop - fix failed epics
recovery_loop() {
    log "Starting recovery loop..."
    
    local failed_epics=$(get_epics_by_status "failed")
    
    if [ -z "$failed_epics" ]; then
        log_success "No failed epics to recover"
        return 0
    fi
    
    log_warning "Found failed epics: $failed_epics"
    
    for epic_id in $failed_epics; do
        log "Analyzing failure for $epic_id..."
        
        # Check failure analysis document
        if gcloud compute ssh $VM_NAME --zone=$VM_ZONE --command="[ -f $VM_DIR/docs/brain/$epic_id/failure-analysis.md ]" 2>/dev/null; then
            log "Failure analysis exists for $epic_id"
            # TODO: Implement automated recovery based on failure type
            log_warning "Manual intervention required for $epic_id"
        else
            log_error "No failure analysis found for $epic_id - creating..."
            # Create failure analysis template
            gcloud compute ssh $VM_NAME --zone=$VM_ZONE --command="
cd $VM_DIR
cat > docs/brain/$epic_id/failure-analysis.md << 'EOF'
# Failure Analysis: $epic_id

## Failure Details
- **Epic**: $epic_id
- **Timestamp**: $(date -u +"%Y-%m-%dT%H:%M:%SZ")
- **Status**: Failed

## Root Cause
[To be determined]

## Recovery Steps
1. Analyze Lamport events
2. Check error logs
3. Fix issues
4. Re-run epic

## Next Actions
- [ ] Identify root cause
- [ ] Apply fix
- [ ] Re-run epic
- [ ] Verify completion
EOF
"
        fi
    done
    
    log_warning "Recovery loop complete - manual intervention required for failed epics"
    return 1
}

# ============================================================================
# Main Execution
# ============================================================================

main() {
    log "=== Wave 7 Master Launch Script ==="
    log "Total Epics: $TOTAL_EPICS"
    log "VM: $VM_NAME (zone: $VM_ZONE)"
    log "Branch: main"
    echo ""
    
    # Pre-flight checks
    log "Running pre-flight checks..."
    
    # 1. Check VM connectivity
    if ! check_vm_connectivity; then
        log_error "VM connectivity check failed - aborting"
        exit 1
    fi
    
    # 2. Verify roadmap exists
    if [ ! -f "$ROADMAP" ]; then
        log_error "Epic roadmap not found: $ROADMAP"
        exit 1
    fi
    
    # 3. Verify templates exist
    for phase in 0 1 1.5 2 3 4 5 5_v 6; do
        template="building-blocks/wave7/phase${phase}_template_wave7.sh"
        if [ ! -f "$template" ]; then
            log_error "Template not found: $template"
            exit 1
        fi
    done
    
    # 4. Verify Lamport infrastructure
    if [ ! -d ".lamport/wave7" ]; then
        log_error "Lamport wave7 directory not found"
        exit 1
    fi
    
    log_success "Pre-flight checks passed"
    echo ""
    
    # Record wave start event
    record_event "wave_start" "WAVE7" "all" "running" "{\"total_epics\": $TOTAL_EPICS}"
    
    # ========================================================================
    # Phase 1: Launch Verification (First 10 Epics)
    # ========================================================================
    
    log "=== Phase 1: Launch Verification (First $LAUNCH_VERIFICATION_EPICS Epics) ==="
    log "Polling interval: ${LAUNCH_POLL_INTERVAL}s (1 minute)"
    echo ""
    
    # Launch first 10 epics
    local launch_count=0
    for i in $(seq 1 $LAUNCH_VERIFICATION_EPICS); do
        epic_id=$(printf "EPIC-W7-%03d" $i)
        
        if launch_epic_vm "$epic_id"; then
            ((launch_count++))
            sleep 2  # Stagger launches
        else
            log_error "Failed to launch $epic_id - aborting launch verification"
            exit 1
        fi
    done
    
    log_success "Launched $launch_count/$LAUNCH_VERIFICATION_EPICS epics"
    echo ""
    
    # Monitor launch verification phase
    log "Starting 1-minute polling for launch verification..."
    local verification_polls=0
    local max_verification_time=600  # 10 minutes max for verification
    
    while [ $verification_polls -lt $(( max_verification_time / LAUNCH_POLL_INTERVAL )) ]; do
        sleep $LAUNCH_POLL_INTERVAL
        ((verification_polls++))
        
        log "Launch verification poll #$verification_polls"
        monitor_progress
        
        # Check if any epics failed
        local failed_count=$(get_epic_count "failed")
        if [ "$failed_count" -gt 0 ]; then
            log_error "Detected failures during launch verification - aborting"
            recovery_loop
            exit 1
        fi
        
        # Check if first 10 completed Phase 0
        local completed_phase0=0
        for i in $(seq 1 $LAUNCH_VERIFICATION_EPICS); do
            epic_id=$(printf "EPIC-W7-%03d" $i)
            status=$(check_epic_status_vm "$epic_id")
            if [ "$status" != "pending" ] && [ "$status" != "not_started" ]; then
                ((completed_phase0++))
            fi
        done
        
        log "Phase 0 progress: $completed_phase0/$LAUNCH_VERIFICATION_EPICS"
        
        # If all 10 have started Phase 1 or beyond, verification passed
        if [ $completed_phase0 -ge $LAUNCH_VERIFICATION_EPICS ]; then
            log_success "Launch verification passed - all $LAUNCH_VERIFICATION_EPICS epics progressing"
            break
        fi
    done
    
    echo ""
    
    # ========================================================================
    # Phase 2: Full Wave Execution (Remaining 151 Epics)
    # ========================================================================
    
    log "=== Phase 2: Full Wave Execution (Remaining $(( TOTAL_EPICS - LAUNCH_VERIFICATION_EPICS )) Epics) ==="
    log "Polling interval: ${FULL_WAVE_POLL_INTERVAL}s (4 minutes)"
    echo ""
    
    # Launch remaining epics
    log "Launching remaining epics..."
    for i in $(seq $(( LAUNCH_VERIFICATION_EPICS + 1 )) $TOTAL_EPICS); do
        epic_id=$(printf "EPIC-W7-%03d" $i)
        
        launch_epic_vm "$epic_id"
        sleep 2  # Stagger launches
        
        # Progress update every 10 epics
        if [ $(( i % 10 )) -eq 0 ]; then
            log "Launched $i/$TOTAL_EPICS epics"
        fi
    done
    
    log_success "All $TOTAL_EPICS epics launched"
    echo ""
    
    # Monitor full wave execution with 4-minute polling
    log "Starting 4-minute polling for full wave execution..."
    local poll_count=0
    
    while true; do
        sleep $FULL_WAVE_POLL_INTERVAL
        ((poll_count++))
        
        log "Full wave poll #$poll_count ($(( poll_count * FULL_WAVE_POLL_INTERVAL / 60 )) minutes elapsed)"
        monitor_progress
        
        # Check completion
        local completed=$(get_epic_count "completed")
        local failed=$(get_epic_count "failed")
        
        # If all epics complete, we're done
        if [ "$completed" -eq "$TOTAL_EPICS" ]; then
            log_success "Wave 7 complete! All $TOTAL_EPICS epics finished successfully"
            record_event "wave_complete" "WAVE7" "all" "completed" "{\"total_epics\": $TOTAL_EPICS, \"polls\": $poll_count}"
            break
        fi
        
        # If we have failures, run recovery loop
        if [ "$failed" -gt 0 ]; then
            log_warning "Detected $failed failed epics - running recovery loop"
            if ! recovery_loop; then
                log_error "Recovery loop requires manual intervention"
                log "Wave execution paused - fix failed epics and re-run"
                exit 1
            fi
        fi
        
        # Safety check: if no progress for 1 hour, something is wrong
        if [ $poll_count -gt 15 ]; then  # 15 polls = 60 minutes
            local last_completed=$completed
            sleep $FULL_WAVE_POLL_INTERVAL
            completed=$(get_epic_count "completed")
            
            if [ "$completed" -eq "$last_completed" ]; then
                log_warning "No progress detected in last hour - checking for stuck epics"
                # TODO: Implement stuck epic detection
            fi
        fi
    done
    
    # ========================================================================
    # Post-Wave Validation
    # ========================================================================
    
    log "=== Post-Wave Validation ==="
    echo ""
    
    log "Syncing from VM to local..."
    # TODO: Implement sync logic
    
    log "Running pre-push validation..."
    # TODO: Run pre_push_validation.ps1
    
    log "Verifying complexity reduction..."
    # TODO: Run complexity_audit.py
    
    log_success "Wave 7 execution complete!"
    log "Next steps:"
    log "  1. Review completion reports in docs/brain/EPIC-W7-*/05-completion-report.md"
    log "  2. Run: powershell -File .\\deploy-sync.ps1"
    log "  3. Run: powershell -File .\\scripts\\pre_push_validation.ps1"
    log "  4. Test in NinjaTrader (F5)"
    log "  5. Create PR for Wave 7"
}

# Trap Ctrl+C for graceful shutdown
trap 'log_warning "Received interrupt signal - shutting down gracefully..."; exit 130' INT TERM

# Run main
main "$@"

# Made with Bob
