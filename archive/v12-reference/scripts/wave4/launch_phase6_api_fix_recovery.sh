#!/bin/bash
# Launch Phase 6 for 4 epics with API key fix
# EPIC-003: filename pattern fixed
# EPIC-015, 030, 045: API key fixed

cd /home/malhitticrypto/universal-or-strategy

echo "[$(date)] Starting Phase 6 API Fix Recovery (4 epics)"
mkdir -p logs/phase6

# Launch EPIC-CCN-003 (filename pattern fixed)
echo "[$(date)] Launching EPIC-CCN-003"
screen -dmS p6-003-retry bash -l -c \
    "./scripts/wave4/_p6_003.sh 2>&1 | tee logs/phase6/EPIC-CCN-003-retry.log"
sleep 12

# Launch EPIC-CCN-015 (API key fixed, but prerequisite check may fail)
echo "[$(date)] Launching EPIC-CCN-015"
screen -dmS p6-015-retry bash -l -c \
    "./scripts/wave4/_p6_015.sh 2>&1 | tee logs/phase6/EPIC-CCN-015-retry.log"
sleep 12

# Launch EPIC-CCN-030 (API key fixed, but prerequisite check may fail)
echo "[$(date)] Launching EPIC-CCN-030"
screen -dmS p6-030-retry bash -l -c \
    "./scripts/wave4/_p6_030.sh 2>&1 | tee logs/phase6/EPIC-CCN-030-retry.log"
sleep 12

# Launch EPIC-CCN-045 (API key fixed)
echo "[$(date)] Launching EPIC-CCN-045"
screen -dmS p6-045-retry bash -l -c \
    "./scripts/wave4/_p6_045.sh 2>&1 | tee logs/phase6/EPIC-CCN-045-retry.log"

echo "[$(date)] All 4 epics launched"
echo "Monitor with: screen -ls | grep 'p6-.*-retry'"
echo "Check files: ls docs/brain/EPIC-CCN-{003,015,030,045}/06-completion-report.md 2>/dev/null | wc -l"

# Made with Bob
