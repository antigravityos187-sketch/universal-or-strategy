#!/usr/bin/env python3
"""Generate Phase 6 scripts for remaining 10 epics using building-blocks method."""

import json
from pathlib import Path

# Load API keys
api_keys = []
for api_file in sorted(Path('docs/API').glob('*.json')):
    with open(api_file) as f:
        data = json.load(f)
        api_keys.append(data['apikey'])

print(f"Loaded {len(api_keys)} API keys")

# Target epics (10 remaining)
target_epics = [3, 15, 30, 31, 33, 42, 45, 55, 60, 75]

# Create output directory
output_dir = Path('scripts/wave4_remaining')
output_dir.mkdir(parents=True, exist_ok=True)

for i, epic_num in enumerate(target_epics):
    epic_id = f"EPIC-CCN-{epic_num:03d}"
    api_key = api_keys[i % len(api_keys)]  # Round-robin
    
    script = f"""#!/bin/bash
# Phase 6 (Verification) for {epic_id}
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="{epic_id}"
API_KEY="{api_key}"

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Verify Phase 5 completion file exists (robust OR logic)
if ! find docs/brain/{epic_id} -maxdepth 1 \( -name "05-*.md" -o -name "ticket-*-completion.md" \) -print -quit | grep -q .; then
    echo "ERROR: Missing Phase 5 completion files for {epic_id}"
    echo "Expected: docs/brain/{epic_id}/05-*.md OR ticket-*-completion.md"
    exit 1
fi

# Create message file
cat > /tmp/phase6_msg_{epic_num:03d}.txt << 'EOFMSG'
Use the phase-6-review MCP server to execute Phase 6 for {epic_id}.

Call the execute_phase_6 tool with epic_id="{epic_id}".

The tool will verify:
1. All tickets executed successfully
2. Complexity targets met
3. Build passes
4. No behavioral changes
5. All acceptance criteria satisfied

**Verification**: Confirm completion report exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell
bob --yolo "$(cat /tmp/phase6_msg_{epic_num:03d}.txt)"

# Verify completion report created (FIXED: correct filename)
if [ -f "docs/brain/{epic_id}/06-completion-report.md" ]; then
    echo "SUCCESS: Phase 6 complete for {epic_id}"
    echo "File: docs/brain/{epic_id}/06-completion-report.md"
    ls -lh docs/brain/{epic_id}/06-completion-report.md
else
    echo "ERROR: No completion report created for {epic_id}"
    exit 1
fi
"""
    
    output_path = output_dir / f'_p6r_{epic_num:03d}.sh'
    output_path.write_text(script)
    print(f"Generated: {output_path}")

print(f"\nTotal scripts generated: {len(target_epics)}")

# Made with Bob
