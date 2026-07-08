#!/bin/bash
# Phase 3 (DNA & PR Audit) for EPIC-CCN-021
# Generated: 2026-06-15
# Method: MCP tool (phase-3-audit server)

set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-021"
API_KEY="bob_prod_bob-admin_3vzs4jptuwZ7Z63gqpyn3aNy89ozwWyanh2aNB7TQDa22rfmiRJXWCUivJphxYNLAoT8nJMEYmUxaTgWA5Z8URUd_F6U16mpCReKejNsSHgrd7VxPEHuX8sedjJm4hrV7srcQ"

# Export API key
export BOBSHELL_API_KEY="$API_KEY"

# Create message file (avoids bash multi-line escaping)
cat > /tmp/phase3_msg_021.txt << 'EOFMSG'
Use the phase-3-audit MCP server to execute Phase 3 for EPIC-CCN-021.

Call the execute_phase_3 tool with epic_id="EPIC-CCN-021".

The tool will return complete instructions for DNA & PR audit.
Follow those instructions to create the audit report.

**Verification**: Confirm file exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell (uses phase-3-audit MCP tool)
bob --yolo "$(cat /tmp/phase3_msg_021.txt)"

# Verify file created
if [ -f "docs/brain/EPIC-CCN-021/03-audit-report.md" ]; then
    echo "SUCCESS: Phase 3 complete for EPIC-CCN-021"
    echo "File: docs/brain/EPIC-CCN-021/03-audit-report.md"
    ls -lh "docs/brain/EPIC-CCN-021/03-audit-report.md"
else
    echo "ERROR: File not created for EPIC-CCN-021"
    exit 1
fi
