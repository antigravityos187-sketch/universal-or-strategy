#!/bin/bash
# Wave 1 Phase 0 Launcher - 5 Epics
# Launches all 5 Phase 0 scripts in parallel using screen sessions

cd /home/malhitticrypto/universal-or-strategy

echo "Starting Wave 1 Phase 0 for 5 epics..."
echo "Time: $(date)"

# Launch EPIC-001
screen -dmS p0-001 bash -l -c '/home/malhitticrypto/universal-or-strategy/_p0_001.sh'
echo "Launched EPIC-001 in screen session p0-001"

# Launch EPIC-002
screen -dmS p0-002 bash -l -c '/home/malhitticrypto/universal-or-strategy/_p0_002.sh'
echo "Launched EPIC-002 in screen session p0-002"

# Launch EPIC-003
screen -dmS p0-003 bash -l -c '/home/malhitticrypto/universal-or-strategy/_p0_003.sh'
echo "Launched EPIC-003 in screen session p0-003"

# Launch EPIC-004
screen -dmS p0-004 bash -l -c '/home/malhitticrypto/universal-or-strategy/_p0_004.sh'
echo "Launched EPIC-004 in screen session p0-004"

# Launch EPIC-005
screen -dmS p0-005 bash -l -c '/home/malhitticrypto/universal-or-strategy/_p0_005.sh'
echo "Launched EPIC-005 in screen session p0-005"

echo ""
echo "All 5 Phase 0 scripts launched!"
echo "Monitor with: screen -ls"
echo "Attach to session: screen -r p0-001"
echo "Detach from session: Ctrl+A, then D"
echo ""
echo "Check completion: screen -ls (should show 'No Sockets found' when done)"
echo "Verify files: ls docs/brain/EPIC-*/00-hotspots.md | wc -l (expect 5)"
echo "Extract bobcoins: grep -E 'Cost:.*Balance:' logs/phase0/EPIC-*.log"

# Made with Bob
