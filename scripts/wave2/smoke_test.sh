#!/bin/bash
# Smoke test - single epic launch via screen + bash -l
# Upload this file to the VM and run: bash /tmp/smoke_test.sh
set -euo pipefail

REPO="/home/malhitticrypto/universal-or-strategy"
LOG="$REPO/logs/EPIC-CCN-16.log"

git config --global user.email "malhitticrypto@gmail.com"
git config --global user.name "malhitticrypto"

mkdir -p "$REPO/logs"

echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Launching EPIC-CCN-16 smoke test..."

screen -dmS v12-EPIC-CCN-16 \
  bash -l -c "cd $REPO && bob --accept-license --max-coins 30 -p 'Run epic-intake for EPIC-CCN-16' > $LOG 2>&1; echo DONE_EXIT=\$? >> $LOG"

sleep 2
echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Screen sessions:"
screen -ls || true
echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Smoke test launched. Monitor: tail -f $LOG"
