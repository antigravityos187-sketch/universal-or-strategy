#!/bin/bash
# Wave 4 Phase 1 Master Launch Script
# Launches all 80 epics with staggered delays (12-54 seconds)

set -e
cd /home/malhitticrypto/universal-or-strategy

PHASE=1
EPICS=($(seq -f "%03g" 1 80))
BASE_DELAY=12
MAX_DELAY=54

echo "[$(date)] Starting Wave 4 Phase 1 launch (80 epics)"
echo "[$(date)] Staggered delays: ${BASE_DELAY}-${MAX_DELAY} seconds"

for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    
    # Calculate staggered delay (12-54 seconds)
    DELAY=$((BASE_DELAY + (i % (MAX_DELAY - BASE_DELAY + 1))))
    
    echo "[$(date)] Launching EPIC-CCN-${EPIC} (delay: ${DELAY}s)"
    
    # Launch in screen session
    screen -dmS p${PHASE}-${EPIC} bash -l -c \
        "./_p${PHASE}_${EPIC}.sh 2>&1 | tee logs/phase${PHASE}/EPIC-CCN-${EPIC}.log"
    
    # Wait before next launch
    sleep ${DELAY}
done

echo "[$(date)] All 80 epics launched for Phase ${PHASE}"
echo "[$(date)] Monitor with: screen -ls"
echo "[$(date)] Check completion: ls docs/brain/EPIC-CCN-*/01-scope.md | wc -l"

# Made with Bob
