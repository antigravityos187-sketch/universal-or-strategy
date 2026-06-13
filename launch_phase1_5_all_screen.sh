#!/bin/bash
# Wave 2 Phase 1.5 (Scope Boundary Validation) Launcher
# Launches all 9 epics in parallel using screen sessions
# Each epic runs in isolated session with unique API key

set -e

echo "=== Wave 2 Phase 1.5 Launcher ==="
echo "Starting 9 epics in parallel screen sessions..."
echo ""

# Epic list with API allocations
declare -A EPICS=(
    ["107"]="107"
    ["108"]="108"
    ["109"]="109"
    ["110"]="110"
    ["111"]="111"
    ["112"]="112"
    ["113"]="113"
    ["114"]="114"
    ["115"]="115"
)

# Launch each epic in separate screen session
for epic_num in "${!EPICS[@]}"; do
    api_key="${EPICS[$epic_num]}"
    session_name="phase1_5_epic_${epic_num}"
    script_name="_p1_5_${epic_num}.sh"
    
    echo "[$(date +%H:%M:%S)] Launching EPIC-CCN-${epic_num} in screen session '${session_name}'"
    echo "  - API Key: ${api_key}"
    echo "  - Script: ${script_name}"
    
    # Create detached screen session and run script
    screen -dmS "$session_name" bash -l "$script_name"
    
    # Small delay to avoid overwhelming system
    sleep 2
done

echo ""
echo "=== All 9 Phase 1.5 epics launched ==="
echo ""
echo "Monitor with:"
echo "  screen -ls                    # List all sessions"
echo "  screen -r phase1_5_epic_107     # Attach to specific epic"
echo "  Ctrl+A, D                     # Detach from session"
echo ""
echo "Check progress:"
echo "  ls -lh docs/brain/EPIC-CCN-*/01-scope-boundary.md"
echo "  cat docs/brain/EPIC-CCN-107/manifest.json"
echo ""

# Made with Bob
