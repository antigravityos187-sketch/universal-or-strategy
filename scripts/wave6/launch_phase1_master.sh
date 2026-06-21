#!/bin/bash
# Wave 6 Phase 1 Master Launch Script
# Cost-Optimized: 4-minute polling intervals (88% cost reduction)
# Protocol: docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md

set -e

WAVE="6"
PHASE="1"
TOTAL_EPICS=78
ZONE="us-central1-a"
VM="v12-test-golden-v2"

echo "=========================================="
echo "Wave $WAVE Phase $PHASE Master Launch"
echo "=========================================="
echo "Total Epics: $TOTAL_EPICS"
echo "Polling: 4-minute intervals (cache-optimized)"
echo "VM: $VM"
echo "Zone: $ZONE"
echo ""

# Step 1: Launch all Phase 1 scripts in parallel
echo "Step 1: Launching all $TOTAL_EPICS Phase 1 scripts..."
echo "----------------------------------------"

gcloud compute ssh $VM --zone=$ZONE --command="
cd /home/malhitticrypto/universal-or-strategy

# Create logs directory
mkdir -p logs/wave6/phase1

# Launch all Phase 1 scripts in screen sessions
for SCRIPT in scripts/wave6/_p1_epic_ccn_*.sh; do
    EPIC_ID=\$(basename \$SCRIPT .sh | sed 's/_p1_epic_ccn_/EPIC-CCN-/')
    SESSION_NAME=\"wave6-p1-\${EPIC_ID}\"
    
    # Launch in detached screen
    screen -dmS \"\$SESSION_NAME\" bash \"\$SCRIPT\"
done

echo \"✅ Launched \$TOTAL_EPICS Phase 1 scripts\"
screen -ls | grep -c 'wave6-p1' || echo '0'
"

echo ""
echo "✅ All scripts launched"
echo ""

# Step 2: Initial status check (1 minute)
echo "Step 2: Initial status check (T+1 min)..."
echo "----------------------------------------"
sleep 60

gcloud compute ssh $VM --zone=$ZONE --command="
cd /home/malhitticrypto/universal-or-strategy

RUNNING=\$(screen -ls | grep -c 'wave6-p1' || echo '0')
COMPLETE=\$(find docs/brain/EPIC-CCN-*/manifest.json 2>/dev/null | xargs grep -l '\"1\": {\"status\": \"completed\"' 2>/dev/null | wc -l)

echo \"Running agents: \$RUNNING\"
echo \"Completed epics: \$COMPLETE/$TOTAL_EPICS\"
"

echo ""

# Step 3: Continuous monitoring (4-minute polling)
echo "Step 3: Starting 4-minute polling cycle..."
echo "----------------------------------------"
echo "Press Ctrl+C to stop monitoring (scripts will continue running)"
echo ""

POLL_COUNT=0
START_TIME=$(date +%s)

while true; do
    sleep 240  # 4 minutes (cache-optimized)
    POLL_COUNT=$((POLL_COUNT + 1))
    CURRENT_TIME=$(date +%s)
    ELAPSED=$((CURRENT_TIME - START_TIME))
    ELAPSED_MIN=$((ELAPSED / 60))
    
    echo "=== Poll #$POLL_COUNT (T+${ELAPSED_MIN} min) at $(date) ==="
    
    # Get status from VM
    STATUS=$(gcloud compute ssh $VM --zone=$ZONE --command="
cd /home/malhitticrypto/universal-or-strategy

RUNNING=\$(screen -ls | grep -c 'wave6-p1' || echo '0')
COMPLETE=\$(find docs/brain/EPIC-CCN-*/manifest.json 2>/dev/null | xargs grep -l '\"1\": {\"status\": \"completed\"' 2>/dev/null | wc -l)
ERRORS=\$(grep -i 'error\|failed\|exception' logs/wave6/phase1/*.log 2>/dev/null | wc -l)

echo \"RUNNING:\$RUNNING\"
echo \"COMPLETE:\$COMPLETE\"
echo \"ERRORS:\$ERRORS\"
")
    
    RUNNING=$(echo "$STATUS" | grep "RUNNING:" | cut -d: -f2)
    COMPLETE=$(echo "$STATUS" | grep "COMPLETE:" | cut -d: -f2)
    ERRORS=$(echo "$STATUS" | grep "ERRORS:" | cut -d: -f2)
    
    echo "  Running agents: $RUNNING"
    echo "  Completed: $COMPLETE/$TOTAL_EPICS"
    echo "  Errors detected: $ERRORS"
    
    # Check if all complete
    if [ "$COMPLETE" -eq "$TOTAL_EPICS" ]; then
        echo ""
        echo "=========================================="
        echo "✅ Wave $WAVE Phase $PHASE COMPLETE!"
        echo "=========================================="
        echo "Total time: ${ELAPSED_MIN} minutes"
        echo "Total polls: $POLL_COUNT"
        echo "Cache hit rate: ~$((100 * (POLL_COUNT - 1) / POLL_COUNT))%"
        echo ""
        break
    fi
    
    # Sample errors if any
    if [ "$ERRORS" -gt 0 ]; then
        echo "  Sampling errors..."
        gcloud compute ssh $VM --zone=$ZONE --command="
cd /home/malhitticrypto/universal-or-strategy
grep -i 'error\|failed\|exception' logs/wave6/phase1/*.log 2>/dev/null | head -3
" || true
    fi
    
    echo ""
done

echo "Next: Proceed to Phase 1.5 (Scope Boundary Validation)"
echo ""

# Made with Bob
