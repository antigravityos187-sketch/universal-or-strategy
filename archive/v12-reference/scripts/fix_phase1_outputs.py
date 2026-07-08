#!/usr/bin/env python3
"""
Fix Phase 1 output_artifacts in manifests.
Add 00-scope.md to Phase 1 output_artifacts.
"""

import json
from pathlib import Path

def fix_manifest(epic_id: str):
    """Add Phase 1 output to manifest."""
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    
    if not manifest_path.exists():
        print(f"[SKIP] {epic_id}: No manifest found")
        return False
    
    with open(manifest_path, 'r') as f:
        manifest = json.load(f)
    
    # Check if Phase 1 exists and is completed
    phase_1 = manifest.get('phases', {}).get('1', {})
    if phase_1.get('status') != 'completed':
        print(f"[SKIP] {epic_id}: Phase 1 not completed")
        return False
    
    # Check if 00-scope.md exists
    scope_file = Path(f"docs/brain/{epic_id}/00-scope.md")
    if not scope_file.exists():
        print(f"[SKIP] {epic_id}: 00-scope.md not found")
        return False
    
    # Add to output_artifacts if not already there
    output_artifacts = phase_1.get('output_artifacts', [])
    expected_output = f"docs/brain/{epic_id}/00-scope.md"
    
    if expected_output not in output_artifacts:
        output_artifacts.append(expected_output)
        phase_1['output_artifacts'] = output_artifacts
        
        # Save manifest
        with open(manifest_path, 'w') as f:
            json.dump(manifest, f, indent=4)
        
        print(f"[OK] {epic_id}: Added 00-scope.md to Phase 1 outputs")
        return True
    else:
        print(f"[SKIP] {epic_id}: 00-scope.md already in outputs")
        return False

def main():
    print("Fixing Phase 1 output_artifacts...")
    print("=" * 60)
    
    # Fix pilot epics
    pilot_epics = ['EPIC-CCN-001', 'EPIC-CCN-002', 'EPIC-CCN-004']
    
    fixed_count = 0
    for epic_id in pilot_epics:
        if fix_manifest(epic_id):
            fixed_count += 1
    
    print("=" * 60)
    print(f"[OK] Fixed {fixed_count}/{len(pilot_epics)} manifests")
    print("\nNext: Verify Phase 1.5 can execute")

if __name__ == '__main__':
    main()

# Made with Bob
