#!/usr/bin/env python3
"""
Wave 3 Phase 0 Script Generator
Following WAVE_PHASE_SCRIPT_GENERATION_SOP.md

CRITICAL RULE: Copy working Wave 2 Phase 0 scripts, don't generate from scratch.
"""

import json
import os
from pathlib import Path

# Wave 3 Epic Configuration
WAVE3_EPICS = [
    {
        "id": "116",
        "method": "HandleFlatPosition_CleanupActivePositions",
        "file": "src/V12_002.Orders.Callbacks.Execution.cs",
        "cyc": 17,
        "loc": 30,
        "api_file": "b (2).json"  # Reuse Wave 2 API (has balance)
    },
    {
        "id": "117",
        "method": "SyncLimitTarget",
        "file": "src/V12_002.Orders.Management.StopSync.cs",
        "cyc": 17,
        "loc": 128,
        "api_file": "b.json"
    },
    {
        "id": "118",
        "method": "ProcessSingleFleetRMAAccount",
        "file": "src/V12_002.SIMA.Execution.cs",
        "cyc": 16,
        "loc": 85,  # Estimated
        "api_file": "bob (1).json"
    },
    {
        "id": "119",
        "method": "EmergencyFlattenSingleFleetAccount",
        "file": "src/V12_002.SIMA.Flatten.cs",
        "cyc": 16,
        "loc": 73,
        "api_file": "bob (2).json"
    },
    {
        "id": "120",
        "method": "AuditMaster_HandleNakedPosition",
        "file": "src/V12_002.REAPER.Audit.cs",
        "cyc": 15,
        "loc": 38,
        "api_file": "bob (3).json"
    },
    {
        "id": "121",
        "method": "ProcessQueuedAccountOrder",
        "file": "src/V12_002.Orders.Callbacks.AccountOrders.cs",
        "cyc": 15,
        "loc": 34,
        "api_file": "bob (4).json"
    },
    {
        "id": "122",
        "method": "TBD_FromComplexityAudit",
        "file": "TBD",
        "cyc": 14,
        "loc": 50,  # Estimated
        "api_file": "bob (5).json"
    },
    {
        "id": "123",
        "method": "TBD_FromComplexityAudit",
        "file": "TBD",
        "cyc": 13,
        "loc": 45,  # Estimated
        "api_file": "bob (6).json"
    },
    {
        "id": "124",
        "method": "TBD_FromComplexityAudit",
        "file": "TBD",
        "cyc": 12,
        "loc": 40,  # Estimated
        "api_file": "bob.json"
    },
    {
        "id": "125",
        "method": "TBD_FromComplexityAudit",
        "file": "TBD",
        "cyc": 11,
        "loc": 35,  # Estimated
        "api_file": "sean.carter.jr@atomicmail.io.json"  # Reserve API
    }
]

def load_api_key(api_file):
    """Load API key from JSON file."""
    json_path = Path("docs/API") / api_file
    with open(json_path, 'r') as f:
        data = json.load(f)
        return data['apikey']

