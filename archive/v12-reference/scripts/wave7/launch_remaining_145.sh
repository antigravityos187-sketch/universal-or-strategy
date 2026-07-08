#!/bin/bash
# Wave 7 - Launch Remaining 145 Epics
# Continuous execution with live monitoring
# Sync every 20 completions
# Building-Blocks Method: Copied from previous wave launch scripts

set -e

echo "========================================="
echo "Wave 7 - Launch Remaining 145 Epics"
echo "========================================="
echo ""
echo "Current status: 16/161 complete (after pilot: 19/161)"
echo "Target: 161/161 complete (100%)"
echo "Remaining: 145 epics (or 142 after pilot)"
echo ""
echo "Execution Strategy:"
echo "  - Continuous execution (no batches)"
echo "  - 20 API keys rotate automatically"
echo "  - Live monitoring by agent"
echo "  - Manual sync every 20 completions"
echo ""
echo "Sync Checkpoints:"
echo "  - After 20 new epics (36 total)"
echo "  - After 40 new epics (56 total)"
echo "  - After 60 new epics (76 total)"
echo "  - After 80 new epics (96 total)"
echo "  - After 100 new epics (116 total)"
echo "  - After 120 new epics (136 total)"
echo "  - After 140 new epics (156 total)"
echo "  - After all 145 new epics (161 total)"
echo ""

SCRIPT_DIR="scripts/wave7/phase0_scripts"

# Get list of all epics
ALL_EPICS=($(ls -1 "$SCRIPT_DIR" | grep "EPIC-W7-" | sed 's/_phase0.sh//' | sort))

# Get list of completed epics
COMPLETED_EPICS=($(find docs/brain -maxdepth 1 -type d -name 'EPIC-W7-*' -exec test -f {}/00-hotspots.md \; -print | xargs -n1 basename | sort))

# Calculate remaining epics
REMAINING_EPICS=()
for epic in "${ALL_EPICS[@]}"; do
    if [[ ! " ${COMPLETED_EPICS[@]} " =~ " ${epic} " ]]; then
        REMAINING_EPICS+=("$epic")
    fi
done

TOTAL_REMAINING=${#REMAINING_EPICS[@]}

echo "Found $TOTAL_REMAINING remaining epics to launch"
echo ""

if [ $TOTAL_REMAINING -eq 0 ]; then
    echo "✅ All epics already complete!"
    exit 0
fi

echo "Launching $TOTAL_REMAINING epics..."
echo ""

LAUNCHED=0
for EPIC_ID in "${REMAINING_EPICS[@]}"; do
    SCRIPT_PATH="$SCRIPT_DIR/${EPIC_ID}_phase0.sh"
    
    if [ ! -f "$SCRIPT_PATH" ]; then
        echo "⚠️  WARNING: Script not found: $SCRIPT_PATH"
        continue
    fi
    
    echo "🚀 Launching $EPIC_ID... ($((LAUNCHED + 1))/$TOTAL_REMAINING)"
    bash "$SCRIPT_PATH" &
    
    LAUNCHED=$((LAUNCHED + 1))
    
    # 12-second stagger between launches
    sleep 12
done

echo ""
echo "✅ All $LAUNCHED epics launched"
echo ""
echo "Monitor progress:"
echo "  screen -ls                                    # List active sessions"
echo "  screen -r EPIC-W7-XXX-phase0                  # Attach to specific epic"
echo "  find docs/brain -maxdepth 1 -type d -name 'EPIC-W7-*' -exec test -f {}/00-hotspots.md \; -print | wc -l"
echo ""
echo "Sync to GitHub every 20 completions:"
echo "  bash scripts/wave7/sync_epics_from_vm.sh"
echo ""
echo "Expected final: 161/161 complete"
echo ""

# Made with Bob
