#!/bin/bash
# Phase 4 Full Wave Launcher - All 80 Epics
# Generated: 2026-06-15
# Delay: Constant 12s (building-blocks method)

set -e

echo "[$(date)] Starting Phase 4 full wave launch (80 epics)"

# Create logs directory
mkdir -p logs/phase4

# Launch all 80 epics with constant 12s delay
for i in $(seq -f "%03g" 1 80); do
    EPIC="EPIC-CCN-${i}"
    
    echo "[$(date)] Launching ${EPIC} (delay: 12s)"
    
    # Launch in screen session
    screen -dmS p4-${i} bash -l -c \
        "./scripts/wave4/_p4_${i}.sh 2>&1 | tee logs/phase4/${EPIC}.log"
    
    # Constant 12s delay
    sleep 12
done

echo "[$(date)] All 80 epics launched for Phase 4"
echo "Launch duration: 16 minutes (80 × 12s)"
echo "Monitor with: screen -ls | grep -c 'p4-'"
echo "Check files: ls docs/brain/EPIC-CCN-*/04-tickets.md | wc -l"
