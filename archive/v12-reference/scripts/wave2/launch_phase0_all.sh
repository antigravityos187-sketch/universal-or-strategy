#!/bin/bash
# Launch Phase 0 for all 9 epics
# Wave 2 v4 - File Persistence Fix

cd /home/malhitticrypto/universal-or-strategy

# Make scripts executable
chmod +x _p0_*.sh

# Create logs directory if needed
mkdir -p logs/phase0

# Launch each epic in a screen session
for i in 107 108 109 110 111 112 113 114 115; do
  echo "Launching Phase 0 for EPIC-CCN-$i..."
  screen -dmS p0-$i bash -l -c "./_p0_$i.sh 2>&1 | tee logs/phase0/EPIC-CCN-$i.log"
done

# Wait a moment for sessions to start
sleep 2

# Show active screen sessions
echo ""
echo "Active screen sessions:"
screen -ls

echo ""
echo "Phase 0 launched for all 9 epics"
echo "Monitor with: screen -r p0-107 (or any epic ID)"
echo "Check logs: tail -f logs/phase0/EPIC-CCN-107.log"

# Made with Bob
