#!/usr/bin/env python3
"""
Reset Wave 6 manifests to clean state for Phase 0 execution.
Removes all phase entries except Phase 0 (if exists).
"""

import json
import sys
from pathlib import Path

def reset_manifest(epic_id: str, brain_dir: Path) -> bool:
    """Reset manifest for a single epic."""
    manifest_path = brain_dir / "manifest.json"
    
    if not manifest_path.exists():
        print(f"  ⚠️  No manifest found for {epic_id}")
        return False
    
    try:
        with open(manifest_path, 'r') as f:
            manifest = json.load(f)
        
        # Keep only Phase 0 if it exists and is completed
        phases = manifest.get("phases", {})
        phase_0 = phases.get("0", {})
        
        if phase_0.get("status") == "completed":
            # Keep Phase 0
            manifest["phases"] = {"0": phase_0}
            print(f"  ✅ {epic_id}: Kept Phase 0 (completed), removed {len(phases) - 1} other phases")
        else:
            # Remove all phases
            manifest["phases"] = {}
            print(f"  ✅ {epic_id}: Removed all {len(phases)} phases (Phase 0 not completed)")
        
        # Write back
        with open(manifest_path, 'w') as f:
            json.dump(manifest, f, indent=2)
        
        return True
        
    except Exception as e:
        print(f"  ❌ {epic_id}: Error - {e}")
        return False

def main():
    """Reset all Wave 6 manifests."""
    brain_root = Path("docs/brain")
    
    # Find all EPIC-CCN-* directories
    epic_dirs = sorted([d for d in brain_root.glob("EPIC-CCN-*") if d.is_dir()])
    
    print(f"=== Resetting Wave 6 Manifests ===")
    print(f"Found {len(epic_dirs)} epic directories\n")
    
    success_count = 0
    for epic_dir in epic_dirs:
        epic_id = epic_dir.name
        if reset_manifest(epic_id, epic_dir):
            success_count += 1
    
    print(f"\n=== Reset Complete ===")
    print(f"Success: {success_count}/{len(epic_dirs)} manifests reset")
    
    return 0 if success_count == len(epic_dirs) else 1

if __name__ == "__main__":
    sys.exit(main())

# Made with Bob
