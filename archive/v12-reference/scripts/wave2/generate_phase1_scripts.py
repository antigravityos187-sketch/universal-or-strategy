#!/usr/bin/env python3
"""
Generate Phase 1 (Scope Definition) scripts for Wave 2 epics.
Based on Phase 0 pattern with --yolo flag and HARDCODED API keys.
"""

import os
import json

# Epic configuration with API key files
EPICS = [
    ("107", "b (2).json", "HydrateFromOpenPositions", 31),
    ("108", "b.json", "ProcessOnExecutionUpdate", 67),
    ("109", "bob (1).json", "HydrateFSMsFromWorkingOrders", 45),
    ("110", "bob (2).json", "HandleFlatPositionUpdate", 37),
    ("111", "bob (3).json", "AdoptFleetOrders", 37),
    ("112", "bob (4).json", "ClassifyOrderByPrefix", 17),
    ("113", "bob (5).json", "SweepBrokerOrders", 28),
    ("114", "bob (6).json", "FlattenSinglePosition", 27),
    ("115", "bob.json", "ExecuteRetestEntry", 26),
]

def load_api_key(api_file):
    """Load API key from JSON file."""
    json_path = os.path.join("docs", "API", api_file)
    with open(json_path, 'r') as f:
        data = json.load(f)
        return data['apikey']

SCRIPT_TEMPLATE = """#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/EPIC-CCN-{epic_id}
mkdir -p logs/phase1

cat > /tmp/phase1_msg_{epic_id}.txt << 'EOFMSG'
You are executing Phase 1 (Scope Definition) for EPIC-CCN-{epic_id}.

**Input Artifact**: Read `docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md` for hotspot analysis.

**Your Task**: Define the extraction scope based on the hotspot analysis.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-{epic_id}/00-scope.md` with:
   - Target method details
   - Extraction strategy (what to extract, what to keep)
   - Boundary definition (single method only, no scope creep)
   - Success criteria (target complexity <= 8)
   - Risk assessment

2. Update `docs/brain/EPIC-CCN-{epic_id}/manifest.json`:
   - Set phase "1" status to "completed"
   - Add "00-scope.md" to outputs

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Keep scope to single method (V12.23 No Scope Creep Protocol)
- Target complexity <= 8 (Jane Street alignment)

**Phase**: 1 (Scope Definition)
EOFMSG

bob --yolo --chat-mode plan "$(cat /tmp/phase1_msg_{epic_id}.txt)" 2>&1 | tee logs/phase1/EPIC-CCN-{epic_id}.log
echo "DONE_EXIT=$?"

# Made with Bob
"""

def generate_scripts():
    """Generate Phase 1 scripts for all epics with hardcoded API keys."""
    output_dir = "."
    
    for epic_id, api_file, method, complexity in EPICS:
        # Load API key from JSON file
        api_key = load_api_key(api_file)
        
        script_content = SCRIPT_TEMPLATE.format(
            epic_id=epic_id,
            api_key=api_key,
            method=method,
            complexity=complexity
        )
        
        script_path = os.path.join(output_dir, f"_p1_{epic_id}.sh")
        with open(script_path, 'w', encoding='utf-8', newline='\n') as f:
            f.write(script_content)
        
        # Make executable
        os.chmod(script_path, 0o755)
        
        print(f"[OK] Generated {script_path}")

if __name__ == "__main__":
    generate_scripts()
    print(f"\n[OK] Generated {len(EPICS)} Phase 1 scripts")

# Made with Bob
