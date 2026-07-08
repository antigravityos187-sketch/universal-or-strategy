#!/bin/bash
# Phase 6 (Verification) for EPIC-CCN-075
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-075"
API_KEY="bob_prod_bob-admin_3abxQUhB6oz3484pgXxkjkeZEXxTEJfFGwg4D5cY6GWrCXFjT6uUQhvtLz5n8dB5g9Pue31DVuLwR9wa34zrBNmT_DdGCwiky7h1JVUEzJZVTrDxZNUigAnSRPPdUEJNzeLZT"

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Accept multiple Phase 5 filename patterns
if ! find docs/brain/EPIC-CCN-075 -maxdepth 1 \( -name "05-*.md" -o -name "ticket-*-completion.md" -o -name "ticket-completion.md" \) -print -quit | grep -q .; then
    echo "ERROR: Missing Phase 5 completion files for EPIC-CCN-075"
    exit 1
fi

# Create message file
cat > /tmp/phase6_msg_075.txt << 'EOFMSG'
Use the phase-6-review MCP server to execute Phase 6 for EPIC-CCN-075.

Call the execute_phase_6 tool with epic_id="EPIC-CCN-075".

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
bob --yolo "$(cat /tmp/phase6_msg_075.txt)"

# Verify verification report created
if [ -f "docs/brain/EPIC-CCN-075/06-completion-report.md" ]; then
    echo "SUCCESS: Phase 6 complete for EPIC-CCN-075"
    echo "File: docs/brain/EPIC-CCN-075/06-completion-report.md"
    ls -lh docs/brain/EPIC-CCN-075/06-completion-report.md
else
    echo "ERROR: No verification report created for EPIC-CCN-075"
    exit 1
fi
