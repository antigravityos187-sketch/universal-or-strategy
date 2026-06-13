#!/bin/bash
set -e

echo "=========================================="
echo "Wave 2 Launch - Parallel Execution"
echo "=========================================="
echo "Architecture: 1 VM × 10 Bob Shell Agents"
echo "Start Time: $(date)"
echo ""

# Configuration
PROJECT="project-14c86305-3cba-493f-a73"
ZONE="us-central1-a"
IMAGE="v12-bob-shell-golden-v2"
MACHINE_TYPE="n2-standard-8"
DISK_SIZE="100GB"
VM_NAME="v12-wave2-parallel"

# Wave 2 epics (10 epics)
EPICS=(
  "EPIC-CCN-164:IsCommandForThisInstrument:36"
  "EPIC-CCN-107:OnBarUpdate:28"
  "EPIC-CCN-108:OnOrderUpdate:26"
  "EPIC-CCN-109:OnExecutionUpdate:24"
  "EPIC-CCN-110:OnPositionUpdate:22"
  "EPIC-CCN-111:OnAccountItemUpdate:21"
  "EPIC-CCN-112:ProcessMarketData:20"
  "EPIC-CCN-113:ValidateOrderParameters:20"
  "EPIC-CCN-114:CalculatePositionSize:19"
  "EPIC-CCN-115:UpdateRiskMetrics:19"
)

echo "Wave 2 Configuration:"
echo "- Total epics: ${#EPICS[@]}"
echo "- Execution mode: Parallel (10 agents on 1 VM)"
echo "- Machine type: $MACHINE_TYPE (8 vCPUs, 32 GB RAM)"
echo "- Image: $IMAGE"
echo "- VM name: $VM_NAME"
echo ""

# Step 1: Launch VM
echo "=========================================="
echo "Step 1: Launching VM"
echo "=========================================="
echo "VM: $VM_NAME"
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

echo "✅ VM launched successfully"
echo ""

# Step 2: Wait for VM to be ready
echo "=========================================="
echo "Step 2: Waiting for VM Boot"
echo "=========================================="
echo "Waiting 30 seconds for VM to initialize..."
sleep 30
echo "✅ VM ready"
echo ""

# Step 3: Create logs directory on VM
echo "=========================================="
echo "Step 3: Preparing VM Environment"
echo "=========================================="
gcloud compute ssh "$VM_NAME" \
  --project="$PROJECT" \
  --zone="$ZONE" \
  --command="mkdir -p ~/universal-or-strategy/logs"

echo "✅ Logs directory created"
echo ""

# Step 4: Execute 10 parallel Bob Shell agents
echo "=========================================="
echo "Step 4: Launching 10 Parallel Bob Shell Agents"
echo "=========================================="
echo "Time: $(date)"
echo ""

# Build the parallel command
PARALLEL_CMD="cd ~/universal-or-strategy && "

for epic_spec in "${EPICS[@]}"; do
  IFS=':' read -r epic_id method_name cyc <<< "$epic_spec"
  echo "Launching agent for $epic_id ($method_name, CYC $cyc)"
  
  PARALLEL_CMD+="bob --accept-license --auth-method api-key -p 'Run epic-intake for $epic_id. Target: Reduce complexity in $method_name (CYC $cyc to 8)' --max-coins 30 > logs/${epic_id}.log 2>&1 & "
done

# Add wait command to wait for all background processes
PARALLEL_CMD+="wait && echo 'All 10 epics complete'"

echo ""
echo "Executing parallel agents..."
echo ""

# Execute the parallel command via SSH
gcloud compute ssh "$VM_NAME" \
  --project="$PROJECT" \
  --zone="$ZONE" \
  --command="$PARALLEL_CMD"

echo ""
echo "✅ All 10 Bob Shell agents completed"
echo ""

# Step 5: Retrieve logs
echo "=========================================="
echo "Step 5: Retrieving Logs"
echo "=========================================="
echo "Downloading logs from VM..."
echo ""

# Create local logs directory
mkdir -p logs/wave2

# Download all logs
for epic_spec in "${EPICS[@]}"; do
  IFS=':' read -r epic_id method_name cyc <<< "$epic_spec"
  
  gcloud compute scp "$VM_NAME:~/universal-or-strategy/logs/${epic_id}.log" \
    "logs/wave2/${epic_id}.log" \
    --project="$PROJECT" \
    --zone="$ZONE" || echo "⚠️ Warning: Could not retrieve log for $epic_id"
done

echo "✅ Logs retrieved"
echo ""

# Step 6: Retrieve epic artifacts
echo "=========================================="
echo "Step 6: Retrieving Epic Artifacts"
echo "=========================================="
echo "Downloading epic artifacts from VM..."
echo ""

for epic_spec in "${EPICS[@]}"; do
  IFS=':' read -r epic_id method_name cyc <<< "$epic_spec"
  
  # Download entire epic directory
  gcloud compute scp --recurse \
    "$VM_NAME:~/universal-or-strategy/docs/brain/${epic_id}" \
    "docs/brain/" \
    --project="$PROJECT" \
    --zone="$ZONE" || echo "⚠️ Warning: Could not retrieve artifacts for $epic_id"
done

echo "✅ Artifacts retrieved"
echo ""

# Step 7: Stop VM
echo "=========================================="
echo "Step 7: Stopping VM"
echo "=========================================="
echo "Stopping VM to save costs..."
echo ""

gcloud compute instances stop "$VM_NAME" \
  --project="$PROJECT" \
  --zone="$ZONE"

echo "✅ VM stopped"
echo ""

# Step 8: Summary
echo "=========================================="
echo "Wave 2 Complete"
echo "=========================================="
echo "End Time: $(date)"
echo ""
echo "Summary:"
echo "- Total epics processed: ${#EPICS[@]}"
echo "- Execution mode: Parallel (10 agents on 1 VM)"
echo "- VM: $VM_NAME (stopped)"
echo ""
echo "Artifacts:"
for epic_spec in "${EPICS[@]}"; do
  IFS=':' read -r epic_id method_name cyc <<< "$epic_spec"
  echo "  - docs/brain/${epic_id}/"
done
echo ""
echo "Logs:"
for epic_spec in "${EPICS[@]}"; do
  IFS=':' read -r epic_id method_name cyc <<< "$epic_spec"
  echo "  - logs/wave2/${epic_id}.log"
done
echo ""
echo "Next steps:"
echo "1. Review epic artifacts in docs/brain/EPIC-*/"
echo "2. Review logs in logs/wave2/"
echo "3. Validate Phase 0 outputs (hotspot analysis)"
echo "4. Delete VM: gcloud compute instances delete $VM_NAME --zone=$ZONE"
echo "5. Launch Wave 3 if successful"
echo ""
echo "Cost estimate: ~\$0.047 (30 min × \$0.093/hour)"

# Made with Bob
