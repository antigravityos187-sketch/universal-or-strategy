#!/bin/bash
# Wave 4 Phase 3 Launcher
# Launches all 10 epics in parallel using screen sessions

set -e
cd /home/malhitticrypto/universal-or-strategy

echo "Starting Wave 4 Phase 3 (DNA & PR Audit) for 10 epics..."
echo "Estimated time: 10 minutes per epic (parallel execution)"


echo "Launching EPIC-CCN-126 Phase 3..."
screen -dmS p3-126 bash -l -c './_p3_126.sh 2>&1 | tee logs/phase3/EPIC-CCN-126.log'
sleep 2

echo "Launching EPIC-CCN-127 Phase 3..."
screen -dmS p3-127 bash -l -c './_p3_127.sh 2>&1 | tee logs/phase3/EPIC-CCN-127.log'
sleep 2

echo "Launching EPIC-CCN-128 Phase 3..."
screen -dmS p3-128 bash -l -c './_p3_128.sh 2>&1 | tee logs/phase3/EPIC-CCN-128.log'
sleep 2

echo "Launching EPIC-CCN-129 Phase 3..."
screen -dmS p3-129 bash -l -c './_p3_129.sh 2>&1 | tee logs/phase3/EPIC-CCN-129.log'
sleep 2

echo "Launching EPIC-CCN-130 Phase 3..."
screen -dmS p3-130 bash -l -c './_p3_130.sh 2>&1 | tee logs/phase3/EPIC-CCN-130.log'
sleep 2

echo "Launching EPIC-CCN-131 Phase 3..."
screen -dmS p3-131 bash -l -c './_p3_131.sh 2>&1 | tee logs/phase3/EPIC-CCN-131.log'
sleep 2

echo "Launching EPIC-CCN-132 Phase 3..."
screen -dmS p3-132 bash -l -c './_p3_132.sh 2>&1 | tee logs/phase3/EPIC-CCN-132.log'
sleep 2

echo "Launching EPIC-CCN-133 Phase 3..."
screen -dmS p3-133 bash -l -c './_p3_133.sh 2>&1 | tee logs/phase3/EPIC-CCN-133.log'
sleep 2

echo "Launching EPIC-CCN-134 Phase 3..."
screen -dmS p3-134 bash -l -c './_p3_134.sh 2>&1 | tee logs/phase3/EPIC-CCN-134.log'
sleep 2

echo "Launching EPIC-CCN-135 Phase 3..."
screen -dmS p3-135 bash -l -c './_p3_135.sh 2>&1 | tee logs/phase3/EPIC-CCN-135.log'
sleep 2

echo ""
echo "All Phase 3 sessions launched!"
echo "Monitor with: screen -ls"
echo "Attach to session: screen -r p3-126"
echo "Detach from session: Ctrl+A, then D"
echo ""
echo "Expected completion: 10-15 minutes"
echo "Verify completion: screen -ls (should show 'No Sockets found')"
