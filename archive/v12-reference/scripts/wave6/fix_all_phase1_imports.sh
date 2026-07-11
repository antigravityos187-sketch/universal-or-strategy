#!/bin/bash
# Fix all Phase 1 scripts to use importlib (Building-Blocks Method)
# Downloads fixed template, applies to all scripts on VM

set -e

echo "=========================================="
echo "Fixing Phase 1 Import Statements"
echo "Building-Blocks Method: VM → Local → VM"
echo "=========================================="

# Step 1: Upload fixed template to VM
echo ""
echo "Step 1: Uploading fixed template..."
gcloud compute scp scripts/wave6/_p1_epic_ccn_001_vm.sh \
    v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/scripts/wave6/_p1_FIXED_TEMPLATE.sh \
    --zone=us-central1-a

# Step 2: Apply fix to all 77 scripts on VM
echo ""
echo "Step 2: Applying fix to all 77 Phase 1 scripts on VM..."
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="
cd /home/malhitticrypto/universal-or-strategy

# Read the fixed import patterns from template
VERIFY_DEPS=\$(grep -m1 'verify_dependencies' scripts/wave6/_p1_FIXED_TEMPLATE.sh)
VERIFY_CAN=\$(grep -m1 'verify_can_execute' scripts/wave6/_p1_FIXED_TEMPLATE.sh)
VERIFY_FS=\$(grep -m1 'verify_filesystem_state' scripts/wave6/_p1_FIXED_TEMPLATE.sh)
START_PHASE=\$(grep -m1 'start_phase_execution' scripts/wave6/_p1_FIXED_TEMPLATE.sh | head -1)
COMPLETE_PHASE=\$(grep -m1 'complete_phase_execution' scripts/wave6/_p1_FIXED_TEMPLATE.sh)

echo 'Fixing 77 scripts...'
FIXED_COUNT=0

for script in scripts/wave6/_p1_epic_ccn_*.sh; do
    if [ -f \"\$script\" ] && [ \"\$script\" != \"scripts/wave6/_p1_FIXED_TEMPLATE.sh\" ]; then
        # Replace all 5 import patterns
        sed -i 's|python3 -c \"import sys; sys.path.insert(0, '\''scripts'\''); from epic_manifest import verify_dependencies.*\"|python3 -c \"import importlib.util; spec = importlib.util.spec_from_file_location('\''epic_manifest'\'', '\''scripts/epic_manifest.py'\''); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); result = module.verify_dependencies('\''\\$EPIC_ID'\'', '\''\\$PHASE'\''); import sys; sys.exit(0 if result else 1)\"|g' \"\$script\"
        
        sed -i 's|python3 -c \"import sys; sys.path.insert(0, '\''scripts'\''); from epic_manifest import verify_can_execute.*\"|python3 -c \"import importlib.util; spec = importlib.util.spec_from_file_location('\''epic_manifest'\'', '\''scripts/epic_manifest.py'\''); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); can_exec, reason = module.verify_can_execute('\''\\$EPIC_ID'\'', '\''\\$PHASE'\'', '\''\\$AGENT_ID'\''); print(reason if not can_exec else '\''OK'\''); import sys; sys.exit(0 if can_exec else 1)\"|g' \"\$script\"
        
        sed -i 's|python3 -c \"import sys; sys.path.insert(0, '\''scripts'\''); from epic_manifest import verify_filesystem_state.*\"|python3 -c \"import importlib.util; spec = importlib.util.spec_from_file_location('\''epic_manifest'\'', '\''scripts/epic_manifest.py'\''); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); result = module.verify_filesystem_state('\''\\$EPIC_ID'\'', '\''\\$PHASE'\''); import sys; sys.exit(0 if result else 1)\"|g' \"\$script\"
        
        sed -i 's|python3 -c \"import sys; sys.path.insert(0, '\''scripts'\''); from epic_manifest import start_phase_execution.*\"|python3 -c \"import importlib.util; spec = importlib.util.spec_from_file_location('\''epic_manifest'\'', '\''scripts/epic_manifest.py'\''); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); started, reason = module.start_phase_execution('\''\\$EPIC_ID'\'', '\''\\$PHASE'\'', '\''\\$AGENT_ID'\''); print(reason if not started else '\''OK'\''); import sys; sys.exit(0 if started else 1)\"|g' \"\$script\"
        
        sed -i 's|python3 -c \"import sys; sys.path.insert(0, '\''scripts'\''); from epic_manifest import complete_phase_execution.*\"|python3 -c \"import importlib.util; spec = importlib.util.spec_from_file_location('\''epic_manifest'\'', '\''scripts/epic_manifest.py'\''); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); completed, reason = module.complete_phase_execution('\''\\$EPIC_ID'\'', '\''\\$PHASE'\'', '\''\\$AGENT_ID'\'', ['\''\\$OUTPUT_FILE'\'']); print(reason if not completed else '\''OK'\''); import sys; sys.exit(0 if completed else 1)\"|g' \"\$script\"
        
        sed -i 's|python3 -c \"import sys; sys.path.insert(0, '\''scripts'\''); from epic_manifest import fail_phase_execution.*\"|python3 -c \"import importlib.util; spec = importlib.util.spec_from_file_location('\''epic_manifest'\'', '\''scripts/epic_manifest.py'\''); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); module.fail_phase_execution('\''\\$EPIC_ID'\'', '\''\\$PHASE'\'', '\''\\$AGENT_ID'\'', '\''\\$ERROR_MSG'\'')\"|g' \"\$script\"
        
        FIXED_COUNT=\$((FIXED_COUNT + 1))
    fi
done

echo \"✅ Fixed \$FIXED_COUNT scripts\"

# Step 3: Test one script
echo ''
echo 'Step 3: Testing EPIC-CCN-001...'
timeout 30 bash scripts/wave6/_p1_epic_ccn_001.sh 2>&1 | head -25
"

echo ""
echo "=========================================="
echo "✅ Import fix complete"
echo "=========================================="

# Made with Bob
