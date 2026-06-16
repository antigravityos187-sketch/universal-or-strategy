#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_t9tV9fuaYCkKYJNm5xCaHWAAR5yJT59mUXoLRHLyb3G4uVHazEQaFacXSz2Nd9Pij2WYNHkvn7THr5amYPqQeDa_ASoyvBNoW8FE2m47D2fhv67cbYGy7TXVeWYswv5N1MNF'
mkdir -p docs/brain/EPIC-CCN-109
mkdir -p logs/phase1_5

cat > /tmp/phase1_5_msg_109.txt << 'EOFMSG'
You are executing Phase 1.5 (Scope Boundary Validation) for EPIC-CCN-109.

**Input Artifact**: Read `docs/brain/EPIC-CCN-109/00-hotspots.md` for hotspot analysis.

**Your Task**: Define the extraction scope based on the hotspot analysis.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-109/01-scope-boundary.md` with:
   - Target method details
   - Extraction strategy (what to extract, what to keep)
   - Boundary definition (single method only, no scope creep)
   - Success criteria (target complexity <= 8)
   - Risk assessment

2. Update `docs/brain/EPIC-CCN-109/manifest.json`:
   - Set phase "1.5" status to "completed"
   - Add "01-scope-boundary.md" to outputs

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Keep scope to single method (V12.23 No Scope Creep Protocol)
- Target complexity <= 8 (Jane Street alignment)

**Phase**: 1.5 (Scope Boundary Validation)
EOFMSG

bob --yolo /epic-scope-boundary EPIC-CCN-109 --phase 1.5 2>&1 | tee logs/phase1_5/EPIC-CCN-109.log
echo "DONE_EXIT=$?"

# Made with Bob
