#!/bin/bash
# Wave 4 Phase 0 Test Launch Script
# Tests first 2 epics (EPIC-CCN-001, EPIC-CCN-002) before full wave

set -e

REPO_DIR="/home/malhitticrypto/universal-or-strategy"
cd "$REPO_DIR"

echo "=== Wave 4 Phase 0: Test Launch (2 Epics) ==="
echo "Start time: $(date)"
echo ""

# Test with first 2 epics
EPICS=("001" "002")

echo "Configuration:"
echo "  - Test epics: EPIC-CCN-001, EPIC-CCN-002"
echo "  - Delay: 15 seconds between launches"
echo "  - Purpose: Validate launch pattern before full wave"
echo ""

for EPIC in "${EPICS[@]}"; do
    SCRIPT_PATH="${REPO_DIR}/_p0_${EPIC}.sh"
    
    echo "[$(date '+%H:%M:%S')] Launching EPIC-CCN-${EPIC}"
    
    # Launch in background, log to file
    bash "${SCRIPT_PATH}" > "logs/phase0/EPIC-CCN-${EPIC}.log" 2>&1 &
    
    # Store PID
    echo $! > "logs/phase0/EPIC-CCN-${EPIC}.pid"
    
    # Wait before next launch
    sleep 15
done

echo ""
echo "=== Test Launch Complete ==="
echo "End time: $(date)"
echo ""
echo "Monitor progress:"
echo "  tail -f logs/phase0/EPIC-CCN-001.log"
echo "  tail -f logs/phase0/EPIC-CCN-002.log"
echo ""
echo "Check completion:"
echo "  ls -lh docs/brain/EPIC-CCN-001/00-hotspots.md"
echo "  ls -lh docs/brain/EPIC-CCN-002/00-hotspots.md"
echo "  ls -lh docs/brain/EPIC-CCN-001/manifest.json"
echo "  ls -lh docs/brain/EPIC-CCN-002/manifest.json"
echo ""
echo "Expected output: 4 files total (2 x 00-hotspots.md + 2 x manifest.json)"
echo ""
echo "If test succeeds, proceed with full wave:"
echo "  bash launch_wave4_phase0_all.sh"

# Made with Bob
