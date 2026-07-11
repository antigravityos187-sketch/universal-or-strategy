#!/bin/bash
# Launch Phase 6 for 7 recovered Phase 5 epics
set -e
cd /home/malhitticrypto/universal-or-strategy

echo "[$(date)] Launching Phase 6 for 7 recovered Phase 5 epics"
mkdir -p logs/phase6

# Launch each epic with 12-second stagger
for epic in 003 015 030 031 033 042 055; do
    echo "[$(date)] Launching EPIC-CCN-$epic (Phase 6)"
    screen -dmS p6-$epic bash -l -c \
        "./scripts/wave4/_p6_$epic.sh 2>&1 | tee logs/phase6/EPIC-CCN-$epic.log"
    sleep 12
done

echo "[$(date)] All 7 Phase 6 epics launched"
echo "Monitor with: screen -ls | grep 'p6-'"
echo "Check files: ls docs/brain/EPIC-CCN-{003,015,030,031,033,042,055}/06-completion-report.md"

# Made with Bob
