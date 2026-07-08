#!/usr/bin/env python3
"""
Wave 2 Phase 0 Launch with File Verification
Ensures Bob actually writes files to disk and verifies they exist.
"""

import subprocess
import sys
import time
from pathlib import Path

# Epic configuration
EPICS = [
    {"id": "107", "method": "ProcessIpcCommands", "file": "src/V12_002.cs", "cyc": 76},
    {"id": "108", "method": "ProcessOnExecutionUpdate", "file": "src/V12_002.cs", "cyc": 67},
    {"id": "109", "method": "HydrateFSMsFromWorkingOrders", "file": "src/V12_002.cs", "cyc": 45},
    {"id": "110", "method": "HandleFlatPositionUpdate", "file": "src/V12_002.cs", "cyc": 37},
    {"id": "111", "method": "AdoptFleetOrders", "file": "src/V12_002.cs", "cyc": 37},
    {"id": "112", "method": "ExtractTargetConfiguration", "file": "src/V12_002.cs", "cyc": 31},
    {"id": "113", "method": "SweepBrokerOrders", "file": "src/V12_002.cs", "cyc": 28},
    {"id": "114", "method": "FlattenSinglePosition", "file": "src/V12_002.cs", "cyc": 27},
    {"id": "115", "method": "ExecuteRetestEntry", "file": "src/V12_002.cs", "cyc": 26},
]

# API allocation (IMMUTABLE)
API_ALLOCATION = {
    "107": "b (2).json",
    "108": "b.json",
    "109": "bob (1).json",
    "110": "bob (2).json",
    "111": "bob (3).json",
    "112": "bob (4).json",
    "113": "bob (5).json",
    "114": "bob (6).json",
    "115": "bob.json",
}

VM_NAME = "v12-test-golden-v2"
ZONE = "us-central1-a"
REPO_PATH = "/home/malhitticrypto/universal-or-strategy"

def create_phase0_script_with_verification(epic_id: str, method: str, file: str, cyc: int, api_file: str) -> str:
    """Generate Phase 0 script with explicit file write verification."""
    return f"""#!/bin/bash
set -e

cd {REPO_PATH}

# Set API key
export BOB_API_KEY_FILE="$HOME/.bob/api-keys/{api_file}"

# Create epic directory
mkdir -p docs/brain/EPIC-CCN-{epic_id}

# Execute Phase 0 with MANDATORY file write verification
bob --mode advanced \\
    --message "Execute Phase 0 (Hotspot Analysis) for EPIC-CCN-{epic_id}.

**CRITICAL**: You MUST write files to disk and verify they exist.

## Target Method
- Method: {method}
- File: {file}
- Complexity: {cyc}

## Required Actions

### Step 1: Use jCodemunch to gather data
Use these jCodemunch tools:
1. get_hotspots(repo='universal-or-strategy', top_n=50)
2. get_blast_radius(repo='universal-or-strategy', symbol='{method}')
3. get_call_hierarchy(repo='universal-or-strategy', symbol_id='{method}')
4. get_symbol_complexity(repo='universal-or-strategy', symbol_id='{method}')

### Step 2: Write 00-hotspots.md
Use write_to_file tool to create docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md with:
- Method signature and location
- Complexity metrics (cyclomatic, nesting, parameters)
- Blast radius (files affected, importers)
- Call hierarchy (callers and callees)
- Risk assessment (LOW/MEDIUM/HIGH)

### Step 3: Write manifest.json
Use write_to_file tool to create docs/brain/EPIC-CCN-{epic_id}/manifest.json:
```json
{{
  "epic_id": "EPIC-CCN-{epic_id}",
  "method": "{method}",
  "file": "{file}",
  "complexity": {cyc},
  "phases": {{
    "0": {{
      "status": "completed",
      "outputs": ["00-hotspots.md"]
    }}
  }}
}}
```

### Step 4: VERIFY files exist
Use read_file tool to verify BOTH files were created:
1. read_file docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md
2. read_file docs/brain/EPIC-CCN-{epic_id}/manifest.json

If either file is missing, CREATE IT AGAIN.

### Step 5: Confirm completion
Only use attempt_completion when BOTH files exist and you've verified them with read_file.

## Success Criteria
- 00-hotspots.md exists and contains hotspot analysis
- manifest.json exists and shows phase 0 completed
- Both files verified with read_file tool
- No file creation errors"

echo "DONE_EXIT=$?"
"""

def launch_phase0():
    """Launch Phase 0 for all 9 epics."""
    print("=" * 80)
    print("Wave 2 Phase 0 Launch (With File Verification)")
    print("=" * 80)
    print(f"Epics: {len(EPICS)}")
    print(f"VM: {VM_NAME}")
    print()
    
    # Validate API allocation
    api_values = list(API_ALLOCATION.values())
    if len(api_values) != len(set(api_values)):
        duplicates = [x for x in api_values if api_values.count(x) > 1]
        print(f"❌ ERROR: Duplicate API keys: {duplicates}")
        sys.exit(1)
    print(f"[OK] Validated {len(api_values)} unique API keys")
    print()
    
    # Create scripts
    scripts_created = []
    for epic in EPICS:
        epic_id = epic["id"]
        api_file = API_ALLOCATION[epic_id]
        
        script_content = create_phase0_script_with_verification(
            epic_id, epic["method"], epic["file"], epic["cyc"], api_file
        )
        
        local_script = f"_phase0_epic_{epic_id}_verified.sh"
        with open(local_script, "w", newline="\n") as f:
            f.write(script_content)
        
        scripts_created.append(local_script)
        print(f"[OK] Created {local_script}")
    
    print()
    print("Uploading scripts...")
    
    for script in scripts_created:
        subprocess.run([
            "gcloud", "compute", "scp",
            script,
            f"{VM_NAME}:{REPO_PATH}/{script}",
            f"--zone={ZONE}"
        ], check=True)
        print(f"✓ Uploaded {script}")
    
    print()
    print("Launching agents...")
    
    for epic in EPICS:
        epic_id = epic["id"]
        script_name = f"_phase0_epic_{epic_id}_verified.sh"
        
        launch_cmd = (
            f"cd {REPO_PATH} && "
            f"chmod +x {script_name} && "
            f"screen -dmS phase0-{epic_id} bash -l -c "
            f"'./{script_name} 2>&1 | tee logs/phase0/EPIC-CCN-{epic_id}.log'"
        )
        
        subprocess.run([
            "gcloud", "compute", "ssh", VM_NAME,
            f"--zone={ZONE}",
            f"--command={launch_cmd}"
        ], check=True)
        
        print(f"[OK] Launched EPIC-CCN-{epic_id}")
        time.sleep(0.5)
    
    print()
    print("=" * 80)
    print("[SUCCESS] Phase 0 Launch Complete!")
    print("=" * 80)
    print()
    print("Monitor: gcloud compute ssh", VM_NAME, f"--zone={ZONE}")
    print("Logs: tail -f logs/phase0/EPIC-CCN-107.log")

if __name__ == "__main__":
    launch_phase0()

# Made with Bob
