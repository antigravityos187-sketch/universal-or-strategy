#!/bin/bash
# Monitor Wave 6 Phase 1 completion for 24 relaunched epics
# Polls every 4 minutes (cost-optimized per protocol)

EPICS=(
  "001" "004" "016" "020" "021" "028"
  "050" "051" "052" "053" "054" "055" "056" "057" "058" "059" "060" "061"
  "070" "073" "076" "077" "078" "079"
)

echo "=== Wave 6 Phase 1 - Monitoring 24 Epics ==="
echo "Poll interval: 4 minutes (cost-optimized)"
echo ""

while true; do
  TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')
  echo "[$TIMESTAMP] Checking status..."
  
  COMPLETED=0
  IN_PROGRESS=0
  PENDING=0
  FAILED=0
  
  for EPIC_NUM in "${EPICS[@]}"; do
    EPIC_ID="EPIC-CCN-${EPIC_NUM}"
    MANIFEST="docs/brain/${EPIC_ID}/manifest.json"
    
    if [ -f "$MANIFEST" ]; then
      STATUS=$(python3 -c "import json; print(json.load(open('$MANIFEST'))['phases']['1']['status'])" 2>/dev/null || echo "unknown")
      
      case "$STATUS" in
        "completed")
          ((COMPLETED++))
          ;;
        "in_progress")
          ((IN_PROGRESS++))
          ;;
        "pending")
          ((PENDING++))
          ;;
        "failed")
          ((FAILED++))
          echo "  ⚠️  $EPIC_ID: FAILED"
          ;;
        *)
          echo "  ❓ $EPIC_ID: $STATUS"
          ;;
      esac
    else
      echo "  ❌ $EPIC_ID: manifest not found"
    fi
  done
  
  TOTAL=24
  PERCENT=$((COMPLETED * 100 / TOTAL))
  
  echo ""
  echo "Progress: $COMPLETED/$TOTAL ($PERCENT%)"
  echo "  ✅ Completed: $COMPLETED"
  echo "  ⏳ In Progress: $IN_PROGRESS"
  echo "  ⏸️  Pending: $PENDING"
  echo "  ❌ Failed: $FAILED"
  echo ""
  
  if [ $COMPLETED -eq $TOTAL ]; then
    echo "🎉 All 24 epics completed!"
    break
  fi
  
  if [ $FAILED -gt 0 ]; then
    echo "⚠️  $FAILED epic(s) failed - review logs"
  fi
  
  echo "Next check in 4 minutes..."
  sleep 240
done

echo ""
echo "=== Monitoring Complete ==="
echo "Final status: $COMPLETED/$TOTAL completed"

# Made with Bob
