#!/bin/bash
# Wave 4 Phase 0 - Remaining 59 Epics (020-080) - FOREGROUND VERSION
# Generated: 2026-06-14
# Launches with constant 12s delays and cost-optimized polling

PHASE=0
DELAY=12
EPICS=($(seq -f "%03g" 20 80))

echo "=========================================="
echo "Wave 4 Phase 0 - Remaining 59 Epics"
echo "Launch Strategy: Constant 12s delays"
echo "Polling Strategy: 1 min initial + 4 min continuous"
echo "=========================================="
echo ""

# Create logs directory
mkdir -p logs/phase0

# Launch all epics with constant delay
echo "[$(date '+%Y-%m-%d %H:%M:%S')] Starting launches..."
for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    EPIC_NUM=$((i + 1))
    
    echo "[$(date '+%H:%M:%S')] Launching EPIC-CCN-${EPIC} (${EPIC_NUM}/59)"
    
    # Launch in screen session
    screen -dmS p0-${EPIC} bash -l -c \
        "./_p0_${EPIC}.sh 2>&1 | tee logs/phase0/EPIC-CCN-${EPIC}.log"
    
    # Constant delay between launches
    sleep ${DELAY}
done

echo ""
echo "[$(date '+%Y-%m-%d %H:%M:%S')] All 59 epics launched!"
echo ""

# Initial status check (1 minute)
echo "[$(date '+%H:%M:%S')] Waiting 1 minute before first status check..."
sleep 60

echo ""
echo "=========================================="
echo "Initial Status Check"
echo "=========================================="
COMPLETED=$(ls docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l)
RUNNING=$(screen -ls | grep -c "p0-" || echo "0")
echo "Completed: ${COMPLETED}/80 epics"
echo "Running: ${RUNNING} agents"
echo ""

# Continuous polling (4-minute intervals for cache optimization)
POLL_COUNT=1
while true; do
    sleep 240  # 4 minutes (90% cache hit rate)
    
    echo "=========================================="
    echo "Poll #${POLL_COUNT} - $(date '+%Y-%m-%d %H:%M:%S')"
    echo "=========================================="
    
    COMPLETED=$(ls docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l)
    RUNNING=$(screen -ls | grep -c "p0-" || echo "0")
    
    echo "Completed: ${COMPLETED}/80 epics"
    echo "Running: ${RUNNING} agents"
    echo ""
    
    # Exit when all complete
    if [ "$COMPLETED" -eq 80 ]; then
        echo "=========================================="
        echo "ALL 80 EPICS COMPLETE!"
        echo "=========================================="
        break
    fi
    
    POLL_COUNT=$((POLL_COUNT + 1))
done

echo ""
echo "Wave 4 Phase 0 execution complete!"
echo "Final count: ${COMPLETED}/80 epics"

# Made with Bob
