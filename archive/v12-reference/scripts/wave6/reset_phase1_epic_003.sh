#!/bin/bash
# Reset Phase 1 status for EPIC-CCN-003

set -e

EPIC_ID="EPIC-CCN-003"
MANIFEST_PATH="docs/brain/$EPIC_ID/manifest.json"

echo "Resetting Phase 1 status for $EPIC_ID..."

python3 << 'EOFPY'
import json

manifest_path = 'docs/brain/EPIC-CCN-003/manifest.json'

# Read manifest
with open(manifest_path, 'r') as f:
    manifest = json.load(f)

# Show current status
print(f"Current Phase 1 status: {manifest['phases']['1']['status']}")

# Reset to pending
manifest['phases']['1']['status'] = 'pending'

# Write back
with open(manifest_path, 'w') as f:
    json.dump(manifest, f, indent=2)

print(f"✅ Reset Phase 1 status to: pending")
EOFPY

echo "✅ Phase 1 reset complete"

# Made with Bob
