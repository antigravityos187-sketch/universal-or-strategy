#!/usr/bin/env python3
"""
Add Phase 1.5 to all Wave 6 epic manifests.
Phase 1.5 was added to V12 workflow but manifests weren't updated.
"""

import json
import sys
from pathlib import Path

# Add scripts to path for imports
sys.path.insert(0, 'scripts')

def add_phase_1_5_to_manifest(epic_id: str) -> bool:
    """Add Phase 1.5 definition to manifest if missing."""
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    
    if not manifest_path.exists():
        print(f"SKIP {epic_id} - manifest not found")
        return False
    
    # Load manifest
    with open(manifest_path, 'r', encoding='utf-8') as f:
        manifest = json.load(f)
    
    # Check if Phase 1.5 already exists
    if "1.5" in manifest.get("phases", {}):
        print(f"SKIP {epic_id} - Phase 1.5 already exists")
        return False
    
    # Check if Phase 1 is completed
    phase_1 = manifest.get("phases", {}).get("1", {})
    if phase_1.get("status") != "completed":
        print(f"SKIP {epic_id} - Phase 1 not completed (status: {phase_1.get('status')})")
        return False
    
    # Add Phase 1.5 definition
    manifest["phases"]["1.5"] = {
        "status": "pending",
        "dependencies": ["1"],
        "mode": "v12-phase1-5-boundary",
        "mcp_tools": [
            "jcodemunch-mcp",
            "sequential-thinking"
        ],
        "started_at": None,
        "completed_at": None,
        "output_artifacts": [],
        "notes": ""
    }
    
    # Add Phase 1.5 to dependencies map
    if "dependencies" not in manifest:
        manifest["dependencies"] = {}
    manifest["dependencies"]["1.5"] = ["1"]
    
    # Update Phase 2 dependencies if it exists
    if "2" in manifest.get("phases", {}):
        manifest["dependencies"]["2"] = ["1.5"]
    
    # Save updated manifest
    with open(manifest_path, 'w', encoding='utf-8') as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)
    
    print(f"OK   {epic_id} - Phase 1.5 added")
    return True

def main():
    """Add Phase 1.5 to all Wave 6 epic manifests."""
    print("Adding Phase 1.5 to Wave 6 epic manifests...")
    print()
    
    # All Wave 6 epics (excluding EPIC-027)
    epic_ids = [f"EPIC-CCN-{i:03d}" for i in range(1, 81) if i != 27]
    
    added_count = 0
    skip_count = 0
    
    for epic_id in epic_ids:
        if add_phase_1_5_to_manifest(epic_id):
            added_count += 1
        else:
            skip_count += 1
    
    print()
    print("=" * 50)
    print(f"Added: {added_count} manifests")
    print(f"Skipped: {skip_count} manifests")
    print("=" * 50)

if __name__ == "__main__":
    main()

# Made with Bob
