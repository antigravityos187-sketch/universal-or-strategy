#!/usr/bin/env python3
"""
Clear Lamport Clock Conflicts for Wave 6 Blocked Epics
Removes lamport_clock field from manifest to allow fresh execution
"""

import json
from pathlib import Path

BRAIN_DIR = Path('docs/brain')
CONFLICTED_EPICS = ['EPIC-CCN-001', 'EPIC-CCN-004', 'EPIC-CCN-016', 'EPIC-CCN-028']

def clear_lamport_conflict(epic_id: str) -> bool:
    """Clear Lamport clock history for an epic"""
    manifest_path = BRAIN_DIR / epic_id / 'manifest.json'
    
    if not manifest_path.exists():
        print(f"  ⚠️  {epic_id}: Manifest not found")
        return False
    
    try:
        with open(manifest_path, 'r') as f:
            manifest = json.load(f)
        
        # Remove lamport_clock field if it exists
        if 'lamport_clock' in manifest:
            del manifest['lamport_clock']
            print(f"  ✅ {epic_id}: Cleared Lamport clock history")
        else:
            print(f"  ℹ️  {epic_id}: No Lamport clock found (already clean)")
        
        # Write back
        with open(manifest_path, 'w') as f:
            json.dump(manifest, f, indent=2)
            f.write('\n')
        
        return True
        
    except Exception as e:
        print(f"  ❌ {epic_id}: Error - {e}")
        return False

def main():
    print("=== Clearing Lamport Clock Conflicts ===")
    print(f"Epics: {', '.join(CONFLICTED_EPICS)}\n")
    
    success_count = 0
    for epic_id in CONFLICTED_EPICS:
        if clear_lamport_conflict(epic_id):
            success_count += 1
    
    print(f"\n=== Complete ===")
    print(f"Success: {success_count}/{len(CONFLICTED_EPICS)} epics cleared")
    print("\nReady to relaunch Phase 0 for these epics")

if __name__ == '__main__':
    main()

# Made with Bob
