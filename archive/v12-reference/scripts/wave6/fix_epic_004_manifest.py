#!/usr/bin/env python3
"""
Fix EPIC-CCN-004 manifest epic_id field.
The manifest has epic_id="EPIC-CCN-028" but should be "EPIC-CCN-004".
"""

import json
from pathlib import Path

def fix_epic_004_manifest():
    """Fix the top-level epic_id field in EPIC-CCN-004 manifest."""
    manifest_path = Path("docs/brain/EPIC-CCN-004/manifest.json")
    
    if not manifest_path.exists():
        print(f"❌ Manifest not found: {manifest_path}")
        return False
    
    # Load manifest
    with open(manifest_path, 'r') as f:
        manifest = json.load(f)
    
    # Check current epic_id
    current_id = manifest.get('epic_id')
    print(f"Current epic_id: {current_id}")
    
    if current_id == "EPIC-CCN-004":
        print("✅ Manifest already correct!")
        return True
    
    # Fix epic_id
    manifest['epic_id'] = "EPIC-CCN-004"
    
    # Write back
    with open(manifest_path, 'w') as f:
        json.dump(manifest, f, indent=2)
    
    print(f"✅ Fixed epic_id: {current_id} → EPIC-CCN-004")
    
    # Verify
    with open(manifest_path, 'r') as f:
        verify = json.load(f)
    
    if verify['epic_id'] == "EPIC-CCN-004":
        print("✅ Verification passed!")
        return True
    else:
        print(f"❌ Verification failed: {verify['epic_id']}")
        return False

if __name__ == "__main__":
    success = fix_epic_004_manifest()
    exit(0 if success else 1)

# Made with Bob
