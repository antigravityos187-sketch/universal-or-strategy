#!/usr/bin/env python3
"""Identify remaining Phase 1 epics and generate relaunch script."""

import json
import glob
from pathlib import Path

def identify_remaining():
    """Find epics that need Phase 1 completion."""
    manifests = sorted(glob.glob('/home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/manifest.json'))
    
    remaining = []
    
    for manifest_path in manifests:
        epic_id = Path(manifest_path).parent.name
        try:
            with open(manifest_path) as f:
                data = json.load(f)
                phase1_status = data.get('phases', {}).get('1', {}).get('status', 'unknown')
                
                if phase1_status != 'completed':
                    remaining.append(epic_id)
        except Exception as e:
            print(f"Error reading {epic_id}: {e}")
    
    print(f"\n=== Remaining Phase 1 Epics ===")
    print(f"Total: {len(remaining)}")
    print(f"\nEpics: {', '.join(remaining)}")
    
    # Generate relaunch script
    with open('/home/malhitticrypto/universal-or-strategy/scripts/wave6/relaunch_phase1_remaining.sh', 'w') as f:
        f.write('#!/bin/bash\n')
        f.write('# Relaunch remaining Phase 1 epics\n\n')
        f.write('cd /home/malhitticrypto/universal-or-strategy\n\n')
        
        for epic_id in remaining:
            epic_num = epic_id.split('-')[-1]
            f.write(f'echo "Launching {epic_id} in screen session: wave6_p1_epic_{epic_num}"\n')
            f.write(f'screen -dmS wave6_p1_epic_{epic_num} bash scripts/wave6/_p1_epic_ccn_{epic_num}.sh\n')
        
        f.write('\necho ""\n')
        f.write(f'echo "Launched {len(remaining)} epics in screen sessions"\n')
        f.write('echo "Monitor with: screen -ls | grep wave6_p1"\n')
    
    print(f"\n✅ Relaunch script created: scripts/wave6/relaunch_phase1_remaining.sh")

if __name__ == '__main__':
    identify_remaining()

# Made with Bob
