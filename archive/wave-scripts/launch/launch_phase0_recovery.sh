#!/bin/bash
# Wave 4 Phase 0 Recovery Script
# Relaunch 20 missing/failed epics

PHASE=0
DELAY=12

# Missing epics from aborted first launch (003-019)
MISSING_EPICS=(003 004 005 006 007 008 009 010 011 012 013 014 015 016 017 018 019)

# Failed epics with file write errors (033, 044, 047)
FAILED_EPICS=(033 044 047)

# Combine all epics to relaunch
ALL_EPICS=("${MISSING_EPICS[@]}" "${FAILED_EPICS[@]}")

echo "=========================================="
echo "Wave 4 Phase 0 - Recovery Launch"
echo "Relaunching: ${#ALL_EPICS[@]} epics"
echo "Missing: ${#MISSING_EPICS[@]} | Failed: ${#FAILED_EPICS[@]}"
echo "=========================================="
echo ""

# Launch each epic
for i in "${!ALL_EPICS[@]}"; do
    EPIC="${ALL_EPICS[$i]}"
    EPIC_NUM=$((i + 1))
    
    echo "[$(date '+%H:%M:%S')] Launching EPIC-CCN-${EPIC} (${EPIC_NUM}/${#ALL_EPICS[@]})"
    
    # Launch in screen session
    screen -dmS p0-${EPIC} bash -l -c \
        "./_p0_${EPIC}.sh 2>&1 | tee logs/phase0/EPIC-CCN-${EPIC}.log"
    
    # Wait before next launch
    sleep ${DELAY}
done

echo ""
echo "[$(date)] All ${#ALL_EPICS[@]} epics launched!"
echo ""

# Initial check (1 minute)
echo "Waiting 1 minute for agents to start..."
sleep 60

COMPLETED=$(ls docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l)
RUNNING=$(screen -ls | grep -c "p0-" || echo "0")
echo "Completed: ${COMPLETED}/80 epics"
echo "Running: ${RUNNING} agents"
echo ""

# Continuous polling (4-minute intervals)
echo "Starting 4-minute polling cycle..."
POLL_COUNT=1
while true; do
    sleep 240
    
    COMPLETED=$(ls docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l)
    RUNNING=$(screen -ls | grep -c "p0-" || echo "0")
    
    echo "[Poll #${POLL_COUNT}] Completed: ${COMPLETED}/80 | Running: ${RUNNING}"
    
    # Exit when all 80 complete
    if [ "$COMPLETED" -eq 80 ]; then
        echo ""
        echo "=========================================="
        echo "SUCCESS: All 80 epics completed!"
        echo "=========================================="
        break
    fi
    
    POLL_COUNT=$((POLL_COUNT + 1))
done

echo ""
echo "Recovery complete. Verify with:"
echo "  ls docs/brain/EPIC-CCN-*/00-hotspots.md | wc -l"

# Made with Bob
