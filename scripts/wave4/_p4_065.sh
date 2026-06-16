#!/bin/bash
# Phase 4 (Ticket Generation) for EPIC-CCN-065
# Generated: 2026-06-15
# Method: MCP tool (phase-4-tickets server)

set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-065"
API_KEY="bob_prod_bob-admin_5A6hXsy7FL4vf9T2jqr11gdYTmAZcFgxVm1dGD9qGPmpD5fV6emRy6XYzZPsqw56mjCtoiEbJmLU8B2VL4ZtgXeS_ALp1DF9sj3R3cU3dzddRRAVu44Y52VHhkt1BNkSdC2Nq"

# Export API key
export BOBSHELL_API_KEY="$API_KEY"

# Create message file (avoids bash multi-line escaping)
cat > /tmp/phase4_msg_065.txt << 'EOFMSG'
Use the phase-4-tickets MCP server to execute Phase 4 for EPIC-CCN-065.

Call the execute_phase_4 tool with epic_id="EPIC-CCN-065".

The tool will return complete instructions for ticket generation.
Follow those instructions to create the ticket breakdown.

**Verification**: Confirm file exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell (uses phase-4-tickets MCP tool)
bob --yolo "$(cat /tmp/phase4_msg_065.txt)"

# Verify file created
if [ -f "docs/brain/EPIC-CCN-065/04-tickets.md" ]; then
    echo "SUCCESS: Phase 4 complete for EPIC-CCN-065"
    echo "File: docs/brain/EPIC-CCN-065/04-tickets.md"
    ls -lh "docs/brain/EPIC-CCN-065/04-tickets.md"
else
    echo "ERROR: File not created for EPIC-CCN-065"
    exit 1
fi
