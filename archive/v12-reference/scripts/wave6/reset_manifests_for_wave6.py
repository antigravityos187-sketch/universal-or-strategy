#!/usr/bin/env python3
"""
Reset all epic manifests to pending status for Wave 6 execution.
Preserves manifest structure but resets all phase statuses to 'pending'.
"""

import json
import sys
from pathlib import Path

def reset_manifest(manifest_path: Path) -> bool:
    """Reset all phases in manifest to pending status."""
    try:
        with open(manifest_path, 'r', encoding='utf-8') as f:
            manifest = json.load(f)
        
        # Reset all phase statuses to pending
        modified = False
        
        # Handle both old format (phases at root) and new format (phases nested)
        phases_dict = manifest.get('phases', manifest)
        
        for phase_key in phases_dict.keys():
            if phase_key in ['0', '1', '1.5', '2', '3', '4', '4.5', '5', '5.V', '6']:
                if phases_dict[phase_key].get('status') != 'pending':
                    phases_dict[phase_key]['status'] = 'pending'
                    phases_dict[phase_key]['outputs'] = []
                    modified = True
        
        if modified:
            with open(manifest_path, 'w', encoding='utf-8') as f:
                json.dump(manifest, f, indent=2)
            return True
        return False
        
    except Exception as e:
        print(f"Error processing {manifest_path}: {e}")
        return False

def main():
    brain_dir = Path('docs/brain')
    
    if not brain_dir.exists():
        print(f"Error: Brain directory not found: {brain_dir}")
        sys.exit(1)
    
    print("=== Wave 6 Manifest Reset ===")
    print("Resetting all phase statuses to 'pending'...")
    print()
    
    reset_count = 0
    skip_count = 0
    
    for epic_dir in sorted(brain_dir.glob('EPIC-CCN-*')):
        if epic_dir.is_dir():
            manifest_path = epic_dir / 'manifest.json'
            if manifest_path.exists():
                epic_id = epic_dir.name
                if reset_manifest(manifest_path):
                    print(f"Reset {epic_id}")
                    reset_count += 1
                else:
                    skip_count += 1
    
    print()
    print("=== Reset Complete ===")
    print(f"Reset: {reset_count} manifests")
    print(f"Skipped: {skip_count} manifests (already pending)")
    print()
    print("Manifests ready for Wave 6 execution.")

if __name__ == '__main__':
    main()

# Made with Bob
