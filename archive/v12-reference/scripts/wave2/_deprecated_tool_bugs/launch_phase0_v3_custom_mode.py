#!/usr/bin/env python3
"""Wave 2 Phase 0 v3 - Custom Mode with Explicit write_to_file"""
import subprocess, sys, json
from pathlib import Path

EPICS = [
    {"id": "107", "method": "ProcessIpcCommands", "cyc": 76},
    {"id": "108", "method": "ProcessOnExecutionUpdate", "cyc": 67},
    {"id": "109", "method": "HydrateFSMsFromWorkingOrders", "cyc": 45},
    {"id": "110", "method": "HandleFlatPositionUpdate", "cyc": 37},
    {"id": "111", "method": "AdoptFleetOrders", "cyc": 37},
    {"id": "112", "method": "ExtractTargetConfiguration", "cyc": 31},
    {"id": "113", "method": "SweepBrokerOrders", "cyc": 28},
    {"id": "114", "method": "FlattenSinglePosition", "cyc": 27},
    {"id": "115", "method": "ExecuteRetestEntry", "cyc": 26},
]

API_ALLOCATION = {
    "107": "b (2).json", "108": "b.json", "109": "bob (1).json",
    "110": "bob (2).json", "111": "bob (3).json", "112": "bob (4).json",
    "113": "bob (5).json", "114": "bob (6).json", "115": "bob.json",
}

def load_api_key(filename: str) -> str:
    return json.loads((Path("docs/API") / filename).read_text())["apikey"]

def create_script(epic_id: str, method: str, cyc: int, api_key: str) -> str:
    return f'''#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/EPIC-CCN-{epic_id}

cat > /tmp/phase0_msg_{epic_id}.txt << 'EOFMSG'
Phase 0 Hotspot Analysis for EPIC-CCN-{epic_id}

Target: {method} (CYC {cyc})

MANDATORY STEPS:
1. jCodemunch: get_hotspots, get_blast_radius, get_call_hierarchy
2. write_to_file docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md
3. read_file docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md (VERIFY)
4. write_to_file docs/brain/EPIC-CCN-{epic_id}/manifest.json
5. read_file docs/brain/EPIC-CCN-{epic_id}/manifest.json (VERIFY)
6. attempt_completion ONLY after BOTH read_file calls succeed

CRITICAL: Use write_to_file tool, NOT run_shell_command with cat.
EOFMSG

bob --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_{epic_id}.txt)"
echo "DONE_EXIT=$?"
'''

def main():
    print("Generating Phase 0 v3 scripts (custom mode)...")
    for epic in EPICS:
        api_key = load_api_key(API_ALLOCATION[epic["id"]])
        script = create_script(epic["id"], epic["method"], epic["cyc"], api_key)
        Path(f"_p0_{epic['id']}.sh").write_text(script, newline="\n")
        print(f"[OK] _p0_{epic['id']}.sh")
    
    print("\nUpload: gcloud compute scp _p0_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a")
    print("Launch: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='cd /home/malhitticrypto/universal-or-strategy && bash launch_phase0_all.sh'")

if __name__ == "__main__":
    main()

# Made with Bob
