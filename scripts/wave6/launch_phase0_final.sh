#!/bin/bash
# Wave 6 Phase 0 Final Recovery - 19 Missing Epics
# Generated: 2026-06-17 after V3 reset

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_DIR="$SCRIPT_DIR/../../logs/wave6/phase0"
mkdir -p "$LOG_DIR"

# Missing epics (19 total)
MISSING_EPICS=(
  001 004 016 028 064 065 066 067 068 069
  071 073 074 075 076 077 078 079 080
)

echo "=== Wave 6 Phase 0 Final Recovery ==="
echo "Launching 19 missing epics"
echo "Threshold: 8 (Jane Street strict)"
echo ""

for epic_num in "${MISSING_EPICS[@]}"; do
  epic_id="EPIC-CCN-${epic_num}"
  script="${SCRIPT_DIR}/_p0_epic_ccn_${epic_num}.sh"
  log_file="${LOG_DIR}/epic_ccn_${epic_num}.log"
  session_name="wave6_p0_epic_${epic_num}"
  
  if [ -f "$script" ]; then
    echo "Launching $epic_id in screen session: $session_name"
    screen -dmS "$session_name" bash -c "cd ~/universal-or-strategy && bash $script 2>&1 | tee $log_file"
    sleep 0.5
  else
    echo "⚠️  Script not found: $script"
  fi
done

echo ""
echo "=== Launch Complete ==="
echo "Target: 77/78 (excluding EPIC-CCN-027)"
echo "Monitor: watch -n 30 'find docs/brain/EPIC-CCN-0* -name \"00-hotspots.md\" | wc -l'"

# Made with Bob
