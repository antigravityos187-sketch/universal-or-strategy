#!/usr/bin/env python3
"""Wait for Phase 4 to complete, checking every 30 seconds."""

import json
import time
from pathlib import Path

EPIC_IDS = [
    "EPIC-CCN-107", "EPIC-CCN-108", "EPIC-CCN-109",
    "EPIC-CCN-110", "EPIC-CCN-111", "EPIC-CCN-112",
    "EPIC-CCN-113", "EPIC-CCN-114", "EPIC-CCN-115"
]

def check_completion():
    """Check if all epics have completed Phase 4."""
    completed = 0
    
    for epic_id in EPIC_IDS:
        manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
        tickets_path = Path(f"docs/brain/{epic_id}/04-tickets.md")
        
        if not manifest_path.exists():
            continue
            
        with open(manifest_path) as f:
            manifest = json.load(f)
        
        phase4 = manifest.get("phases", {}).get("4", {})
        status = phase4.get("status", "pending")
        
        if status == "completed" and tickets_path.exists():
            completed += 1
    
    return completed

def main():
    """Monitor Phase 4 completion."""
    print("[MONITOR] Waiting for Phase 4 completion...")
    print("=" * 60)
    
    max_wait = 600  # 10 minutes
    check_interval = 30  # 30 seconds
    elapsed = 0
    
    while elapsed < max_wait:
        completed = check_completion()
        print(f"[{elapsed}s] Completed: {completed}/9")
        
        if completed == 9:
            print("\n[SUCCESS] All 9 epics completed Phase 4!")
            return True
        
        time.sleep(check_interval)
        elapsed += check_interval
    
    print(f"\n[TIMEOUT] After {max_wait}s, only {completed}/9 completed")
    print("Agents may still be running. Check manually:")
    print("  python scripts/wave2/check_phase4_local.py")
    return False

if __name__ == "__main__":
    success = main()
    exit(0 if success else 1)

# Made with Bob
