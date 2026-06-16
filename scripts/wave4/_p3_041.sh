#!/bin/bash
# Phase 3 (DNA & PR Audit) for EPIC-CCN-041
# Generated: 2026-06-15
# Method: MCP tool (phase-3-audit server)

set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-041"
API_KEY="bob_prod_bob-admin_44TtZXuuACpNu133KVpJ7nSGsRr8hhdVUJj3h3jYe5MUk44L1xm6bUAbv5WDab98VadJx53pvp1Kdxmch4E4Qh1H_7J5ULr6U54NC12M2tpGVD6FWjmjk5rgZWcDie42W6mRh"

# Export API key
export BOBSHELL_API_KEY="$API_KEY"

# Create message file (avoids bash multi-line escaping)
cat > /tmp/phase3_msg_041.txt << 'EOFMSG'
Use the phase-3-audit MCP server to execute Phase 3 for EPIC-CCN-041.

Call the execute_phase_3 tool with epic_id="EPIC-CCN-041".

The tool will return complete instructions for DNA & PR audit.
Follow those instructions to create the audit report.

**Verification**: Confirm file exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell (uses phase-3-audit MCP tool)
bob --yolo "$(cat /tmp/phase3_msg_041.txt)"

# Verify file created
if [ -f "docs/brain/EPIC-CCN-041/03-audit-report.md" ]; then
    echo "SUCCESS: Phase 3 complete for EPIC-CCN-041"
    echo "File: docs/brain/EPIC-CCN-041/03-audit-report.md"
    ls -lh "docs/brain/EPIC-CCN-041/03-audit-report.md"
else
    echo "ERROR: File not created for EPIC-CCN-041"
    exit 1
fi
