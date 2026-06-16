#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_5A6hXsy7FL4vf9T2jqr11gdYTmAZcFgxVm1dGD9qGPmpD5fV6emRy6XYzZPsqw56mjCtoiEbJmLU8B2VL4ZtgXeS_ALp1DF9sj3R3cU3dzddRRAVu44Y52VHhkt1BNkSdC2Nq'
mkdir -p docs/brain/EPIC-CCN-115
mkdir -p logs/phase1

cat > /tmp/phase1_msg_115.txt << 'EOFMSG'
You are executing Phase 1 (Scope Definition) for EPIC-CCN-115.

**Input Artifact**: Read `docs/brain/EPIC-CCN-115/00-hotspots.md` for hotspot analysis.

**Your Task**: Define the extraction scope based on the hotspot analysis.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-115/00-scope.md` with:
   - Target method details
   - Extraction strategy (what to extract, what to keep)
   - Boundary definition (single method only, no scope creep)
   - Success criteria (target complexity <= 8)
   - Risk assessment

2. Update `docs/brain/EPIC-CCN-115/manifest.json`:
   - Set phase "1" status to "completed"
   - Add "00-scope.md" to outputs

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Keep scope to single method (V12.23 No Scope Creep Protocol)
- Target complexity <= 8 (Jane Street alignment)

**Phase**: 1 (Scope Definition)
EOFMSG

bob --yolo /epic-intake EPIC-CCN-115 2>&1 | tee logs/phase1/EPIC-CCN-115.log
echo "DONE_EXIT=$?"

# Made with Bob
