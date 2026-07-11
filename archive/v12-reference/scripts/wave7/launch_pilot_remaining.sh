#!/bin/bash
# Wave 7 Pilot Test - 3 Remaining Epics
# Tests low/medium/high complexity before full launch
# Building-Blocks Method: Copied from previous wave pilot scripts

set -e

echo "========================================="
echo "Wave 7 Pilot Test - 3 Remaining Epics"
echo "========================================="
echo ""
echo "Testing 3 epics before launching all 145:"
echo "  - EPIC-W7-002 (Low complexity: CYC 9)"
echo "  - EPIC-W7-050 (Medium complexity: CYC 12)"
echo "  - EPIC-W7-100 (High complexity: CYC 15)"
echo ""
echo "Current status: 16/161 complete"
echo "After pilot: 19/161 complete"
echo ""

# Pilot epics (not yet completed) - using numeric IDs
PILOT_EPICS=(
    "002"
    "050"
    "100"
)

SCRIPT_DIR="scripts/wave7"

echo "Launching pilot epics..."
echo ""

for EPIC_NUM in "${PILOT_EPICS[@]}"; do
    SCRIPT_PATH="$SCRIPT_DIR/_p0_${EPIC_NUM}.sh"
    
    if [ ! -f "$SCRIPT_PATH" ]; then
        echo "❌ ERROR: Script not found: $SCRIPT_PATH"
        exit 1
    fi
    
    echo "🚀 Launching EPIC-W7-${EPIC_NUM}..."
    bash "$SCRIPT_PATH" &
    
    # 12-second stagger between launches
    sleep 12
done

echo ""
echo "✅ All 3 pilot epics launched"
echo ""
echo "Monitor progress:"
echo "  screen -ls                    # List active sessions"
echo "  screen -r EPIC-W7-002-phase0  # Attach to specific epic"
echo ""
echo "Check completion:"
echo "  find docs/brain -maxdepth 1 -type d -name 'EPIC-W7-*' -exec test -f {}/00-hotspots.md \; -print | wc -l"
echo ""
echo "Expected: 19/161 complete after pilot"
echo ""

# Made with Bob
