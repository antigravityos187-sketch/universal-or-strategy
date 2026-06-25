#!/usr/bin/env python3
"""
Generate missing Phase 0 scripts for Wave 7 using Building-Blocks Method.
Copies from _p0_001.sh (actual working script), not the template.
"""

import json
import os
import sys
import re

def load_roadmap():
    """Load epic roadmap."""
    with open('epic_roadmap.json', 'r') as f:
        return json.load(f)

def extract_epic_number(epic_number_field):
    """Extract numeric epic number from various formats."""
    if isinstance(epic_number_field, int):
        return epic_number_field
    
    # Handle string formats like "EPIC-CCN-14" or "14"
    if isinstance(epic_number_field, str):
        # Try to extract number from "EPIC-CCN-XX" format
        match = re.search(r'(\d+)', epic_number_field)
        if match:
            return int(match.group(1))
    
    return None

def generate_phase0_script(epic_data, template):
    """Generate Phase 0 script from working template and epic data."""
    epic_num = extract_epic_number(epic_data['epic_number'])
    if epic_num is None:
        raise ValueError(f"Cannot extract epic number from: {epic_data['epic_number']}")
    
    epic_id = f"EPIC-CCN-{epic_num:03d}"
    epic_id_padded = f"{epic_num:03d}"
    
    method = epic_data['method']
    file = epic_data['file']
    cyc = epic_data['cyclomatic']
    
    # Replace all occurrences in template
    script = template.replace('EPIC-CCN-001', epic_id)
    script = script.replace('_001', f'_{epic_id_padded}')
    script = script.replace('/001', f'/{epic_id_padded}')
    script = script.replace('SymmetryGuardReplaceExistingFollowerTarget', method)
    script = script.replace('src/V12_002.Symmetry.Replace.cs', file)
    script = script.replace('Complexity: 18', f'Complexity: {cyc}')
    script = script.replace('"complexity": 18', f'"complexity": {cyc}')
    
    return script

def main():
    """Generate all missing Phase 0 scripts."""
    print("=== Generate Missing Phase 0 Scripts ===")
    print("Using Building-Blocks Method (copy from _p0_001.sh)")
    print()
    
    # Load working template (actual script, not building-blocks template)
    print("Step 1: Load working script template...")
    template_path = '_p0_001.sh'
    if not os.path.exists(template_path):
        print(f"❌ Template not found: {template_path}")
        return 1
    
    with open(template_path, 'r') as f:
        template = f.read()
    print(f"✅ Template loaded: {template_path}")
    print()
    
    # Load roadmap
    print("Step 2: Load epic roadmap...")
    roadmap = load_roadmap()
    print(f"✅ Roadmap loaded: {len(roadmap)} epics")
    print()
    
    # Identify missing scripts
    print("Step 3: Identify missing scripts...")
    missing_ranges = [
        range(81, 107),   # EPIC-CCN-081 through 106 (26 epics)
        range(116, 162)   # EPIC-CCN-116 through 161 (46 epics)
    ]
    
    missing_epics = []
    for r in missing_ranges:
        missing_epics.extend(list(r))
    
    print(f"Missing scripts: {len(missing_epics)} epics")
    print(f"  Range 1: EPIC-CCN-081 through 106 ({len(list(range(81, 107)))} epics)")
    print(f"  Range 2: EPIC-CCN-116 through 161 ({len(list(range(116, 162)))} epics)")
    print()
    
    # Generate scripts
    print("Step 4: Generate scripts...")
    generated = 0
    errors = []
    
    for epic_num in missing_epics:
        # Find epic data in roadmap
        epic_data = None
        for epic in roadmap:
            extracted_num = extract_epic_number(epic['epic_number'])
            if extracted_num == epic_num:
                epic_data = epic
                break
        
        if not epic_data:
            errors.append(f"EPIC-CCN-{epic_num:03d}: Not found in roadmap")
            continue
        
        # Generate script
        script_path = f"_p0_{epic_num:03d}.sh"
        
        # Skip if already exists
        if os.path.exists(script_path):
            print(f"  ⏭️  {script_path} already exists - skipping")
            continue
        
        try:
            script_content = generate_phase0_script(epic_data, template)
            
            with open(script_path, 'w') as f:
                f.write(script_content)
            
            # Make executable
            os.chmod(script_path, 0o755)
            
            generated += 1
            print(f"  ✅ {script_path} generated")
            
        except Exception as e:
            errors.append(f"{script_path}: {str(e)}")
            print(f"  ❌ {script_path} failed: {e}")
    
    print()
    print("=== Generation Complete ===")
    print(f"Generated: {generated} scripts")
    print(f"Errors: {len(errors)}")
    
    if errors:
        print()
        print("Errors:")
        for error in errors:
            print(f"  - {error}")
        return 1
    
    # Verify total count
    print()
    print("Step 5: Verify total script count...")
    total_scripts = len([f for f in os.listdir('.') if f.startswith('_p0_') and f.endswith('.sh')])
    print(f"Total Phase 0 scripts: {total_scripts}/161")
    
    if total_scripts == 161:
        print("✅ All 161 Phase 0 scripts present!")
        return 0
    else:
        print(f"⚠️  Expected 161 scripts, found {total_scripts}")
        return 1

if __name__ == '__main__':
    sys.exit(main())

# Made with Bob
