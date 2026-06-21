#!/bin/bash
# Wave 6 Phase 1 Launch - All 78 Epics
# Generated: 2026-06-18 (copied from launch_phase0_final.sh)
# Building-blocks method: Phase 0 → Phase 1

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_DIR="$SCRIPT_DIR/../../logs/wave6/phase1"
mkdir -p "$LOG_DIR"

# All epics (78 total, excluding EPIC-CCN-024, 027)
ALL_EPICS=(
  001 002 003 004 005 006 007 008 009 010
  011 012 013 014 015 016 017 018 019 020
  021 022 023 025 026 028 029 030
  031 032 033 034 035 036 037 038 039 040
  041 042 043 044 045 046 047 048 049 050
  051 052 053 054 055 056 057 058 059 060
  061 062 063 064 065 066 067 068 069 070
  071 072 073 074 075 076 077 078 079 080
)

echo "=== Wave 6 Phase 1 Launch ==="
echo "Launching 78 epics (Scope Definition)"
echo "Threshold: 8 (Jane Street strict)"
echo ""

for epic_num in "${ALL_EPICS[@]}"; do
  epic_id="EPIC-CCN-${epic_num}"
  script="${SCRIPT_DIR}/_p1_epic_ccn_${epic_num}.sh"
  log_file="${LOG_DIR}/epic_ccn_${epic_num}.log"
  session_name="wave6_p1_epic_${epic_num}"
  
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
echo "Target: 78/78 (excluding EPIC-CCN-024, 027)"
echo "Monitor: watch -n 240 'find docs/brain/EPIC-CCN-*/manifest.json | xargs grep -l \"\\\"1\\\": {\\\"status\\\": \\\"completed\\\"\" | wc -l'"
echo ""
echo "Cost-Optimized Polling: 4-minute intervals (88% cost reduction)"

# Made with Bob