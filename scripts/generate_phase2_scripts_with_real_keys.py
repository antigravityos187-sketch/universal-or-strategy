#!/usr/bin/env python3
"""
Generate Phase 2 scripts with REAL API key rotation
Building-Blocks Method: Copy from Phase 1.5 success pattern
"""

import sys
sys.path.insert(0, 'scripts')
from load_api_keys import load_api_keys_from_folder

# Load REAL API keys from docs/API/
API_KEYS = load_api_keys_from_folder()

if len(API_KEYS) < 1:
    print(f"ERROR: No API keys found")
    sys.exit(1)

print(f"✅ Loaded {len(API_KEYS)} unique API keys")
print(f"✅ Keys are unique: {len(API_KEYS) == len(set(API_KEYS))}")

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

def generate_phase2_script(epic_id, api_key):
    """Generate Phase 2 script for one epic"""
    epic_num = epic_id.split('-')[-1]
    
    script_content = TEMPLATE.format(
        EPIC_ID=epic_id,
        EPIC_NUM=epic_num,
        API_KEY=api_key
    )
    
    script_path = f"_p2_{epic_num}.sh"
    with open(script_path, 'w') as f:
        f.write(script_content)
    
    # Make executable
    import os
    os.chmod(script_path, 0o755)
    
    return script_path

def main():
    # Load epic roadmap
    import json
    with open('epic_roadmap_wave7.json', 'r') as f:
        roadmap = json.load(f)
    
    # Get epics that completed Phase 1.5
    epics = [e['epic_id'] for e in roadmap['epics'] 
             if e.get('phase_1_5_complete', False)]
    
    print(f"Generating Phase 2 scripts for {len(epics)} epics...")
    print(f"API key rotation: {len(API_KEYS)} keys")
    print("=" * 60)
    
    generated = []
    for i, epic_id in enumerate(epics):
        # Rotate through REAL API keys
        api_key = API_KEYS[i % len(API_KEYS)]
        script_path = generate_phase2_script(epic_id, api_key)
        generated.append((epic_id, script_path))
        
        if (i + 1) % 20 == 0:
            print(f"  Generated {i + 1}/{len(epics)} scripts...")
    
    print(f"\n✅ Generated {len(generated)} Phase 2 scripts")
    print(f"✅ API key rotation: {len(API_KEYS)} UNIQUE keys cycling")
    print("=" * 60)
    
    return 0

if __name__ == "__main__":
    sys.exit(main())