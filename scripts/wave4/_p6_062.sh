#!/bin/bash
# Phase 6 (Verification) for EPIC-CCN-062
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-062"
API_KEY="bob_prod_bob-admin_V8sa2xf9tLezoczf9f7WZADcMhiUphzZPhDfRiMwx82Wxo1VtH3KMprtBvQFAmRYgECy254WHMSeWFxAuzBGzLj_2SQz2BrZKRs3WsotGTN56eL2Gthg4voAhcMZeefDi7wp"

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Verify Phase 5 completion file exists
if [ ! -f "docs/brain/EPIC-CCN-062/05-completion.md" ]; then
    echo "ERROR: Missing Phase 5 completion file for EPIC-CCN-062"
    exit 1
fi

# Create message file
cat > /tmp/phase6_msg_062.txt << 'EOFMSG'
Use the phase-6-review MCP server to execute Phase 6 for EPIC-CCN-062.

Call the execute_phase_6 tool with epic_id="EPIC-CCN-062".

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
bob --yolo "$(cat /tmp/phase6_msg_062.txt)"

# Verify completion report created (FIXED: correct filename)
if [ -f "docs/brain/EPIC-CCN-062/06-completion-report.md" ]; then
    echo "SUCCESS: Phase 6 complete for EPIC-CCN-062"
    echo "File: docs/brain/EPIC-CCN-062/06-completion-report.md"
    ls -lh docs/brain/EPIC-CCN-062/06-completion-report.md
else
    echo "ERROR: No completion report created for EPIC-CCN-062"
    exit 1
fi
