#!/usr/bin/env python3
"""
Wave 2 Phase 0 Launch (Fixed - Uses Message Files)
"""

import subprocess
import sys
import time
from pathlib import Path

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

def create_phase0_script_fixed(epic_id: str, method: str, file: str, cyc: int, api_file: str) -> str:
    """Generate Phase 0 script using message file approach."""
    return f"""#!/bin/bash
set -e

cd {REPO_PATH}

# Set API key
export BOB_API_KEY_FILE="$HOME/.bob/api-keys/{api_file}"

# Create epic directory
mkdir -p docs/brain/EPIC-CCN-{epic_id}

# Create message file
cat > /tmp/phase0_msg_{epic_id}.txt << 'EOFMSG'
Execute Phase 0 (Hotspot Analysis) for EPIC-CCN-{epic_id}.

CRITICAL: You MUST write files to disk and verify they exist.

Target Method: {method}
File: {file}
Complexity: {cyc}

Required Actions:

Step 1: Use jCodemunch
- get_hotspots(repo='universal-or-strategy', top_n=50)
- get_blast_radius(repo='universal-or-strategy', symbol='{method}')
- get_call_hierarchy(repo='universal-or-strategy', symbol_id='{method}')
- get_symbol_complexity(repo='universal-or-strategy', symbol_id='{method}')

Step 2: Write 00-hotspots.md
Use write_to_file to create docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md with:
- Method signature and location
- Complexity metrics
- Blast radius
- Call hierarchy
- Risk assessment

Step 3: Write manifest.json
Use write_to_file to create docs/brain/EPIC-CCN-{epic_id}/manifest.json

Step 4: VERIFY with read_file
- read_file docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md
- read_file docs/brain/EPIC-CCN-{epic_id}/manifest.json

Step 5: Only use attempt_completion when BOTH files verified
EOFMSG

# Execute Bob with message file
bob --mode advanced --message "$(cat /tmp/phase0_msg_{epic_id}.txt)"

echo "DONE_EXIT=$?"
"""

def launch():
    print("=" * 80)
    print("Wave 2 Phase 0 Launch (Fixed)")
    print("=" * 80)
    
    # Validate API allocation
    api_values = list(API_ALLOCATION.values())
    if len(api_values) != len(set(api_values)):
        print("ERROR: Duplicate API keys")
        sys.exit(1)
    print(f"[OK] {len(api_values)} unique APIs\\n")
    
    # Create scripts
    for epic in EPICS:
        epic_id = epic["id"]
        script_content = create_phase0_script_fixed(
            epic_id, epic["method"], epic["file"], epic["cyc"], API_ALLOCATION[epic_id]
        )
        
        script_name = f"_p0_{epic_id}.sh"
        with open(script_name, "w", newline="\n") as f:
            f.write(script_content)
        print(f"[OK] {script_name}")
    
    print("\\nUploading...")
    subprocess.run([
        "gcloud", "compute", "scp", "_p0_*.sh",
        f"{VM_NAME}:{REPO_PATH}/",
        f"--zone={ZONE}"
    ], check=True)
    
    print("\\nLaunching...")
    for epic in EPICS:
        epic_id = epic["id"]
        cmd = (
            f"cd {REPO_PATH} && chmod +x _p0_{epic_id}.sh && "
            f"screen -dmS p0-{epic_id} bash -l -c "
            f"'./_p0_{epic_id}.sh 2>&1 | tee logs/phase0/EPIC-CCN-{epic_id}.log'"
        )
        subprocess.run([
            "gcloud", "compute", "ssh", VM_NAME,
            f"--zone={ZONE}",
            f"--command={cmd}"
        ], check=True)
        print(f"[OK] EPIC-CCN-{epic_id}")
        time.sleep(0.3)
    
    print("\\n" + "=" * 80)
    print("[SUCCESS] All 9 agents launched")
    print("=" * 80)

if __name__ == "__main__":
    launch()

# Made with Bob
