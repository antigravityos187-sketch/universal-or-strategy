#!/usr/bin/env python3
"""Regenerate Phase 6 scripts for EPIC-CCN-060 and 075 with correct prerequisite check."""

import json
from pathlib import Path

# Load API keys
api_keys = []
for api_file in Path('docs/API').glob('*.json'):
    with open(api_file) as f:
        data = json.load(f)
        api_keys.append(data['apikey'])

# Generate scripts for EPIC-CCN-060 and 075
for epic_num in ['060', '075']:
    epic_id = f"EPIC-CCN-{epic_num}"
    api_key = api_keys[int(epic_num) % len(api_keys)]  # Round-robin
    
    script = f"""#!/bin/bash
# Phase 6 (Verification) for {epic_id}
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="{epic_id}"
API_KEY="{api_key}"

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Accept multiple Phase 5 filename patterns
if ! find docs/brain/{epic_id} -maxdepth 1 \\( -name "05-*.md" -o -name "ticket-*-completion.md" -o -name "ticket-completion.md" \\) -print -quit | grep -q .; then
    echo "ERROR: Missing Phase 5 completion files for {epic_id}"
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
if [ -f "docs/brain/{epic_id}/06-completion-report.md" ]; then
    echo "SUCCESS: Phase 6 complete for {epic_id}"
    echo "File: docs/brain/{epic_id}/06-completion-report.md"
    ls -lh docs/brain/{epic_id}/06-completion-report.md
else
    echo "ERROR: No verification report created for {epic_id}"
    exit 1
fi
"""
    
    output_path = Path(f'scripts/wave4/_p6_{epic_num}.sh')
    # Write in binary mode with explicit LF line endings
    output_path.write_bytes(script.encode('utf-8'))
    print(f"Generated: {output_path}")

print(f"\nTotal scripts generated: 2 (EPIC-CCN-060, 075)")
print("Line endings: Unix LF (binary mode write)")

# Made with Bob
