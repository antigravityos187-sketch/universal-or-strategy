#!/bin/bash
# Phase 5 (Ticket Execution) for EPIC-CCN-001
# Generated: 2026-06-17
# Wave: 5 (Retry after Wave 4 rollback)
# Protocol: V12.40 (MCP Server Setup + Rollback Context)

set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-001"
API_KEY="bob_prod_bob-admin_yN7cbWSG9B926LkYPex4pXBGgTbZdN7Xg1ihASxzGdFGz7N8Z5WWDiqeWGUvsXiTWMzag9Hur9EA53BtXQRr2E4_4Z2YTW686zBchNH8KMgN69E3YGDzeRYcWMYxtKkxooeR"

# Export API key
export BOBSHELL_API_KEY="$API_KEY"

# CRITICAL: Clean up stale manifest data from pre-rollback
# The manifest.json shows Phase 5 "completed" but this is STALE data from before Wave 4 rollback
# Wave 4 rollback deleted all Phase 5-6 files, so this epic needs FULL re-execution
echo "=== Wave 5 Rollback Context ==="
echo "EPIC-CCN-001 was rolled back in Wave 4"
echo "Manifest.json contains STALE completion data from before rollback"
echo "Phase 5-6 files were deleted - epic needs FULL re-execution"
echo "================================"

# Prerequisite check: Verify Phase 4 file exists
if [ ! -f "docs/brain/EPIC-CCN-001/04-tickets.md" ]; then
    echo "ERROR: Missing prerequisite file: docs/brain/EPIC-CCN-001/04-tickets.md"
    echo "Phase 4 must complete before Phase 5 can execute"
    exit 1
fi

# Create message file with explicit rollback context
cat > /tmp/phase5_msg_001_v2.txt << 'EOFMSG'
**CRITICAL CONTEXT**: This is a Wave 5 retry after Wave 4 rollback.

The manifest.json for EPIC-CCN-001 shows Phase 5 status "completed" but this is STALE data from BEFORE the rollback. Wave 4 rollback deleted all Phase 5-6 completion files.

**Your Task**: Execute Phase 5 for EPIC-CCN-001 from scratch. IGNORE the manifest.json completion status - it is stale. The epic needs FULL re-execution.

Use the phase-5-execute MCP server to execute Phase 5 for EPIC-CCN-001.

Call the execute_phase_5 tool with epic_id="EPIC-CCN-001".

The tool will return complete instructions for ticket execution.
Follow those instructions to execute all tickets surgically.

**Verification**: Confirm execution files exist on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell (V12.43: Explicit mode enforcement)
# CRITICAL: --chat-mode v12-engineer flag ensures protocol compliance
# Wave 4 violated V12.18 by defaulting to code mode when MCP failed
bob --yolo --chat-mode v12-engineer "$(cat /tmp/phase5_msg_001_v2.txt)"

# Verify execution files created (at least one ticket completion file)
if ls docs/brain/EPIC-CCN-001/ticket-*-completion.md 1> /dev/null 2>&1; then
    echo "SUCCESS: Phase 5 complete for EPIC-CCN-001"
    echo "Files: docs/brain/EPIC-CCN-001/ticket-*-completion.md"
    ls -lh docs/brain/EPIC-CCN-001/ticket-*-completion.md
else
    echo "ERROR: No ticket completion files created for EPIC-CCN-001"
    exit 1
fi

# Made with Bob