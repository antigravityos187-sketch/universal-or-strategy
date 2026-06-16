#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu'
mkdir -p docs/brain/EPIC-CCN-107
mkdir -p logs/phase1_5

cat > /tmp/phase1_5_msg_107.txt << 'EOFMSG'
You are executing Phase 1.5 (Scope Boundary Validation) for EPIC-CCN-107.

**Input Artifact**: Read `docs/brain/EPIC-CCN-107/00-hotspots.md` for hotspot analysis.

**Your Task**: Define the extraction scope based on the hotspot analysis.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-107/01-scope-boundary.md` with:
   - Target method details
   - Extraction strategy (what to extract, what to keep)
   - Boundary definition (single method only, no scope creep)
   - Success criteria (target complexity <= 8)
   - Risk assessment

2. Update `docs/brain/EPIC-CCN-107/manifest.json`:
   - Set phase "1.5" status to "completed"
   - Add "01-scope-boundary.md" to outputs

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Keep scope to single method (V12.23 No Scope Creep Protocol)
- Target complexity <= 8 (Jane Street alignment)

**Phase**: 1.5 (Scope Boundary Validation)
EOFMSG

bob --yolo /epic-scope-boundary EPIC-CCN-107 --phase 1.5 2>&1 | tee logs/phase1_5/EPIC-CCN-107.log
echo "DONE_EXIT=$?"

# Made with Bob
