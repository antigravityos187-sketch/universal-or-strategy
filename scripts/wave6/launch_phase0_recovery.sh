#!/bin/bash
# Wave 6 Phase 0 Recovery Launch - 43 Failed Epics
# Generated: 2026-06-17 after manifest reset

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_DIR="$SCRIPT_DIR/../../logs/wave6/phase0"
mkdir -p "$LOG_DIR"

# Failed epics (43 total)
FAILED_EPICS=(
  004 005 007 008 011 013 014 016 018 019
  020 022 023 027 028 029 031 032 033 035
  036 037 038 039 041 042 043 044 045 047
  049 050 051 052 054 056 057 058 059 060
  061 062 063
)

echo "=== Wave 6 Phase 0 Recovery Launch ==="
echo "Relaunching 43 failed epics with clean manifests"
echo "Threshold: 8 (Jane Street strict)"
echo ""

for epic_num in "${FAILED_EPICS[@]}"; do
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
echo "Monitor progress: watch -n 30 'find docs/brain -name \"00-hotspots.md\" | wc -l'"
echo "Check logs: tail -f logs/wave6/phase0/*.log"

# Made with Bob
