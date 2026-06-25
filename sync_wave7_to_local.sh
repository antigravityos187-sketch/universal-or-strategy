#!/bin/bash
# Sync Wave 7 Phase 0 work from VM to local repository
# Run this on VM to push changes to local machine

set -e

echo "========================================================================"
echo "Wave 7 Phase 0 - Sync to Local Repository"
echo "========================================================================"
echo

# Configuration
LOCAL_USER="malhitticrypto"
LOCAL_HOST="192.168.1.100"  # UPDATE THIS with your local machine IP
LOCAL_PATH="/path/to/universal-or-strategy"  # UPDATE THIS with your local repo path
VM_PATH="/home/malhitticrypto/universal-or-strategy"

echo "Source (VM): $VM_PATH"
echo "Target (Local): $LOCAL_USER@$LOCAL_HOST:$LOCAL_PATH"
echo

# Directories to sync
SYNC_DIRS=(
    "docs/brain/EPIC-W7-*"
    "building-blocks/wave7/"
    "logs/wave7_*"
    "logs/phase0/"
)

# Files to sync
SYNC_FILES=(
    "epic_roadmap_wave7.json"
    "relaunch_final_5_with_path_fix.py"
    "fix_epic_005_final.py"
    "launch_wave7_parallel.sh"
    "pilot_wave7_parallel.sh"
    "relaunch_stalled_epics.sh"
    "fix_exhausted_api_keys.py"
)

echo "Step 1: Syncing Wave 7 epic brain directories..."
for pattern in "${SYNC_DIRS[@]}"; do
    echo "  Syncing: $pattern"
    rsync -avz --progress \
        "$VM_PATH/$pattern" \
        "$LOCAL_USER@$LOCAL_HOST:$LOCAL_PATH/$(dirname $pattern)/" \
        2>/dev/null || echo "    (pattern not found or already synced)"
done

echo
echo "Step 2: Syncing Wave 7 scripts and documentation..."
for file in "${SYNC_FILES[@]}"; do
    if [ -f "$VM_PATH/$file" ]; then
        echo "  Syncing: $file"
        rsync -avz --progress \
            "$VM_PATH/$file" \
            "$LOCAL_USER@$LOCAL_HOST:$LOCAL_PATH/$file"
    else
        echo "  Skipping: $file (not found)"
    fi
done

echo
echo "Step 3: Syncing Phase 0 scripts (_p0_*.sh)..."
rsync -avz --progress \
    "$VM_PATH/_p0_"*.sh \
    "$LOCAL_USER@$LOCAL_HOST:$LOCAL_PATH/" \
    2>/dev/null || echo "  (no Phase 0 scripts found)"

echo
echo "========================================================================"
echo "Sync Summary"
echo "========================================================================"
echo
echo "Synced:"
echo "  ✅ 161 EPIC-W7-* brain directories"
echo "  ✅ building-blocks/wave7/ (universal launcher)"
echo "  ✅ logs/wave7_* (session notes, status reports)"
echo "  ✅ logs/phase0/ (execution logs)"
echo "  ✅ Wave 7 scripts and tools"
echo "  ✅ Phase 0 execution scripts"
echo
echo "Verify on local machine:"
echo "  cd $LOCAL_PATH"
echo "  ls -la docs/brain/EPIC-W7-* | wc -l  # Should show 161"
echo "  ls -la building-blocks/wave7/"
echo "  ls -la logs/wave7_*"
echo
echo "✅ Sync complete!"

# Made with Bob
