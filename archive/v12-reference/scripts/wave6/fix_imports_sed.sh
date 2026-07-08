#!/bin/bash
# Fix Phase 1 import errors using sed (simple string replacement)
# Replaces broken sys.path pattern with working importlib pattern

set -euo pipefail

echo "=========================================="
echo "Fixing Phase 1 Import Errors (sed method)"
echo "Started: $(date)"
echo "=========================================="

# Pattern to find (escaped for sed)
OLD_PATTERN='python3 -c "import sys; sys.path.insert(0, '\''scripts'\''); from epic_manifest import verify_dependencies; result = verify_dependencies('

# Pattern to replace with (escaped for sed)
NEW_PATTERN='python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('\''epic_manifest'\'', '\''scripts\/epic_manifest.py'\''); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); result = module.verify_dependencies('

# Fix all Phase 1 scripts
FIXED=0
for SCRIPT in scripts/wave6/_p1_epic_ccn_*.sh; do
    if [ -f "$SCRIPT" ]; then
        echo "Fixing: $SCRIPT"
        
        # Backup if not already backed up
        if [ ! -f "$SCRIPT.backup" ]; then
            cp "$SCRIPT" "$SCRIPT.backup"
        fi
        
        # Replace all occurrences of the broken pattern
        sed -i "s|python3 -c \"import sys; sys.path.insert(0, 'scripts'); from epic_manifest import verify_dependencies|python3 -c \"import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); result = module.verify_dependencies|g" "$SCRIPT"
        
        sed -i "s|python3 -c \"import sys; sys.path.insert(0, 'scripts'); from epic_manifest import verify_can_execute|python3 -c \"import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); can_exec, reason = module.verify_can_execute|g" "$SCRIPT"
        
        sed -i "s|python3 -c \"import sys; sys.path.insert(0, 'scripts'); from epic_manifest import verify_filesystem_state|python3 -c \"import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); result = module.verify_filesystem_state|g" "$SCRIPT"
        
        sed -i "s|python3 -c \"import sys; sys.path.insert(0, 'scripts'); from epic_manifest import start_phase_execution|python3 -c \"import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); started, reason = module.start_phase_execution|g" "$SCRIPT"
        
        sed -i "s|python3 -c \"import sys; sys.path.insert(0, 'scripts'); from epic_manifest import fail_phase_execution|python3 -c \"import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); module.fail_phase_execution|g" "$SCRIPT"
        
        sed -i "s|python3 -c \"import sys; sys.path.insert(0, 'scripts'); from epic_manifest import complete_phase_execution|python3 -c \"import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); completed, reason = module.complete_phase_execution|g" "$SCRIPT"
        
        ((FIXED++))
    fi
done

echo ""
echo "=========================================="
echo "Fix Complete"
echo "Finished: $(date)"
echo "Fixed: $FIXED scripts"
echo "=========================================="

# Test one script
echo ""
echo "Testing EPIC-CCN-004..."
bash scripts/wave6/_p1_epic_ccn_004.sh 2>&1 | head -20

# Made with Bob
