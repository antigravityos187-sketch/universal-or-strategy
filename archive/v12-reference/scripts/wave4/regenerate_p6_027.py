#!/usr/bin/env python3
"""Regenerate Phase 6 script for EPIC-CCN-027 with correct prerequisite check."""

import json
from pathlib import Path

# Load API key from EPIC-CCN-001 (known working)
api_file = Path('docs/API/bob.json')
with open(api_file) as f:
    data = json.load(f)
    api_key = data['apikey']

epic_num = "027"
epic_id = f"EPIC-CCN-{epic_num}"

script = f"""#!/bin/bash
# Phase 6 (Verification) for {epic_id}
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="{epic_id}"
API_KEY="{api_key}"

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Accept BOTH ticket-*-completion.md AND ticket-completion.md AND 05-*.md
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
# Write in binary mode to ensure Unix LF line endings
output_path.write_bytes(script.encode('utf-8'))
print(f"Generated: {output_path}")
print(f"Line endings: Unix LF (binary mode write)")

# Made with Bob
