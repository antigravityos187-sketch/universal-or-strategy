#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu'
mkdir -p docs/brain/EPIC-CCN-119
mkdir -p logs/phase1

cat > /tmp/phase1_msg_119.txt << 'EOFMSG'
You are executing Phase 1 (Scope + Boundary) for EPIC-CCN-119.

**IMPORTANT**: Phase 1 now combines Scope Definition AND Boundary Validation (V12.25 10-phase workflow).

**Input Artifact**: Read `docs/brain/EPIC-CCN-119/00-hotspots.md` for hotspot analysis.

**Your Task**: Define the extraction scope AND validate boundary constraints.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-119/00-scope.md` with:
   - Target method details
   - Extraction strategy (what to extract, what to keep)
   - Boundary definition (single method only, no scope creep)
   - Success criteria (target complexity <= 8, Jane Street alignment)
   - Risk assessment
   - **Boundary Validation Section**:
     * Confirm extraction stays within single method
     * List any dependencies that would violate boundary
     * Explicit statement: "Boundary validated: YES/NO"

2. Update `docs/brain/EPIC-CCN-119/manifest.json`:
   - Set phase "1" status to "completed"
   - Add "00-scope.md" to outputs

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Keep scope to single method (V12.23 No Scope Creep Protocol)
- Target complexity <= 8 (Jane Street HFT alignment, NOT 15)
- **MANDATORY**: Boundary validation must explicitly confirm single-method scope

**Phase**: 1 (Scope + Boundary)
**Target Complexity**: <= 8 (Jane Street standard)
EOFMSG

bob --yolo --chat-mode plan "$(cat /tmp/phase1_msg_119.txt)" 2>&1 | tee logs/phase1/EPIC-CCN-119.log
echo "DONE_EXIT=$?"

# Made with Bob
