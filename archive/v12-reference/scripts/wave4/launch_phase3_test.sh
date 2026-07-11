#!/bin/bash
# Phase 3 Test Launcher - First 2 Epics
# Generated: 2026-06-15

set -e

echo "[$(date)] Starting Phase 3 test launch (2 epics)"

# Create logs directory
mkdir -p logs/phase3

# Launch EPIC-CCN-001
echo "[$(date)] Launching EPIC-CCN-001"
screen -dmS p3-001 bash -l -c './scripts/wave4/_p3_001.sh 2>&1 | tee logs/phase3/EPIC-CCN-001.log'

# Wait 12 seconds
sleep 12

# Launch EPIC-CCN-002
echo "[$(date)] Launching EPIC-CCN-002"
screen -dmS p3-002 bash -l -c './scripts/wave4/_p3_002.sh 2>&1 | tee logs/phase3/EPIC-CCN-002.log'

echo "[$(date)] Test launch complete (2 epics)"
echo "Monitor with: screen -ls"
echo "Check files: ls docs/brain/EPIC-CCN-{001,002}/03-audit-report.md"
