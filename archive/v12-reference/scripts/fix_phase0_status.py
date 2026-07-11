#!/usr/bin/env python3
"""
Fix Phase 0 status for epics with completed status but missing hotspot files.
This resolves Lamport clock non-determinism errors.
"""

import json
from pathlib import Path

EPICS = ["EPIC-CCN-001", "EPIC-CCN-004", "EPIC-CCN-016", "EPIC-CCN-028"]

def fix_epic(epic_id):
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    
    if not manifest_path.exists():
        print(f"  ⚠️  {epic_id}: Manifest not found")
        return False
    
    with open(manifest_path, 'r') as f:
        manifest = json.load(f)
    
    # Check if Phase 0 exists and is marked completed
    if '0' not in manifest.get('phases', {}):
        print(f"  ⚠️  {epic_id}: Phase 0 not in manifest")
        return False
    
    phase0 = manifest['phases']['0']
    if phase0.get('status') != 'completed':
        print(f"  ℹ️  {epic_id}: Phase 0 already {phase0.get('status')}")
        return False
    
    # Check if hotspot file exists
    hotspot_path = Path(f"docs/brain/{epic_id}/00-hotspots.md")
    if hotspot_path.exists():
        print(f"  ℹ️  {epic_id}: Hotspot file exists, no fix needed")
        return False
    
    # Fix: Reset Phase 0 to pending
    manifest['phases']['0']['status'] = 'pending'
    if 'outputs' in manifest['phases']['0']:
        del manifest['phases']['0']['outputs']
    if 'created_at' in manifest['phases']['0']:
        del manifest['phases']['0']['created_at']
    
    # Write back
    with open(manifest_path, 'w') as f:
        json.dump(manifest, f, indent=2)
    
    print(f"  ✅ {epic_id}: Reset Phase 0 to pending")
    return True

def main():
    print("=== Fixing Phase 0 Status for Lamport Recovery ===")
    print(f"Epics: {', '.join(EPICS)}\n")
    
    fixed = 0
    for epic in EPICS:
        if fix_epic(epic):
            fixed += 1
    
    print(f"\n=== Complete ===")
    print(f"Fixed: {fixed}/{len(EPICS)} epics")
    print("\nReady to relaunch Phase 0 for these epics")

if __name__ == "__main__":
    main()

# Made with Bob
