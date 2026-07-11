#!/bin/bash
# Fix Phase 1 scripts - Replace direct script calls with inline Python
# Root cause: epic_manifest.py uses relative imports that fail when run as script

set -euo pipefail

echo "Fixing Phase 1 scripts for PYTHONPATH issue..."

cd /home/malhitticrypto/universal-or-strategy

# Fix all Phase 1 scripts
for script in scripts/wave6/_p1_epic_ccn_*.sh; do
    if [ -f "$script" ]; then
        echo "Fixing: $script"
        
        # Create backup
        cp "$script" "${script}.bak"
        
        # Replace all python3 scripts/epic_manifest.py calls with inline Python
        # Pattern 1: verify_dependencies
        sed -i 's|python3 scripts/epic_manifest.py verify_dependencies "\$EPIC_ID" "\$PHASE"|python3 -c "import sys; sys.path.insert(0, '\''scripts'\''); from epic_manifest import verify_dependencies; result = verify_dependencies('\''$EPIC_ID'\'', '\''$PHASE'\''); sys.exit(0 if result else 1)"|g' "$script"
        
        # Pattern 2: verify_can_execute  
        sed -i 's|python3 scripts/epic_manifest.py verify_can_execute "\$EPIC_ID" "\$PHASE"|python3 -c "import sys; sys.path.insert(0, '\''scripts'\''); from epic_manifest import verify_can_execute; can_exec, reason = verify_can_execute('\''$EPIC_ID'\'', '\''$PHASE'\'', '\''$AGENT_ID'\''); print(reason if not can_exec else '\''OK'\''); sys.exit(0 if can_exec else 1)"|g' "$script"
        
        # Pattern 3: verify_filesystem_state
        sed -i 's|python3 scripts/epic_manifest.py verify_filesystem_state "\$EPIC_ID" "\$PHASE"|python3 -c "import sys; sys.path.insert(0, '\''scripts'\''); from epic_manifest import verify_filesystem_state; result = verify_filesystem_state('\''$EPIC_ID'\'', '\''$PHASE'\''); sys.exit(0 if result else 1)"|g' "$script"
        
        # Pattern 4: start_phase_execution
        sed -i 's|python3 scripts/epic_manifest.py start_phase_execution "\$EPIC_ID" "\$PHASE" "\$AGENT_ID"|python3 -c "import sys; sys.path.insert(0, '\''scripts'\''); from epic_manifest import start_phase_execution; started, reason = start_phase_execution('\''$EPIC_ID'\'', '\''$PHASE'\'', '\''$AGENT_ID'\''); print(reason if not started else '\''OK'\''); sys.exit(0 if started else 1)"|g' "$script"
        
        # Pattern 5: fail_phase_execution
        sed -i 's|python3 scripts/epic_manifest.py fail_phase_execution "\$EPIC_ID" "\$PHASE" "\$AGENT_ID" "\$ERROR_MSG"|python3 -c "import sys; sys.path.insert(0, '\''scripts'\''); from epic_manifest import fail_phase_execution; fail_phase_execution('\''$EPIC_ID'\'', '\''$PHASE'\'', '\''$AGENT_ID'\'', '\''$ERROR_MSG'\'')"|g' "$script"
        
        # Pattern 6: complete_phase_execution
        sed -i 's|python3 scripts/epic_manifest.py complete_phase_execution "\$EPIC_ID" "\$PHASE" "\$AGENT_ID" "\$OUTPUT_FILE"|python3 -c "import sys; sys.path.insert(0, '\''scripts'\''); from epic_manifest import complete_phase_execution; completed, reason = complete_phase_execution('\''$EPIC_ID'\'', '\''$PHASE'\'', '\''$AGENT_ID'\'', ['\''$OUTPUT_FILE'\'']); print(reason if not completed else '\''OK'\''); sys.exit(0 if completed else 1)"|g' "$script"
        
        echo "  Fixed: $script"
    fi
done

echo ""
echo "All Phase 1 scripts fixed!"
echo "Backups saved as *.bak"

# Made with Bob
