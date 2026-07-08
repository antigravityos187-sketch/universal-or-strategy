#!/bin/bash
# Phase 5 (Ticket Execution) for EPIC-CCN-003
# Generated: 2026-06-15
# Method: MCP tool (phase-5-execute server)

set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-003"
API_KEY="bob_prod_bob-admin_t9tV9fuaYCkKYJNm5xCaHWAAR5yJT59mUXoLRHLyb3G4uVHazEQaFacXSz2Nd9Pij2WYNHkvn7THr5amYPqQeDa_ASoyvBNoW8FE2m47D2fhv67cbYGy7TXVeWYswv5N1MNF"

# Export API key
export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Verify Phase 4 file exists
if [ ! -f "docs/brain/EPIC-CCN-003/04-tickets.md" ]; then
    echo "ERROR: Missing prerequisite file: docs/brain/EPIC-CCN-003/04-tickets.md"
    echo "Phase 4 must complete before Phase 5 can execute"
    exit 1
fi

# Create message file (avoids bash multi-line escaping)
cat > /tmp/phase5_msg_003.txt << 'EOFMSG'
Use the phase-5-execute MCP server to execute Phase 5 for EPIC-CCN-003.

Call the execute_phase_5 tool with epic_id="EPIC-CCN-003".

The tool will return complete instructions for ticket execution.
Follow those instructions to execute all tickets surgically.

**Verification**: Confirm execution files exist on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell (uses phase-5-execute MCP tool)
bob --yolo --chat-mode v12-engineer "$(cat /tmp/phase5_msg_003.txt)"

# Verify execution files created (at least one ticket completion file)
if ls docs/brain/EPIC-CCN-003/ticket-*-completion.md 1> /dev/null 2>&1; then
    echo "SUCCESS: Phase 5 complete for EPIC-CCN-003"
    echo "Files: docs/brain/EPIC-CCN-003/ticket-*-completion.md"
    ls -lh docs/brain/EPIC-CCN-003/ticket-*-completion.md
else
    echo "ERROR: No ticket completion files created for EPIC-CCN-003"
    exit 1
fi
