#!/bin/bash
set -e

echo "=========================================="
echo "Wave 2 Launch - Sequential Execution"
echo "=========================================="
echo "Start Time: $(date)"
echo ""

# Configuration
PROJECT="project-14c86305-3cba-493f-a73"
ZONE="us-central1-a"
IMAGE="v12-bob-shell-golden-v2"
MACHINE_TYPE="n2-standard-8"
DISK_SIZE="100GB"

# Wave 2 epics (10 epics)
EPICS=(
  "EPIC-CCN-164"
  "EPIC-CCN-107"
  "EPIC-CCN-108"
  "EPIC-CCN-109"
  "EPIC-CCN-110"
  "EPIC-CCN-111"
  "EPIC-CCN-112"
  "EPIC-CCN-113"
  "EPIC-CCN-114"
  "EPIC-CCN-115"
)

echo "Wave 2 Configuration:"
echo "- Total epics: ${#EPICS[@]}"
echo "- Execution mode: Sequential (due to vCPU quota: 12 global)"
echo "- Machine type: $MACHINE_TYPE (8 vCPUs)"
echo "- Image: $IMAGE"
echo ""

# Function to launch and execute epic on VM
execute_epic() {
  local epic_id=$1
  local vm_name="v12-wave2-${epic_id,,}"  # Convert to lowercase
  
  echo "=========================================="
  echo "Processing: $epic_id"
  echo "=========================================="
  echo "VM: $vm_name"
  echo "Start: $(date)"
  echo ""
  
  # Step 1: Launch VM
  echo "Step 1: Launching VM..."
  gcloud compute instances create "$vm_name" \
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
  
  # Step 2: Wait for VM to be ready (30 seconds)
  echo "Step 2: Waiting for VM to boot..."
  sleep 30
  echo "✅ VM ready"
  echo ""
  
  # Step 3: Execute epic workflow
  echo "Step 3: Executing epic workflow..."
  gcloud compute ssh "$vm_name" \
    --project="$PROJECT" \
    --zone="$ZONE" \
    --command="cd ~/universal-or-strategy && bob --accept-license --auth-method api-key -p 'Run epic-intake for $epic_id' --max-coins 30"
  
  echo "✅ Epic workflow complete"
  echo ""
  
  # Step 4: Stop VM (to save costs)
  echo "Step 4: Stopping VM..."
  gcloud compute instances stop "$vm_name" \
    --project="$PROJECT" \
    --zone="$ZONE"
  
  echo "✅ VM stopped"
  echo ""
  
  echo "Completed: $epic_id"
  echo "End: $(date)"
  echo ""
}

# Execute all epics sequentially
for epic in "${EPICS[@]}"; do
  execute_epic "$epic"
done

echo "=========================================="
echo "Wave 2 Complete"
echo "=========================================="
echo "End Time: $(date)"
echo ""
echo "Summary:"
echo "- Total epics processed: ${#EPICS[@]}"
echo "- VMs created: ${#EPICS[@]}"
echo "- VMs stopped: ${#EPICS[@]}"
echo ""
echo "Next steps:"
echo "1. Review epic artifacts in docs/brain/EPIC-*/
echo "2. Delete stopped VMs: gcloud compute instances delete v12-wave2-* --zone=$ZONE"
echo "3. Launch Wave 3 if successful"

# Made with Bob
