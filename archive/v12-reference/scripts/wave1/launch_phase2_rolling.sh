#!/bin/bash
# Rolling launch script for Phase 2 (Architecture Planning)
# Launches all pending epics with 30-second delays to prevent VM overload

set -e

DELAY=15  # seconds between launches (optimized for 25-min execution)
LOG_DIR="logs/phase2"

# Create log directory
mkdir -p "$LOG_DIR"

# Get list of Phase 2 scripts
SCRIPTS=($(ls _p2_*.sh 2>/dev/null | sort))

if [ ${#SCRIPTS[@]} -eq 0 ]; then
    echo "ERROR: No Phase 2 scripts found (_p2_*.sh)"
    exit 1
fi

echo "=========================================="
echo "Wave 1 Phase 2 Rolling Launch"
echo "=========================================="
echo "Total epics: ${#SCRIPTS[@]}"
echo "Launch delay: ${DELAY} seconds"
echo "Estimated launch time: $((${#SCRIPTS[@]} * DELAY / 60)) minutes"
echo "=========================================="
echo ""

# Launch each script with delay
for i in "${!SCRIPTS[@]}"; do
    script="${SCRIPTS[$i]}"
    epic_id=$(echo "$script" | sed 's/_p2_\([0-9]*\)\.sh/\1/')
    session_name="p2-${epic_id}"
    
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] Launching EPIC-${epic_id} ($(($i + 1))/${#SCRIPTS[@]})"
    
    # Launch in screen session
    screen -dmS "$session_name" bash -l -c "./$script 2>&1 | tee $LOG_DIR/EPIC-${epic_id}.log"
    
    # Check if screen session started
    if screen -ls | grep -q "$session_name"; then
        echo "  ✓ Screen session '$session_name' started"
    else
        echo "  ✗ WARNING: Screen session '$session_name' failed to start"
    fi
    
    # Don't delay after last epic
    if [ $i -lt $((${#SCRIPTS[@]} - 1)) ]; then
        echo "  Waiting ${DELAY} seconds before next launch..."
        sleep $DELAY
    fi
done

echo ""
echo "=========================================="
echo "All agents launched!"
echo "=========================================="
echo ""
echo "Monitor with:"
echo "  screen -ls                    # List all sessions"
echo "  screen -r p2-001              # Attach to specific session (Ctrl+A, D to detach)"
echo "  tail -f $LOG_DIR/EPIC-001.log # Watch specific log"
echo ""
echo "Check completion:"
echo "  ls docs/brain/EPIC-*/02-architecture-plan.md 2>/dev/null | wc -l"
echo ""
echo "Check VM load:"
echo "  uptime && free -h"
echo ""

# Made with Bob
