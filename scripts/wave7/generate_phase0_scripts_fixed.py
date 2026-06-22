#!/usr/bin/env python3
"""
Generate Phase 0 (Hotspot Analysis) scripts for Wave 7 epics.
FIXED VERSION: Uses Python file writing instead of bash heredocs to avoid syntax errors.

Wave 7 Changes from Wave 4:
- Epic format: EPIC-W7-XXX (not EPIC-CCN-XXX)
- Epic count: 161 (not 80)
- Source: epic_roadmap_wave7.json (not epic_roadmap.json)
- Template: building-blocks/wave7/phase0_template_wave7.sh
- FIX: No nested heredocs (causes bash syntax errors in screen sessions)
"""

import os
import json

# Load epic roadmap for Wave 7
def load_pending_epics():
    """Load pending epics from epic_roadmap_wave7.json (all 161 epics)."""
    with open('epic_roadmap_wave7.json', 'r') as f:
        data = json.load(f)
    
    # Extract epics from the "epics" dictionary
    epics_list = []
    for epic_id, epic_data in data['epics'].items():
        if epic_data.get('status') != 'complete':
            epics_list.append({
                'epic_number': epic_id,
                'method': epic_data['method'],
                'file': epic_data['file'],
                'cyclomatic': epic_data['cyc_before']
            })
    
    # Sort by epic number (EPIC-W7-001, EPIC-W7-002, etc.)
    epics_list.sort(key=lambda e: int(e['epic_number'].split('-')[-1]))
    
    return epics_list

