#!/bin/bash
set -e

echo "=========================================="
echo "Wave 2 TEST - 2 Parallel Agents"
echo "=========================================="
echo "Architecture: 1 VM × 2 Bob Shell Agents"
echo "Start Time: $(date)"
echo ""

# Configuration
PROJECT="project-14c86305-3cba-493f-a73"
ZONE="us-central1-a"
IMAGE="v12-bob-shell-golden-v2"
MACHINE_TYPE="n2-standard-8"
DISK_SIZE="100GB"
VM_NAME="v12-wave2-test-2agents"

# Test with 2 epics only
EPICS=(
  "EPIC-CCN-164:IsCommandForThisInstrument:36"
  "EPIC-CCN-107:OnBarUpdate:28"
)

echo "Test Configuration:"
echo "- Total epics: ${#EPICS[@]}"
echo "- Execution mode: Parallel (2 agents on 1 VM)"
echo "- Machine type: $MACHINE_TYPE (8 vCPUs, 32 GB RAM)"
echo "- VM name: $VM_NAME"
echo ""

# Step 1: Launch VM
echo "=========================================="
echo "Step 1: Launching VM"
echo "=========================================="
echo "Time: $(date)"
echo ""

gcloud compute instances create "$VM_NAME" \
  --project="$PROJECT" \
  --zone="$ZONE" \
  --machine-type="$MACHINE_TYPE" \
  --image="$IMAGE" \
  --boot-disk-size="$DISK_SIZE" \
  --maintenance-policy=TERMINATE \
  --provisioning-model=SPOT \
  --scopes=cloud-platform

echo "✅ VM launched"
echo ""

# Step 2: Wait for VM boot
echo "=========================================="
echo "Step 2: Waiting for VM Boot"
echo "=========================================="
echo "Waiting 30 seconds..."
sleep 30
echo "✅ VM ready"
echo ""

# Step 3: Create logs directory
echo "=========================================="
echo "Step 3: Preparing Environment"
echo "=========================================="
gcloud compute ssh "$VM_NAME" \
  --project="$PROJECT" \
  --zone="$ZONE" \
  --command="mkdir -p ~/universal-or-strategy/logs"

echo "✅ Logs directory created"
echo ""

# Step 4: Launch 2 parallel agents
echo "=========================================="
echo "Step 4: Launching 2 Parallel Agents"
echo "=========================================="
echo "Time: $(date)"
echo ""

# Build parallel command
PARALLEL_CMD="cd ~/universal-or-strategy && "

for epic_spec in "${EPICS[@]}"; do
  IFS=':' read -r epic_id method_name cyc <<< "$epic_spec"
  echo "Launching agent for $epic_id ($method_name, CYC $cyc)"
  
  PARALLEL_CMD+="bob --accept-license --auth-method api-key -p 'Run epic-intake for $epic_id. Target: Reduce complexity in $method_name (CYC $cyc to 8)' --max-coins 30 > logs/${epic_id}.log 2>&1 & "
done

PARALLEL_CMD+="wait && echo 'Both agents complete'"

echo ""
echo "Executing parallel agents..."
echo ""

# Execute via SSH (this will block until both agents complete)
gcloud compute ssh "$VM_NAME" \
  --project="$PROJECT" \
  --zone="$ZONE" \
  --command="$PARALLEL_CMD"

echo ""
echo "✅ Both agents completed"
echo ""

# Step 5: Retrieve logs
echo "=========================================="
echo "Step 5: Retrieving Logs"
echo "=========================================="

mkdir -p logs/wave2-test

for epic_spec in "${EPICS[@]}"; do
  IFS=':' read -r epic_id method_name cyc <<< "$epic_spec"
  
  gcloud compute scp "$VM_NAME:~/universal-or-strategy/logs/${epic_id}.log" \
    "logs/wave2-test/${epic_id}.log" \
    --project="$PROJECT" \
    --zone="$ZONE" || echo "⚠️ Could not retrieve log for $epic_id"
done

echo "✅ Logs retrieved"
echo ""

# Step 6: Display log summaries
echo "=========================================="
echo "Step 6: Log Summaries"
echo "=========================================="

for epic_spec in "${EPICS[@]}"; do
  IFS=':' read -r epic_id method_name cyc <<< "$epic_spec"
  
  echo ""
  echo "--- $epic_id Log (last 20 lines) ---"
  tail -20 "logs/wave2-test/${epic_id}.log" || echo "⚠️ Log not found"
  echo ""
done

# Step 7: Stop VM
echo "=========================================="
echo "Step 7: Stopping VM"
echo "=========================================="

gcloud compute instances stop "$VM_NAME" \
  --project="$PROJECT" \
  --zone="$ZONE"

echo "✅ VM stopped"
echo ""

# Summary
echo "=========================================="
echo "Test Complete"
echo "=========================================="
echo "End Time: $(date)"
echo ""
echo "Results:"
echo "- Epics processed: ${#EPICS[@]}"
echo "- Logs: logs/wave2-test/"
echo ""
echo "Next steps:"
echo "1. Review logs above"
echo "2. If successful, run full Wave 2 (10 agents)"
echo "3. Delete test VM: gcloud compute instances delete $VM_NAME --zone=$ZONE"

# Made with Bob
