#!/bin/bash
# Wave 7 Status Check Script
# Version: 1.0
# Purpose: Detailed status monitoring for Wave 7 execution

set -e

# Configuration
WAVE_ID="wave7"
TOTAL_EPICS=161
VM_NAME="v12-test-golden-v2"
VM_ZONE="us-central1-a"
VM_USER="malhitticrypto"
VM_DIR="/home/malhitticrypto/universal-or-strategy"
ROADMAP="epic_roadmap_wave7.json"
LAMPORT_LOG=".lamport/wave7/event_log.jsonl"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# ============================================================================
# Helper Functions
# ============================================================================

print_header() {
    echo ""
    echo -e "${CYAN}========================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}========================================${NC}"
}

print_section() {
    echo ""
    echo -e "${BLUE}--- $1 ---${NC}"
}

# Get epic count by status from local roadmap
get_local_epic_count() {
    local status=$1
    python3 -c "
import json
with open('$ROADMAP', 'r') as f:
    roadmap = json.load(f)
count = sum(1 for epic in roadmap['epics'].values() if epic['status'] == '$status')
print(count)
" 2>/dev/null || echo "0"
}

# Get VM epic count by checking manifest files
get_vm_epic_count() {
    local status=$1
    gcloud compute ssh $VM_NAME --zone=$VM_ZONE --command="
cd $VM_DIR
count=0
for manifest in docs/brain/EPIC-W7-*/manifest.json; do
    if [ -f \"\$manifest\" ]; then
        epic_status=\$(python3 -c \"
import json
try:
    with open('\$manifest', 'r') as f:
        m = json.load(f)
    print(m.get('status', 'unknown'))
except:
    print('error')
\")
        if [ \"\$epic_status\" = \"$status\" ]; then
            ((count++))
        fi
    fi
done
echo \$count
" 2>/dev/null || echo "0"
}

# Get running screen sessions on VM
get_running_sessions() {
    gcloud compute ssh $VM_NAME --zone=$VM_ZONE --command="
screen -ls | grep -c 'EPIC-W7-' || echo '0'
" 2>/dev/null || echo "0"
}

# Get phase distribution from VM
get_phase_distribution() {
    gcloud compute ssh $VM_NAME --zone=$VM_ZONE --command="
cd $VM_DIR
echo 'Phase Distribution:'
for phase in 0 1 1.5 2 3 4 5 5.V 6; do
    count=0
    for manifest in docs/brain/EPIC-W7-*/manifest.json; do
        if [ -f \"\$manifest\" ]; then
            current_phase=\$(python3 -c \"
import json
try:
    with open('\$manifest', 'r') as f:
        m = json.load(f)
    print(m.get('current_phase', 'unknown'))
except:
    print('error')
\")
            if [ \"\$current_phase\" = \"$phase\" ]; then
                ((count++))
            fi
        fi
    done
    if [ \$count -gt 0 ]; then
        echo \"  Phase $phase: \$count epics\"
    fi
done
" 2>/dev/null
}

# Check for recent errors in Lamport log
check_lamport_errors() {
    if [ -f "$LAMPORT_LOG" ]; then
        local error_count=$(grep -c '"event_type": "phase_fail"' "$LAMPORT_LOG" 2>/dev/null || echo "0")
        local last_5_errors=$(grep '"event_type": "phase_fail"' "$LAMPORT_LOG" 2>/dev/null | tail -5 || echo "")
        
        if [ "$error_count" -gt 0 ]; then
            echo -e "${RED}Found $error_count phase failures${NC}"
            if [ -n "$last_5_errors" ]; then
                echo "Last 5 failures:"
                echo "$last_5_errors" | while read line; do
                    epic=$(echo "$line" | grep -o 'EPIC-W7-[0-9]*' || echo "unknown")
                    phase=$(echo "$line" | grep -o '"phase": "[^"]*"' | cut -d'"' -f4 || echo "unknown")
                    echo "  - $epic (Phase $phase)"
                done
            fi
        else
            echo -e "${GREEN}No phase failures detected${NC}"
        fi
    else
        echo -e "${YELLOW}Lamport log not found${NC}"
    fi
}

# Check VM disk usage
check_vm_disk_usage() {
    gcloud compute ssh $VM_NAME --zone=$VM_ZONE --command="
df -h $VM_DIR | tail -1 | awk '{print \"Disk Usage: \" \$3 \" / \" \$2 \" (\" \$5 \" used)\"}' 
" 2>/dev/null || echo "Unable to check disk usage"
}

# Get average phase duration from Lamport log
get_phase_durations() {
    if [ -f "$LAMPORT_LOG" ]; then
        python3 -c "
import json
from collections import defaultdict
from datetime import datetime

phase_times = defaultdict(list)

try:
    with open('$LAMPORT_LOG', 'r') as f:
        events = [json.loads(line) for line in f if line.strip()]
    
    # Group by epic and phase
    epic_phases = defaultdict(dict)
    for event in events:
        epic_id = event.get('epic_id', '')
        phase = event.get('phase', '')
        event_type = event.get('event_type', '')
        timestamp = event.get('timestamp', '')
        
        if epic_id and phase:
            key = f'{epic_id}_{phase}'
            if event_type == 'phase_start':
                epic_phases[key]['start'] = timestamp
            elif event_type == 'phase_complete':
                epic_phases[key]['end'] = timestamp
    
    # Calculate durations
    for key, times in epic_phases.items():
        if 'start' in times and 'end' in times:
            phase = key.split('_')[1]
            start = datetime.fromisoformat(times['start'].replace('Z', '+00:00'))
            end = datetime.fromisoformat(times['end'].replace('Z', '+00:00'))
            duration = (end - start).total_seconds() / 60  # minutes
            phase_times[phase].append(duration)
    
    # Print averages
    print('Average Phase Durations:')
    for phase in ['0', '1', '1.5', '2', '3', '4', '5', '5.V', '6']:
        if phase in phase_times and phase_times[phase]:
            avg = sum(phase_times[phase]) / len(phase_times[phase])
            count = len(phase_times[phase])
            print(f'  Phase {phase}: {avg:.1f} min (n={count})')
except Exception as e:
    print(f'Error calculating durations: {e}')
" 2>/dev/null || echo "Unable to calculate phase durations"
    fi
}

# ============================================================================
# Main Status Report
# ============================================================================

main() {
    print_header "Wave 7 Status Report - $(date +'%Y-%m-%d %H:%M:%S')"
    
    # Overall Progress
    print_section "Overall Progress"
    
    local vm_completed=$(get_vm_epic_count "completed")
    local vm_running=$(get_vm_epic_count "running")
    local vm_failed=$(get_vm_epic_count "failed")
    local vm_pending=$(get_vm_epic_count "pending")
    
    local completion_pct=$(( vm_completed * 100 / TOTAL_EPICS ))
    
    echo "  Completed: $vm_completed/$TOTAL_EPICS ($completion_pct%)"
    echo "  Running:   $vm_running"
    echo "  Failed:    $vm_failed"
    echo "  Pending:   $vm_pending"
    
    # Progress bar
    local bar_width=50
    local filled=$(( completion_pct * bar_width / 100 ))
    local empty=$(( bar_width - filled ))
    printf "  ["
    printf "%${filled}s" | tr ' ' '='
    printf "%${empty}s" | tr ' ' '-'
    printf "] %d%%\n" $completion_pct
    
    # VM Status
    print_section "VM Status"
    
    local running_sessions=$(get_running_sessions)
    echo "  Active screen sessions: $running_sessions"
    check_vm_disk_usage
    
    # Phase Distribution
    print_section "Phase Distribution"
    get_phase_distribution
    
    # Error Analysis
    print_section "Error Analysis"
    check_lamport_errors
    
    # Performance Metrics
    print_section "Performance Metrics"
    get_phase_durations
    
    # Recent Activity (last 10 Lamport events)
    print_section "Recent Activity (Last 10 Events)"
    if [ -f "$LAMPORT_LOG" ]; then
        tail -10 "$LAMPORT_LOG" | while read line; do
            epic=$(echo "$line" | grep -o 'EPIC-W7-[0-9]*' || echo "unknown")
            phase=$(echo "$line" | grep -o '"phase": "[^"]*"' | cut -d'"' -f4 || echo "?")
            event=$(echo "$line" | grep -o '"event_type": "[^"]*"' | cut -d'"' -f4 || echo "?")
            status=$(echo "$line" | grep -o '"status": "[^"]*"' | cut -d'"' -f4 || echo "?")
            
            case $event in
                "phase_start")
                    echo -e "  ${BLUE}▶${NC} $epic Phase $phase started"
                    ;;
                "phase_complete")
                    echo -e "  ${GREEN}✓${NC} $epic Phase $phase completed"
                    ;;
                "phase_fail")
                    echo -e "  ${RED}✗${NC} $epic Phase $phase failed"
                    ;;
                *)
                    echo -e "  ${YELLOW}•${NC} $epic $event (Phase $phase)"
                    ;;
            esac
        done
    else
        echo "  No Lamport log found"
    fi
    
    # Recommendations
    print_section "Recommendations"
    
    if [ "$vm_failed" -gt 0 ]; then
        echo -e "  ${RED}⚠${NC}  Run recovery loop for $vm_failed failed epics"
    fi
    
    if [ "$vm_completed" -eq "$TOTAL_EPICS" ]; then
        echo -e "  ${GREEN}✓${NC} Wave 7 complete! Run post-wave validation"
    elif [ "$vm_running" -eq 0 ] && [ "$vm_pending" -gt 0 ]; then
        echo -e "  ${YELLOW}⚠${NC}  No running epics but $vm_pending pending - check for stuck launches"
    fi
    
    print_header "End of Status Report"
    echo ""
}

# Run main
main "$@"

# Made with Bob
