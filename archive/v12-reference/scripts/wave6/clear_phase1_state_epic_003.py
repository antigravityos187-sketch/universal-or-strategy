#!/usr/bin/env python3
"""Clear Phase 1 state_hash and agent_id for EPIC-CCN-003"""

import json

manifest_path = 'docs/brain/EPIC-CCN-003/manifest.json'

# Read manifest
with open(manifest_path, 'r') as f:
    manifest = json.load(f)

print(f"Phase 1 status: {manifest['phases']['1']['status']}")
print(f"Phase 1 state_hash: {manifest['phases']['1'].get('state_hash', 'None')}")
print(f"Phase 1 agent_id: {manifest['phases']['1'].get('agent_id', 'None')}")

# Clear state_hash and agent_id if present
if 'state_hash' in manifest['phases']['1']:
    del manifest['phases']['1']['state_hash']
    print("✅ Cleared state_hash")

if 'agent_id' in manifest['phases']['1']:
    del manifest['phases']['1']['agent_id']
    print("✅ Cleared agent_id")

# Write back
with open(manifest_path, 'w') as f:
    json.dump(manifest, f, indent=2)

print("✅ Manifest updated - ready for Phase 1 execution")

# Made with Bob
