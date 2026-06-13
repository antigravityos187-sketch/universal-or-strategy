#!/bin/bash
# Launch all Phase 0 agents in parallel
set -e
cd /home/malhitticrypto/universal-or-strategy

echo "[WAVE2-P0] Launching 9 parallel agents..."

screen -dmS p0-107 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && bash _p0_107.sh'
echo "[WAVE2-P0] Launched: EPIC-CCN-107 (HydrateFromOpenPositions, CYC 31)"
sleep 1

screen -dmS p0-108 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && bash _p0_108.sh'
echo "[WAVE2-P0] Launched: EPIC-CCN-108 (SweepBrokerOrders, CYC 24)"
sleep 1

screen -dmS p0-109 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && bash _p0_109.sh'
echo "[WAVE2-P0] Launched: EPIC-CCN-109 (HydrateWorkingOrdersFromBroker, CYC 19)"
sleep 1

screen -dmS p0-110 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && bash _p0_110.sh'
echo "[WAVE2-P0] Launched: EPIC-CCN-110 (AdoptMasterOrders, CYC 19)"
sleep 1

screen -dmS p0-111 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && bash _p0_111.sh'
echo "[WAVE2-P0] Launched: EPIC-CCN-111 (HydrateExpectedPositionsFromBroker, CYC 17)"
sleep 1

screen -dmS p0-112 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && bash _p0_112.sh'
echo "[WAVE2-P0] Launched: EPIC-CCN-112 (ClassifyOrderByPrefix, CYC 17)"
sleep 1

screen -dmS p0-113 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && bash _p0_113.sh'
echo "[WAVE2-P0] Launched: EPIC-CCN-113 (HydrateFSMsFromWorkingOrders, CYC 14)"
sleep 1

screen -dmS p0-114 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && bash _p0_114.sh'
echo "[WAVE2-P0] Launched: EPIC-CCN-114 (ProcessShutdownSIMA, CYC 11)"
sleep 1

screen -dmS p0-115 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && bash _p0_115.sh'
echo "[WAVE2-P0] Launched: EPIC-CCN-115 (SweepTrackedOrders, CYC 10)"
sleep 1

echo "[WAVE2-P0] All 9 agents launched."
echo "[WAVE2-P0] Monitor: screen -ls"
echo "[WAVE2-P0] Logs: tail -f logs/phase0/EPIC-CCN-107.log"
