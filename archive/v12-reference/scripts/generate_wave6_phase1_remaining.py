#!/usr/bin/env python3
"""
Generate Phase 1 scripts for remaining 24 Wave 6 epics
Building-blocks method: Copy phase1_template_v12_52.sh, replace placeholders
"""

import os
from pathlib import Path

# Missing epics (24 total)
MISSING_EPICS = [
    1, 4, 16, 20, 21, 28, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
    60, 61, 70, 73, 76, 77, 78, 79
]

# API rotation (10 keys, cycle through)
API_KEYS = [
    "alprofit", "bob", "b", "davidgreen77", "bob2", "bob3", "bob4", "bob5", "bob6", "bob7"
]

def generate_phase1_script(epic_num: int, agent_id: str) -> str:
    """Generate Phase 1 script from template"""
    epic_id = f"EPIC-CCN-{epic_num:03d}"
    
    # Read template
    template_path = Path("building-blocks/autonomous-refactoring/phase1_template_v12_52.sh")
    with open(template_path, encoding='utf-8') as f:
        template = f.read()
    
    # Replace placeholders
    script = template.replace("{EPIC_ID}", epic_id)
    script = script.replace("{AGENT_ID}", agent_id)
    
    return script

def main():
    """Generate all Phase 1 scripts"""
    output_dir = Path("scripts/wave6")
    output_dir.mkdir(parents=True, exist_ok=True)
    
    print(f"Generating Phase 1 scripts for {len(MISSING_EPICS)} epics...")
    
    for i, epic_num in enumerate(MISSING_EPICS):
        epic_id = f"EPIC-CCN-{epic_num:03d}"
        agent_id = API_KEYS[i % len(API_KEYS)]
        
        script_content = generate_phase1_script(epic_num, agent_id)
        
        output_file = output_dir / f"_p1_epic_ccn_{epic_num:03d}.sh"
        with open(output_file, 'w', encoding='utf-8', newline='\n') as f:
            f.write(script_content)
        
        # Make executable
        os.chmod(output_file, 0o755)
        
        print(f"  [OK] {epic_id} -> {agent_id}")
    
    print(f"\n[OK] Generated {len(MISSING_EPICS)} Phase 1 scripts in {output_dir}")
    print(f"\nNext: Upload to VM and launch")

if __name__ == "__main__":
    main()

# Made with Bob
