#!/usr/bin/env python3
"""
Generate Phase 0 scripts with Jane Street integration for Wave 4.
Creates 80 individual epic scripts + master launcher.
"""

import json
from pathlib import Path

# Load epic roadmap (handle UTF-8 BOM)
roadmap_path = Path(__file__).parent.parent.parent / "epic_roadmap_wave4_fresh.json"
with open(roadmap_path, 'r', encoding='utf-8-sig') as f:
    epics = json.load(f)

# Load API keys
api_keys_path = Path(__file__).parent.parent.parent / "docs" / "API"
api_files = [
    "bob.json", "bob (1).json", "bob (2).json", "bob (3).json", "bob (4).json",
    "bob (5).json", "bob (6).json", "jessica.json", "mikethelife.json",
    "sammy96.json", "sean.carter.jr@atomicmail.io.json", "tory.json",
    "b.json", "b (2).json", "b (3).json"
]

api_keys = []
for api_file in api_files:
    api_path = api_keys_path / api_file
    if api_path.exists():
        with open(api_path, 'r') as f:
            data = json.load(f)
            api_keys.append(data['apikey'])

print(f"Loaded {len(api_keys)} API keys")

# Generate individual epic scripts
output_dir = Path(__file__).parent
output_dir.mkdir(exist_ok=True)

for i, epic in enumerate(epics, 1):
    epic_id = epic['epic_number']
    epic_num = epic_id.split('-')[-1]  # Extract "001" from "EPIC-CCN-001"
    method = epic['method']
    file = epic['file']
    cyc = epic['cyclomatic']
    
    # Create script content
    script_content = f"""#!/bin/bash
# Phase 0 script for {epic_id} (WITH Jane Street integration)
# Generated: 2026-06-15

EPIC_ID="{epic_id}"
METHOD="{method}"
FILE="{file}"
CYC={cyc}

# Execute Phase 0 with Jane Street validation
python3 scripts/wave4/execute_phase0_with_jane_street.py "${{EPIC_ID}}" "${{METHOD}}" "${{FILE}}" "${{CYC}}"
"""
    
    # Write script
    script_path = output_dir / f"_p0_{epic_num}.sh"
    with open(script_path, 'w', newline='\n') as f:
        f.write(script_content)
    
    print(f"Created {script_path.name}")

# Generate master launcher with 12-second delays
launcher_content = """#!/bin/bash
# Master launcher for Phase 0 (80 epics with Jane Street integration)
# Uses 12-second staggered delays

PHASE=0
EPICS=($(seq -f "%03g" 1 80))
DELAY=12

echo "[$(date)] Starting Phase 0 launch for 80 epics"
echo "[$(date)] Using 12-second delays between launches"

for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    
    echo "[$(date)] Launching EPIC-CCN-${EPIC} ($(($i + 1))/80)"
    
    # Launch in screen session
    screen -dmS p0-${EPIC} bash -l -c \\
        "./_p0_${EPIC}.sh 2>&1 | tee logs/phase0/EPIC-CCN-${EPIC}.log"
    
    # Wait before next launch (except for last epic)
    if [ $i -lt $((${#EPICS[@]} - 1)) ]; then
        sleep ${DELAY}
    fi
done

echo "[$(date)] All 80 epics launched for Phase 0"
echo "[$(date)] Total launch time: $((80 * 12 / 60)) minutes"
echo ""
echo "Polling protocol:"
echo "  1. Wait 1 minute"
echo "  2. Check: screen -ls"
echo "  3. Poll every 4 minutes until complete"
"""

launcher_path = output_dir / "launch_phase0_all.sh"
with open(launcher_path, 'w', newline='\n') as f:
    f.write(launcher_content)

print(f"\nCreated {launcher_path.name}")
print(f"\nGenerated {len(epics)} Phase 0 scripts with Jane Street integration")
print("\nNext steps:")
print("1. Upload scripts to VM:")
print("   gcloud compute scp scripts/wave4/_p0_*.sh v12-test-golden-v2:~/universal-or-strategy/")
print("   gcloud compute scp scripts/wave4/launch_phase0_all.sh v12-test-golden-v2:~/universal-or-strategy/")
print("2. Set permissions:")
print("   gcloud compute ssh v12-test-golden-v2 --command='chmod +x ~/universal-or-strategy/_p0_*.sh ~/universal-or-strategy/launch_phase0_all.sh'")
print("3. Launch:")
print("   gcloud compute ssh v12-test-golden-v2 --command='cd ~/universal-or-strategy && ./launch_phase0_all.sh'")
print("4. Poll (1 min + 4 min intervals):")
print("   gcloud compute ssh v12-test-golden-v2 --command='screen -ls'")

# Made with Bob
