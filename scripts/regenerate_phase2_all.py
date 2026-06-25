#!/usr/bin/env python3
"""
Regenerate ALL Phase 2 scripts with real API key rotation
Simpler approach: Generate for all 161 epics, let script check Phase 1.5 dependency
"""

import sys
import os
sys.path.insert(0, 'scripts')
from load_api_keys import load_api_keys_from_folder

# Load REAL API keys
API_KEYS = load_api_keys_from_folder()
print(f"✅ Loaded {len(API_KEYS)} unique API keys")

TEMPLATE = """#!/bin/bash
# Building-Blocks Method: Copied from _p1_5_002.sh (successful pattern)
# Changes: phase1 -> phase2, scope -> architecture
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{API_KEY}'
mkdir -p docs/brain/{EPIC_ID}
mkdir -p logs/wave7/phase2

if [ ! -f "docs/brain/{EPIC_ID}/01-scope-boundary.md" ]; then
    echo "BLOCKED: Phase 1.5 not complete"
    exit 1
fi

cat > /tmp/phase2_msg_{EPIC_NUM}.txt << 'EOFMSG'
Execute Phase 2 (Architecture Planning) for {EPIC_ID}.

CRITICAL FILE I/O PROTOCOL:
1. NEVER use write_to_file, read_file, or run_shell_command tools
2. ALWAYS use execute_command tool with cat > file
3. ALWAYS set cwd parameter to /home/malhitticrypto/universal-or-strategy

Input: docs/brain/{EPIC_ID}/01-scope-boundary.md

Required Actions:
1. Read scope boundary validation
2. Query Jane Street KB for extraction patterns: python scripts/query_kb.py "complexity reduction"
3. Design extraction architecture (method splitting, parameter reduction, FSM patterns)
4. Write docs/brain/{EPIC_ID}/02-architecture-plan.md using execute_command
5. Update manifest.json using execute_command
6. Verify both files exist using execute_command

Success Criteria:
- 02-architecture-plan.md exists and contains architecture design
- manifest.json updated to show phase 2 completed
EOFMSG

bob --yolo --chat-mode plan "$(cat /tmp/phase2_msg_{EPIC_NUM}.txt)" 2>&1 | tee logs/wave7/phase2/{EPIC_ID}.log
echo "DONE_EXIT=$?"

# Made with Bob
"""

# Generate for all 161 epics
for i in range(1, 162):
    epic_num = f"{i:03d}"
    epic_id = f"EPIC-W7-{epic_num}"
    
    # Rotate through API keys
    api_key = API_KEYS[i % len(API_KEYS)]
    
    script_content = TEMPLATE.format(
        EPIC_ID=epic_id,
        EPIC_NUM=epic_num,
        API_KEY=api_key
    )
    
    script_path = f"_p2_{epic_num}.sh"
    with open(script_path, 'w') as f:
        f.write(script_content)
    
    os.chmod(script_path, 0o755)
    
    if (i % 20) == 0:
        print(f"  Generated {i}/161 scripts...")

print(f"\n✅ Generated 161 Phase 2 scripts")
print(f"✅ API key rotation: {len(API_KEYS)} unique keys")
print(f"✅ Each key handles ~{161 // len(API_KEYS)} epics")
print("=" * 60)