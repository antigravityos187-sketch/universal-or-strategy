#!/bin/bash

# Monitor Wave 7 Phase 0 completion progress
# Polls every 30 seconds until 161/161 complete

TARGET=161
POLL_INTERVAL=30

echo "=========================================="
echo "Wave 7 Phase 0 Completion Monitor"
echo "=========================================="
echo "Target: $TARGET epics"
echo "Poll interval: ${POLL_INTERVAL}s"
echo ""

while true; do
    COMPLETED=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
        --command="cd universal-or-strategy && ls docs/brain/EPIC-W7-*/00-hotspots.md 2>/dev/null | wc -l" 2>/dev/null | grep -v WARNING | tr -d ' ')
    
    TIMESTAMP=$(date -u +"%Y-%m-%d %H:%M:%S UTC")
    PERCENT=$((COMPLETED * 100 / TARGET))
    
    echo "[$TIMESTAMP] Progress: $COMPLETED/$TARGET ($PERCENT%)"
    
    if [ "$COMPLETED" -eq "$TARGET" ]; then
        echo ""
        echo "=========================================="
        echo "✓ COMPLETE: $TARGET/$TARGET (100%)"
        echo "=========================================="
        exit 0
    fi
    
    sleep $POLL_INTERVAL
done

# Made with Bob
