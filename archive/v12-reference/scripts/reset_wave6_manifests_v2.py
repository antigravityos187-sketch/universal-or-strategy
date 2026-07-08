#!/usr/bin/env python3
"""
Reset Wave 6 Manifests - V2 (Complete Reset)
Removes ALL phase data AND dependencies to allow clean Phase 0 execution
"""

import json
from pathlib import Path

def reset_manifest(manifest_path: Path) -> dict:
    """Reset manifest to minimal state for Phase 0 execution"""
    with open(manifest_path, 'r') as f:
        manifest = json.load(f)
    
    epic_id = manifest.get('epic_id', manifest_path.parent.name)
    
    # Check if Phase 0 is completed
    phases = manifest.get('phases', {})
    phase_0_complete = '0' in phases and phases['0'].get('status') == 'completed'
    
    # Build clean manifest with Phase 0 pre-created
    clean_manifest = {
        'epic_id': epic_id,
        'method': manifest.get('method', ''),
        'file': manifest.get('file', ''),
        'complexity': manifest.get('complexity', 0),
        'threshold': 8,  # Jane Street strict
        'phases': {
            '0': {
                'status': 'pending',
                'dependencies': [],
                'mode': 'v12-phase0-hotspot',
                'mcp_tools': ['jcodemunch-mcp', 'sequential-thinking']
            }
        },
        'dependencies': {'0': []},  # Phase 0 has no dependencies
        'description': manifest.get('description', ''),
        'status': 'pending',
        'created_at': manifest.get('created_at', '')
    }
    
    # If Phase 0 was completed, update its status
    if phase_0_complete:
        clean_manifest['phases']['0'] = phases['0']
        clean_manifest['status'] = 'in_progress'
    
    return clean_manifest, phase_0_complete, len(phases)

def main():
    brain_dir = Path('docs/brain')
    epic_dirs = sorted([d for d in brain_dir.iterdir() if d.is_dir() and d.name.startswith('EPIC-CCN-')])
    
    print("=== Resetting Wave 6 Manifests (V2 - Complete Reset) ===")
    print(f"Found {len(epic_dirs)} epic directories\n")
    
    success_count = 0
    
    for epic_dir in epic_dirs:
        manifest_path = epic_dir / 'manifest.json'
        
        if not manifest_path.exists():
            print(f"  ⚠️  No manifest found for {epic_dir.name}")
            continue
        
        try:
            clean_manifest, phase_0_kept, removed_count = reset_manifest(manifest_path)
            
            # Write clean manifest
            with open(manifest_path, 'w') as f:
                json.dump(clean_manifest, f, indent=2)
            
            if phase_0_kept:
                print(f"  ✅ {epic_dir.name}: Kept Phase 0 (completed), removed {removed_count - 1} other phases + dependencies")
            else:
                print(f"  ✅ {epic_dir.name}: Removed all {removed_count} phases + dependencies (Phase 0 not completed)")
            
            success_count += 1
            
        except Exception as e:
            print(f"  ❌ {epic_dir.name}: Error - {e}")
    
    print(f"\n=== Reset Complete ===")
    print(f"Success: {success_count}/{len(epic_dirs)} manifests reset")

if __name__ == '__main__':
    main()

# Made with Bob
