#!/usr/bin/env python3
"""Check Wave 6 Phase 1 status - ONLY Wave 6 epics (EPIC-CCN-003 through EPIC-CCN-080)."""

import json
import glob
from pathlib import Path

def check_phase1_status():
    """Check Phase 1 completion status for Wave 6 epics only."""
    # Wave 6 is EPIC-CCN-003 through EPIC-CCN-080 (78 epics)
    wave6_range = range(3, 81)  # 3-80 inclusive
    
    completed = []
    in_progress = []
    pending = []
    failed = []
    
    for epic_num in wave6_range:
        epic_id = f"EPIC-CCN-{epic_num:03d}"
        manifest_path = f'/home/malhitticrypto/universal-or-strategy/docs/brain/{epic_id}/manifest.json'
        
        try:
            with open(manifest_path) as f:
                data = json.load(f)
                phase1_status = data.get('phases', {}).get('1', {}).get('status', 'unknown')
                
                if phase1_status == 'completed':
                    completed.append(epic_id)
                elif phase1_status == 'in_progress':
                    in_progress.append(epic_id)
                elif phase1_status == 'pending':
                    pending.append(epic_id)
                elif phase1_status == 'failed':
                    failed.append(epic_id)
        except FileNotFoundError:
            print(f"Warning: {epic_id} manifest not found")
        except Exception as e:
            print(f"Error reading {epic_id}: {e}")
    
    total = len(wave6_range)
    print(f"\n=== Wave 6 Phase 1 Status (EPIC-CCN-003 to EPIC-CCN-080) ===")
    print(f"Total Epics: {total}")
    print(f"Completed: {len(completed)}/{total} ({len(completed)*100//total}%)")
    print(f"In Progress: {len(in_progress)}")
    print(f"Pending: {len(pending)}")
    print(f"Failed: {len(failed)}")
    
    if completed:
        print(f"\nCompleted: {', '.join(completed[:10])}" + (" ..." if len(completed) > 10 else ""))
    if failed:
        print(f"\nFailed: {', '.join(failed)}")

if __name__ == '__main__':
    check_phase1_status()

# Made with Bob
