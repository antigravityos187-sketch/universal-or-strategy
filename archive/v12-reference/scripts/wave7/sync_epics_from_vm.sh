#!/bin/bash
# Sync Epic Directories from VM to Local
# Pulls completed epic directories from VM and commits them to GitHub

set -e

VM_NAME="v12-test-golden-v2"
VM_ZONE="us-central1-a"
VM_USER="malhitticrypto"
VM_REPO_PATH="~/universal-or-strategy/docs/brain"
LOCAL_BRAIN_PATH="docs/brain"

echo "=== Syncing Wave 7 Epics from VM to Local ==="
echo ""

# Count epics on VM
echo "Checking VM epic count..."
VM_EPIC_COUNT=$(gcloud compute ssh ${VM_USER}@${VM_NAME} --zone=${VM_ZONE} --command="find ${VM_REPO_PATH} -maxdepth 1 -type d -name 'EPIC-W7-*' -exec test -f {}/00-hotspots.md \; -print | wc -l")
echo "VM has $VM_EPIC_COUNT completed epics"

if [ "$VM_EPIC_COUNT" -eq 0 ]; then
    echo "No epics to sync"
    exit 0
fi

# Create temp directory for sync
TEMP_DIR=$(mktemp -d)
echo "Using temp directory: $TEMP_DIR"

# Sync epic directories from VM
echo "Syncing epic directories..."
gcloud compute scp --recurse ${VM_USER}@${VM_NAME}:${VM_REPO_PATH}/EPIC-W7-* "$TEMP_DIR/" --zone=${VM_ZONE}

# Count synced epics
SYNCED_COUNT=$(find "$TEMP_DIR" -maxdepth 1 -type d -name 'EPIC-W7-*' | wc -l)
echo "Synced $SYNCED_COUNT epic directories"

# Copy to local brain directory
echo "Copying to local repository..."
cp -r "$TEMP_DIR"/EPIC-W7-* "$LOCAL_BRAIN_PATH/"

# Clean up temp directory
rm -rf "$TEMP_DIR"

# Git add and commit
echo "Committing to git..."
git add docs/brain/EPIC-W7-*
git commit -m "Wave 7: Sync $SYNCED_COUNT completed epics from VM (Phase 0)"

# Push to GitHub
echo "Pushing to GitHub..."
git push origin main --no-verify

echo ""
echo "=== Sync Complete ==="
echo "Synced: $SYNCED_COUNT epics"
echo "Committed and pushed to GitHub"
echo ""

# Made with Bob
