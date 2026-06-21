#!/usr/bin/env python3
"""
Register existing output files in manifest.
Used when files exist from previous attempts but aren't registered.
"""

import json
from pathlib import Path

# Map files to phases
FILE_TO_PHASE = {
    '00-hotspots.md': '0',
    '01-scope.md': '1',
    '01-scope-boundary.md': '1.5',
    '02-architecture-plan.md': '2',
    '03-audit-report.md': '3',
    '04-tickets.md': '4'
}

def register_outputs(epic_id: str):
    """Register existing output files in manifest."""
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    brain_dir = Path(f"docs/brain/{epic_id}")
    
    if not manifest_path.exists():
        print(f"[ERROR] Manifest not found: {manifest_path}")
        return False
    
    with open(manifest_path, 'r') as f:
        manifest = json.load(f)
    
    modified = False
    for filename, phase_id in FILE_TO_PHASE.items():
        file_path = brain_dir / filename
        
        if file_path.exists() and phase_id in manifest['phases']:
            phase = manifest['phases'][phase_id]
            output_path = f"docs/brain/{epic_id}/{filename}"
            
            # Add to output_artifacts if not already there
            if output_path not in phase.get('output_artifacts', []):
                if 'output_artifacts' not in phase:
                    phase['output_artifacts'] = []
                phase['output_artifacts'].append(output_path)
                modified = True
                print(f"  [ADD] Registered {filename} as output of Phase {phase_id}")
    
    if modified:
        with open(manifest_path, 'w') as f:
            json.dump(manifest, f, indent=2)
        print(f"[OK] {epic_id} manifest updated")
    else:
        print(f"[INFO] {epic_id} all outputs already registered")
    
    return True

def main():
    pilot_epics = ['EPIC-CCN-001', 'EPIC-CCN-002', 'EPIC-CCN-004']
    
    print("Registering existing output files...")
    print("=" * 60)
    
    for epic_id in pilot_epics:
        print(f"\n{epic_id}:")
        register_outputs(epic_id)
    
    print("\n" + "=" * 60)
    print("[OK] Output registration complete")
    print("\nNext: Verify Phase 1.5 can execute")

if __name__ == '__main__':
    main()

# Made with Bob