# Load 20 API keys from docs/API/*.json files (verified on VM)
# Updated 2026-06-22: Removed exhausted "b.json", added "danfarah.json" and "snyder.johnson.json"
# Strategy: Use ALL 20 keys for even distribution (161 epics ÷ 20 = 8.05 epics per key)
API_FILES = [
    "bob.json", "bob (1).json", "bob (2).json", "pepeescobar.json",
    "bob (4).json", "bob (5).json", "bob (6).json",
    "danfarah.json", "snyder.johnson.json",  # Fresh keys (replaced exhausted b.json)
    "b (3).json",
    "jessica.json", "mikethelife.json", "sammy96.json",
    "sean.carter.jr@atomicmail.io.json", "tory.json", "iyanajackson.json",
    "alprofit.json", "rakaarababa.json", "ranirabah (1).json", "jimmydore.json"
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

# Bob CLI message content (will be written to temp file by Python, not bash heredoc)
def get_bob_message(epic_id, method, file_path, complexity):
    """Generate Bob CLI message content."""
    return f"""Execute Phase 0 (Hotspot Analysis) for {epic_id}.

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
- Complexity: {complexity}

## Required Actions

### Step 1: Use jCodemunch to gather data
Use these jCodemunch tools:
1. get_hotspots(repo='universal-or-strategy', top_n=50)
2. get_blast_radius(repo='universal-or-strategy', symbol='{method}')
3. get_call_hierarchy(repo='universal-or-strategy', symbol_id='{method}')
4. get_symbol_complexity(repo='universal-or-strategy', symbol_id='{method}')

### Step 2: Write 00-hotspots.md using execute_command
Create the hotspot analysis file with actual data from jCodemunch tools.

### Step 3: Write manifest.json using execute_command
Create the manifest file marking Phase 0 as completed.

### Step 4: VERIFY files exist using execute_command
Verify BOTH files were created with ls and cat commands.

### Step 5: Confirm completion
Only use attempt_completion when both files exist and are verified.

## Success Criteria
- 00-hotspots.md exists and contains hotspot analysis (verify with wc -l)
- manifest.json exists and shows phase 0 completed (verify with cat)
- Both files verified with execute_command shell commands (ls + cat/head)
- No file creation errors

## Critical Reminder
ALWAYS use execute_command with cwd parameter. NEVER use run_shell_command, write_to_file, or read_file in SSH mode.
"""

# Script template - NO HEREDOCS, uses Python to write message file
SCRIPT_TEMPLATE = """#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/{epic_id}
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_{epic_num}.txt)" 2>&1 | tee logs/phase0/{epic_id}.log
echo "DONE_EXIT=$?"
"""

def generate_scripts(failed_only=False, failed_list=None):
    """Generate Phase 0 scripts for Wave 7 epics with API rotation.
    
    Args:
        failed_only: If True, only regenerate scripts for failed epics
        failed_list: List of failed epic numbers (e.g., ['002', '003', '008'])
    """
    
    # Load pending epics and API keys
    epics = load_pending_epics()
    api_keys = load_api_keys()
    
    if len(api_keys) != 20:
        print(f"[ERROR] Expected 20 API keys, got {len(api_keys)}")
        return
    
    print(f"[*] Loaded {len(api_keys)} API keys")
    print(f"[*] Found {len(epics)} pending epics for Wave 7")
    
    # Filter to failed epics if requested
    if failed_only and failed_list:
        epics = [e for e in epics if e['epic_number'].split('-')[-1] in failed_list]
        print(f"[*] Regenerating {len(epics)} failed epics only")
    
    output_dir = "scripts/wave7"
    msg_dir = "/tmp"  # Message files go to /tmp on VM
    
    for i, epic in enumerate(epics):
        epic_id = epic['epic_number']
        epic_num = epic_id.split('-')[-1]  # Extract "001" from "EPIC-W7-001"
        method = epic['method']
        file_path = epic['file']
        complexity = epic['cyclomatic']
        
        # Round-robin API key selection (20 keys for even distribution)
        api_index = i % 20
        api_key = api_keys[api_index]
        
        # Generate Bob CLI message content
        message_content = get_bob_message(epic_id, method, file_path, complexity)
        
        # Write message file (Python writes it, not bash heredoc)
        msg_path = os.path.join(msg_dir, f"phase0_msg_{epic_num}.txt")
        with open(msg_path, 'w', encoding='utf-8', newline='\n') as f:
            f.write(message_content)
        
        # Generate script (no heredocs, just references the message file)
        script_content = SCRIPT_TEMPLATE.format(
            epic_id=epic_id,
            epic_num=epic_num,
            api_key=api_key
        )
        
        script_path = os.path.join(output_dir, f"_p0_{epic_num}.sh")
        with open(script_path, 'w', encoding='utf-8', newline='\n') as f:
            f.write(script_content)
        
        # Make executable
        os.chmod(script_path, 0o755)
        
        print(f"[OK] Generated {script_path} + {msg_path} (API {api_index + 1}/15)")
    
    print(f"\n[OK] Generated {len(epics)} Phase 0 scripts")
    print(f"\n[*] API Distribution (20 keys, ~8 epics each):")
    for i in range(20):
        count = len([e for idx, e in enumerate(epics) if idx % 20 == i])
        key_name = API_FILES[i].replace('.json', '')
        print(f"    API {i+1:2d} ({key_name:30s}): {count} epics")
    
    if not failed_only:
        print(f"\n[*] Next steps:")
        print(f"    1. Run pilot: ./scripts/wave7/launch_phase0_pilot.sh")
        print(f"    2. Verify pilot success (3 epics)")
        print(f"    3. Run full wave: ./scripts/wave7/launch_phase0_all.sh")
    else:
        print(f"\n[*] Next steps:")
        print(f"    1. Deploy to VM: git add . && git commit -m 'Fix Phase 0 scripts' && git push")
        print(f"    2. Pull on VM: git pull")
        print(f"    3. Run recovery: ./scripts/wave7/recover_failed_phase0.sh")

if __name__ == "__main__":
    import sys
    if len(sys.argv) > 1 and sys.argv[1] == "--failed-only":
        # Read failed epic list from file
        try:
            with open('scripts/wave7/failed_epics_phase0.txt', 'r') as f:
                failed_list = [line.strip() for line in f if line.strip()]
            generate_scripts(failed_only=True, failed_list=failed_list)
        except FileNotFoundError:
            print("[ERROR] failed_epics_phase0.txt not found. Run recovery script first.")
    else:
        generate_scripts()

# Made with Bob - Building-Blocks Method (fixed heredoc issue)