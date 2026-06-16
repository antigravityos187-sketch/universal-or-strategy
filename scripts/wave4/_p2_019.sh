#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_5eZYFvHuinQHMnDWNZDZ7ciMX4oiUBsfkVyscGyoEahtNto1a7KNWHo5BFmoN4uPy8rbBYJrUsBtnshvB12nrYQJ_7tiXqEriChoWjAwta66uaZ76JKhxrqiQb6mR5C7AZQyo'
mkdir -p docs/brain/EPIC-CCN-019
mkdir -p logs/phase2

cat > /tmp/phase2_msg_019.txt << 'EOFMSG'
Execute Phase 2 (Architecture Planning) for EPIC-CCN-019.

**CRITICAL FILE I/O PROTOCOL - READ THIS FIRST**

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

## Phase 2 Task: Architecture Planning

**Input**: Read `docs/brain/EPIC-CCN-019/01-scope-boundary.md`

**Target Method**:
- Method: TryHandleFleet_MoveTarget
- File: V12_002.UI.IPC.Commands.Fleet.cs
- Complexity: 15
- LOC: 33
- Tier: 1

**Phase 2: Architecture Planning**

Create `docs/brain/EPIC-CCN-019/02-architecture-plan.md` with:

1. **Extraction Strategy**:
   - Current method: TryHandleFleet_MoveTarget
   - Current complexity: 15
   - Target complexity: ≤8 (Jane Street strict standard)
   - Proposed helper methods: 2-3 methods with clear responsibilities

2. **Method Signatures**:
   - Original method signature (from jCodemunch)
   - Proposed helper method signatures
   - Parameter types and return types
   - Access modifiers (private/internal)

3. **Call Graph**:
   - Which helper calls which
   - Data flow between methods
   - Shared state (if any)

4. **Lock-Free Validation**:
   - ✅ No lock() statements
   - ✅ Uses FSM/Actor Enqueue pattern
   - ✅ Atomic primitives only

5. **Jane Street Compliance**:
   - Query Jane Street KB for extraction patterns
   - Validate against HFT microsecond-latency requirements
   - Ensure cognitive simplicity (CYC ≤8)

**Jane Street Validation** (MANDATORY):
Query Jane Street KB for FSM extraction patterns:
```bash
python scripts/query_kb.py "FSM extraction patterns"
```

**Sequential Thinking** (MANDATORY):
Use sequential thinking MCP to break down architectural decisions:
- Step 1: Analyze method complexity
- Step 2: Identify extraction boundaries
- Step 3: Design helper method signatures
- Step 4: Validate lock-free compliance
- Step 5: Verify Jane Street alignment

**File Creation Commands** (COPY THESE EXACTLY):

```bash
# Create 02-architecture-plan.md
execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
cat > docs/brain/EPIC-CCN-019/02-architecture-plan.md << 'EOF'
[Your architecture plan content here]
EOF

# Verify file
execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
ls -lh docs/brain/EPIC-CCN-019/02-architecture-plan.md && wc -l docs/brain/EPIC-CCN-019/02-architecture-plan.md

# Update manifest
execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
cat docs/brain/EPIC-CCN-019/manifest.json
```

**CRITICAL**: Only use attempt_completion AFTER file is verified to exist on disk.

EOFMSG

bob --yolo --chat-mode plan "$(cat /tmp/phase2_msg_019.txt)" 2>&1 | tee logs/phase2/EPIC-CCN-019.log
echo "DONE_EXIT=$?"
