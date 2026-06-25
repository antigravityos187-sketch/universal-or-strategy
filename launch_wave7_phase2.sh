#!/bin/bash
# Wave 7 Phase 2 Master Launch Script
# V12.52 - Architecture Planning for 161 epics
# Cost-Optimized: 4-minute polling intervals (88% cost reduction)

set -euo pipefail

echo "=========================================="
echo "Wave 7 Phase 2 Master Launch"
echo "Architecture Planning: 161 epics"
echo "Cost-Optimized: 4-minute polling"
echo "=========================================="
echo ""

# Configuration
PHASE="2"
POLL_INTERVAL=240  # 4 minutes (cost optimization)
MAX_PARALLEL=10    # Parallel execution limit
LOG_DIR="logs/wave7/phase2"

# Create log directory
mkdir -p "$LOG_DIR"

# Get list of all Phase 2 scripts
SCRIPTS=($(ls _p2_*.sh | sort -V))
TOTAL=${#SCRIPTS[@]}

echo "Found $TOTAL Phase 2 scripts to execute"
echo "Polling interval: ${POLL_INTERVAL}s (4 minutes)"
echo "Max parallel: $MAX_PARALLEL"
echo ""

# Launch all scripts in parallel (with limit)
echo "Step 1: Launching Phase 2 scripts..."
echo "-----------------------------------"

LAUNCHED=0
RUNNING=()

for script in "${SCRIPTS[@]}"; do
    # Wait if we've hit the parallel limit
    while [ ${#RUNNING[@]} -ge $MAX_PARALLEL ]; do
        # Check which processes are still running
        NEW_RUNNING=()
        for pid in "${RUNNING[@]}"; do
            if kill -0 "$pid" 2>/dev/null; then
                NEW_RUNNING+=("$pid")
            fi
        done
        RUNNING=("${NEW_RUNNING[@]}")
        
        if [ ${#RUNNING[@]} -ge $MAX_PARALLEL ]; then
            sleep 10  # Brief wait before checking again
        fi
    done
    
    # Launch script in background
    epic_num=$(echo "$script" | sed 's/_p2_\([0-9]*\)\.sh/\1/')
    epic_id="EPIC-W7-$epic_num"
    
    echo "Launching: $script ($epic_id)"
    bash "$script" > "$LOG_DIR/${epic_id}.log" 2>&1 &
    pid=$!
    RUNNING+=("$pid")
    LAUNCHED=$((LAUNCHED + 1))
    
    # 12-second delay between launches (prevent API rate limits)
    if [ $LAUNCHED -lt $TOTAL ]; then
        sleep 12
    fi
done

echo ""
echo "✅ Launched $LAUNCHED Phase 2 scripts"
echo ""

# Step 2: Monitor progress with 4-minute polling
echo "Step 2: Monitoring progress (4-minute polling)..."
echo "-----------------------------------"

COMPLETED=0
FAILED=0
LAST_COMPLETED=0

while [ $COMPLETED -lt $TOTAL ]; do
    # Count completed epics
    COMPLETED=$(find docs/brain/EPIC-W7-* -name "02-architecture-plan.md" 2>/dev/null | wc -l)
    
    # Count failed epics (check Lamport event log)
    FAILED=$(python3 -c "
import json
from pathlib import Path

failed = 0
event_log = Path('.lamport/wave7/event_log.jsonl')
if event_log.exists():
    with open(event_log) as f:
        for line in f:
            event = json.loads(line)
            if event.get('phase') == '2' and event.get('event_type') == 'phase_failed':
                failed += 1

print(failed)
" 2>/dev/null || echo "0")
    
    # Calculate progress
    IN_PROGRESS=$((TOTAL - COMPLETED - FAILED))
    PROGRESS_PCT=$((COMPLETED * 100 / TOTAL))
    
    # Show progress
    echo ""
    echo "Progress: $COMPLETED/$TOTAL complete ($PROGRESS_PCT%)"
    echo "  Completed: $COMPLETED"
    echo "  Failed: $FAILED"
    echo "  In Progress: $IN_PROGRESS"
    
    # Show newly completed epics
    if [ $COMPLETED -gt $LAST_COMPLETED ]; then
        NEW_COMPLETED=$((COMPLETED - LAST_COMPLETED))
        echo "  ✅ $NEW_COMPLETED new completions since last check"
        LAST_COMPLETED=$COMPLETED
    fi
    
    # Check if all done
    if [ $COMPLETED -ge $TOTAL ]; then
        break
    fi
    
    # Wait 4 minutes before next poll (cost optimization)
    echo ""
    echo "Next poll in ${POLL_INTERVAL}s (4 minutes)..."
    sleep $POLL_INTERVAL
done

echo ""
echo "=========================================="
echo "Wave 7 Phase 2 Complete"
echo "=========================================="
echo "Total: $TOTAL"
echo "Completed: $COMPLETED"
echo "Failed: $FAILED"
echo ""

if [ $FAILED -gt 0 ]; then
    echo "⚠️  $FAILED epics failed - review logs in $LOG_DIR"
    echo ""
    echo "Failed epics:"
    python3 -c "
import json
from pathlib import Path

event_log = Path('.lamport/wave7/event_log.jsonl')
if event_log.exists():
    failed_epics = set()
    with open(event_log) as f:
        for line in f:
            event = json.loads(line)
            if event.get('phase') == '2' and event.get('event_type') == 'phase_failed':
                failed_epics.add(event.get('epic_id'))
    
    for epic_id in sorted(failed_epics):
        print(f'  - {epic_id}')
"
    exit 1
else
    echo "✅ All epics completed successfully!"
    exit 0
fi

# Made with Bob
