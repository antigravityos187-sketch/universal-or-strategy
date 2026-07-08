#!/usr/bin/env python3
"""Check Phase 1 output_artifacts in manifests."""

import json
from pathlib import Path

def check_manifest(epic_id: str):
    """Check Phase 1 outputs in manifest."""
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    
    if not manifest_path.exists():
        print(f"[ERROR] {epic_id}: No manifest found")
        return
    
    with open(manifest_path, 'r') as f:
        manifest = json.load(f)
    
    print(f"\n{epic_id}")
    print("=" * 60)
    
    phase_1 = manifest.get('phases', {}).get('1', {})
    print(f"Phase 1 status: {phase_1.get('status')}")
    print(f"Phase 1 output_artifacts: {phase_1.get('output_artifacts', [])}")
    
    # Check what files actually exist
    brain_dir = Path(f"docs/brain/{epic_id}")
    if brain_dir.exists():
        existing_files = [f.name for f in brain_dir.glob("*.md")]
        print(f"Existing .md files: {existing_files}")

def main():
    print("Checking Phase 1 output_artifacts...")
    
    pilot_epics = ['EPIC-CCN-001', 'EPIC-CCN-002', 'EPIC-CCN-004']
    
    for epic_id in pilot_epics:
        check_manifest(epic_id)

if __name__ == '__main__':
    main()

# Made with Bob
