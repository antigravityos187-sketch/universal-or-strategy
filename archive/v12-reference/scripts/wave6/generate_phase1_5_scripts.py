#!/usr/bin/env python3
"""
Generate Phase 1.5 scripts for all 79 Wave 6 epics
Building-blocks method: Copy template, modify EPIC_ID and AGENT_ID
"""

import os
import sys
from pathlib import Path

# Force UTF-8 encoding for Windows
if sys.platform == 'win32':
    sys.stdout.reconfigure(encoding='utf-8')

# Wave 6 scope: 80 epics, excluding EPIC-027
EPIC_RANGE = range(1, 81)
EXCLUDED_EPICS = [27]

# Agent mapping (from Phase 1 scripts)
AGENT_MAP = {
    1: "alprofit",
    2: "davidgreen77",
    3: "iyanajackson",
    4: "jessica",
    5: "mikethelife",
    6: "rakaarababa",
    7: "ranirabah",
    8: "sammy96",
    9: "sean.carter.jr@atomicmail.io",
    10: "tory",
}

def get_agent_id(epic_num):
    """Get agent ID for epic using round-robin"""
    return AGENT_MAP.get((epic_num % 10) or 10, "alprofit")

def generate_phase1_5_script(epic_num):
    """Generate Phase 1.5 script for a single epic"""
    epic_id = f"EPIC-CCN-{epic_num:03d}"
    agent_id = get_agent_id(epic_num)
    
    # Read template with UTF-8 encoding
    template_path = Path("building-blocks/autonomous-refactoring/phase1_5_template_v12_52.sh")
    with open(template_path, 'r', encoding='utf-8') as f:
        template = f.read()
    
    # Replace placeholders
    script = template.replace("EPIC-CCN-XXX", epic_id)
    script = script.replace("AGENT_ID", agent_id)
    
    # Write script with UTF-8 encoding
    output_path = Path(f"scripts/wave6/_p1_5_epic_ccn_{epic_num:03d}.sh")
    output_path.parent.mkdir(parents=True, exist_ok=True)
    
    with open(output_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(script)
    
    # Make executable (Unix only)
    if sys.platform != 'win32':
        os.chmod(output_path, 0o755)
    
    return output_path

def main():
    """Generate all Phase 1.5 scripts"""
    print("Generating Phase 1.5 scripts for Wave 6...")
    print(f"Target: {len([e for e in EPIC_RANGE if e not in EXCLUDED_EPICS])} epics")
    print()
    
    generated = []
    skipped = []
    
    for epic_num in EPIC_RANGE:
        if epic_num in EXCLUDED_EPICS:
            skipped.append(epic_num)
            print(f"SKIP EPIC-CCN-{epic_num:03d} - SKIPPED (excluded)")
            continue
        
        try:
            output_path = generate_phase1_5_script(epic_num)
            generated.append(epic_num)
            print(f"OK   EPIC-CCN-{epic_num:03d} - {output_path}")
        except Exception as e:
            print(f"ERR  EPIC-CCN-{epic_num:03d} - ERROR: {e}")
    
    print()
    print("=" * 50)
    print(f"Generated: {len(generated)} scripts")
    print(f"Skipped: {len(skipped)} epics (excluded)")
    print(f"Output: scripts/wave6/_p1_5_epic_ccn_*.sh")
    print("=" * 50)

if __name__ == "__main__":
    main()

# Made with Bob
