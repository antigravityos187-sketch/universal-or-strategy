#!/usr/bin/env python3
"""
Single Epic Test - EPIC-CCN-107

Tests the Phase 0 workflow with proper epic data population
before launching all Wave 2 epics.
"""

import json
from pathlib import Path

def load_epic_roadmap():
    """Load epic roadmap data"""
    roadmap_path = Path("epic_roadmap.json")
    with open(roadmap_path, 'r', encoding='utf-8') as f:
        return json.load(f)

def get_epic_data(roadmap, epic_id):
    """Extract epic data from roadmap"""
    epic_key = f"EPIC-CCN-{epic_id}"
    for epic in roadmap:
        if epic.get("epic_number") == epic_key:
            return {
                "epic_id": epic_key,
                "method": epic["method"],
                "file": epic["file"],
                "cyc": epic["cyclomatic"]
            }
    raise ValueError(f"Epic {epic_key} not found in roadmap")

def load_template():
    """Load message template"""
    template_path = Path("scripts/wave2/phase0_message_template_shell.txt")
    with open(template_path, 'r', encoding='utf-8') as f:
        return f.read()

def populate_template(template, epic_data):
    """Fill in template placeholders with epic data"""
    # Use replace() instead of format() to avoid JSON curly brace conflicts
    result = template
    result = result.replace("{EPIC_ID}", epic_data["epic_id"])
    result = result.replace("{METHOD}", epic_data["method"])
    result = result.replace("{FILE}", epic_data["file"])
    result = result.replace("{CYC}", str(epic_data["cyc"]))
    return result

def generate_test_script(epic_data, message):
    """Generate test script for EPIC-CCN-107"""
    
    script = f'''#!/bin/bash
# Test Script for EPIC-CCN-107
# Tests Phase 0 workflow with proper epic data population

set -e

EPIC_ID="{epic_data["epic_id"]}"
API_KEY="b (2).json"
LOG_DIR="/home/malhitticrypto/universal-or-strategy/logs/phase0"

echo "========================================="
echo "Testing Phase 0 for $EPIC_ID"
echo "========================================="
echo "Method: {epic_data["method"]}"
echo "File: {epic_data["file"]}"
echo "Complexity: {epic_data["cyc"]}"
echo "API Key: $API_KEY"
echo ""

# Create log directory
mkdir -p "$LOG_DIR"

# Set API key environment variable
export BOBSHELL_API_KEY=$(cat "/home/malhitticrypto/universal-or-strategy/docs/API/$API_KEY" | jq -r '.api_key')

# Launch Bob Shell in detached screen session
screen -dmS "test-p0-107" bash -c "
    cd /home/malhitticrypto/universal-or-strategy && \\
    bob --mode v12-phase0-hotspot --message '{message.replace("'", "'\\''")}' \\
    2>&1 | tee $LOG_DIR/$EPIC_ID-test.log
"

echo "✓ Launched test agent in screen session: test-p0-107"
echo ""
echo "Monitor with:"
echo "  screen -r test-p0-107"
echo "  tail -f $LOG_DIR/$EPIC_ID-test.log"
echo ""
echo "Expected outputs:"
echo "  docs/brain/$EPIC_ID/00-hotspots.md"
echo "  docs/brain/$EPIC_ID/manifest.json"
echo ""
echo "Verify with:"
echo "  ls -lh docs/brain/$EPIC_ID/"
echo "  cat docs/brain/$EPIC_ID/00-hotspots.md"
'''
    
    return script

def main():
    """Generate test script for EPIC-CCN-107"""
    
    print("Loading epic roadmap...")
    roadmap = load_epic_roadmap()
    
    print("Getting data for EPIC-CCN-107...")
    epic_data = get_epic_data(roadmap, 107)
    
    print(f"\nEpic Data:")
    print(f"  ID: {epic_data['epic_id']}")
    print(f"  Method: {epic_data['method']}")
    print(f"  File: {epic_data['file']}")
    print(f"  Complexity: {epic_data['cyc']}")
    
    print("\nLoading message template...")
    template = load_template()
    
    print("Populating template with epic data...")
    message = populate_template(template, epic_data)
    
    print("\nGenerating test script...")
    script = generate_test_script(epic_data, message)
    
    # Write script
    script_path = "test_phase0_epic_107.sh"
    with open(script_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(script)
    
    import os
    os.chmod(script_path, 0o755)
    
    print(f"[OK] Generated {script_path}")
    
    print("\n" + "="*50)
    print("SUCCESS! Test script generated.")
    print("="*50)
    print("\nNext steps:")
    print("1. Upload to VM:")
    print("   scp test_phase0_epic_107.sh malhitticrypto@v12-test-golden-v2:~/universal-or-strategy/")
    print("\n2. On VM, run test:")
    print("   ./test_phase0_epic_107.sh")
    print("\n3. Monitor:")
    print("   screen -r test-p0-107")
    print("   tail -f logs/phase0/EPIC-CCN-107-test.log")
    print("\n4. Verify outputs:")
    print("   ls -lh docs/brain/EPIC-CCN-107/")
    print("   cat docs/brain/EPIC-CCN-107/00-hotspots.md")
    print("\n5. If successful, generate all Wave 2 scripts:")
    print("   python scripts/wave2/launch_phase0_v5_with_epic_data.py")

if __name__ == "__main__":
    main()

# Made with Bob
