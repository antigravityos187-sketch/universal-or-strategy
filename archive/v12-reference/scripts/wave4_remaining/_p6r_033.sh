#!/bin/bash
# Phase 6 (Verification) for EPIC-CCN-033
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-033"
API_KEY="bob_prod_bob-admin_5eZYFvHuinQHMnDWNZDZ7ciMX4oiUBsfkVyscGyoEahtNto1a7KNWHo5BFmoN4uPy8rbBYJrUsBtnshvB12nrYQJ_7tiXqEriChoWjAwta66uaZ76JKhxrqiQb6mR5C7AZQyo"

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Verify Phase 5 completion file exists (robust OR logic)
if ! find docs/brain/EPIC-CCN-033 -maxdepth 1 \( -name "05-*.md" -o -name "ticket-*-completion.md" \) -print -quit | grep -q .; then
    echo "ERROR: Missing Phase 5 completion files for EPIC-CCN-033"
    echo "Expected: docs/brain/EPIC-CCN-033/05-*.md OR ticket-*-completion.md"
    exit 1
fi

# Create message file
cat > /tmp/phase6_msg_033.txt << 'EOFMSG'
Use the phase-6-review MCP server to execute Phase 6 for EPIC-CCN-033.

Call the execute_phase_6 tool with epic_id="EPIC-CCN-033".

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
bob --yolo "$(cat /tmp/phase6_msg_033.txt)"

# Verify completion report created (FIXED: correct filename)
if [ -f "docs/brain/EPIC-CCN-033/06-completion-report.md" ]; then
    echo "SUCCESS: Phase 6 complete for EPIC-CCN-033"
    echo "File: docs/brain/EPIC-CCN-033/06-completion-report.md"
    ls -lh docs/brain/EPIC-CCN-033/06-completion-report.md
else
    echo "ERROR: No completion report created for EPIC-CCN-033"
    exit 1
fi
