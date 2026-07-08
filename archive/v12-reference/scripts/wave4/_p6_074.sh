#!/bin/bash
# Phase 6 (Verification) for EPIC-CCN-074
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-074"
API_KEY="bob_prod_bob-admin_2RC6JDaVuiwh9Ag5xuFucgJo81gJW3KZQp3yumcVfpCkY9hCvZhhvaGzx6KiuWtXqNJamkoDzdNLxUEAN3MjbCXp_9zESTyeEwLZJ1y7apWYhu24fmp1gc84qcCEsGn4iJo6S"

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Verify Phase 5 completion file exists
if [ ! -f "docs/brain/EPIC-CCN-074/05-completion.md" ]; then
    echo "ERROR: Missing Phase 5 completion file for EPIC-CCN-074"
    exit 1
fi

# Create message file
cat > /tmp/phase6_msg_074.txt << 'EOFMSG'
Use the phase-6-review MCP server to execute Phase 6 for EPIC-CCN-074.

Call the execute_phase_6 tool with epic_id="EPIC-CCN-074".

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
bob --yolo "$(cat /tmp/phase6_msg_074.txt)"

# Verify completion report created (FIXED: correct filename)
if [ -f "docs/brain/EPIC-CCN-074/06-completion-report.md" ]; then
    echo "SUCCESS: Phase 6 complete for EPIC-CCN-074"
    echo "File: docs/brain/EPIC-CCN-074/06-completion-report.md"
    ls -lh docs/brain/EPIC-CCN-074/06-completion-report.md
else
    echo "ERROR: No completion report created for EPIC-CCN-074"
    exit 1
fi
