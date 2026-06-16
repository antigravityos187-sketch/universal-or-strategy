#!/bin/bash
# Master launcher for Phase 0 (80 epics with Jane Street integration)
# Uses 12-second staggered delays

PHASE=0
EPICS=($(seq -f "%03g" 1 80))
DELAY=12

echo "[$(date)] Starting Phase 0 launch for 80 epics"
echo "[$(date)] Using 12-second delays between launches"

for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    
    echo "[$(date)] Launching EPIC-CCN-${EPIC} ($(($i + 1))/80)"
    
    # Launch in screen session
    screen -dmS p0-${EPIC} bash -l -c \
        "./_p0_${EPIC}.sh 2>&1 | tee logs/phase0/EPIC-CCN-${EPIC}.log"
    
    # Wait before next launch (except for last epic)
    if [ $i -lt $((${#EPICS[@]} - 1)) ]; then
        sleep ${DELAY}
    fi
done

echo "[$(date)] All 80 epics launched for Phase 0"
echo "[$(date)] Total launch time: $((80 * 12 / 60)) minutes"
echo ""
echo "Polling protocol:"
echo "  1. Wait 1 minute"
echo "  2. Check: screen -ls"
echo "  3. Poll every 4 minutes until complete"
