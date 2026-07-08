#!/usr/bin/env python3
"""
Generate Phase 2 scripts for Wave 7 using Building-Blocks Method
V12.52 - Architecture Planning
"""

import os
import sys
from pathlib import Path

def get_epics_needing_phase2():
    """Find all epics with Phase 1.5 complete but Phase 2 not started"""
    brain_dir = Path("docs/brain")
    epics = []
    
    for epic_dir in sorted(brain_dir.glob("EPIC-W7-*")):
        epic_id = epic_dir.name
        
        # Check if Phase 1.5 is complete
        boundary_file = epic_dir / "01-scope-boundary.md"
        if not boundary_file.exists():
            continue
            
        # Check if Phase 2 is not yet complete
        arch_file = epic_dir / "02-architecture-plan.md"
        if arch_file.exists():
            continue
            
        epics.append(epic_id)
    
    return epics

def generate_phase2_script(epic_id, agent_id):
    """Generate Phase 2 script for a single epic using template"""
    
    # Read template
    template_path = Path("building-blocks/wave7/phase2_template_wave7.sh")
    with open(template_path, 'r') as f:
        template = f.read()
    
    # Replace placeholders
    script = template.replace("{EPIC_ID}", epic_id)
    script = script.replace("{AGENT_ID}", agent_id)
    
    # Extract epic number for filename
    epic_num = epic_id.split("-")[-1]
    
    # Write script
    script_path = Path(f"_p2_{epic_num}.sh")
    with open(script_path, 'w') as f:
        f.write(script)
    
    # Make executable
    os.chmod(script_path, 0o755)
    
    return script_path

def main():
    print("=" * 60)
    print("Wave 7 Phase 2 Script Generation")
    print("Building-Blocks Method: Copy from phase2_template_wave7.sh")
    print("=" * 60)
    print()
    
    # Get epics needing Phase 2
    epics = get_epics_needing_phase2()
    print(f"Found {len(epics)} epics needing Phase 2 execution")
    print()
    
    if not epics:
        print("✅ No epics need Phase 2 - all complete!")
        return 0
    
    # Generate scripts
    print("Generating Phase 2 scripts...")
    generated = []
    
    for i, epic_id in enumerate(epics, 1):
        agent_id = f"phase2-agent-{i:03d}"
        script_path = generate_phase2_script(epic_id, agent_id)
        generated.append((epic_id, script_path))
        
        if i % 20 == 0:
            print(f"  Generated {i}/{len(epics)} scripts...")
    
    print(f"✅ Generated {len(generated)} Phase 2 scripts")
    print()
    
    # Show first 5 and last 5
    print("Sample scripts generated:")
    for epic_id, script_path in generated[:5]:
        print(f"  {script_path} -> {epic_id}")
    if len(generated) > 10:
        print("  ...")
        for epic_id, script_path in generated[-5:]:
            print(f"  {script_path} -> {epic_id}")
    
    print()
    print("=" * 60)
    print(f"✅ Phase 2 script generation complete: {len(generated)} scripts")
    print("=" * 60)
    
    return 0

if __name__ == "__main__":
    sys.exit(main())

# Made with Bob
