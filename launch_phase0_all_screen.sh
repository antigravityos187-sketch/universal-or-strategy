#!/bin/bash
# Launch all 9 Phase 0 epics in separate screen sessions
# Each epic gets its own screen session for parallel execution

set -e

REPO_DIR="/home/malhitticrypto/universal-or-strategy"
cd "$REPO_DIR"

echo "=== Launching 9 Phase 0 Epics in Screen Sessions ==="
echo "Start time: $(date)"
echo ""

# Array of epic IDs
EPICS=(107 108 109 110 111 112 113 114 115)

# Launch each epic in its own screen session
for EPIC in "${EPICS[@]}"; do
    SESSION_NAME="epic-ccn-${EPIC}"
    SCRIPT_PATH="${REPO_DIR}/_p0_${EPIC}.sh"
    
    echo "Launching EPIC-CCN-${EPIC} in screen session: ${SESSION_NAME}"
    
    # Create detached screen session and run the script
    screen -dmS "${SESSION_NAME}" bash -l "${SCRIPT_PATH}"
    
    # Small delay to avoid overwhelming the system
    sleep 2
done

echo ""
echo "=== All 9 Epics Launched ==="
echo ""
echo "Monitor with:"
echo "  screen -ls                    # List all sessions"
echo "  screen -r epic-ccn-107        # Attach to specific epic"
echo "  screen -S epic-ccn-107 -X stuff 'exit\n'  # Kill specific session"
echo ""
echo "Check status:"
echo "  ls -lh docs/brain/EPIC-CCN-*/  # View created files"
echo "  grep -r 'DONE_EXIT' docs/brain/EPIC-CCN-*/  # Check completion"
echo ""
echo "Expected output: 18 files total (9 × 00-hotspots.md + 9 × manifest.json)"

# Made with Bob
