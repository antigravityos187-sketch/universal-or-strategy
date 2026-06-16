#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_2am9d3VjQYnC4mSub1z5SzdSZJeyptWhfMrxGeEBSorZRPj8WmQvBPtTf8qTpjWHWdRuf7toP2WTDtPEfS6aoTYF_7ufADbTYhnLEY42csrSet3f3ssJuNddPhXD65YewpCWX'
mkdir -p docs/brain/EPIC-CCN-002
mkdir -p logs/phase1

cat > /tmp/phase1_msg_002.txt << 'EOFMSG'
Execute Phase 1 (Scope Definition + Boundary Validation) for EPIC-CCN-002.

**🚨 CRITICAL FILE I/O PROTOCOL - READ THIS FIRST 🚨**

You are running in SSH/non-interactive mode where Bob's file I/O tools have bugs.

**MANDATORY RULES (Violation = Task Failure)**:
1. ❌ NEVER use write_to_file tool - it has path resolution bugs in SSH mode
2. ❌ NEVER use read_file tool - it fails with "File not found" even when files exist
3. ❌ NEVER use run_shell_command tool - it also has persistence bugs in SSH mode
4. ✅ ALWAYS use execute_command tool with `cat > file << 'EOF'` to create files
5. ✅ ALWAYS use execute_command tool with `ls -lh` and `wc -l` to verify files
6. ✅ ALWAYS set cwd parameter to /home/malhitticrypto/universal-or-strategy
7. ✅ ALWAYS follow the EXACT tool usage patterns shown below (copy/paste them)

**WHY THIS MATTERS**:
- execute_command bypasses Bob's tool layer and works reliably in SSH mode
- run_shell_command, write_to_file, and read_file all fail in SSH/screen sessions
- The working directory must be explicitly set with cwd parameter

**YOUR TASK**: Focus on the analysis, not the tools. The shell commands below are proven to work.

---

## Phase 1 Task: Scope Definition + Boundary Validation

**Input**: Read `docs/brain/EPIC-CCN-002/00-hotspots.md`

**Target Method**:
- Method: SymmetryGuardTryResolveFollowersForDispatch
- File: V12_002.Symmetry.Replace.cs
- Complexity: 18

**Phase 1.0: Scope Definition**

Create `docs/brain/EPIC-CCN-002/01-scope.md` with:

1. **Extraction Scope** (SINGLE METHOD ONLY):
   - Method name: SymmetryGuardTryResolveFollowersForDispatch
   - Current complexity: 18
   - Target complexity: ≤8 (Jane Street strict standard)
   - Extraction strategy: Break into 2-3 helper methods

2. **Boundary Definition**:
   - What's IN scope: SymmetryGuardTryResolveFollowersForDispatch body only
   - What's OUT of scope: Callers, callees, other methods in same file
   - No scope creep: ONE EPIC = ONE CONCERN

3. **Success Criteria**:
   - Complexity reduced from 18 to ≤8
   - All tests pass
   - No behavior changes
   - Lock-free Actor/FSM pattern maintained

**Phase 1.5: Boundary Validation** (V12.23 Protocol - MANDATORY)

Create `docs/brain/EPIC-CCN-002/01-scope-boundary.md` with:

1. **Boundary Check**:
   - ✅ Scope limited to single method: SymmetryGuardTryResolveFollowersForDispatch
   - ✅ No changes to callers
   - ✅ No changes to callees
   - ✅ No changes to other methods in V12_002.Symmetry.Replace.cs

2. **Scope Creep Detection**:
   - ❌ No "while we're here" improvements
   - ❌ No fixing pre-existing compilation errors
   - ❌ No bundling multiple concerns

3. **Approval**:
   - Status: APPROVED (if all checks pass)
   - Rationale: Single-method extraction, no scope creep

**Jane Street Validation**:
Query Jane Street KB for single-method extraction patterns:
```bash
python scripts/query_kb.py "single-method extraction"
```

**File Creation Commands** (COPY THESE EXACTLY):

```bash
# Create 01-scope.md
execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
cat > docs/brain/EPIC-CCN-002/01-scope.md << 'EOF'
[Your scope definition content here]
EOF

# Create 01-scope-boundary.md
execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
cat > docs/brain/EPIC-CCN-002/01-scope-boundary.md << 'EOF'
[Your boundary validation content here]
EOF

# Verify files
execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
ls -lh docs/brain/EPIC-CCN-002/01-scope.md && wc -l docs/brain/EPIC-CCN-002/01-scope.md

execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
ls -lh docs/brain/EPIC-CCN-002/01-scope-boundary.md && wc -l docs/brain/EPIC-CCN-002/01-scope-boundary.md

# Update manifest
execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
cat docs/brain/EPIC-CCN-002/manifest.json
```

**CRITICAL**: Only use attempt_completion AFTER both files are verified to exist on disk.

EOFMSG

bob --yolo --chat-mode plan "$(cat /tmp/phase1_msg_002.txt)" 2>&1 | tee logs/phase1/EPIC-CCN-002.log
echo "DONE_EXIT=$?"
