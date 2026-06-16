#!/bin/bash
# Launch 7 Phase 5 recovery epics
set -e
cd /home/malhitticrypto/universal-or-strategy

echo "[$(date)] Step 2: Launching 7 Phase 5 recovery epics"
mkdir -p logs/phase5

# Launch each epic with 12-second stagger
for epic in 003 015 030 031 033 042 055; do
    echo "[$(date)] Launching EPIC-CCN-$epic"
    screen -dmS p5-$epic bash -l -c \
        "./scripts/wave4/_p5_$epic.sh 2>&1 | tee logs/phase5/EPIC-CCN-$epic.log"
    sleep 12
done

echo "[$(date)] All 7 Phase 5 epics launched"
echo "Active screen sessions:"
screen -ls | grep 'p5-' || echo "No p5- sessions found"

# Made with Bob
