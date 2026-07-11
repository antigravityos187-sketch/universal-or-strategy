#!/bin/bash
# Wave 3 Phase 2 Launcher - Architecture Planning
# Launches all 10 epics in parallel using screen sessions

cd /home/malhitticrypto/universal-or-strategy

echo "Launching Wave 3 Phase 2 (Architecture Planning) for 10 epics..."
echo "Start time: $(date)"

screen -dmS p2-116 bash -l -c "./_p2_116.sh 2>&1 | tee logs/phase2/EPIC-CCN-116.log"
screen -dmS p2-117 bash -l -c "./_p2_117.sh 2>&1 | tee logs/phase2/EPIC-CCN-117.log"
screen -dmS p2-118 bash -l -c "./_p2_118.sh 2>&1 | tee logs/phase2/EPIC-CCN-118.log"
screen -dmS p2-119 bash -l -c "./_p2_119.sh 2>&1 | tee logs/phase2/EPIC-CCN-119.log"
screen -dmS p2-120 bash -l -c "./_p2_120.sh 2>&1 | tee logs/phase2/EPIC-CCN-120.log"
screen -dmS p2-121 bash -l -c "./_p2_121.sh 2>&1 | tee logs/phase2/EPIC-CCN-121.log"
screen -dmS p2-122 bash -l -c "./_p2_122.sh 2>&1 | tee logs/phase2/EPIC-CCN-122.log"
screen -dmS p2-123 bash -l -c "./_p2_123.sh 2>&1 | tee logs/phase2/EPIC-CCN-123.log"
screen -dmS p2-124 bash -l -c "./_p2_124.sh 2>&1 | tee logs/phase2/EPIC-CCN-124.log"
screen -dmS p2-125 bash -l -c "./_p2_125.sh 2>&1 | tee logs/phase2/EPIC-CCN-125.log"

echo "All Phase 2 sessions launched!"
echo "Check status with: screen -ls"
echo "View logs with: tail -f logs/phase2/EPIC-CCN-*.log"
