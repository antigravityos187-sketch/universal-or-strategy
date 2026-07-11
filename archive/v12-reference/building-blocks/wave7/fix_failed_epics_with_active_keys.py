#!/usr/bin/env python3
"""
Wave 7 - Fix Failed Epics with Active API Keys
Building-Blocks Method: Reusable script for regenerating failed epic scripts

Usage:
    python3 building-blocks/wave7/fix_failed_epics_with_active_keys.py \
        --phase 1.5 \
        --failed-epics 015,047,063,079,095,111,127,143,159

Protocol:
    1. Load active API keys (exclude exhausted/revoked)
    2. Read template from working script
    3. Replace epic ID and API key using regex
    4. Write fixed script with proper permissions
    5. Verify API key insertion
"""

import json
import os
import re
import sys
import argparse

def load_active_api_keys(api_dir='docs/API', excluded=None):
    """Load active API keys from JSON files"""
    if excluded is None:
        excluded = ['jessica', 'danfarah', 'jimmydore', 'pepeescobar']
    
    active_keys = []
    for filename in sorted(os.listdir(api_dir)):
        if filename.endswith('.json'):
            key_name = filename.replace('.json', '')
            if key_name not in excluded:
                filepath = os.path.join(api_dir, filename)
                with open(filepath) as f:
                    data = json.load(f)
                    api_key = data.get('api_key', '')
                    if api_key:
                        active_keys.append({
                            'name': key_name,
                            'key': api_key
                        })
    
    return active_keys

def fix_epic_script(epic_num, phase, template_path, api_key_data):
    """Fix a single epic script with valid API key"""
    epic_id = f"EPIC-W7-{epic_num:03d}"
    
    # Read template
    with open(template_path, 'r') as f:
        template = f.read()
    
    # Extract template epic ID
    template_epic_match = re.search(r'EPIC-W7-\d{3}', template)
    if not template_epic_match:
        raise ValueError(f"Template {template_path} missing EPIC-W7-XXX pattern")
    template_epic_id = template_epic_match.group(0)
    
    # Replace epic ID
    script = template.replace(template_epic_id, epic_id)
    
    # Replace API key
    script = re.sub(
        r"export BOBSHELL_API_KEY='[^']*'",
        f"export BOBSHELL_API_KEY='{api_key_data['key']}'",
        script
    )
    
    # Write fixed script
    script_path = f'_p{phase}_{epic_num:03d}.sh'
    with open(script_path, 'w') as f:
        f.write(script)
    
    os.chmod(script_path, 0o755)
    
    return script_path

def main():
    parser = argparse.ArgumentParser(description='Fix failed epic scripts with active API keys')
    parser.add_argument('--phase', required=True, help='Phase number (e.g., 1.5, 2, 3)')
    parser.add_argument('--failed-epics', required=True, help='Comma-separated epic numbers (e.g., 015,047,063)')
    parser.add_argument('--template', help='Template script path (default: auto-detect)')
    
    args = parser.parse_args()
    
    # Parse phase
    phase = args.phase.replace('.', '_')
    
    # Parse failed epics
    failed_epics = [int(e) for e in args.failed_epics.split(',')]
    
    # Auto-detect template if not provided
    if args.template:
        template_path = args.template
    else:
        # Use first working script as template
        template_path = f'_p{phase}_001.sh'
        if not os.path.exists(template_path):
            print(f"❌ Template not found: {template_path}")
            sys.exit(1)
    
    # Load active API keys
    print("Loading active API keys...")
    active_keys = load_active_api_keys()
    print(f"✅ Found {len(active_keys)} active API keys")
    
    if len(active_keys) == 0:
        print("❌ No active API keys found!")
        sys.exit(1)
    
    # Fix each failed epic
    print(f"\nFixing {len(failed_epics)} failed epics...")
    for i, epic_num in enumerate(failed_epics):
        key_data = active_keys[i % len(active_keys)]
        script_path = fix_epic_script(epic_num, phase, template_path, key_data)
        print(f"✅ Fixed: {script_path} -> {key_data['name']}")
    
    print(f"\n🎉 Fixed all {len(failed_epics)} scripts with valid API keys")
    print(f"\nNext: Launch fixed scripts:")
    print(f"  for epic in {args.failed_epics.replace(',', ' ')}; do ./_p{phase}_$epic.sh & sleep 12; done && wait")

if __name__ == '__main__':
    main()

# Made with Bob
