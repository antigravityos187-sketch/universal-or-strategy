#!/usr/bin/env python3
"""Check Phase 4 status by reading local manifests."""

import json
from pathlib import Path

EPIC_IDS = [
    "EPIC-CCN-107", "EPIC-CCN-108", "EPIC-CCN-109",
    "EPIC-CCN-110", "EPIC-CCN-111", "EPIC-CCN-112",
    "EPIC-CCN-113", "EPIC-CCN-114", "EPIC-CCN-115"
]

def check_phase4_status():
    """Check Phase 4 status for all Wave 2 epics."""
    print("[STATUS] Phase 4 Progress Check")
    print("=" * 60)
    
    completed = []
    in_progress = []
    pending = []
    
    for epic_id in EPIC_IDS:
        manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
        tickets_path = Path(f"docs/brain/{epic_id}/04-tickets.md")
        
        if not manifest_path.exists():
            pending.append(epic_id)
            continue
            
        with open(manifest_path) as f:
            manifest = json.load(f)
        
        phase4 = manifest.get("phases", {}).get("4", {})
        status = phase4.get("status", "pending")
        
        # Check if tickets file exists
        has_tickets = tickets_path.exists()
        
        if status == "completed" and has_tickets:
            completed.append(epic_id)
            print(f"[OK] {epic_id}: COMPLETED (tickets exist)")
        elif status == "in_progress":
            in_progress.append(epic_id)
            print(f"[..] {epic_id}: IN PROGRESS (waiting for VM agent)")
        else:
            pending.append(epic_id)
            print(f"[--] {epic_id}: PENDING")
    
    print("\n" + "=" * 60)
    print(f"Completed: {len(completed)}/9")
    print(f"In Progress: {len(in_progress)}/9")
    print(f"Pending: {len(pending)}/9")
    
    if completed:
        print(f"\n[OK] Completed: {', '.join(completed)}")
    if in_progress:
        print(f"\n[..] In Progress: {', '.join(in_progress)}")
    if pending:
        print(f"\n[--] Pending: {', '.join(pending)}")
    
    return len(completed) == 9

if __name__ == "__main__":
    all_done = check_phase4_status()
    exit(0 if all_done else 1)

# Made with Bob
