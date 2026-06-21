#!/bin/bash
# Wave 6 Phase 1 Local Launch & Monitor
# Launches scripts on VM, monitors from local machine
# Cost-Optimized: 4-minute polling intervals

set -e

WAVE="6"
PHASE="1"
TOTAL_EPICS=78
ZONE="us-central1-a"
VM="v12-test-golden-v2"

echo "=========================================="
echo "Wave $WAVE Phase $PHASE Launch (Local)"
echo "=========================================="
echo "Total Epics: $TOTAL_EPICS"
echo "Polling: 4-minute intervals"
echo "VM: $VM"
echo ""

# Step 1: Launch all scripts on VM
echo "Step 1: Launching all $TOTAL_EPICS scripts on VM..."
echo "----------------------------------------"

gcloud compute ssh $VM --zone=$ZONE --command="
cd /home/malhitticrypto/universal-or-strategy
mkdir -p logs/wave6/phase1

# Launch all Phase 1 scripts in screen sessions
for SCRIPT in scripts/wave6/_p1_epic_ccn_*.sh; do
    EPIC_NUM=\$(basename \$SCRIPT .sh | sed 's/_p1_epic_ccn_//')
    SESSION_NAME=\"w6p1-\${EPIC_NUM}\"
    screen -dmS \"\$SESSION_NAME\" bash \"\$SCRIPT\"
done

echo \"Launched scripts. Checking screen sessions...\"
screen -ls | grep -c 'w6p1-' || echo '0'
"

echo ""
echo "✅ Scripts launched on VM"
echo ""

# Step 2: Initial check (1 minute)
echo "Step 2: Initial status check (T+1 min)..."
sleep 60

RUNNING=$(gcloud compute ssh $VM --zone=$ZONE --command="screen -ls | grep -c 'w6p1-' || echo '0'")
COMPLETE=$(gcloud compute ssh $VM --zone=$ZONE --command="find /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/manifest.json 2>/dev/null | xargs grep -l '\"1\": {\"status\": \"completed\"' 2>/dev/null | wc -l")

echo "  Running: $RUNNING"
echo "  Complete: $COMPLETE/$TOTAL_EPICS"
echo ""

# Step 3: 4-minute polling
echo "Step 3: Starting 4-minute polling..."
echo "Press Ctrl+C to stop (scripts continue on VM)"
echo ""

POLL_COUNT=0
START_TIME=$(date +%s)

while true; do
    sleep 240  # 4 minutes
    POLL_COUNT=$((POLL_COUNT + 1))
    ELAPSED=$(( $(date +%s) - START_TIME ))
    ELAPSED_MIN=$((ELAPSED / 60))
    
    echo "=== Poll #$POLL_COUNT (T+${ELAPSED_MIN} min) ==="
    
    RUNNING=$(gcloud compute ssh $VM --zone=$ZONE --command="screen -ls | grep -c 'w6p1-' || echo '0'")
    COMPLETE=$(gcloud compute ssh $VM --zone=$ZONE --command="find /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/manifest.json 2>/dev/null | xargs grep -l '\"1\": {\"status\": \"completed\"' 2>/dev/null | wc -l")
    
    echo "  Running: $RUNNING"
    echo "  Complete: $COMPLETE/$TOTAL_EPICS"
    
    if [ "$COMPLETE" -eq "$TOTAL_EPICS" ]; then
        echo ""
        echo "✅ Wave $WAVE Phase $PHASE COMPLETE!"
        echo "Time: ${ELAPSED_MIN} min, Polls: $POLL_COUNT"
        break
    fi
    echo ""
done

# Made with Bob
