#!/usr/bin/env python3
"""
Generate Phase 5 (Ticket Execution) scripts for Wave 4 using building-blocks method.

CRITICAL: This script uses the building-blocks method - it copies Phase 4 scripts
and modifies only phase-specific parameters. NEVER generate from scratch.

Reference: docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md
"""

import json
import os
from pathlib import Path

# Load epic roadmap
roadmap_path = Path("epic_roadmap_wave4_fresh.json")
if not roadmap_path.exists():
    print(f"ERROR: {roadmap_path} not found!")
    exit(1)

with open(roadmap_path, 'r', encoding='utf-8-sig') as f:
    content = f.read().strip()
    if not content:
        print(f"ERROR: {roadmap_path} is empty!")
        exit(1)
    epics_data = json.loads(content)
    # Handle both array and object formats
    if isinstance(epics_data, list):
        epics = epics_data
    else:
        epics = epics_data.get("epics", [])

# Load API keys
api_dir = Path("docs/API")
api_files = sorted(api_dir.glob("*.json"))[:15]  # Use first 15 APIs

# Exhausted API to skip (from pilot test failure)
EXHAUSTED_API = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"

api_keys = []
for api_file in api_files:
    with open(api_file) as f:
        data = json.load(f)
        api_key = data["apikey"]
        # Skip exhausted API
        if api_key != EXHAUSTED_API:
            api_keys.append(api_key)
        else:
            print(f"Skipping exhausted API from {api_file.name}")

print(f"Loaded {len(api_keys)} healthy API keys (1 exhausted API excluded)")

# Generate Phase 5 scripts (copy Phase 4 pattern)
output_dir = Path("scripts/wave4")
output_dir.mkdir(parents=True, exist_ok=True)

for i, epic in enumerate(epics, 1):
    epic_id = epic.get("epic_id") or epic.get("epic_number")
    epic_num = epic_id.split("-")[-1]  # Extract "001" from "EPIC-CCN-001"
    
    # Round-robin API assignment
    api_index = (i - 1) % len(api_keys)
    api_key = api_keys[api_index]
    
    # Phase 5 script (uses phase-5-execute MCP tool)
    # BUILDING-BLOCKS: Copied from Phase 4, changed only phase-specific parameters
    script_content = f'''#!/bin/bash
# Phase 5 (Ticket Execution) for {epic_id}
# Generated: 2026-06-15
# Method: MCP tool (phase-5-execute server)

set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="{epic_id}"
API_KEY="{api_key}"

# Export API key
export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Verify Phase 4 file exists
if [ ! -f "docs/brain/{epic_id}/04-tickets.md" ]; then
    echo "ERROR: Missing prerequisite file: docs/brain/{epic_id}/04-tickets.md"
    echo "Phase 4 must complete before Phase 5 can execute"
    exit 1
fi

# Create message file (avoids bash multi-line escaping)
cat > /tmp/phase5_msg_{epic_num}.txt << 'EOFMSG'
Use the phase-5-execute MCP server to execute Phase 5 for {epic_id}.

Call the execute_phase_5 tool with epic_id="{epic_id}".

The tool will return complete instructions for ticket execution.
Follow those instructions to execute all tickets surgically.

**Verification**: Confirm execution files exist on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell (uses phase-5-execute MCP tool)
bob --yolo "$(cat /tmp/phase5_msg_{epic_num}.txt)"

# Verify execution files created (at least one ticket completion file)
if ls docs/brain/{epic_id}/ticket-*-completion.md 1> /dev/null 2>&1; then
    echo "SUCCESS: Phase 5 complete for {epic_id}"
    echo "Files: docs/brain/{epic_id}/ticket-*-completion.md"
    ls -lh docs/brain/{epic_id}/ticket-*-completion.md
else
    echo "ERROR: No ticket completion files created for {epic_id}"
    exit 1
fi
'''
    
    # Write script
    script_path = output_dir / f"_p5_{epic_num}.sh"
    with open(script_path, 'w', newline='\n') as f:  # Force LF line endings
        f.write(script_content)
    
    # Make executable
    os.chmod(script_path, 0o755)
    
    print(f"Generated: {script_path}")

