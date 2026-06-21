#!/bin/bash
# Proper import fix using importlib pattern from working pilot script
# Building-Blocks Method: Copy working pattern from _p1_epic_ccn_001_vm.sh

cd /home/malhitticrypto/universal-or-strategy

echo "=========================================="
echo "Fixing Phase 1 Import Statements (Proper Fix)"
echo "Using importlib pattern from pilot script"
echo "Started: $(date)"
echo "=========================================="

# Restore from backups first
echo "Restoring from backups..."
for backup in scripts/wave6/_p1_epic_ccn_*.sh.backup; do
    if [ -f "$backup" ]; then
        original="${backup%.backup}"
        cp "$backup" "$original"
        echo "Restored: $original"
    fi
done

# Now apply proper fix
FIXED=0
for script in scripts/wave6/_p1_epic_ccn_*.sh; do
    if [ ! -f "$script" ]; then
        continue
    fi
    
    echo "Fixing: $script"
    
    # Create new backup
    cp "$script" "${script}.backup2"
    
    # Replace all python3 -c "from epic_manifest import X" patterns
    # with python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); ..."
    
    # This is complex, so use Python to do the replacement
    python3 << 'PYTHON_SCRIPT'
import sys
import re

script_file = sys.argv[1]

with open(script_file, 'r') as f:
    content = f.read()

# Pattern 1: verify_dependencies
content = re.sub(
    r'python3 -c "from epic_manifest import verify_dependencies.*?sys\.exit\(0 if result else 1\)"',
    r'''python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); result = module.verify_dependencies('\$EPIC_ID', '\$PHASE'); import sys; sys.exit(0 if result else 1)"''',
    content,
    flags=re.DOTALL
)

# Pattern 2: verify_can_execute
content = re.sub(
    r'python3 -c "from epic_manifest import verify_can_execute.*?sys\.exit\(0 if can_execute else 1\)"',
    r'''python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); can_exec, reason = module.verify_can_execute('\$EPIC_ID', '\$PHASE', '\$AGENT_ID'); print(reason if not can_exec else 'OK'); import sys; sys.exit(0 if can_exec else 1)"''',
    content,
    flags=re.DOTALL
)

# Pattern 3: verify_filesystem_state
content = re.sub(
    r'python3 -c "from epic_manifest import verify_filesystem_state.*?sys\.exit\(0 if result else 1\)"',
    r'''python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location('epic_manifest', 'scripts/epic_manifest.py'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); result = module.verify_filesystem_state('\$EPIC_ID', '\$PHASE'); import sys; sys.exit(0 if result else 1)"''',
    content,
    flags=re.DOTALL
)

# Write back
with open(script_file, 'w') as f:
    f.write(content)

print(f"Fixed: {script_file}")
PYTHON_SCRIPT
    
    python3 - "$script"
    
    ((FIXED++))
done

echo ""
echo "=========================================="
echo "Fix Complete"
echo "Finished: $(date)"
echo "Fixed: $FIXED scripts"
echo "=========================================="

# Test one script
echo ""
echo "Testing EPIC-CCN-005..."
if [ -f "scripts/wave6/_p1_epic_ccn_005.sh" ]; then
    timeout 30 bash scripts/wave6/_p1_epic_ccn_005.sh 2>&1 | head -30
fi

# Made with Bob
