#!/bin/bash
# Phase 3 (DNA & PR Audit) for EPIC-CCN-074
# Generated: 2026-06-15
# Method: MCP tool (phase-3-audit server)

set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-074"
API_KEY="bob_prod_bob-admin_2am9d3VjQYnC4mSub1z5SzdSZJeyptWhfMrxGeEBSorZRPj8WmQvBPtTf8qTpjWHWdRuf7toP2WTDtPEfS6aoTYF_7ufADbTYhnLEY42csrSet3f3ssJuNddPhXD65YewpCWX"

# Export API key
export BOBSHELL_API_KEY="$API_KEY"

# Create message file (avoids bash multi-line escaping)
cat > /tmp/phase3_msg_074.txt << 'EOFMSG'
Use the phase-3-audit MCP server to execute Phase 3 for EPIC-CCN-074.

Call the execute_phase_3 tool with epic_id="EPIC-CCN-074".

The tool will return complete instructions for DNA & PR audit.
Follow those instructions to create the audit report.

**Verification**: Confirm file exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell (uses phase-3-audit MCP tool)
bob --yolo "$(cat /tmp/phase3_msg_074.txt)"

# Verify file created
if [ -f "docs/brain/EPIC-CCN-074/03-audit-report.md" ]; then
    echo "SUCCESS: Phase 3 complete for EPIC-CCN-074"
    echo "File: docs/brain/EPIC-CCN-074/03-audit-report.md"
    ls -lh "docs/brain/EPIC-CCN-074/03-audit-report.md"
else
    echo "ERROR: File not created for EPIC-CCN-074"
    exit 1
fi
