#!/usr/bin/env python3
"""
Update all Wave 7 Phase 0 scripts with fresh API keys from docs/API/
Rotates through 16 available keys to distribute bobcoin usage
"""

import json
import os
import re
from pathlib import Path

def load_api_keys():
    """Load all API keys from docs/API/*.json"""
    api_dir = Path("docs/API")
    keys = []
    
    for json_file in sorted(api_dir.glob("*.json")):
        with open(json_file, 'r') as f:
            data = json.load(f)
            keys.append({
                'name': data['name'],
                'key': data['apikey'],
                'file': json_file.name
            })
    
    print(f"Loaded {len(keys)} API keys from docs/API/")
    for i, k in enumerate(keys, 1):
        print(f"  {i}. {k['name']} ({k['file']})")
    
    return keys

def update_script(script_path, new_key):
    """Replace API key in a Phase 0 script"""
    with open(script_path, 'r') as f:
        content = f.read()
    
    # Replace the BOBSHELL_API_KEY line
    pattern = r"export BOBSHELL_API_KEY='bob_prod_bob-admin_[^']+'"
    replacement = f"export BOBSHELL_API_KEY='{new_key}'"
    
    new_content = re.sub(pattern, replacement, content)
    
    if new_content == content:
        print(f"  ⚠️  No API key found in {script_path}")
        return False
    
    with open(script_path, 'w') as f:
        f.write(new_content)
    
    return True

def main():
    print("=" * 80)
    print("WAVE 7 API KEY UPDATE")
    print("=" * 80)
    print()
    
    # Load API keys
    api_keys = load_api_keys()
    if not api_keys:
        print("❌ No API keys found in docs/API/")
        return 1
    
    print()
    
    # Load remaining epics
    with open("wave7_remaining_epics.txt", 'r') as f:
        remaining_epics = [line.strip() for line in f if line.strip()]
    
    print(f"Found {len(remaining_epics)} remaining epics to update")
    print()
    
    # Update scripts with rotating keys
    updated = 0
    failed = 0
    
    for idx, epic_id in enumerate(remaining_epics):
        # Extract epic number from EPIC-W7-XXX
        epic_num = int(epic_id.split('-')[-1])
        script_path = f"_p0_{epic_num:03d}.sh"
        
        if not os.path.exists(script_path):
            print(f"⚠️  Script not found: {script_path}")
            failed += 1
            continue
        
        # Rotate through API keys
        key_idx = idx % len(api_keys)
        api_key = api_keys[key_idx]['key']
        api_name = api_keys[key_idx]['name']
        
        if update_script(script_path, api_key):
            if idx < 10 or idx % 10 == 0:  # Show first 10 and every 10th
                print(f"✅ {script_path} → {api_name}")
            updated += 1
        else:
            print(f"❌ Failed to update {script_path}")
            failed += 1
    
    print()
    print("=" * 80)
    print("UPDATE COMPLETE")
    print("=" * 80)
    print(f"Updated: {updated}/{len(remaining_epics)}")
    print(f"Failed: {failed}/{len(remaining_epics)}")
    print()
    print(f"API key distribution: {len(api_keys)} keys rotating")
    print(f"Epics per key: ~{len(remaining_epics) // len(api_keys)}")
    print()
    
    if updated == len(remaining_epics):
        print("✅ All scripts updated successfully!")
        print("Ready to run: ./pilot_wave7_phase0.sh")
    else:
        print(f"⚠️  {failed} scripts failed to update")
    
    return 0 if failed == 0 else 1

if __name__ == "__main__":
    exit(main())

# Made with Bob
