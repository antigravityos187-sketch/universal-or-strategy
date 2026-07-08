#!/bin/bash
# Wave 4 Phase 3 Recovery Launcher
# Launches 62 failed epics with fixed scripts
# Generated: 2026-06-15T15:33:00Z
# Method: Building-blocks (12s constant delay)

set -e
cd /home/malhitticrypto/universal-or-strategy

echo "=== Wave 4 Phase 3 Recovery Launch ==="
echo "Target: 62 failed epics"
echo "Start time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo ""

# Failed epics list (excluding 18 successful ones)
FAILED_EPICS=(
  "003" "008" "010" "012" "013" "014" "016" "017" "018" "019"
  "020" "021" "022" "023" "025" "026" "029" "030" "031" "032"
  "033" "035" "036" "037" "038" "039" "041" "043" "044" "045"
  "046" "047" "048" "049" "050" "051" "052" "053" "054" "055"
  "057" "058" "059" "060" "061" "062" "063" "064" "065" "066"
  "067" "068" "069" "071" "072" "073" "074" "076" "078" "079"
  "080" "081"
)

TOTAL=${#FAILED_EPICS[@]}
DELAY=12

echo "Launching $TOTAL epics with ${DELAY}s delay..."
echo ""

for i in "${!FAILED_EPICS[@]}"; do
  EPIC_NUM="${FAILED_EPICS[$i]}"
  SCRIPT="scripts/wave4/_p3_${EPIC_NUM}.sh"
  SESSION_NAME="p3-${EPIC_NUM}-retry"
  
  if [ ! -f "$SCRIPT" ]; then
    echo "ERROR: Script not found: $SCRIPT"
    continue
  fi
  
  # Launch in screen session (use bash -l for both outer and inner shells)
  screen -dmS "$SESSION_NAME" bash -l -c "cd /home/malhitticrypto/universal-or-strategy && bash -l $SCRIPT"
  
  PROGRESS=$((i + 1))
  echo "[$PROGRESS/$TOTAL] Launched EPIC-CCN-${EPIC_NUM} (session: $SESSION_NAME)"
  
  # Delay between launches (except last)
  if [ $PROGRESS -lt $TOTAL ]; then
    sleep $DELAY
  fi
done

echo ""
echo "=== Launch Complete ==="
echo "End time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo "Total epics launched: $TOTAL"
echo "Expected duration: ~20 minutes (parallel execution)"
echo ""
echo "Monitor with:"
echo "  screen -ls | grep 'p3-.*-retry'"
echo "  ls docs/brain/EPIC-CCN-*/03-audit-report.md | wc -l"
echo ""

# Made with Bob
