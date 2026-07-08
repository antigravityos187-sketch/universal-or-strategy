#!/usr/bin/env python3
"""
Generate Wave 4 Phase 0 scripts for all 80 epics.
Uses building-blocks method: copy template, modify epic-specific parameters.
"""

import json
import os
from pathlib import Path

# Load epic roadmap
with open('epic_roadmap_wave4_fresh.json', 'r', encoding='utf-8-sig') as f:
    epics = json.load(f)

# Load API keys (round-robin allocation)
api_dir = Path('docs/API')
api_files = sorted([f for f in api_dir.glob('*.json')])
api_keys = []

for api_file in api_files:
    with open(api_file, 'r') as f:
        api_data = json.load(f)
        api_keys.append(api_data.get('apikey', ''))

print(f"Loaded {len(api_keys)} API keys")
print(f"Loaded {len(epics)} epics")

# Template for Phase 0 script (from _p0_107.sh)
SCRIPT_TEMPLATE = """#!/bin/bash
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
- Complexity: {complexity}

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
- **Cyclomatic Complexity**: {complexity}

## Complexity Metrics
[Include data from get_symbol_complexity]

## Blast Radius
[Include data from get_blast_radius]

## Call Hierarchy
[Include data from get_call_hierarchy]

## Risk Assessment
[LOW/MEDIUM/HIGH based on metrics]
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
  "complexity": {complexity},
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
- File sizes are reasonable (00-hotspots.md should be >100 lines)
- You can see the content with cat/head commands via execute_command

## Success Criteria
- 00-hotspots.md exists and contains hotspot analysis (verify with wc -l)
- manifest.json exists and shows phase 0 completed (verify with cat)
- Both files verified with execute_command shell commands (ls + cat/head)
- No file creation errors

## Critical Reminder
ALWAYS use execute_command with cwd parameter. NEVER use run_shell_command, write_to_file, or read_file in SSH mode.
EOFMSG

bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_{epic_num}.txt)" 2>&1 | tee logs/phase0/{epic_id}.log
echo "DONE_EXIT=$?"
"""

# Generate scripts for all 80 epics
for i, epic in enumerate(epics):
    epic_id = epic['epic_number']
    epic_num = epic_id.split('-')[-1]  # Extract "001" from "EPIC-CCN-001"
    method = epic['method']
    file = epic['file']
    complexity = epic['cyclomatic']
    
    # Round-robin API key allocation
    api_key = api_keys[i % len(api_keys)]
    
    # Generate script content
    script_content = SCRIPT_TEMPLATE.format(
        api_key=api_key,
        epic_id=epic_id,
        epic_num=epic_num,
        method=method,
        file=file,
        complexity=complexity
    )
    
    # Write script file
    script_path = f"_p0_{epic_num}.sh"
    with open(script_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(script_content)
    
    # Make executable
    os.chmod(script_path, 0o755)
    
    print(f"Generated {script_path} for {epic_id} ({method}, CYC={complexity})")

print(f"\n✅ Generated {len(epics)} Phase 0 scripts")
print(f"✅ API rotation: {len(api_keys)} APIs, ~{len(epics)//len(api_keys)} epics per API")

# Made with Bob
