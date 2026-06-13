#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_2am9d3VjQYnC4mSub1z5SzdSZJeyptWhfMrxGeEBSorZRPj8WmQvBPtTf8qTpjWHWdRuf7toP2WTDtPEfS6aoTYF_7ufADbTYhnLEY42csrSet3f3ssJuNddPhXD65YewpCWX'
mkdir -p docs/brain/EPIC-CCN-110
mkdir -p logs/phase1_5

cat > /tmp/phase1_5_msg_110.txt << 'EOFMSG'
You are executing Phase 1.5 (Scope Boundary Validation) for EPIC-CCN-110.

**Input Artifact**: Read `docs/brain/EPIC-CCN-110/00-hotspots.md` for hotspot analysis.

**Your Task**: Define the extraction scope based on the hotspot analysis.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-110/01-scope-boundary.md` with:
   - Target method details
   - Extraction strategy (what to extract, what to keep)
   - Boundary definition (single method only, no scope creep)
   - Success criteria (target complexity <= 8)
   - Risk assessment

2. Update `docs/brain/EPIC-CCN-110/manifest.json`:
   - Set phase "1.5" status to "completed"
   - Add "01-scope-boundary.md" to outputs

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Keep scope to single method (V12.23 No Scope Creep Protocol)
- Target complexity <= 8 (Jane Street alignment)

**Phase**: 1.5 (Scope Boundary Validation)
EOFMSG

bob --yolo /epic-scope-boundary EPIC-CCN-110 --phase 1.5 2>&1 | tee logs/phase1_5/EPIC-CCN-110.log
echo "DONE_EXIT=$?"

# Made with Bob
