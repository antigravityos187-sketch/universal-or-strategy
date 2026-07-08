#!/bin/bash
# Phase 4 (Ticket Generation) for EPIC-CCN-061
# Generated: 2026-06-15
# Method: MCP tool (phase-4-tickets server)

set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-061"
API_KEY="bob_prod_bob-admin_5eZYFvHuinQHMnDWNZDZ7ciMX4oiUBsfkVyscGyoEahtNto1a7KNWHo5BFmoN4uPy8rbBYJrUsBtnshvB12nrYQJ_7tiXqEriChoWjAwta66uaZ76JKhxrqiQb6mR5C7AZQyo"

# Export API key
export BOBSHELL_API_KEY="$API_KEY"

# Create message file (avoids bash multi-line escaping)
cat > /tmp/phase4_msg_061.txt << 'EOFMSG'
Use the phase-4-tickets MCP server to execute Phase 4 for EPIC-CCN-061.

Call the execute_phase_4 tool with epic_id="EPIC-CCN-061".

The tool will return complete instructions for ticket generation.
Follow those instructions to create the ticket breakdown.

**Verification**: Confirm file exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell (uses phase-4-tickets MCP tool)
bob --yolo "$(cat /tmp/phase4_msg_061.txt)"

# Verify file created
if [ -f "docs/brain/EPIC-CCN-061/04-tickets.md" ]; then
    echo "SUCCESS: Phase 4 complete for EPIC-CCN-061"
    echo "File: docs/brain/EPIC-CCN-061/04-tickets.md"
    ls -lh "docs/brain/EPIC-CCN-061/04-tickets.md"
else
    echo "ERROR: File not created for EPIC-CCN-061"
    exit 1
fi
