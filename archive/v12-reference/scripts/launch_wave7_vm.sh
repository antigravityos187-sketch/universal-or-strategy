#!/bin/bash
# Wave 7 Master Launch Script (VM-Native Version)
# Version: 1.0
# Purpose: Launch and monitor all 161 Wave 7 epics directly on VM
# Run this ON the VM, not from local machine

set -e

# ============================================================================
# Configuration
# ============================================================================

WAVE_ID="wave7"
TOTAL_EPICS=161
ROADMAP="epic_roadmap_wave7.json"
LAMPORT_LOG=".lamport/wave7/event_log.jsonl"

# Polling configuration
LAUNCH_VERIFICATION_EPICS=10
LAUNCH_POLL_INTERVAL=60
FULL_WAVE_POLL_INTERVAL=240

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

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
    
    python3 -c "
import sys
sys.path.insert(0, 'scripts')
try:
    from epic_manifest import record_lamport_event
    record_lamport_event('$event_type', '$epic_id', '$phase', '$status', {})
except Exception as e:
    print(f'Warning: Could not record event: {e}', file=sys.stderr)
" 2>/dev/null || true
}

# Get epic count by status from manifests
get_epic_count() {
    local status=$1
    local count=0
    
    for manifest in docs/brain/EPIC-W7-*/manifest.json; do
        if [ -f "$manifest" ]; then
            epic_status=$(python3 -c "
import json
try:
    with open('$manifest', 'r') as f:
        m = json.load(f)
    print(m.get('status', 'unknown'))
except:
    print('error')
" 2>/dev/null)
            
            if [ "$epic_status" = "$status" ]; then
                ((count++))
            fi
        fi
    done
    
    echo $count
}

# Launch single epic
launch_epic() {
    local epic_id=$1
    local epic_num=$(echo $epic_id | sed 's/EPIC-W7-0*//')
    
    log "Launching $epic_id..."
    
    # Get epic details
    local epic_data=$(python3 -c "
import json
try:
    with open('$ROADMAP', 'r') as f:
        roadmap = json.load(f)
    epic = roadmap['epics']['$epic_id']
    print(f\"{epic['method']}|{epic['file']}|{epic['cyc_before']}\")
except Exception as e:
    print(f'ERROR|ERROR|0')
")
    
    if [[ "$epic_data" == ERROR* ]]; then
        log_error "Failed to get epic data for $epic_id"
        return 1
    fi
    
    IFS='|' read -r method file cyc <<< "$epic_data"
    
    # Create epic directory
    mkdir -p docs/brain/$epic_id
    
    # Generate Phase 0 script from template
    cat building-blocks/wave7/phase0_template_wave7.sh | \
        sed "s/{EPIC_ID}/$epic_id/g" | \
        sed "s/{AGENT_ID}/wave7-phase0-$epic_num/g" | \
        sed "s/{METHOD}/$method/g" | \
        sed "s/{FILE}/$file/g" | \
        sed "s/{CYC}/$cyc/g" > docs/brain/$epic_id/_phase0.sh
    
    chmod +x docs/brain/$epic_id/_phase0.sh
    
    # Launch in screen session
    screen -dmS $epic_id bash -c "cd $(pwd) && docs/brain/$epic_id/_phase0.sh > logs/$epic_id-phase0.log 2>&1"
    
    if [ $? -eq 0 ]; then
        log_success "Launched $epic_id"
        record_event "epic_start" "$epic_id" "0" "running"
        return 0
    else
        log_error "Failed to launch $epic_id"
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
    
    # Check screen sessions
    local sessions=$(screen -ls 2>/dev/null | grep -c "EPIC-W7-" || echo "0")
    echo "  Active screen sessions: $sessions"
    echo ""
    
    echo "=============================="
}

# ============================================================================
# Main Execution
# ============================================================================

main() {
    log "=== Wave 7 Master Launch Script (VM-Native) ==="
    log "Total Epics: $TOTAL_EPICS"
    log "Working Directory: $(pwd)"
    echo ""
    
    # Pre-flight checks
    log "Running pre-flight checks..."
    
    if [ ! -f "$ROADMAP" ]; then
        log_error "Epic roadmap not found: $ROADMAP"
        exit 1
    fi
    
    if [ ! -d "building-blocks/wave7" ]; then
        log_error "Templates directory not found"
        exit 1
    fi
    
    if [ ! -d ".lamport/wave7" ]; then
        log_error "Lamport directory not found"
        exit 1
    fi
    
    # Create logs directory
    mkdir -p logs
    
    log_success "Pre-flight checks passed"
    echo ""
    
    # Record wave start
    record_event "wave_start" "WAVE7" "all" "running"
    
    # ========================================================================
    # Phase 1: Launch Verification (First 10 Epics)
    # ========================================================================
    
    log "=== Phase 1: Launch Verification (First $LAUNCH_VERIFICATION_EPICS Epics) ==="
    log "Polling interval: ${LAUNCH_POLL_INTERVAL}s (1 minute)"
    echo ""
    
    local launch_count=0
    for i in $(seq 1 $LAUNCH_VERIFICATION_EPICS); do
        epic_id=$(printf "EPIC-W7-%03d" $i)
        
        if launch_epic "$epic_id"; then
            ((launch_count++))
            sleep 2
        else
            log_error "Failed to launch $epic_id - aborting"
            exit 1
        fi
    done
    
    log_success "Launched $launch_count/$LAUNCH_VERIFICATION_EPICS epics"
    echo ""
    
    # Monitor launch verification
    log "Monitoring launch verification (1-minute polls)..."
    local verification_polls=0
    local max_verification_polls=10
    
    while [ $verification_polls -lt $max_verification_polls ]; do
        sleep $LAUNCH_POLL_INTERVAL
        ((verification_polls++))
        
        log "Launch verification poll #$verification_polls"
        monitor_progress
        
        local failed_count=$(get_epic_count "failed")
        if [ "$failed_count" -gt 0 ]; then
            log_error "Detected failures during launch verification"
            exit 1
        fi
        
        # Check if first 10 have progressed
        local progressed=0
        for i in $(seq 1 $LAUNCH_VERIFICATION_EPICS); do
            epic_id=$(printf "EPIC-W7-%03d" $i)
            if [ -f "docs/brain/$epic_id/manifest.json" ]; then
                ((progressed++))
            fi
        done
        
        log "Progressed: $progressed/$LAUNCH_VERIFICATION_EPICS"
        
        if [ $progressed -ge $LAUNCH_VERIFICATION_EPICS ]; then
            log_success "Launch verification passed"
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
    
    log "Launching remaining epics..."
    for i in $(seq $(( LAUNCH_VERIFICATION_EPICS + 1 )) $TOTAL_EPICS); do
        epic_id=$(printf "EPIC-W7-%03d" $i)
        launch_epic "$epic_id"
        sleep 2
        
        if [ $(( i % 10 )) -eq 0 ]; then
            log "Launched $i/$TOTAL_EPICS epics"
        fi
    done
    
    log_success "All $TOTAL_EPICS epics launched"
    echo ""
    
    # Monitor full wave
    log "Monitoring full wave execution (4-minute polls)..."
    local poll_count=0
    
    while true; do
        sleep $FULL_WAVE_POLL_INTERVAL
        ((poll_count++))
        
        log "Full wave poll #$poll_count ($(( poll_count * FULL_WAVE_POLL_INTERVAL / 60 )) minutes elapsed)"
        monitor_progress
        
        local completed=$(get_epic_count "completed")
        local failed=$(get_epic_count "failed")
        
        if [ "$completed" -eq "$TOTAL_EPICS" ]; then
            log_success "Wave 7 complete! All $TOTAL_EPICS epics finished"
            record_event "wave_complete" "WAVE7" "all" "completed"
            break
        fi
        
        if [ "$failed" -gt 0 ]; then
            log_warning "Detected $failed failed epics - manual intervention required"
        fi
    done
    
    log_success "Wave 7 execution complete!"
    log "Next steps:"
    log "  1. Review completion reports"
    log "  2. Sync to local: deploy-sync.ps1"
    log "  3. Run pre-push validation"
    log "  4. Test in NinjaTrader (F5)"
}

# Trap Ctrl+C
trap 'log_warning "Received interrupt - shutting down..."; exit 130' INT TERM

# Run main
main "$@"

# Made with Bob
