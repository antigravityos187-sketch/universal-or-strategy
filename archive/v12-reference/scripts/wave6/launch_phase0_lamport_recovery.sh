#!/bin/bash
# Wave 6 Phase 0 - Lamport Recovery Launch
# Relaunch 4 epics that were blocked by Lamport conflicts
# Generated: 2026-06-17

set -e

EPICS=(
  "EPIC-CCN-001"
  "EPIC-CCN-004"
  "EPIC-CCN-016"
  "EPIC-CCN-028"
)

echo "=== Wave 6 Phase 0 - Lamport Recovery Launch ==="
echo "Relaunching ${#EPICS[@]} epics after Lamport clock clearing"
echo ""

for epic in "${EPICS[@]}"; do
  epic_num=$(echo "$epic" | sed 's/EPIC-CCN-//')
  script="scripts/wave6/_p0_epic_ccn_${epic_num}.sh"
  
  if [ ! -f "$script" ]; then
    echo "❌ Script not found: $script"
    continue
  fi
  
  echo "🚀 Launching $epic..."
  screen -dmS "wave6_p0_${epic_num}" bash -c "cd ~/universal-or-strategy && bash $script 2>&1 | tee logs/wave6/phase0/${epic}_lamport_recovery.log"
  echo "   ✅ Screen session: wave6_p0_${epic_num}"
  sleep 2
done

echo ""
echo "=== Launch Complete ==="
echo "Monitor progress:"
echo "  screen -ls | grep wave6_p0"
echo "  find docs/brain -name '00-hotspots.md' | wc -l"
echo ""
echo "Check logs:"
echo "  tail -f logs/wave6/phase0/EPIC-CCN-*_lamport_recovery.log"

# Made with Bob
