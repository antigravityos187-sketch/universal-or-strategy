#!/usr/bin/env python3
"""Fix final 3 blocked epics for Wave 6 Phase 0."""

import json
import os
from pathlib import Path

def fix_epic_004():
    """Reset EPIC-CCN-004 status from completed to pending."""
    manifest_path = Path('docs/brain/EPIC-CCN-004/manifest.json')
    with open(manifest_path) as f:
        manifest = json.load(f)
    
    manifest['phases']['0']['status'] = 'pending'
    manifest['phases']['0'].pop('outputs', None)
    manifest['phases']['0'].pop('created_at', None)
    
    with open(manifest_path, 'w') as f:
        json.dump(manifest, f, indent=2)
    
    print("✅ EPIC-CCN-004: Status reset to pending")

def fix_epic_016():
    """Remove stale files from EPIC-CCN-016."""
    brain_dir = Path('docs/brain/EPIC-CCN-016')
    stale_files = [
        brain_dir / 'CORRECTED_EXTRACTION_PLAN.md',
        brain_dir / 'PHASE5_MANUAL_COMPLETION_PLAN.md'
    ]
    
    for file in stale_files:
        if file.exists():
            file.unlink()
            print(f"✅ Removed: {file.name}")
    
    print("✅ EPIC-CCN-016: Stale files removed")

def fix_epic_028():
    """Remove stale file from EPIC-CCN-028."""
    stale_file = Path('docs/brain/EPIC-CCN-028/phase-5-summary.md')
    if stale_file.exists():
        stale_file.unlink()
        print(f"✅ Removed: {stale_file.name}")
    
    print("✅ EPIC-CCN-028: Stale file removed")

if __name__ == '__main__':
    print("=== Fixing Final 3 Epics ===\n")
    fix_epic_004()
    fix_epic_016()
    fix_epic_028()
    print("\n✅ All fixes applied")

# Made with Bob
