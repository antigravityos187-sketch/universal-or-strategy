#!/usr/bin/env python3
"""Regenerate 4 Phase 6 scripts with correct prerequisite check."""

import json
from pathlib import Path

# Working API key from EPIC-CCN-001 (verified successful)
working_key = "bob_prod_bob-admin_yN7cbWSG9B926LkYPex4pXBGgTbZdN7Xg1ihASxzGdFGz7N8Z5WWDiqeWGUvsXiTWMzag9Hur9EA53BtXQRr2E4_4Z2YTW686zBchNH8KMgN69E3YGDzeRYcWMYxtKkxooeR"

# Epics to regenerate
epics = [
    ("003", "EPIC-CCN-003"),
    ("015", "EPIC-CCN-015"),
    ("030", "EPIC-CCN-030"),
    ("045", "EPIC-CCN-045"),
]

for epic_num, epic_id in epics:
    script = f"""#!/bin/bash
# Phase 6 (Verification) for {epic_id} - REGENERATED with correct prerequisite
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="{epic_id}"
API_KEY="{working_key}"

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Accept BOTH ticket-*-completion.md AND ticket-completion.md
if ! find docs/brain/{epic_id} -maxdepth 1 \\( -name "05-*.md" -o -name "ticket-*-completion.md" -o -name "ticket-completion.md" \\) -print -quit | grep -q .; then
    echo "ERROR: Missing Phase 5 completion files for {epic_id}"
    echo "Expected: docs/brain/{epic_id}/05-*.md OR ticket-*-completion.md OR ticket-completion.md"
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
    output_path.write_text(script)
    print(f"Regenerated: {output_path}")

print(f"\nTotal scripts regenerated: {len(epics)}")
print("\nNext: Upload to VM with verification (V12.27 Protocol)")

# Made with Bob
