#!/usr/bin/env python3
"""
Generate Phase 1 scripts by copying Phase 0 pattern.
Building-blocks method: Copy Phase 0, find-and-replace phase-specific changes only.
"""

import json
import os
from pathlib import Path

# Load API keys
api_keys_file = Path("docs/API")
api_files = sorted([f for f in api_keys_file.glob("*.json") if f.stem not in ["b", "bob"]])

api_keys = []
for api_file in api_files[:15]:  # Use first 15 APIs
    with open(api_file) as f:
        data = json.load(f)
        api_keys.append(data["apikey"])

print(f"Loaded {len(api_keys)} API keys")

# Load epic roadmap
with open("epic_roadmap_wave4_fresh.json", encoding="utf-8-sig") as f:
    epics = json.load(f)  # Already a list of 80 epics

print(f"Loaded {len(epics)} epics")

# Create output directory
output_dir = Path("scripts/wave4")
output_dir.mkdir(exist_ok=True)

# Generate Phase 1 scripts by copying Phase 0 pattern
for i, epic in enumerate(epics):
    epic_id = epic["epic_number"]  # e.g., "EPIC-CCN-001"
    epic_num = epic_id.split("-")[-1]  # e.g., "001"
    method = epic["method"]
    file_path = epic["file"]
    cyc = epic["cyclomatic"]
    
    # Rotate API keys
    api_key = api_keys[i % len(api_keys)]
    
    # Read Phase 0 template
    phase0_script = f"scripts/wave4/_p0_{epic_num}.sh"
    if not Path(phase0_script).exists():
        print(f"Warning: Phase 0 script not found: {phase0_script}")
        continue
    
    with open(phase0_script, encoding='utf-8') as f:
        script_content = f.read()
    
    # Find-and-replace for Phase 1
    script_content = script_content.replace("phase0", "phase1")
    script_content = script_content.replace("Phase 0", "Phase 1")
    script_content = script_content.replace("PHASE 0", "PHASE 1")
    script_content = script_content.replace("Hotspot Analysis", "Scope Definition + Boundary Validation")
    script_content = script_content.replace("v12-phase0-hotspot", "plan")
    script_content = script_content.replace("v12-phase1-hotspot", "plan")  # Fix mode name
    script_content = script_content.replace("epic-intake", "epic-scope-boundary")
    script_content = script_content.replace("00-hotspots.md", "01-scope.md and 01-scope-boundary.md")
    script_content = script_content.replace("/tmp/phase0_msg", "/tmp/phase1_msg")
    
    # Update prompt section (find the EOFMSG block and replace)
    # This is the key phase-specific change
    prompt_start = script_content.find("cat > /tmp/phase1_msg")
    prompt_end = script_content.find("EOFMSG", prompt_start + 100)
    
    if prompt_start != -1 and prompt_end != -1:
        new_prompt = f"""cat > /tmp/phase1_msg_{epic_num}.txt << 'EOFMSG'
Execute Phase 1 (Scope Definition + Boundary Validation) for {epic_id}.

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

**Input**: Read `docs/brain/{epic_id}/00-hotspots.md`

**Target Method**:
- Method: {method}
- File: {file_path}
- Complexity: {cyc}

**Phase 1.0: Scope Definition**

Create `docs/brain/{epic_id}/01-scope.md` with:

1. **Extraction Scope** (SINGLE METHOD ONLY):
   - Method name: {method}
   - Current complexity: {cyc}
   - Target complexity: ≤8 (Jane Street strict standard)
   - Extraction strategy: Break into 2-3 helper methods

2. **Boundary Definition**:
   - What's IN scope: {method} body only
   - What's OUT of scope: Callers, callees, other methods in same file
   - No scope creep: ONE EPIC = ONE CONCERN

3. **Success Criteria**:
   - Complexity reduced from {cyc} to ≤8
   - All tests pass
   - No behavior changes
   - Lock-free Actor/FSM pattern maintained

**Phase 1.5: Boundary Validation** (V12.23 Protocol - MANDATORY)

Create `docs/brain/{epic_id}/01-scope-boundary.md` with:

1. **Boundary Check**:
   - ✅ Scope limited to single method: {method}
   - ✅ No changes to callers
   - ✅ No changes to callees
   - ✅ No changes to other methods in {file_path}

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
cat > docs/brain/{epic_id}/01-scope.md << 'EOF'
[Your scope definition content here]
EOF

# Create 01-scope-boundary.md
execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
cat > docs/brain/{epic_id}/01-scope-boundary.md << 'EOF'
[Your boundary validation content here]
EOF

# Verify files
execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
ls -lh docs/brain/{epic_id}/01-scope.md && wc -l docs/brain/{epic_id}/01-scope.md

execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
ls -lh docs/brain/{epic_id}/01-scope-boundary.md && wc -l docs/brain/{epic_id}/01-scope-boundary.md

# Update manifest
execute_command with cwd=/home/malhitticrypto/universal-or-strategy:
cat docs/brain/{epic_id}/manifest.json
```

**CRITICAL**: Only use attempt_completion AFTER both files are verified to exist on disk.

EOFMSG"""
        
        script_content = script_content[:prompt_start] + new_prompt + script_content[prompt_end + 6:]
    
    # Write Phase 1 script
    output_file = output_dir / f"_p1_{epic_num}.sh"
    with open(output_file, "w", encoding="utf-8") as f:
        f.write(script_content)
    
    os.chmod(output_file, 0o755)
    
    if (i + 1) % 10 == 0:
        print(f"Generated {i + 1}/{len(epics)} Phase 1 scripts...")

print(f"\n✅ Generated {len(epics)} Phase 1 scripts in {output_dir}")
print(f"✅ Pattern: Copied Phase 0, replaced phase-specific content only")
print(f"✅ Next: Upload to VM and pilot test EPIC-CCN-001")

# Made with Bob
