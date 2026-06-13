#!/bin/bash
# Launch all 9 Phase 0 agents in parallel screen sessions
# Each agent runs in its own screen session with logging

cd /home/malhitticrypto/universal-or-strategy

echo "[WAVE2-PHASE0] Launching 9 parallel Phase 0 agents in screen sessions..."
echo "[WAVE2-PHASE0] Each agent uses dedicated API key and v12-phase0-hotspot mode"
echo ""

# Launch each epic in a separate screen session
screen -dmS p0-107 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && ./_p0_107.sh 2>&1 | tee logs/phase0/EPIC-CCN-107.log'
echo "[WAVE2-PHASE0] Launched: EPIC-CCN-107 (HydrateFromOpenPositions, CYC 31)"
sleep 1

screen -dmS p0-108 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && ./_p0_108.sh 2>&1 | tee logs/phase0/EPIC-CCN-108.log'
echo "[WAVE2-PHASE0] Launched: EPIC-CCN-108"
sleep 1

screen -dmS p0-109 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && ./_p0_109.sh 2>&1 | tee logs/phase0/EPIC-CCN-109.log'
echo "[WAVE2-PHASE0] Launched: EPIC-CCN-109"
sleep 1

screen -dmS p0-110 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && ./_p0_110.sh 2>&1 | tee logs/phase0/EPIC-CCN-110.log'
echo "[WAVE2-PHASE0] Launched: EPIC-CCN-110"
sleep 1

screen -dmS p0-111 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && ./_p0_111.sh 2>&1 | tee logs/phase0/EPIC-CCN-111.log'
echo "[WAVE2-PHASE0] Launched: EPIC-CCN-111"
sleep 1

screen -dmS p0-112 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && ./_p0_112.sh 2>&1 | tee logs/phase0/EPIC-CCN-112.log'
echo "[WAVE2-PHASE0] Launched: EPIC-CCN-112"
sleep 1

screen -dmS p0-113 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && ./_p0_113.sh 2>&1 | tee logs/phase0/EPIC-CCN-113.log'
echo "[WAVE2-PHASE0] Launched: EPIC-CCN-113"
sleep 1

screen -dmS p0-114 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && ./_p0_114.sh 2>&1 | tee logs/phase0/EPIC-CCN-114.log'
echo "[WAVE2-PHASE0] Launched: EPIC-CCN-114"
sleep 1

screen -dmS p0-115 bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && ./_p0_115.sh 2>&1 | tee logs/phase0/EPIC-CCN-115.log'
echo "[WAVE2-PHASE0] Launched: EPIC-CCN-115"
sleep 1

echo ""
echo "[WAVE2-PHASE0] All 9 agents launched successfully!"
echo ""
echo "Monitor with:"
echo "  screen -ls                    # List all sessions"
echo "  screen -r p0-107              # Attach to specific session (Ctrl+A, D to detach)"
echo "  ls docs/brain/EPIC-CCN-*/00-hotspots.md | wc -l  # Count completed files"
echo ""
echo "Check logs:"
echo "  tail -f logs/phase0/EPIC-CCN-107.log"
echo "  grep 'DONE_EXIT' logs/phase0/*.log"
echo ""

# Show current screen sessions
screen -ls || true

# Made with Bob
