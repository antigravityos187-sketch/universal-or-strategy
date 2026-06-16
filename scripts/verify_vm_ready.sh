#!/bin/bash
# Quick VM readiness check script
# Usage: ./verify_vm_ready.sh <vm-name> <project> <zone>

VM_NAME=${1:-v12-epic-executor-v3}
PROJECT=${2:-project-14c86305-3cba-493f-a73}
ZONE=${3:-us-central1-a}

echo "=== Checking VM: $VM_NAME ==="

# Check VM status
echo "1. VM Status:"
gcloud compute instances describe "$VM_NAME" --project="$PROJECT" --zone="$ZONE" --format="value(status)" 2>/dev/null || echo "VM not found"

# Check status file
echo -e "\n2. Startup Status:"
gcloud compute ssh "$VM_NAME" --project="$PROJECT" --zone="$ZONE" --strict-host-key-checking=no --command="cat /tmp/vm_status.json 2>/dev/null || echo 'Status file not created yet'" 2>/dev/null

# Check Bob installation
echo -e "\n3. Bob Shell:"
gcloud compute ssh "$VM_NAME" --project="$PROJECT" --zone="$ZONE" --strict-host-key-checking=no --command="which bob && bob --version 2>/dev/null || echo 'Bob not installed'" 2>/dev/null

# Check repository
echo -e "\n4. Repository:"
gcloud compute ssh "$VM_NAME" --project="$PROJECT" --zone="$ZONE" --strict-host-key-checking=no --command="ls -la ~/universal-or-strategy/.git 2>/dev/null && echo 'Repository present' || echo 'Repository not cloned'" 2>/dev/null

echo -e "\n=== Verification Complete ==="

# Made with Bob
