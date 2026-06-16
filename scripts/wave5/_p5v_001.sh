#!/bin/bash
# Phase 5.V (Verification) for EPIC-CCN-001
# Generated: 2026-06-16
# Wave: 5 (Pilot Test)
# Method: Building-blocks (adapted from _p5_001.sh)

set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-001"
API_KEY="bob_prod_bob-admin_yN7cbWSG9B926LkYPex4pXBGgTbZdN7Xg1ihASxzGdFGz7N8Z5WWDiqeWGUvsXiTWMzag9Hur9EA53BtXQRr2E4_4Z2YTW686zBchNH8KMgN69E3YGDzeRYcWMYxtKkxooeR"

# Export API key
export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Verify Phase 5 ticket completion files exist
if ! ls docs/brain/EPIC-CCN-001/ticket-*-completion.md 1> /dev/null 2>&1; then
    echo "ERROR: Missing prerequisite: No ticket completion files found"
    echo "Phase 5 must complete before Phase 5.V can execute"
    exit 1
fi

# Create message file (avoids bash multi-line escaping)
cat > /tmp/phase5v_msg_001.txt << 'EOFMSG'
Use the phase-5-verify MCP server to execute Phase 5.V (Verification) for EPIC-CCN-001.

Call the execute_phase_5_verify tool with epic_id="EPIC-CCN-001".

The tool will return complete instructions for the 5 MANDATORY CHECKS:
1. Compilation (dotnet build)
2. Complexity Reduction (CYC ≤8)
3. Scope Compliance (ONLY target method modified)
4. Test Coverage (xUnit tests passing)
5. Encoding Compliance (UTF-8 without BOM)

Follow those instructions to execute all 5 checks and generate the verification report.

**ALL 5 CHECKS MUST PASS** before marking epic as complete.

**Verification**: Confirm 06-verification-report.md exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell (uses phase-5-verify MCP tool)
bob --yolo "$(cat /tmp/phase5v_msg_001.txt)"

# Verify verification report created
if [ -f "docs/brain/EPIC-CCN-001/06-verification-report.md" ]; then
    echo "SUCCESS: Phase 5.V complete for EPIC-CCN-001"
    echo "File: docs/brain/EPIC-CCN-001/06-verification-report.md"
    ls -lh docs/brain/EPIC-CCN-001/06-verification-report.md
else
    echo "ERROR: Verification report not created for EPIC-CCN-001"
    exit 1
fi

# Made with Bob