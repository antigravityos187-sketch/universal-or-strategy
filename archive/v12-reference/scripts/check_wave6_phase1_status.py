#!/usr/bin/env python3
"""Check Wave 6 Phase 1 status across all epics."""

import json
import glob
from pathlib import Path

def check_phase1_status():
    """Check Phase 1 completion status."""
    manifests = sorted(glob.glob('/home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/manifest.json'))
    
    completed = []
    in_progress = []
    pending = []
    failed = []
    
    for manifest_path in manifests:
        epic_id = Path(manifest_path).parent.name
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
        except Exception as e:
            print(f"Error reading {epic_id}: {e}")
    
    total = len(manifests)
    print(f"\n=== Wave 6 Phase 1 Status ===")
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
