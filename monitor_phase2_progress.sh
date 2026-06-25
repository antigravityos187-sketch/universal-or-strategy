#!/bin/bash
# Monitor Wave 7 Phase 2 progress with 4-minute polling

echo "=== Wave 7 Phase 2 Progress Monitor ==="
echo "Started: $(date)"
echo "Polling interval: 4 minutes"
echo ""

TOTAL=161
LAST_COUNT=0

while true; do
    # Count completed epics
    COMPLETED=$(find docs/brain/EPIC-W7-*/02-architecture-plan.md 2>/dev/null | wc -l)
    PROGRESS_PCT=$((COMPLETED * 100 / TOTAL))
    
    # Show progress
    echo "[$(date '+%H:%M:%S')] Progress: $COMPLETED/$TOTAL ($PROGRESS_PCT%)"
    
    # Show newly completed
    if [ $COMPLETED -gt $LAST_COUNT ]; then
        NEW=$((COMPLETED - LAST_COUNT))
        echo "  ✅ $NEW new completions"
        LAST_COUNT=$COMPLETED
    fi
    
    # Check if done
    if [ $COMPLETED -ge $TOTAL ]; then
        echo ""
        echo "✅ Wave 7 Phase 2 COMPLETE: $COMPLETED/$TOTAL"
        break
    fi
    
    # Wait 4 minutes
    sleep 240
done

# Made with Bob
