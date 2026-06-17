#!/usr/bin/env python3
"""
Generate Phase 0 scripts for Wave 6 using building-blocks method
V12.52 Protocol - Clean Slate Execution
"""

import json
import os
from pathlib import Path

# Load epic roadmap
with open('epic_roadmap_wave4_fresh.json', 'r', encoding='utf-8-sig') as f:
    roadmap = json.load(f)

# Wave 6 scope: 001-026, 028-080 (excluding 024, 027)
wave6_epics = []
for epic in roadmap:
    epic_num = int(epic['epic_number'].split('-')[-1])
    if 1 <= epic_num <= 80:
        # Exclude 024 (local execution) and 027 (invalid target)
        if epic_num not in [24, 27]:
            wave6_epics.append(epic)

print(f"Generating Phase 0 scripts for {len(wave6_epics)} epics...")

# Read template
template_path = Path('building-blocks/autonomous-refactoring/phase0_template_v12_52.sh')
with open(template_path, 'r', encoding='utf-8') as f:
    template = f.read()

# Output directory
output_dir = Path('scripts/wave6')
output_dir.mkdir(exist_ok=True)

# Generate scripts
for epic in wave6_epics:
    epic_id = epic['epic_number']
    method = epic['method']
    file = epic['file']
    cyc = epic['cyclomatic']
    
    # Agent ID format: wave6-p0-{epic_number}
    agent_id = f"wave6-p0-{epic_id.split('-')[-1]}"
    
    # Replace placeholders
    script = template.replace('{EPIC_ID}', epic_id)
    script = script.replace('{AGENT_ID}', agent_id)
    script = script.replace('{METHOD}', method)
    script = script.replace('{FILE}', file)
    script = script.replace('{CYC}', str(cyc))
    
    # Add epic metadata as comments
    header = f"""#!/bin/bash
# Phase 0 (Hotspot Analysis) - Wave 6 Clean Slate
# Epic: {epic_id}
# Method: {method}
# File: {file}
# Cyclomatic: {cyc}
# Agent: {agent_id}
# V12.52 Protocol

"""
    
    # Replace shebang line with enhanced header
    script = script.replace('#!/bin/bash\n# Phase 0 (Hotspot Analysis) Template with V12.52 Lamport Causal Verification\n# Version: V12.52\n# Epic: {EPIC_ID}\n# Agent: {AGENT_ID}\n', header)
    
    # Write script
    script_name = f"_p0_{epic_id.lower().replace('-', '_')}.sh"
    script_path = output_dir / script_name
    
    with open(script_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(script)
    
    print(f"[OK] Generated {script_name}")

print(f"\n{'='*60}")
print(f"Phase 0 script generation complete!")
print(f"Total scripts: {len(wave6_epics)}")
print(f"Output directory: {output_dir}")
print(f"{'='*60}")

# Made with Bob
