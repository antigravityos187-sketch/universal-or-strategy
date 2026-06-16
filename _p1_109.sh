#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_t9tV9fuaYCkKYJNm5xCaHWAAR5yJT59mUXoLRHLyb3G4uVHazEQaFacXSz2Nd9Pij2WYNHkvn7THr5amYPqQeDa_ASoyvBNoW8FE2m47D2fhv67cbYGy7TXVeWYswv5N1MNF'
mkdir -p docs/brain/EPIC-CCN-109
mkdir -p logs/phase1

cat > /tmp/phase1_msg_109.txt << 'EOFMSG'
You are executing Phase 1 (Scope Definition) for EPIC-CCN-109.

**Input Artifact**: Read `docs/brain/EPIC-CCN-109/00-hotspots.md` for hotspot analysis.

**Your Task**: Define the extraction scope based on the hotspot analysis.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-109/00-scope.md` with:
   - Target method details
   - Extraction strategy (what to extract, what to keep)
   - Boundary definition (single method only, no scope creep)
   - Success criteria (target complexity <= 8)
   - Risk assessment

2. Update `docs/brain/EPIC-CCN-109/manifest.json`:
   - Set phase "1" status to "completed"
   - Add "00-scope.md" to outputs

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Keep scope to single method (V12.23 No Scope Creep Protocol)
- Target complexity <= 8 (Jane Street alignment)

**Phase**: 1 (Scope Definition)
EOFMSG

bob --yolo /epic-intake EPIC-CCN-109 2>&1 | tee logs/phase1/EPIC-CCN-109.log
echo "DONE_EXIT=$?"

# Made with Bob
