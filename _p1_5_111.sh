#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_5eZYFvHuinQHMnDWNZDZ7ciMX4oiUBsfkVyscGyoEahtNto1a7KNWHo5BFmoN4uPy8rbBYJrUsBtnshvB12nrYQJ_7tiXqEriChoWjAwta66uaZ76JKhxrqiQb6mR5C7AZQyo'
mkdir -p docs/brain/EPIC-CCN-111
mkdir -p logs/phase1_5

cat > /tmp/phase1_5_msg_111.txt << 'EOFMSG'
You are executing Phase 1.5 (Scope Boundary Validation) for EPIC-CCN-111.

**Input Artifact**: Read `docs/brain/EPIC-CCN-111/00-hotspots.md` for hotspot analysis.

**Your Task**: Define the extraction scope based on the hotspot analysis.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-111/01-scope-boundary.md` with:
   - Target method details
   - Extraction strategy (what to extract, what to keep)
   - Boundary definition (single method only, no scope creep)
   - Success criteria (target complexity <= 8)
   - Risk assessment

2. Update `docs/brain/EPIC-CCN-111/manifest.json`:
   - Set phase "1.5" status to "completed"
   - Add "01-scope-boundary.md" to outputs

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Keep scope to single method (V12.23 No Scope Creep Protocol)
- Target complexity <= 8 (Jane Street alignment)

**Phase**: 1.5 (Scope Boundary Validation)
EOFMSG

bob --yolo /epic-scope-boundary EPIC-CCN-111 --phase 1.5 2>&1 | tee logs/phase1_5/EPIC-CCN-111.log
echo "DONE_EXIT=$?"

# Made with Bob
