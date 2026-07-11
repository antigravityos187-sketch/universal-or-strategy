#!/bin/bash
# Phase 3 (DNA & PR Audit) for EPIC-CCN-020
# Generated: 2026-06-15
# Method: MCP tool (phase-3-audit server)

set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-020"
API_KEY="bob_prod_bob-admin_3abxQUhB6oz3484pgXxkjkeZEXxTEJfFGwg4D5cY6GWrCXFjT6uUQhvtLz5n8dB5g9Pue31DVuLwR9wa34zrBNmT_DdGCwiky7h1JVUEzJZVTrDxZNUigAnSRPPdUEJNzeLZT"

# Export API key
export BOBSHELL_API_KEY="$API_KEY"

# Create message file (avoids bash multi-line escaping)
cat > /tmp/phase3_msg_020.txt << 'EOFMSG'
Use the phase-3-audit MCP server to execute Phase 3 for EPIC-CCN-020.

Call the execute_phase_3 tool with epic_id="EPIC-CCN-020".

The tool will return complete instructions for DNA & PR audit.
Follow those instructions to create the audit report.

**Verification**: Confirm file exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell (uses phase-3-audit MCP tool)
bob --yolo "$(cat /tmp/phase3_msg_020.txt)"

# Verify file created
if [ -f "docs/brain/EPIC-CCN-020/03-audit-report.md" ]; then
    echo "SUCCESS: Phase 3 complete for EPIC-CCN-020"
    echo "File: docs/brain/EPIC-CCN-020/03-audit-report.md"
    ls -lh "docs/brain/EPIC-CCN-020/03-audit-report.md"
else
    echo "ERROR: File not created for EPIC-CCN-020"
    exit 1
fi
