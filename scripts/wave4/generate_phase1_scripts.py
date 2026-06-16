#!/usr/bin/env python3
"""
Wave 4 Phase 1 Script Generator
Generates individual epic scripts for Phase 1 (Scope + Boundary)

BUILDING-BLOCKS METHOD: Copied from Phase 0, modified only phase-specific parameters
"""

import json
import os

# Load 15 API keys from docs/API/*.json files (COPIED FROM PHASE 0)
API_FILES = [
    "bob.json", "bob (1).json", "bob (2).json", "bob (3).json",
    "bob (4).json", "bob (5).json", "bob (6).json",
    "b.json", "b (2).json",
    "jessica.json", "mikethelife.json", "sammy96.json",
    "sean.carter.jr@atomicmail.io.json", "tory.json", "b (3).json"
]

def load_api_keys():
    """Load all 15 API keys from JSON files."""
    api_keys = []
    for api_file in API_FILES:
        json_path = os.path.join("docs", "API", api_file)
        try:
            with open(json_path, 'r') as f:
                data = json.load(f)
                api_keys.append(data['apikey'])
        except Exception as e:
            print(f"[ERROR] Failed to load {api_file}: {e}")
    return api_keys

# Load epic roadmap
with open('epic_roadmap_wave4_fresh.json', 'r', encoding='utf-8-sig') as f:
    roadmap = json.load(f)

# Load API keys
api_keys = load_api_keys()

if len(api_keys) != 15:
    print(f"[ERROR] Expected 15 API keys, got {len(api_keys)}")
    exit(1)

# Phase 1 configuration (ONLY CHANGES FROM PHASE 0)
PHASE = "1"
MODE = "plan"
COMMAND = "epic-scope-boundary"

# Create output directory
os.makedirs('scripts/wave4', exist_ok=True)

# Generate individual epic scripts
for i, epic in enumerate(roadmap):
    epic_id = epic['epic_number']
    epic_num = epic_id.split('-')[-1]  # Extract "001" from "EPIC-CCN-001"
    
    # Round-robin API allocation (COPIED FROM PHASE 0)
    api_index = i % 15
    api_key = api_keys[api_index]
    
    # Generate script content (COPIED FROM PHASE 0 PATTERN)
    script_content = f"""#!/bin/bash
EPIC_ID="{epic_id}"
API_KEY="{api_key}"
MODE="{MODE}"
COMMAND="{COMMAND}"

~/.npm-global/bin/bob ${{MODE}} ${{COMMAND}} ${{EPIC_ID}} "Complexity reduction" --api-key ${{API_KEY}}
"""
    
    # Write script
    script_path = f'scripts/wave4/_p{PHASE}_{epic_num}.sh'
    with open(script_path, 'w') as f:
        f.write(script_content)
    
    # Make executable
    os.chmod(script_path, 0o755)
    
    print(f"[OK] Generated {script_path} (API {api_index + 1}/15)")

print(f"\n✅ Generated {len(roadmap)} Phase {PHASE} scripts")
print(f"Mode: {MODE}")
print(f"Command: {COMMAND}")
print(f"API Keys: {len(api_keys)} (round-robin allocation)")

# Made with Bob
