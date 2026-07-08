#!/bin/bash
# Package Wave 7 Phase 0 work for transfer to local machine
# Creates a compressed archive that can be downloaded via SCP

set -e

echo "========================================================================"
echo "Wave 7 Phase 0 - Package for Local Backup"
echo "========================================================================"
echo

TIMESTAMP=$(date +%Y%m%d_%H%M%S)
ARCHIVE_NAME="wave7_phase0_complete_${TIMESTAMP}.tar.gz"
TEMP_DIR="/tmp/wave7_backup_$$"

echo "Creating temporary staging directory..."
mkdir -p "$TEMP_DIR"

echo
echo "Step 1: Copying Wave 7 epic brain directories (161 epics)..."
mkdir -p "$TEMP_DIR/docs/brain"
cp -r docs/brain/EPIC-W7-* "$TEMP_DIR/docs/brain/" 2>/dev/null || true
EPIC_COUNT=$(ls -d "$TEMP_DIR/docs/brain/EPIC-W7-"* 2>/dev/null | wc -l)
echo "  ✅ Copied $EPIC_COUNT epic directories"

echo
echo "Step 2: Copying building-blocks/wave7/..."
mkdir -p "$TEMP_DIR/building-blocks"
cp -r building-blocks/wave7 "$TEMP_DIR/building-blocks/" 2>/dev/null || true
echo "  ✅ Copied universal launcher and templates"

echo
echo "Step 3: Copying logs..."
mkdir -p "$TEMP_DIR/logs"
cp -r logs/wave7_* "$TEMP_DIR/logs/" 2>/dev/null || true
cp -r logs/phase0 "$TEMP_DIR/logs/" 2>/dev/null || true
echo "  ✅ Copied session logs and execution logs"

echo
echo "Step 4: Copying Wave 7 scripts and tools..."
cp epic_roadmap_wave7.json "$TEMP_DIR/" 2>/dev/null || true
cp relaunch_final_5_with_path_fix.py "$TEMP_DIR/" 2>/dev/null || true
cp fix_epic_005_final.py "$TEMP_DIR/" 2>/dev/null || true
cp launch_wave7_parallel.sh "$TEMP_DIR/" 2>/dev/null || true
cp pilot_wave7_parallel.sh "$TEMP_DIR/" 2>/dev/null || true
cp relaunch_stalled_epics.sh "$TEMP_DIR/" 2>/dev/null || true
cp fix_exhausted_api_keys.py "$TEMP_DIR/" 2>/dev/null || true
cp sync_wave7_to_local.sh "$TEMP_DIR/" 2>/dev/null || true
cp package_wave7_for_local.sh "$TEMP_DIR/" 2>/dev/null || true
echo "  ✅ Copied Wave 7 execution scripts"

echo
echo "Step 5: Copying Phase 0 scripts (_p0_*.sh)..."
mkdir -p "$TEMP_DIR/phase0_scripts"
cp _p0_*.sh "$TEMP_DIR/phase0_scripts/" 2>/dev/null || true
SCRIPT_COUNT=$(ls "$TEMP_DIR/phase0_scripts/_p0_"*.sh 2>/dev/null | wc -l)
echo "  ✅ Copied $SCRIPT_COUNT Phase 0 scripts"

echo
echo "Step 6: Creating archive..."
cd /tmp
tar -czf "$ARCHIVE_NAME" "wave7_backup_$$/"
ARCHIVE_SIZE=$(du -h "$ARCHIVE_NAME" | cut -f1)
echo "  ✅ Created $ARCHIVE_NAME ($ARCHIVE_SIZE)"

echo
echo "Step 7: Moving archive to home directory..."
mv "$ARCHIVE_NAME" ~/
echo "  ✅ Archive location: ~/$ARCHIVE_NAME"

echo
echo "Step 8: Cleaning up temporary directory..."
rm -rf "$TEMP_DIR"
echo "  ✅ Cleanup complete"

echo
echo "========================================================================"
echo "Package Summary"
echo "========================================================================"
echo
echo "Archive: ~/$ARCHIVE_NAME"
echo "Size: $ARCHIVE_SIZE"
echo
echo "Contents:"
echo "  ✅ $EPIC_COUNT EPIC-W7-* brain directories"
echo "  ✅ building-blocks/wave7/ (universal launcher)"
echo "  ✅ logs/wave7_* (session notes, status reports)"
echo "  ✅ logs/phase0/ (execution logs)"
echo "  ✅ $SCRIPT_COUNT Phase 0 execution scripts"
echo "  ✅ Wave 7 tools and documentation"
echo
echo "Download to local machine:"
echo "  scp malhitticrypto@VM_IP:~/$ARCHIVE_NAME ."
echo
echo "Extract on local machine:"
echo "  tar -xzf $ARCHIVE_NAME"
echo "  cd wave7_backup_*/"
echo "  cp -r * /path/to/universal-or-strategy/"
echo
echo "✅ Package complete!"

# Made with Bob
