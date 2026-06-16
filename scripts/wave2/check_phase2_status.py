#!/usr/bin/env python3
"""Check Phase 2 status for all Wave 2 epics."""

import json
import subprocess
from pathlib import Path

EPICS = [107, 108, 109, 110, 111, 112, 113, 114, 115]
VM = "v12-test-golden-v2"
ZONE = "us-central1-a"

def get_epic_status(epic_num):
    """Get Phase 2 status for an epic."""
    epic_id = f"EPIC-CCN-{epic_num}"
    remote_path = f"/home/malhitticrypto/universal-or-strategy/docs/brain/{epic_id}/manifest.json"
    
    # Download manifest
    cmd = [
        "gcloud", "compute", "scp",
        f"{VM}:{remote_path}",
        f"./temp_manifest_{epic_num}.json",
        f"--zone={ZONE}",
        "--quiet"
    ]
    
    try:
        subprocess.run(cmd, check=True, capture_output=True)
        
        # Read manifest
        with open(f"./temp_manifest_{epic_num}.json") as f:
            manifest = json.load(f)
        
        phase2 = manifest.get("phases", {}).get("2", {})
        status = phase2.get("status", "not_started")
        
        # Cleanup
        Path(f"./temp_manifest_{epic_num}.json").unlink(missing_ok=True)
        
        return {
            "epic_id": epic_id,
            "phase2_status": status,
            "overall_status": manifest.get("status", "unknown")
        }
    except Exception as e:
        return {
            "epic_id": epic_id,
            "phase2_status": "error",
            "error": str(e)
        }

def main():
    """Check all epics."""
    print("=== Wave 2 Phase 2 Status Check ===\n")
    
    results = []
    completed = 0
    
    for epic_num in EPICS:
        result = get_epic_status(epic_num)
        results.append(result)
        
        status_icon = "[OK]" if result["phase2_status"] == "completed" else "[  ]"
        print(f"{status_icon} {result['epic_id']}: Phase 2 = {result['phase2_status']}")
        
        if result["phase2_status"] == "completed":
            completed += 1
    
    print(f"\n=== Summary ===")
    print(f"Phase 2 Completed: {completed}/9")
    print(f"Phase 2 Pending: {9 - completed}/9")
    
    if completed == 9:
        print("\n✅ ALL EPICS COMPLETED PHASE 2!")
        print("Next: Deploy updated commands to VM and proceed to Phase 3")
    elif completed > 0:
        print(f"\n⚠️  {completed} epics completed, {9-completed} need attention")
        print("Gate removal worked for some epics")
    else:
        print("\n❌ No epics completed Phase 2")
        print("Need to investigate why gates are still blocking")

if __name__ == "__main__":
    main()

# Made with Bob
