#!/bin/bash
# Wave 3 Phase 0 Launcher
# Launches all 10 epics in parallel using screen sessions

set -e
cd /home/malhitticrypto/universal-or-strategy

echo "Starting Wave 3 Phase 0 (10 epics)..."

# Launch each epic in a screen session

screen -dmS "p0-116" bash -l "_p0_116.sh"
echo "Launched EPIC-CCN-116 in screen session p0-116"

screen -dmS "p0-117" bash -l "_p0_117.sh"
echo "Launched EPIC-CCN-117 in screen session p0-117"

screen -dmS "p0-118" bash -l "_p0_118.sh"
echo "Launched EPIC-CCN-118 in screen session p0-118"

screen -dmS "p0-119" bash -l "_p0_119.sh"
echo "Launched EPIC-CCN-119 in screen session p0-119"

screen -dmS "p0-120" bash -l "_p0_120.sh"
echo "Launched EPIC-CCN-120 in screen session p0-120"

screen -dmS "p0-121" bash -l "_p0_121.sh"
echo "Launched EPIC-CCN-121 in screen session p0-121"

screen -dmS "p0-122" bash -l "_p0_122.sh"
echo "Launched EPIC-CCN-122 in screen session p0-122"

screen -dmS "p0-123" bash -l "_p0_123.sh"
echo "Launched EPIC-CCN-123 in screen session p0-123"

screen -dmS "p0-124" bash -l "_p0_124.sh"
echo "Launched EPIC-CCN-124 in screen session p0-124"

screen -dmS "p0-125" bash -l "_p0_125.sh"
echo "Launched EPIC-CCN-125 in screen session p0-125"

echo ""
echo "All 10 epics launched!"
echo "Monitor with: screen -ls"
echo "Attach to session: screen -r p0-116"
echo "Detach from session: Ctrl+A, then D"
echo ""
echo "Check completion: screen -ls (should show 'No Sockets found' when done)"
echo "Verify files: ls docs/brain/EPIC-CCN-*/00-hotspots.md | wc -l (should show 10)"
