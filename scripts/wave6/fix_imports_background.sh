#!/bin/bash
# Fix Phase 1 import errors in background
# Run via: nohup bash fix_imports_background.sh > logs/import_fix.log 2>&1 &

cd /home/malhitticrypto/universal-or-strategy

echo "=========================================="
echo "Fixing Phase 1 Import Statements"
echo "Started: $(date)"
echo "=========================================="

# Count scripts
TOTAL=$(ls -1 scripts/wave6/_p1_epic_ccn_*.sh 2>/dev/null | wc -l)
echo "Total scripts found: $TOTAL"

# Fix each script
FIXED=0
for script in scripts/wave6/_p1_epic_ccn_*.sh; do
    if [ ! -f "$script" ]; then
        continue
    fi
    
    # Skip if already fixed
    if grep -q "importlib.util.spec_from_file_location" "$script"; then
        echo "✓ Already fixed: $script"
        ((FIXED++))
        continue
    fi
    
    echo "Fixing: $script"
    
    # Create backup
    cp "$script" "${script}.backup"
    
    # Apply fix using sed (simpler than Python)
    # Replace: from epic_manifest import X
    # With: import sys; sys.path.insert(0, 'scripts'); from epic_manifest import X
    
    sed -i 's|from epic_manifest import|import sys; sys.path.insert(0, "scripts"); from epic_manifest import|g' "$script"
    
    ((FIXED++))
done

echo ""
echo "=========================================="
echo "Fix Complete"
echo "Finished: $(date)"
echo "Fixed: $FIXED/$TOTAL scripts"
echo "=========================================="

# Test one script
echo ""
echo "Testing EPIC-CCN-005..."
if [ -f "scripts/wave6/_p1_epic_ccn_005_vm.sh" ]; then
    timeout 30 bash scripts/wave6/_p1_epic_ccn_005_vm.sh 2>&1 | head -30
fi

# Made with Bob
