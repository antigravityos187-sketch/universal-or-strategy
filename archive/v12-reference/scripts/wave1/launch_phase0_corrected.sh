#!/bin/bash
# Launch Phase 0 for corrected epics (EPIC-001, 002, 004)
# Building Blocks method: Copy working pattern, change only data

cd /home/malhitticrypto/universal-or-strategy

echo "=== Launching Phase 0 Corrected Epics ==="
echo "EPIC-001: V12_002.Orders.Callbacks.cs (6 methods)"
echo "EPIC-002: V12_002.Orders.Management.Flatten.cs (4 methods)"
echo "EPIC-004: V12_002.SIMA.Dispatch.cs (3 methods)"
echo ""

# Launch in screen sessions
screen -dmS p0-001-fix bash -l -c '/home/malhitticrypto/universal-or-strategy/_p0_001_corrected.sh 2>&1 | tee logs/phase0/EPIC-001-corrected.log'
echo "Started: p0-001-fix"

sleep 2

screen -dmS p0-002-fix bash -l -c '/home/malhitticrypto/universal-or-strategy/_p0_002_corrected.sh 2>&1 | tee logs/phase0/EPIC-002-corrected.log'
echo "Started: p0-002-fix"

sleep 2

screen -dmS p0-004-fix bash -l -c '/home/malhitticrypto/universal-or-strategy/_p0_004_corrected.sh 2>&1 | tee logs/phase0/EPIC-004-corrected.log'
echo "Started: p0-004-fix"

echo ""
echo "=== All 3 corrected epics launched ==="
echo "Monitor with: screen -ls"
echo "Attach with: screen -r p0-001-fix (or p0-002-fix, p0-004-fix)"
echo "Detach with: Ctrl+A, D"
echo ""
echo "Check completion:"
echo "  screen -ls | grep -c 'No Sockets found' (expect 1 when done)"
echo ""
echo "Verify files:"
echo "  ls docs/brain/EPIC-00{1,2,4}/00-hotspots.md 2>/dev/null | wc -l (expect 3)"
echo "  ls docs/brain/EPIC-00{1,2,4}/manifest.json 2>/dev/null | wc -l (expect 3)"

# Made with Bob
