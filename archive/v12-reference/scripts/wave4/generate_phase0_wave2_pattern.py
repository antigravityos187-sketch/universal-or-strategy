#!/usr/bin/env python3
"""
Generate Phase 0 scripts for Wave 4 using Wave 2 pattern.
FOLLOWS SOP: Copy same phase from previous wave, only change epic numbers.
"""

import json
from pathlib import Path

# Load epic roadmap
roadmap_path = Path(__file__).parent.parent.parent / "epic_roadmap_wave4_fresh.json"
with open(roadmap_path, 'r', encoding='utf-8-sig') as f:
    epics = json.load(f)

# Load API keys
api_keys_path = Path(__file__).parent.parent.parent / "docs" / "API"
api_files = [
    "bob.json", "bob (1).json", "bob (2).json", "bob (3).json", "bob (4).json",
    "bob (5).json", "bob (6).json", "jessica.json", "mikethelife.json",
    "sammy96.json", "sean.carter.jr@atomicmail.io.json", "tory.json",
    "b.json", "b (2).json", "b (3).json"
]

api_keys = []
for api_file in api_files:
    api_path = api_keys_path / api_file
    if api_path.exists():
        with open(api_path, 'r') as f:
            data = json.load(f)
            api_keys.append(data['apikey'])

print(f"Loaded {len(api_keys)} API keys")
print(f"Loaded {len(epics)} epics")

# Generate individual epic scripts using Wave 2 pattern
output_dir = Path(__file__).parent
output_dir.mkdir(exist_ok=True)

for i, epic in enumerate(epics, 1):
    epic_id = epic['epic_number']
    epic_num = epic_id.split('-')[-1]  # Extract "001" from "EPIC-CCN-001"
    method = epic['method']
    file = epic['file']
    cyc = epic['cyclomatic']
    
    # Rotate API keys
    api_key = api_keys[i % len(api_keys)]
    
    # Wave 2 pattern: Bob CLI with message file
    script_content = f"""#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/{epic_id}
mkdir -p logs/phase0

cat > /tmp/phase0_msg_{epic_num}.txt << 'EOFMSG'
Execute Phase 0 (Hotspot Analysis) for {epic_id}.

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

## Target Method
- Method: {method}
- File: src/{file}
- Complexity: {cyc}

## Jane Street Validation (NEW for Wave 4)

Before creating 00-hotspots.md, check for Jane Street P0 violations:
1. Use execute_command to read jane_street_p0_violations.json
2. Filter violations by file: src/{file}
3. Count violations in the target method's line range (if known)
4. Include violation count in 00-hotspots.md
5. Elevate risk to HIGH if violations >5

Example command to check violations:
```xml
<execute_command>
<command>
grep -i "{file}" jane_street_p0_violations.json | wc -l
</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

## Required Actions

### Step 1: Use jCodemunch to gather data
Use these jCodemunch tools:
1. get_hotspots(repo='universal-or-strategy', top_n=50)
2. get_blast_radius(repo='universal-or-strategy', symbol='{method}')
3. get_call_hierarchy(repo='universal-or-strategy', symbol_id='{method}')
4. get_symbol_complexity(repo='universal-or-strategy', symbol_id='{method}')

### Step 2: Write 00-hotspots.md using execute_command
Use execute_command (NOT run_shell_command) to create docs/brain/{epic_id}/00-hotspots.md:

```xml
<execute_command>
<command>
cat > docs/brain/{epic_id}/00-hotspots.md << 'EOF'
# Phase 0: Hotspot Analysis - {epic_id}

## Target Method
- **Method**: {method}
- **File**: src/{file}
- **Cyclomatic Complexity**: {cyc}
- **Jane Street Violations**: [COUNT FROM STEP ABOVE]

## Complexity Metrics
[Include data from get_symbol_complexity]

## Blast Radius
[Include data from get_blast_radius]

## Call Hierarchy
[Include data from get_call_hierarchy]

## Risk Assessment
- **Complexity Risk**: [HIGH if cyc >20, MEDIUM if cyc >15, else LOW]
- **Jane Street Risk**: [HIGH if violations >5, MEDIUM if violations >0, else LOW]
- **Overall Risk**: [Highest of the two above]
EOF</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

### Step 3: Write manifest.json using execute_command
Use execute_command (NOT run_shell_command) to create docs/brain/{epic_id}/manifest.json:

```xml
<execute_command>
<command>
cat > docs/brain/{epic_id}/manifest.json << 'EOF'
{{
  "epic_id": "{epic_id}",
  "method": "{method}",
  "file": "src/{file}",
  "complexity": {cyc},
  "jane_street_violations": [COUNT],
  "phases": {{
    "0": {{
      "status": "completed",
      "outputs": ["00-hotspots.md"]
    }}
  }}
}}
EOF</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

### Step 4: VERIFY files exist using execute_command
Use execute_command (NOT run_shell_command) to verify BOTH files were created:

1. Verify 00-hotspots.md:
```xml
<execute_command>
<command>
ls -lh docs/brain/{epic_id}/00-hotspots.md && wc -l docs/brain/{epic_id}/00-hotspots.md</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

2. Verify manifest.json:
```xml
<execute_command>
<command>
ls -lh docs/brain/{epic_id}/manifest.json && cat docs/brain/{epic_id}/manifest.json | head -20</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

If either file is missing, CREATE IT AGAIN using execute_command (NOT run_shell_command).

### Step 5: Confirm completion
Only use attempt_completion when:
- BOTH files exist (verified with ls command via execute_command)
- File sizes are reasonable (00-hotspots.md should be >50 lines)
- You can see the content with cat/head commands via execute_command
- Jane Street violation count is included in both files

## Success Criteria
- 00-hotspots.md exists and contains hotspot analysis (verify with wc -l)
- manifest.json exists and shows phase 0 completed (verify with cat)
- Both files include Jane Street violation count
- Both files verified with execute_command shell commands (ls + cat/head)
- No file creation errors

## Critical Reminder
ALWAYS use execute_command with cwd parameter. NEVER use run_shell_command, write_to_file, or read_file in SSH mode.
EOFMSG

bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_{epic_num}.txt)" 2>&1 | tee logs/phase0/{epic_id}.log
echo "DONE_EXIT=$?"
"""
    
    # Write script (UTF-8 encoding for emoji support)
    script_path = output_dir / f"_p0_{epic_num}.sh"
    with open(script_path, 'w', newline='\n', encoding='utf-8') as f:
        f.write(script_content)
    
    print(f"Created {script_path.name}")

print(f"\nGenerated {len(epics)} Phase 0 scripts using Wave 2 pattern")
print("\nKey differences from previous attempt:")
print("- Uses Bob CLI (not Python wrapper)")
print("- Uses v12-phase0-hotspot mode (proven in Wave 2)")
print("- Uses message file pattern (/tmp/phase0_msg_X.txt)")
print("- Adds Jane Street validation to prompt")
print("- Follows building-blocks method (copied Wave 2 pattern)")

# Made with Bob
