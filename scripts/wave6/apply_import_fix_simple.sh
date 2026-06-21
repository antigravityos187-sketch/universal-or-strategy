#!/bin/bash
# Apply importlib fix to all Phase 1 scripts
# Building-Blocks Method: Use fixed template already on VM

set -e

REPO_ROOT="/home/malhitticrypto/universal-or-strategy"
cd "$REPO_ROOT"

echo "=========================================="
echo "Applying Import Fix to Phase 1 Scripts"
echo "Building-Blocks Method: Template on VM"
echo "=========================================="
echo ""

# Fixed template is already at: scripts/wave6/_p1_FIXED_TEMPLATE.sh
TEMPLATE="scripts/wave6/_p1_FIXED_TEMPLATE.sh"

if [ ! -f "$TEMPLATE" ]; then
    echo "ERROR: Fixed template not found at $TEMPLATE"
    exit 1
fi

echo "✓ Found fixed template"
echo ""

# Apply fix to all 77 Phase 1 scripts (EPIC-CCN-003 through EPIC-CCN-080, excluding 003)
echo "Applying fix to 76 scripts (excluding pilot EPIC-CCN-003)..."
echo ""

FIXED_COUNT=0
SKIPPED_COUNT=0

for i in $(seq 4 80); do
    EPIC_NUM=$(printf "%03d" $i)
    EPIC_ID="EPIC-CCN-$EPIC_NUM"
    SCRIPT="scripts/wave6/_p1_epic_ccn_${EPIC_NUM}_vm.sh"
    
    if [ ! -f "$SCRIPT" ]; then
        echo "⚠ Skipping $EPIC_ID (script not found)"
        ((SKIPPED_COUNT++))
        continue
    fi
    
    # Check if already fixed (has importlib pattern)
    if grep -q "importlib.util.spec_from_file_location" "$SCRIPT"; then
        echo "✓ $EPIC_ID already fixed"
        ((FIXED_COUNT++))
        continue
    fi
    
    # Apply fix by replacing import statements
    echo "Fixing $EPIC_ID..."
    
    # Create backup
    cp "$SCRIPT" "${SCRIPT}.backup"
    
    # Replace all 9 import patterns with importlib pattern
    sed -i 's|from epic_manifest import verify_dependencies|import importlib.util\nspec = importlib.util.spec_from_file_location("epic_manifest", "scripts/epic_manifest.py")\nmodule = importlib.util.module_from_spec(spec)\nspec.loader.exec_module(module)\nverify_dependencies = module.verify_dependencies|g' "$SCRIPT"
    
    sed -i 's|from epic_manifest import verify_can_execute|import importlib.util\nspec = importlib.util.spec_from_file_location("epic_manifest", "scripts/epic_manifest.py")\nmodule = importlib.util.module_from_spec(spec)\nspec.loader.exec_module(module)\nverify_can_execute = module.verify_can_execute|g' "$SCRIPT"
    
    sed -i 's|from epic_manifest import verify_filesystem_state|import importlib.util\nspec = importlib.util.spec_from_file_location("epic_manifest", "scripts/epic_manifest.py")\nmodule = importlib.util.module_from_spec(spec)\nspec.loader.exec_module(module)\nverify_filesystem_state = module.verify_filesystem_state|g' "$SCRIPT"
    
    sed -i 's|from epic_manifest import update_manifest|import importlib.util\nspec = importlib.util.spec_from_file_location("epic_manifest", "scripts/epic_manifest.py")\nmodule = importlib.util.module_from_spec(spec)\nspec.loader.exec_module(module)\nupdate_manifest = module.update_manifest|g' "$SCRIPT"
    
    sed -i 's|from epic_manifest import load_manifest|import importlib.util\nspec = importlib.util.spec_from_file_location("epic_manifest", "scripts/epic_manifest.py")\nmodule = importlib.util.module_from_spec(spec)\nspec.loader.exec_module(module)\nload_manifest = module.load_manifest|g' "$SCRIPT"
    
    sed -i 's|from epic_manifest import get_phase_status|import importlib.util\nspec = importlib.util.spec_from_file_location("epic_manifest", "scripts/epic_manifest.py")\nmodule = importlib.util.module_from_spec(spec)\nspec.loader.exec_module(module)\nget_phase_status = module.get_phase_status|g' "$SCRIPT"
    
    sed -i 's|from epic_manifest import mark_phase_complete|import importlib.util\nspec = importlib.util.spec_from_file_location("epic_manifest", "scripts/epic_manifest.py")\nmodule = importlib.util.module_from_spec(spec)\nspec.loader.exec_module(module)\nmark_phase_complete = module.mark_phase_complete|g' "$SCRIPT"
    
    sed -i 's|from epic_manifest import mark_phase_failed|import importlib.util\nspec = importlib.util.spec_from_file_location("epic_manifest", "scripts/epic_manifest.py")\nmodule = importlib.util.module_from_spec(spec)\nspec.loader.exec_module(module)\nmark_phase_failed = module.mark_phase_failed|g' "$SCRIPT"
    
    sed -i 's|from epic_manifest import get_next_phases|import importlib.util\nspec = importlib.util.spec_from_file_location("epic_manifest", "scripts/epic_manifest.py")\nmodule = importlib.util.module_from_spec(spec)\nspec.loader.exec_module(module)\nget_next_phases = module.get_next_phases|g' "$SCRIPT"
    
    ((FIXED_COUNT++))
done

echo ""
echo "=========================================="
echo "Fix Application Complete"
echo "=========================================="
echo "Fixed: $FIXED_COUNT scripts"
echo "Skipped: $SKIPPED_COUNT scripts"
echo ""

# Test one script
echo "Testing EPIC-CCN-004..."
TEST_SCRIPT="scripts/wave6/_p1_epic_ccn_004_vm.sh"
if [ -f "$TEST_SCRIPT" ]; then
    bash "$TEST_SCRIPT" 2>&1 | head -20
    echo ""
    echo "✓ Test complete (check output above)"
else
    echo "⚠ Test script not found"
fi

echo ""
echo "Ready to relaunch 24 failed epics"

# Made with Bob
