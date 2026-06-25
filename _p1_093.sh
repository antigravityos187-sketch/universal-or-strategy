#!/bin/bash
# Wave 7 Phase 1 Template: Scope Definition (Fixed)
# Epic: EPIC-W7-093
# Agent: jessica
# Dependencies: Phase 0 (00-hotspots.md)
# Output: docs/brain/EPIC-W7-093/00-scope.md

set -euo pipefail

EPIC_ID="EPIC-W7-093"
AGENT_ID="jessica"
PHASE="1"

echo "=========================================="
echo "Wave 7 Phase 1: Scope Definition"
echo "Epic: $EPIC_ID"
echo "Agent: $AGENT_ID"
echo "=========================================="

# Step 1: Verify Dependencies
echo ""
echo "Step 1: Verify Dependencies"
echo "-----------------------------------"

# Check Phase 0 output exists
if [ ! -f "docs/brain/$EPIC_ID/00-hotspots.md" ]; then
    echo "❌ BLOCKED: Phase 0 not complete (00-hotspots.md missing)"
    exit 1
fi
echo "✅ Phase 0 complete (00-hotspots.md exists)"

# Step 2: Create Bob CLI Message
echo ""
echo "Step 2: Create Bob CLI Message"
echo "-----------------------------------"

mkdir -p /tmp
cat > /tmp/phase1_msg_$EPIC_ID.txt << 'EOFMSG'
# Phase 1: Scope Definition

You are in **Plan mode** for Phase 1 (Scope Definition) of epic EPIC-W7-093.

## Context
- **Epic**: EPIC-W7-093
- **Phase**: 1 (Scope Definition)
- **Input**: docs/brain/EPIC-W7-093/00-hotspots.md
- **Output**: docs/brain/EPIC-W7-093/00-scope.md

## Your Task
Read the hotspot analysis and define the refactoring scope:

1. **Read Input**: Read docs/brain/EPIC-W7-093/00-hotspots.md
2. **Define Scope**: 
   - What code will be extracted?
   - What will remain in the original method?
   - What are the boundaries?
3. **Write Output**: Create docs/brain/EPIC-W7-093/00-scope.md with:
   - Extraction targets (methods to extract)
   - Boundary definitions (what stays, what goes)
   - Dependencies and risks
   - Success criteria

## Critical Rules
- Use **Plan mode** (no code changes)
- Read the hotspot analysis first
- Define clear boundaries
- Document all extraction targets
- Identify dependencies

Begin by reading the hotspot analysis, then define the scope.
EOFMSG

echo "✅ Message file created: /tmp/phase1_msg_$EPIC_ID.txt"

# Step 3: Invoke Bob CLI
echo ""
echo "Step 3: Invoke Bob CLI (Plan Mode)"
echo "-----------------------------------"

export BOBSHELL_API_KEY='bob_prod_bob-admin_c8SKNdvWX47LjEA1771m3PtSTg5Rd95DFurnpmpuoEEBD4Q1DAwe9UibFmH1wSeyL5u2MwZFGWDZPbbS5iPh8jC_ESknTx4s3SD4zbfW5Gu6sHTNPA5AYwSnsWy9uS5rkpKu'
~/.npm-global/bin/bob --yolo --chat-mode v12-phase1-scope "$(cat /tmp/phase1_msg_$EPIC_ID.txt)"

if [ $? -ne 0 ]; then
    echo "❌ Bob CLI failed"
    exit 1
fi

# Step 4: Verify Output
echo ""
echo "Step 4: Verify Output"
echo "-----------------------------------"

if [ ! -f "docs/brain/$EPIC_ID/00-scope.md" ]; then
    echo "❌ FAILED: Output file not created (00-scope.md missing)"
    exit 1
fi

echo "✅ Output file created: docs/brain/$EPIC_ID/00-scope.md"

# Step 5: Update Manifest
echo ""
echo "Step 5: Update Manifest"
echo "-----------------------------------"

python3 scripts/epic_manifest.py update "$EPIC_ID" "$PHASE" "completed" "docs/brain/$EPIC_ID/00-scope.md"

if [ $? -ne 0 ]; then
    echo "⚠️  Warning: Manifest update failed (non-blocking)"
fi

echo ""
echo "=========================================="
echo "✅ Phase 1 Complete: $EPIC_ID"
echo "=========================================="

# Made with Bob
