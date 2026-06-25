#!/usr/bin/env python3
"""
Update Phase 0 scripts with API key rotation strategy.
Uses 16 working API keys from docs/API/ directory.
"""

import json
import glob
import os

# Load all 16 working API keys
API_KEYS = []
api_dir = "docs/API"

api_files = [
    "alprofit.json",
    "bob (5).json",
    "bob (6).json",
    "bob.json",
    "danfarah.json",
    "iyanajackson.json",
    "jessica.json",
    "jimmydore.json",
    "mikethelife.json",
    "pepeescobar.json",
    "rakaarababa.json",
    "ranirabah (1).json",
    "sammy96.json",
    "sean.carter.jr@atomicmail.io.json",
    "snyder.johnson.json",
    "tory.json"
]

print("Loading API keys...")
for api_file in api_files:
    path = os.path.join(api_dir, api_file)
    with open(path, 'r') as f:
        data = json.load(f)
        API_KEYS.append(data['apikey'])
        print(f"  ✓ Loaded {data['name']}")

print(f"\nTotal API keys loaded: {len(API_KEYS)}")

# Old revoked key to replace
OLD_KEY = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"

# Get all Phase 0 scripts
scripts = sorted(glob.glob("_p0_*.sh"))
print(f"\nFound {len(scripts)} Phase 0 scripts")

# Update each script with rotating API key
updated = 0
for idx, script in enumerate(scripts):
    # Rotate through API keys
    api_key = API_KEYS[idx % len(API_KEYS)]
    
    with open(script, 'r') as f:
        content = f.read()
    
    # Replace old key with new key
    if OLD_KEY in content:
        content = content.replace(OLD_KEY, api_key)
        
        with open(script, 'w') as f:
            f.write(content)
        
        updated += 1
        if updated <= 10 or updated % 20 == 0:
            print(f"  ✓ Updated {script} with API key #{(idx % len(API_KEYS)) + 1}")

print(f"\n✅ Updated {updated} scripts with rotating API keys")
print(f"   Each key will be used ~{len(scripts) // len(API_KEYS)} times")

# Show distribution
print("\nAPI Key Distribution:")
for i in range(len(API_KEYS)):
    count = len([s for idx, s in enumerate(scripts) if idx % len(API_KEYS) == i])
    print(f"  Key #{i+1}: {count} scripts")

# Made with Bob
