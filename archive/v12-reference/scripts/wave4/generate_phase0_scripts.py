#!/usr/bin/env python3
"""
Generate Phase 0 (Hotspot Analysis) scripts for Wave 4 epics.
Based on Wave 2 pattern with --yolo flag and HARDCODED API keys.
Uses 15 API keys in round-robin rotation.
"""

import os
import json

# Load epic roadmap and filter for pending epics 001-080
def load_pending_epics():
    """Load pending epics from epic_roadmap.json (001-080 range)."""
    with open('epic_roadmap.json', 'r') as f:
        data = json.load(f)
    
    # Filter for EPIC-CCN-001 through EPIC-CCN-080 that are not complete
    epics_001_080 = [
        e for e in data 
        if e['epic_number'].startswith('EPIC-CCN-') 
        and 1 <= int(e['epic_number'].split('-')[-1]) <= 80
        and e.get('status') != 'complete'
    ]
    
    # Sort by epic number
    epics_001_080.sort(key=lambda e: int(e['epic_number'].split('-')[-1]))
    
    return epics_001_080

# Load 15 API keys from docs/API/*.json files
API_FILES = [
    "bob.json", "bob (1).json", "bob (2).json", "bob (3).json", 
    "bob (4).json", "bob (5).json", "bob (6).json",
    "b.json", "b (2).json",
    "jessica.json", "mikethelife.json", "sammy96.json", 
    "sean.carter.jr@atomicmail.io.json", "tory.json", "b (3).json"
]

def load_api_keys():
    """Load all 15 API keys from JSON files."""
    api_keys = []
    for api_file in API_FILES:
        json_path = os.path.join("docs", "API", api_file)
        try:
            with open(json_path, 'r') as f:
                data = json.load(f)
                api_keys.append(data['apikey'])
        except Exception as e:
            print(f"[ERROR] Failed to load {api_file}: {e}")
    return api_keys

# Script template based on Wave 2 _p0_107_template.sh
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
- File: {file}
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
- **File**: {file}
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
  "file": "{file}",
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

screen -dmS p0-{epic_num} bash -l -c "bob --yolo --chat-mode v12-phase0-hotspot \\"$(cat /tmp/phase0_msg_{epic_num}.txt)\\" 2>&1 | tee logs/phase0/{epic_id}.log"
echo "DONE_EXIT=$?"
"""

LAUNCHER_TEMPLATE = """#!/bin/bash
# Wave 4 Phase 0 Master Launcher
# Launches all {count} pending epics in screen sessions

set -e
cd /home/malhitticrypto/universal-or-strategy

echo "[*] Launching Wave 4 Phase 0 for {count} epics..."
echo "[*] Using 15 API keys in round-robin rotation"

{launch_commands}

echo "[*] All {count} screen sessions launched"
echo "[*] Monitor with: screen -ls | grep p0-"
echo "[*] Check logs: tail -f logs/phase0/*.log"
"""

def generate_scripts():
    """Generate Phase 0 scripts for all pending epics with API rotation."""
    
    # Load pending epics and API keys
    epics = load_pending_epics()
    api_keys = load_api_keys()
    
    if len(api_keys) != 15:
        print(f"[ERROR] Expected 15 API keys, got {len(api_keys)}")
        return
    
    print(f"[*] Loaded {len(api_keys)} API keys")
    print(f"[*] Found {len(epics)} pending epics in 001-080 range")
    
    output_dir = "scripts/wave4"
    launch_commands = []
    
    for i, epic in enumerate(epics):
        epic_id = epic['epic_number']
        epic_num = epic_id.split('-')[-1]
        method = epic['method']
        file_path = epic['file']
        complexity = epic['cyclomatic']
        
        # Round-robin API key selection
        api_index = i % 15
        api_key = api_keys[api_index]
        
        # Generate script
        script_content = SCRIPT_TEMPLATE.format(
            epic_id=epic_id,
            epic_num=epic_num,
            method=method,
            file=file_path,
            complexity=complexity,
            api_key=api_key
        )
        
        script_path = os.path.join(output_dir, f"_p0_{epic_num}.sh")
        with open(script_path, 'w', encoding='utf-8', newline='\n') as f:
            f.write(script_content)
        
        # Make executable
        os.chmod(script_path, 0o755)
        
        # Add to launcher
        launch_commands.append(f"./_p0_{epic_num}.sh")
        
        print(f"[OK] Generated {script_path} (API {api_index + 1}/15)")
    
    # Generate master launcher
    launcher_content = LAUNCHER_TEMPLATE.format(
        count=len(epics),
        launch_commands='\n'.join(launch_commands)
    )
    
    launcher_path = os.path.join(output_dir, "launch_phase0_all.sh")
    with open(launcher_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(launcher_content)
    
    os.chmod(launcher_path, 0o755)
    
    print(f"\n[OK] Generated {len(epics)} Phase 0 scripts")
    print(f"[OK] Generated master launcher: {launcher_path}")
    print(f"\n[*] API Distribution:")
    for i in range(15):
        count = len([e for idx, e in enumerate(epics) if idx % 15 == i])
        print(f"    API {i+1}: {count} epics")

if __name__ == "__main__":
    generate_scripts()

# Made with Bob
