#!/usr/bin/env python3
"""Generate Phase 6 scripts using building-blocks method.

Copies from Phase 5 scripts, modifies only phase-specific parameters.
"""

import json
from pathlib import Path

def main():
    # Load API keys
    api_keys = []
    api_dir = Path('docs/API')
    for api_file in sorted(api_dir.glob('*.json')):
        try:
            with open(api_file) as f:
                data = json.load(f)
                if 'apikey' in data:
                    api_keys.append(data['apikey'])
        except Exception as e:
            print(f"Warning: Could not load {api_file}: {e}")
    
    if not api_keys:
        print("ERROR: No API keys found in docs/API/")
        return
    
    print(f"Loaded {len(api_keys)} API keys")
    
    # Create output directory
    output_dir = Path('scripts/wave4')
    output_dir.mkdir(parents=True, exist_ok=True)
    
    # Generate scripts for 79 successful epics (skip EPIC-CCN-016)
    generated = 0
    for i in range(1, 81):
        if i == 16:  # Skip EPIC-CCN-016 (deferred from Phase 5)
            print(f"Skipping EPIC-CCN-016 (deferred)")
            continue
        
        epic_num = f"{i:03d}"
        epic_id = f"EPIC-CCN-{epic_num}"
        api_key = api_keys[i % len(api_keys)]  # Round-robin
        
        script = f"""#!/bin/bash
# Phase 6 (Verification) for {epic_id}
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="{epic_id}"
API_KEY="{api_key}"

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Verify Phase 5 completion file exists
if [ ! -f "docs/brain/{epic_id}/05-completion.md" ]; then
    echo "ERROR: Missing Phase 5 completion file for {epic_id}"
    exit 1
fi

# Create message file
cat > /tmp/phase6_msg_{epic_num}.txt << 'EOFMSG'
Use the phase-6-review MCP server to execute Phase 6 for {epic_id}.

Call the execute_phase_6 tool with epic_id="{epic_id}".

The tool will verify:
1. All tickets executed successfully
2. Complexity targets met
3. Build passes
4. No behavioral changes
5. All acceptance criteria satisfied

**Verification**: Confirm verification report exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell
bob --yolo "$(cat /tmp/phase6_msg_{epic_num}.txt)"

# Verify verification report created
if [ -f "docs/brain/{epic_id}/06-verification-report.md" ]; then
    echo "SUCCESS: Phase 6 complete for {epic_id}"
    echo "File: docs/brain/{epic_id}/06-verification-report.md"
    ls -lh docs/brain/{epic_id}/06-verification-report.md
else
    echo "ERROR: No verification report created for {epic_id}"
    exit 1
fi
"""
        
        output_path = output_dir / f'_p6_{epic_num}.sh'
        output_path.write_text(script)
        generated += 1
        if generated <= 3 or generated % 10 == 0:
            print(f"Generated: {output_path}")
    
    print(f"\n[OK] Total scripts generated: {generated} (skipped EPIC-CCN-016)")
    print(f"Output directory: {output_dir}")

if __name__ == '__main__':
    main()

# Made with Bob
