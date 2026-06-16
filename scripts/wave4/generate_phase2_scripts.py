#!/usr/bin/env python3
"""
Generate Phase 2 (Architecture Planning) scripts for Wave 4.

Uses building-blocks method: Copy Phase 1 scripts and modify for Phase 2.
NEVER generate from scratch - always copy working phase.

Reference: docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md
"""

import json
import os

# Load epic roadmap
with open('epic_roadmap_wave4_fresh.json', 'r', encoding='utf-8-sig') as f:
    roadmap = json.load(f)

# Load API keys
api_files = [
    'docs/API/bob.json',
    'docs/API/bob (1).json',
    'docs/API/bob (2).json',
    'docs/API/bob (3).json',
    'docs/API/bob (4).json',
    'docs/API/bob (5).json',
    'docs/API/bob (6).json',
    'docs/API/jessica.json',
    'docs/API/mikethelife.json',
    'docs/API/sammy96.json',
    'docs/API/sean.carter.jr@atomicmail.io.json',
    'docs/API/tory.json',
    'docs/API/b.json',
    'docs/API/b (2).json',
    'docs/API/b (3).json'
]

api_keys = []
for api_file in api_files:
    with open(api_file, 'r') as f:
        data = json.load(f)
        api_keys.append(data['apikey'])

print(f"Loaded {len(api_keys)} API keys")
print(f"Loaded {len(roadmap)} epics from roadmap")

# Create output directory
os.makedirs('scripts/wave4', exist_ok=True)

# Generate Phase 2 scripts (copy Phase 1 pattern)
for i, epic in enumerate(roadmap):
    epic_num = epic['epic_number']
    epic_id = epic_num.split('-')[-1]  # Extract "001" from "EPIC-CCN-001"
    method = epic['method']
    file = epic['file']
    cyc = epic['cyclomatic']
    loc = epic['loc']
    tier = epic['tier']
    
    # Round-robin API assignment
    api_key = api_keys[i % len(api_keys)]
    
    # Generate script (COPY Phase 1 pattern, modify for Phase 2)
    script_content = f"""#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/{epic_num}
mkdir -p logs/phase2

cat > /tmp/phase2_msg_{epic_id}.txt << 'EOFMSG'
Execute Phase 2 (Architecture Planning) for {epic_num}.

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

**Input**: Read `docs/brain/{epic_num}/01-scope-boundary.md`

**Target Method**:
- Method: {method}
- File: {file}
- Complexity: {cyc}
- LOC: {loc}
- Tier: {tier}

**Phase 2: Architecture Planning**

Create `docs/brain/{epic_num}/02-architecture-plan.md` with:

1. **Extraction Strategy**:
   - Current method: {method}
   - Current complexity: {cyc}
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
cat > docs/brain/{epic_num}/02-architecture-plan.md << 'EOF'
[Your architecture plan content here]
EOF

# Verify file
execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
ls -lh docs/brain/{epic_num}/02-architecture-plan.md && wc -l docs/brain/{epic_num}/02-architecture-plan.md

# Update manifest
execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
cat docs/brain/{epic_num}/manifest.json
```

**CRITICAL**: Only use attempt_completion AFTER file is verified to exist on disk.

EOFMSG

bob --yolo --chat-mode plan "$(cat /tmp/phase2_msg_{epic_id}.txt)" 2>&1 | tee logs/phase2/{epic_num}.log
echo "DONE_EXIT=$?"
"""
    
    # Write script
    script_path = f'scripts/wave4/_p2_{epic_id}.sh'
    with open(script_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(script_content)
    
    print(f"Generated {script_path}")

print(f"\nGenerated {len(roadmap)} Phase 2 scripts")
print(f"Location: scripts/wave4/_p2_001.sh through _p2_080.sh")
print(f"\nNEXT STEP: Fix delay bug in master launch script (constant 12s, not incrementing)")

# Made with Bob
