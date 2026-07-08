#!/bin/bash
# Master launch script for Phase 2 (Architecture Planning) - Wave 4
# Launches all 80 epics with staggered delays (CONSTANT 12s)
# 
# CRITICAL FIX: Uses CONSTANT 12s delay (not incrementing 12,13,14...54)
# Reference: WAVE4_PHASE1_COMPLETION_REPORT.md - Issue #1

set -e

PHASE=2
EPICS=($(seq -f "%03g" 1 80))
DELAY=12  # CONSTANT delay (not incrementing)

echo "[$(date)] Starting Phase 2 launch for 80 epics"
echo "[$(date)] Using CONSTANT delay: ${DELAY}s between launches"
echo "[$(date)] Expected launch time: $((80 * DELAY / 60)) minutes"

for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    
    echo "[$(date)] Launching EPIC-CCN-${EPIC} (${i}/80, delay: ${DELAY}s)"
    
    # Launch in screen session (foreground execution for visibility)
    screen -dmS p${PHASE}-${EPIC} bash -l -c \
        "./_p${PHASE}_${EPIC}.sh 2>&1 | tee logs/phase${PHASE}/EPIC-CCN-${EPIC}.log"
    
    # Wait CONSTANT 12s before next launch
    sleep ${DELAY}
done

echo "[$(date)] All 80 epics launched for Phase ${PHASE}"
echo "[$(date)] Monitor with: screen -ls"
echo "[$(date)] Check files with: ls docs/brain/EPIC-CCN-*/02-architecture-plan.md | wc -l"
echo "[$(date)] Extract bobcoins with: grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase${PHASE}/*.log | head -20"

# Made with Bob
