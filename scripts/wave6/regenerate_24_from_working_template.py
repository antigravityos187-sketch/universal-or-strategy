#!/usr/bin/env python3
"""
Regenerate 24 Broken Phase 1 Scripts from Working Template
Created: 2026-06-18T04:40:00Z

Building-Blocks Method: Copy EPIC-002 (working), parameterize epic-specific values
"""

import os
from pathlib import Path

# Template: EPIC-CCN-002 (working script)
TEMPLATE_PATH = Path("scripts/wave6/_p1_epic_ccn_002_WORKING.sh")

# 24 broken epics
EPICS = [
    "001", "004", "016", "020", "021", "028",
    "050", "051", "052", "053", "054", "055", "056", "057", "058", "059",
    "060", "061", "070", "073", "076", "077", "078", "079"
]

def regenerate_script(epic_num: str, template_content: str) -> str:
    """Generate script for epic by replacing template values."""
    epic_id = f"EPIC-CCN-{epic_num}"
    agent_id = f"wave6-p1-{epic_num}"
    
    # Replace all occurrences
    content = template_content.replace("EPIC-CCN-002", epic_id)
    content = content.replace("wave6-p1-002", agent_id)
    
    return content

def main():
    """Regenerate all 24 scripts from working template."""
    
    # Read working template
    if not TEMPLATE_PATH.exists():
        print(f"❌ Template not found: {TEMPLATE_PATH}")
        return 1
    
    with open(TEMPLATE_PATH, 'r') as f:
        template_content = f.read()
    
    print("=" * 60)
    print("Regenerating 24 Phase 1 Scripts from Working Template")
    print("=" * 60)
    print(f"Template: {TEMPLATE_PATH}")
    print(f"Template size: {len(template_content)} bytes")
    print()
    
    regenerated = 0
    failed = 0
    
    for epic_num in EPICS:
        output_path = Path(f"scripts/wave6/_p1_epic_ccn_{epic_num}.sh")
        
        try:
            # Generate script content
            script_content = regenerate_script(epic_num, template_content)
            
            # Write to file
            with open(output_path, 'w') as f:
                f.write(script_content)
            
            # Make executable
            os.chmod(output_path, 0o755)
            
            print(f"✅ EPIC-CCN-{epic_num}: Regenerated ({len(script_content)} bytes)")
            regenerated += 1
            
        except Exception as e:
            print(f"❌ EPIC-CCN-{epic_num}: Error - {e}")
            failed += 1
    
    print()
    print("=" * 60)
    print(f"Regenerated: {regenerated}/24")
    print(f"Failed: {failed}/24")
    print("=" * 60)
    
    return 0 if failed == 0 else 1

if __name__ == "__main__":
    exit(main())

# Made with Bob
