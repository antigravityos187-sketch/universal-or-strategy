#!/usr/bin/env python3
"""
Regenerate all Phase 1.5 scripts using FIXED template (CLI pattern, not inline Python)
"""

import json
from pathlib import Path

# Load epic roadmap
roadmap_path = Path("epic_roadmap.json")
with open(roadmap_path) as f:
    roadmap = json.load(f)

# Load FIXED template
template_path = Path("building-blocks/autonomous-refactoring/phase1_5_template_v12_52_FIXED.sh")
with open(template_path, encoding='utf-8') as f:
    template = f.read()

# Get in-scope epics (all except EPIC-CCN-027)
in_scope_epics = [
    epic for epic in roadmap
    if epic["epic_number"] != "EPIC-CCN-027"
]

print(f"Regenerating {len(in_scope_epics)} Phase 1.5 scripts with FIXED template...")

output_dir = Path("scripts/wave6")
output_dir.mkdir(exist_ok=True)

for epic in in_scope_epics:
    epic_id = epic["epic_number"]
    agent_id = "alprofit"  # Default agent for all Wave 6 epics
    
    # Generate script from FIXED template
    script_content = template.replace("{EPIC_ID}", epic_id).replace("{AGENT_ID}", agent_id)
    
    # Write script
    script_path = output_dir / f"_p1_5_{epic_id.lower().replace('-', '_')}.sh"
    with open(script_path, "w", encoding='utf-8', newline="\n") as f:
        f.write(script_content)
    
    print(f"[OK] {script_path.name}")

print(f"\n[OK] Regenerated {len(in_scope_epics)} Phase 1.5 scripts")
print("\nNext steps:")
print("1. Upload all scripts to VM")
print("2. Run pilot test with EPIC-CCN-001 and EPIC-CCN-002")
print("3. Verify pilot success before launching remaining 77")

# Made with Bob
