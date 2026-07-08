#!/bin/bash
# Phase 6 Full Wave - 79 Epics (skip EPIC-CCN-016)
set -e
cd /home/malhitticrypto/universal-or-strategy

echo "[$(date)] Starting Phase 6 full wave (79 epics)"
mkdir -p logs/phase6

# Launch all epics except EPIC-CCN-016
for i in $(seq -f "%03g" 1 80); do
    if [ "$i" == "016" ]; then
        echo "[$(date)] Skipping EPIC-CCN-016 (deferred)"
        continue
    fi
    
    EPIC="EPIC-CCN-${i}"
    echo "[$(date)] Launching ${EPIC}"
    
    screen -dmS p6-${i} bash -l -c \
        "./scripts/wave4/_p6_${i}.sh 2>&1 | tee logs/phase6/${EPIC}.log"
    
    sleep 12
done

echo "[$(date)] All 79 epics launched for Phase 6"
echo "Monitor with: screen -ls | grep -c 'p6-'"
echo "Check files: ls docs/brain/EPIC-CCN-*/06-completion-report.md | wc -l"

# Made with Bob
