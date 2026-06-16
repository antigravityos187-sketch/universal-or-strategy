#!/bin/bash
# Phase 6 (Verification) for EPIC-CCN-045 - REGENERATED with correct prerequisite
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-045"
API_KEY="bob_prod_bob-admin_yN7cbWSG9B926LkYPex4pXBGgTbZdN7Xg1ihASxzGdFGz7N8Z5WWDiqeWGUvsXiTWMzag9Hur9EA53BtXQRr2E4_4Z2YTW686zBchNH8KMgN69E3YGDzeRYcWMYxtKkxooeR"

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Accept BOTH ticket-*-completion.md AND ticket-completion.md
if ! find docs/brain/EPIC-CCN-045 -maxdepth 1 \( -name "05-*.md" -o -name "ticket-*-completion.md" -o -name "ticket-completion.md" \) -print -quit | grep -q .; then
    echo "ERROR: Missing Phase 5 completion files for EPIC-CCN-045"
    echo "Expected: docs/brain/EPIC-CCN-045/05-*.md OR ticket-*-completion.md OR ticket-completion.md"
    exit 1
fi

# Create message file
cat > /tmp/phase6_msg_045.txt << 'EOFMSG'
Use the phase-6-review MCP server to execute Phase 6 for EPIC-CCN-045.

Call the execute_phase_6 tool with epic_id="EPIC-CCN-045".

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
bob --yolo "$(cat /tmp/phase6_msg_045.txt)"

# Verify verification report created
if [ -f "docs/brain/EPIC-CCN-045/06-completion-report.md" ]; then
    echo "SUCCESS: Phase 6 complete for EPIC-CCN-045"
    echo "File: docs/brain/EPIC-CCN-045/06-completion-report.md"
    ls -lh docs/brain/EPIC-CCN-045/06-completion-report.md
else
    echo "ERROR: No verification report created for EPIC-CCN-045"
    exit 1
fi
