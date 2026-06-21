#!/bin/bash
# Simplified Phase 1.5 Test - Wave 4 Pattern Only
set -euo pipefail

EPIC_ID="EPIC-CCN-001"
AGENT_ID="test-agent"

echo "=========================================="
echo "Phase 1.5 Test: Wave 4 Pattern"
echo "Epic: $EPIC_ID"
echo "=========================================="

# Create directories
mkdir -p docs/brain/$EPIC_ID
mkdir -p logs/wave6/phase1_5

# Create dummy scope file for testing
cat > docs/brain/$EPIC_ID/00-scope.md << 'EOF'
# Phase 1: Scope Definition

## Target Method
- Method: SymmetryGuardReplaceExistingFollowerTarget
- File: V12_002.Symmetry.Replace.cs
- Complexity: 18

## Extraction Scope
Single method extraction only.
EOF

# Step 1: Create message file (Wave 4 two-step pattern)
cat > /tmp/phase1_5_msg_$EPIC_ID.txt << 'EOFMSG'
Execute Phase 1.5 (Scope Boundary Validation) for EPIC-CCN-001.

**Input**: Read docs/brain/EPIC-CCN-001/00-scope.md

**Task**: Validate that the extraction scope is limited to a SINGLE METHOD only.

**MANDATORY Boundary Checks**:
1. ✅ Scope limited to single method (no multi-method extraction)
2. ✅ No changes to callers
3. ✅ No changes to callees
4. ✅ No changes to other methods in same file

**Output**: Create docs/brain/EPIC-CCN-001/01-scope-boundary.md with:
- Boundary validation results
- Approval status (APPROVED/REJECTED)
- Rationale

Use execute_command with cat > file << 'EOF' pattern for file creation.
EOFMSG

echo "Message file created: /tmp/phase1_5_msg_$EPIC_ID.txt"
echo "Message content:"
cat /tmp/phase1_5_msg_$EPIC_ID.txt
echo ""

# Step 2: Run Bob with --yolo (Wave 4 pattern)
echo "Running Bob CLI with Wave 4 pattern..."
export BOBSHELL_API_KEY=$(grep 'export BOBSHELL_API_KEY' ~/.bashrc | cut -d'=' -f2)
~/.npm-global/bin/bob --yolo --chat-mode v12-phase1-5-boundary "$(cat /tmp/phase1_5_msg_$EPIC_ID.txt)" 2>&1 | tee "logs/wave6/phase1_5/$EPIC_ID.log"

BOB_EXIT_CODE=${PIPESTATUS[0]}

echo ""
echo "=========================================="
echo "Bob CLI exit code: $BOB_EXIT_CODE"
echo "=========================================="

# Verify output file
OUTPUT_FILE="docs/brain/$EPIC_ID/01-scope-boundary.md"
if [ -f "$OUTPUT_FILE" ]; then
    echo "✅ Output file created: $OUTPUT_FILE"
    echo "File size: $(wc -c < $OUTPUT_FILE) bytes"
    echo "Line count: $(wc -l < $OUTPUT_FILE) lines"
    echo ""
    echo "First 10 lines:"
    head -10 "$OUTPUT_FILE"
else
    echo "❌ Output file NOT created: $OUTPUT_FILE"
    exit 1
fi

echo ""
echo "=========================================="
echo "✅ TEST SUCCESS"
echo "=========================================="

# Made with Bob