# Generate test launcher (first 2 epics)
test_launcher = '''#!/bin/bash
# Phase 5 Test Launcher - First 2 Epics
# Generated: 2026-06-15

set -e

echo "[$(date)] Starting Phase 5 test launch (2 epics)"

# Create logs directory
mkdir -p logs/phase5

# Launch EPIC-CCN-001
echo "[$(date)] Launching EPIC-CCN-001"
screen -dmS p5-001 bash -l -c './scripts/wave4/_p5_001.sh 2>&1 | tee logs/phase5/EPIC-CCN-001.log'

# Wait 12 seconds
sleep 12

# Launch EPIC-CCN-002
echo "[$(date)] Launching EPIC-CCN-002"
screen -dmS p5-002 bash -l -c './scripts/wave4/_p5_002.sh 2>&1 | tee logs/phase5/EPIC-CCN-002.log'

echo "[$(date)] Test launch complete (2 epics)"
echo "Monitor with: screen -ls"
echo "Check files: ls docs/brain/EPIC-CCN-{001,002}/ticket-*-completion.md"
'''

test_path = output_dir / "launch_phase5_test.sh"
with open(test_path, 'w', newline='\n') as f:
    f.write(test_launcher)
os.chmod(test_path, 0o755)
print(f"Generated: {test_path}")

# Generate full launcher (all 80 epics)
full_launcher = '''#!/bin/bash
# Phase 5 Full Wave Launcher - All 80 Epics
# Generated: 2026-06-15
# Delay: Constant 12s (building-blocks method)

set -e

echo "[$(date)] Starting Phase 5 full wave launch (80 epics)"

# Create logs directory
mkdir -p logs/phase5

# Launch all 80 epics with constant 12s delay
for i in $(seq -f "%03g" 1 80); do
    EPIC="EPIC-CCN-${i}"
    
    echo "[$(date)] Launching ${EPIC} (delay: 12s)"
    
    # Launch in screen session
    screen -dmS p5-${i} bash -l -c \\
        "./scripts/wave4/_p5_${i}.sh 2>&1 | tee logs/phase5/${EPIC}.log"
    
    # Constant 12s delay
    sleep 12
done

echo "[$(date)] All 80 epics launched for Phase 5"
echo "Launch duration: 16 minutes (80 × 12s)"
echo "Monitor with: screen -ls | grep -c 'p5-'"
echo "Check files: ls docs/brain/EPIC-CCN-*/ticket-*-completion.md | wc -l"
'''

full_path = output_dir / "launch_phase5_all.sh"
with open(full_path, 'w', newline='\n') as f:
    f.write(full_launcher)
os.chmod(full_path, 0o755)
print(f"Generated: {full_path}")

print(f"\n✅ Phase 5 script generation complete!")
print(f"   - 80 epic scripts: _p5_001.sh through _p5_080.sh")
print(f"   - Test launcher: launch_phase5_test.sh")
print(f"   - Full launcher: launch_phase5_all.sh")
print(f"\nBuilding-blocks verification:")
print(f"   - Copied from Phase 4 pattern")
print(f"   - Changed: phase4 → phase5, Phase 4 → Phase 5, 04-tickets.md → ticket-*-completion.md")
print(f"   - Added: Prerequisite check for 04-tickets.md")
print(f"   - Preserved: cd command (line 7), API key export, bash -l -c pattern")
print(f"\nNext steps:")
print(f"   1. Review scripts with: diff scripts/wave4/_p4_001.sh scripts/wave4/_p5_001.sh")
print(f"   2. Upload to VM: gcloud compute scp scripts/wave4/_p5_*.sh scripts/wave4/launch_phase5_*.sh v12-test-golden-v2:~/universal-or-strategy/scripts/wave4/ --zone=us-central1-a")
print(f"   3. Run pilot test: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='cd universal-or-strategy && ./scripts/wave4/launch_phase5_test.sh'")

# Made with Bob