#!/bin/bash
# Wave 6 Phase 1 - Relaunch 24 blocked epics after status reset
# Generated: 2026-06-18
# Building blocks method: Copied from wave6/phase1_template_v12_52.sh

set -e

EPICS=(
  "001" "004" "016" "020" "021" "028"
  "050" "051" "052" "053" "054" "055" "056" "057" "058" "059" "060" "061"
  "070" "073" "076" "077" "078" "079"
)

TOTAL=${#EPICS[@]}
echo "=== Relaunching 24 Blocked Epics - Phase 1 ==="
echo "Total epics: $TOTAL"
echo ""

for i in "${!EPICS[@]}"; do
  EPIC_NUM="${EPICS[$i]}"
  EPIC_ID="EPIC-CCN-${EPIC_NUM}"
  
  echo "[$((i+1))/$TOTAL] Launching $EPIC_ID..."
  
  # Launch in background with nohup
  nohup python3 scripts/wave_orchestrator.py \
    --epic-id "$EPIC_ID" \
    --phase 1 \
    --mode "plan" \
    --agent-id "wave6-p1-${EPIC_NUM}" \
    > "logs/wave6_phase1_${EPIC_NUM}.log" 2>&1 &
  
  # Small delay to avoid overwhelming the system
  sleep 2
done

echo ""
echo "=== All 24 epics launched ==="
echo "Monitor with: tail -f logs/wave6_phase1_*.log"

# Made with Bob
