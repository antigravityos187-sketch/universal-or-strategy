#!/usr/bin/env python3
"""Add Phase 1 to EPIC-024 manifest."""

import json
from pathlib import Path

manifest_path = Path('docs/brain/EPIC-CCN-024/manifest.json')
manifest = json.load(open(manifest_path))

# Add Phase 1 to manifest
manifest['phases']['1'] = {
    'status': 'pending',
    'dependencies': ['0'],
    'mode': 'v12-phase1-scope',
    'mcp_tools': ['jcodemunch-mcp', 'sequential-thinking', 'graphify']
}
manifest['dependencies']['1'] = ['0']

# Write back
with open(manifest_path, 'w') as f:
    json.dump(manifest, f, indent=2)

print('✅ Phase 1 added to manifest')

# Made with Bob
