#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_t9tV9fuaYCkKYJNm5xCaHWAAR5yJT59mUXoLRHLyb3G4uVHazEQaFacXSz2Nd9Pij2WYNHkvn7THr5amYPqQeDa_ASoyvBNoW8FE2m47D2fhv67cbYGy7TXVeWYswv5N1MNF'
mkdir -p docs/brain/EPIC-CCN-109
mkdir -p logs/phase2

cat > /tmp/phase2_msg_109.txt << 'EOFMSG'
You are executing Phase 2 (Architecture Planning) for EPIC-CCN-109.

**Input Artifact**: Read `docs/brain/EPIC-CCN-109/01-scope-boundary.md` for scope definition.

**Your Task**: Create detailed architecture plan for the extraction.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-109/02-architecture-plan.md` with:
   - Method signatures (before/after)
   - Call graph analysis
   - Dependency mapping
   - Extraction sequence
   - Jane Street compliance checks
   - Risk mitigation strategies

2. Update `docs/brain/EPIC-CCN-109/manifest.json`:
   - Set phase "2" status to "completed"
   - Add "02-architecture-plan.md" to outputs

**MANDATORY REPORTING**:
After completing all tasks, you MUST report:
1. Bobcoins used this session: [X.XX]
2. Remaining balance in API key: [Y.YY]
Format: "Cost: X.XX | Balance: Y.YY"

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Target complexity <= 8 (Jane Street alignment)
- Single method extraction only (V12.23 Protocol)

**Phase**: 2 (Architecture Planning)
EOFMSG

bob --yolo /epic-plan EPIC-CCN-109 2>&1 | tee logs/phase2/EPIC-CCN-109.log
echo "DONE_EXIT=$?"

# Made with Bob
