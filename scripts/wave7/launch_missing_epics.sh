#!/bin/bash

# Wave 7 Phase 0 - Launch Missing Epics Only
# Launches the 23 epics that were blocked by bobcoin exhaustion

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

echo "=========================================="
echo "Wave 7 Phase 0 - Launch Missing Epics"
echo "=========================================="
echo "Time: $(date -u +"%Y-%m-%d %H:%M:%S UTC")"
echo ""

# List of 23 incomplete epics
MISSING_EPICS=(
    008 010 018 038 053 060 068 069 072 083
    090 098 099 106 108 113 121 128 135 141
    143 153 158
)

echo "[*] Missing epics to launch: ${#MISSING_EPICS[@]}"
echo ""

# Check if scripts exist
SCRIPT_COUNT=$(ls "$SCRIPT_DIR"/_p0_*.sh 2>/dev/null | wc -l)
if [ "$SCRIPT_COUNT" -eq 0 ]; then
    echo "ERROR: No Phase 0 scripts found in $SCRIPT_DIR"
    echo "Run generate_phase0_scripts_fixed.py first"
    exit 1
fi
echo "[*] Found $SCRIPT_COUNT Phase 0 scripts"
echo ""

# Launch each missing epic
LAUNCHED=0
for EPIC_NUM in "${MISSING_EPICS[@]}"; do
    EPIC_ID="EPIC-W7-${EPIC_NUM}"
    SCRIPT_PATH="$SCRIPT_DIR/_p0_${EPIC_NUM}.sh"
    
    if [ ! -f "$SCRIPT_PATH" ]; then
        echo "⚠️  Script not found: $SCRIPT_PATH"
        continue
    fi
    
    # Check if already complete
    HOTSPOT_FILE="$PROJECT_ROOT/docs/brain/$EPIC_ID/00-hotspots.md"
    if [ -f "$HOTSPOT_FILE" ]; then
        echo "✓ $EPIC_ID already complete (skipping)"
        continue
    fi
    
    # Launch in screen session
    SESSION_NAME="phase0_${EPIC_ID}"
    
    # Kill existing session if present
    screen -S "$SESSION_NAME" -X quit 2>/dev/null || true
    
    # Start new session
    screen -dmS "$SESSION_NAME" bash -c "cd '$PROJECT_ROOT' && bash '$SCRIPT_PATH' > logs/phase0/${EPIC_ID}.log 2>&1"
    
    echo "✓ Launched $EPIC_ID (session: $SESSION_NAME)"
    LAUNCHED=$((LAUNCHED + 1))
    
    # Stagger launches (12 seconds)
    if [ $LAUNCHED -lt ${#MISSING_EPICS[@]} ]; then
        sleep 12
    fi
done

echo ""
echo "=========================================="
echo "Launch Complete"
echo "=========================================="
echo "Launched: $LAUNCHED epics"
echo ""
echo "Monitor progress:"
echo "  ./scripts/wave7/verify_phase0_completion.sh"
echo ""
echo "Check active sessions:"
echo "  screen -ls | grep phase0"
echo ""
echo "Attach to session:"
echo "  screen -r phase0_EPIC-W7-XXX"
echo ""

# Made with Bob
