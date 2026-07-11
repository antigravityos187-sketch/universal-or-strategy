#!/bin/bash
# Phase 4 (Ticket Generation) for EPIC-CCN-080
# Generated: 2026-06-15
# Method: MCP tool (phase-4-tickets server)

set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-080"
API_KEY="bob_prod_bob-admin_34gay3JrSM5CqZF7cg5BjDGDwEk7ZLQdBXUjWdQH9vnaSM6YKaigEytQDQXSygmGqEEXHm7qiLKLupwdhK5DAQp4_61R5yxHVTtKmgDRRR9mcSxJ1HBAPdYnzLcY9utoNmrfo"

# Export API key
export BOBSHELL_API_KEY="$API_KEY"

# Create message file (avoids bash multi-line escaping)
cat > /tmp/phase4_msg_080.txt << 'EOFMSG'
Use the phase-4-tickets MCP server to execute Phase 4 for EPIC-CCN-080.

Call the execute_phase_4 tool with epic_id="EPIC-CCN-080".

The tool will return complete instructions for ticket generation.
Follow those instructions to create the ticket breakdown.

**Verification**: Confirm file exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell (uses phase-4-tickets MCP tool)
bob --yolo "$(cat /tmp/phase4_msg_080.txt)"

# Verify file created
if [ -f "docs/brain/EPIC-CCN-080/04-tickets.md" ]; then
    echo "SUCCESS: Phase 4 complete for EPIC-CCN-080"
    echo "File: docs/brain/EPIC-CCN-080/04-tickets.md"
    ls -lh "docs/brain/EPIC-CCN-080/04-tickets.md"
else
    echo "ERROR: File not created for EPIC-CCN-080"
    exit 1
fi
