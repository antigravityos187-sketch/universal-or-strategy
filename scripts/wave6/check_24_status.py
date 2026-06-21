#!/usr/bin/env python3
"""Check status of 24 relaunched Wave 6 Phase 1 epics"""

import json
from pathlib import Path

EPICS = [
    "001", "004", "016", "020", "021", "028",
    "050", "051", "052", "053", "054", "055", "056", "057", "058", "059", "060", "061",
    "070", "073", "076", "077", "078", "079"
]

def main():
    completed = []
    in_progress = []
    pending = []
    failed = []
    
    for epic_num in EPICS:
        epic_id = f"EPIC-CCN-{epic_num}"
        manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
        
        if not manifest_path.exists():
            print(f"❌ {epic_id}: manifest not found")
            continue
        
        try:
            with open(manifest_path) as f:
                manifest = json.load(f)
            
            status = manifest["phases"]["1"]["status"]
            
            if status == "completed":
                completed.append(epic_num)
            elif status == "in_progress":
                in_progress.append(epic_num)
            elif status == "pending":
                pending.append(epic_num)
            elif status == "failed":
                failed.append(epic_num)
        except Exception as e:
            print(f"❌ {epic_id}: error reading manifest - {e}")
    
    total = len(EPICS)
    percent = (len(completed) * 100) // total
    
    print(f"\n=== Wave 6 Phase 1 Status (24 Relaunched Epics) ===")
    print(f"Progress: {len(completed)}/{total} ({percent}%)")
    print(f"\n✅ Completed ({len(completed)}): {', '.join(completed)}")
    print(f"⏳ In Progress ({len(in_progress)}): {', '.join(in_progress)}")
    print(f"⏸️  Pending ({len(pending)}): {', '.join(pending)}")
    print(f"❌ Failed ({len(failed)}): {', '.join(failed)}")

if __name__ == "__main__":
    main()

# Made with Bob