def generate_phase0_script(epic):
    """Generate Phase 0 script by copying Wave 2 template."""
    epic_id = epic['id']
    method = epic['method']
    file_path = epic['file']
    cyc = epic['cyc']
    api_key = load_api_key(epic['api_file'])
    
    # CRITICAL: Copy Wave 2 Phase 0 pattern exactly
    script_content = f"""#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/EPIC-CCN-{epic_id}
mkdir -p logs/phase0

cat > /tmp/phase0_msg_{epic_id}.txt << 'EOFMSG'
Execute Phase 0 (Hotspot Analysis) for EPIC-CCN-{epic_id}.

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
- File: {file_path}
- Complexity: {cyc}

## Required Actions

### Step 1: Use jCodemunch to gather data
Use these jCodemunch tools:
1. get_hotspots(repo='universal-or-strategy', top_n=50)
2. get_blast_radius(repo='universal-or-strategy', symbol='{method}')
3. get_call_hierarchy(repo='universal-or-strategy', symbol_id='{method}')
4. get_symbol_complexity(repo='universal-or-strategy', symbol_id='{method}')

### Step 2: Write 00-hotspots.md using execute_command
Use execute_command (NOT run_shell_command) to create docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md:

```xml
<execute_command>
<command>
cat > docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md << 'EOF'
# Phase 0: Hotspot Analysis - EPIC-CCN-{epic_id}

## Target Method
- **Method**: {method}
- **File**: {file_path}
- **Cyclomatic Complexity**: {cyc}

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
Use execute_command (NOT run_shell_command) to create docs/brain/EPIC-CCN-{epic_id}/manifest.json:

```xml
<execute_command>
<command>
cat > docs/brain/EPIC-CCN-{epic_id}/manifest.json << 'EOF'
{{
  "epic_id": "EPIC-CCN-{epic_id}",
  "method": "{method}",
  "file": "{file_path}",
  "complexity": {cyc},
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
ls -lh docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md && wc -l docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

2. Verify manifest.json:
```xml
<execute_command>
<command>
ls -lh docs/brain/EPIC-CCN-{epic_id}/manifest.json && cat docs/brain/EPIC-CCN-{epic_id}/manifest.json | head -20</command>
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

**MANDATORY REPORTING**:
After completing all tasks, you MUST report:
1. Bobcoins used this session: [X.XX]
2. Remaining balance in API key: [Y.YY]

Format: "Cost: X.XX | Balance: Y.YY"
EOFMSG

bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_{epic_id}.txt)" 2>&1 | tee logs/phase0/EPIC-CCN-{epic_id}.log
echo "DONE_EXIT=$?"

# Made with Bob
"""
    
    return script_content

def generate_launcher_script():
    """Generate launcher script for all Phase 0 epics."""
    launcher_content = """#!/bin/bash
# Wave 3 Phase 0 Launcher
# Launches all 10 epics in parallel using screen sessions

set -e
cd /home/malhitticrypto/universal-or-strategy

echo "Starting Wave 3 Phase 0 (10 epics)..."

# Launch each epic in a screen session
"""
    
    for epic in WAVE3_EPICS:
        epic_id = epic['id']
        launcher_content += f"""
screen -dmS "p0-{epic_id}" bash -l "_p0_{epic_id}.sh"
echo "Launched EPIC-CCN-{epic_id} in screen session p0-{epic_id}"
"""
    
    launcher_content += """
echo ""
echo "All 10 epics launched!"
echo "Monitor with: screen -ls"
echo "Attach to session: screen -r p0-116"
echo "Detach from session: Ctrl+A, then D"
echo ""
echo "Check completion: screen -ls (should show 'No Sockets found' when done)"
echo "Verify files: ls docs/brain/EPIC-CCN-*/00-hotspots.md | wc -l (should show 10)"
"""
    
    return launcher_content

def main():
    """Generate all Wave 3 Phase 0 scripts."""
    output_dir = Path("scripts/wave3")
    output_dir.mkdir(exist_ok=True)
    
    print("Generating Wave 3 Phase 0 scripts...")
    print(f"Output directory: {output_dir}")
    print()
    
    # Generate individual epic scripts
    for epic in WAVE3_EPICS:
        epic_id = epic['id']
        script_path = output_dir / f"_p0_{epic_id}.sh"
        
        script_content = generate_phase0_script(epic)
        
        with open(script_path, 'w', encoding='utf-8', newline='\n') as f:
            f.write(script_content)
        
        print(f"[OK] Generated: {script_path}")
    
    # Generate launcher script
    launcher_path = output_dir / "launch_phase0_all_screen.sh"
    launcher_content = generate_launcher_script()
    
    with open(launcher_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(launcher_content)
    
    print(f"[OK] Generated: {launcher_path}")
    print()
    print("=" * 60)
    print("Wave 3 Phase 0 scripts generated successfully!")
    print("=" * 60)
    print()
    print("Next steps:")
    print("1. Upload scripts to VM:")
    print("   gcloud compute scp scripts/wave3/_p0_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a")
    print("   gcloud compute scp scripts/wave3/launch_phase0_all_screen.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a")
    print()
    print("2. Make scripts executable:")
    print("   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='chmod +x /home/malhitticrypto/universal-or-strategy/_p0_*.sh /home/malhitticrypto/universal-or-strategy/launch_phase0_all_screen.sh'")
    print()
    print("3. Launch Wave 3 Phase 0:")
    print("   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='/home/malhitticrypto/universal-or-strategy/launch_phase0_all_screen.sh'")
    print()
    print("4. Monitor execution:")
    print("   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='screen -ls'")
    print()

if __name__ == "__main__":
    main()