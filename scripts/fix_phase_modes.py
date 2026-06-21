#!/usr/bin/env python3
"""
Fix existing phases to add missing mode and mcp_tools fields.
"""

import json
from pathlib import Path

# Phase configuration
PHASE_CONFIG = {
    '1.5': {
        'mode': 'v12-phase1-5-boundary',
        'mcp_tools': ['jcodemunch-mcp']
    },
    '2': {
        'mode': 'v12-phase2-architecture',
        'mcp_tools': ['jcodemunch-mcp', 'sequential-thinking']
    },
    '3': {
        'mode': 'v12-phase3-audit',
        'mcp_tools': ['jcodemunch-mcp']
    },
    '4': {
        'mode': 'v12-phase4-tickets',
        'mcp_tools': ['jcodemunch-mcp']
    }
}

def fix_manifest(epic_id: str):
    """Fix manifest phases to add missing fields."""
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    
    if not manifest_path.exists():
        print(f"[ERROR] Manifest not found: {manifest_path}")
        return False
    
    with open(manifest_path, 'r') as f:
        manifest = json.load(f)
    
    modified = False
    for phase_id, config in PHASE_CONFIG.items():
        if phase_id in manifest['phases']:
            phase = manifest['phases'][phase_id]
            
            # Add mode if missing
            if 'mode' not in phase:
                phase['mode'] = config['mode']
                modified = True
                print(f"  [FIX] Added mode '{config['mode']}' to Phase {phase_id}")
            
            # Add mcp_tools if missing
            if 'mcp_tools' not in phase:
                phase['mcp_tools'] = config['mcp_tools']
                modified = True
                print(f"  [FIX] Added mcp_tools to Phase {phase_id}")
            
            # Rename outputs to output_artifacts if needed
            if 'outputs' in phase and 'output_artifacts' not in phase:
                phase['output_artifacts'] = phase.pop('outputs')
                modified = True
                print(f"  [FIX] Renamed 'outputs' to 'output_artifacts' in Phase {phase_id}")
    
    if modified:
        with open(manifest_path, 'w') as f:
            json.dump(manifest, f, indent=2)
        print(f"[OK] {epic_id} manifest updated")
    else:
        print(f"[INFO] {epic_id} manifest already correct")
    
    return True

def main():
    pilot_epics = ['EPIC-CCN-001', 'EPIC-CCN-002', 'EPIC-CCN-004']
    
    print("Fixing phase configurations in manifests...")
    print("=" * 60)
    
    for epic_id in pilot_epics:
        print(f"\n{epic_id}:")
        fix_manifest(epic_id)
    
    print("\n" + "=" * 60)
    print("[OK] All manifests fixed")
    print("\nNext: Verify Phase 1.5 can execute")

if __name__ == '__main__':
    main()

# Made with Bob
