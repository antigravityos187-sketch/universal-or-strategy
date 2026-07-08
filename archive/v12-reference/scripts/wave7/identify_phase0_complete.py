#!/usr/bin/env python3
"""
Identify Wave 7 epics with completed Phase 0
"""

import os
import json
from pathlib import Path

def find_phase0_complete_epics():
    """Find all EPIC-W7-* directories with 00-hotspots.md"""
    brain_dir = Path("docs/brain")
    
    phase0_complete = []
    phase1_started = []
    
    # Find all EPIC-W7-* directories
    for epic_dir in sorted(brain_dir.glob("EPIC-W7-*")):
        epic_id = epic_dir.name
        
        # Check if Phase 0 complete (00-hotspots.md exists)
        hotspot_file = epic_dir / "00-hotspots.md"
        if hotspot_file.exists():
            phase0_complete.append(epic_id)
            
            # Check if Phase 1 started (00-scope.md exists)
            scope_file = epic_dir / "00-scope.md"
            if scope_file.exists():
                phase1_started.append(epic_id)
    
    return phase0_complete, phase1_started

def main():
    phase0_complete, phase1_started = find_phase0_complete_epics()
    
    print(f"EPICs with Phase 0 complete: {len(phase0_complete)}")
    for epic in phase0_complete:
        print(f"  {epic}")
    
    print(f"\nEPICs with Phase 1 started: {len(phase1_started)}")
    for epic in phase1_started:
        print(f"  {epic}")
    
    # EPICs needing Phase 1
    phase1_needed = [e for e in phase0_complete if e not in phase1_started]
    print(f"\nEPICs needing Phase 1: {len(phase1_needed)}")
    for epic in phase1_needed:
        print(f"  {epic}")
    
    # Save to JSON for script generation
    output = {
        "phase0_complete": phase0_complete,
        "phase1_started": phase1_started,
        "phase1_needed": phase1_needed
    }
    
    output_file = Path("scripts/wave7/phase0_status.json")
    output_file.parent.mkdir(parents=True, exist_ok=True)
    with open(output_file, 'w') as f:
        json.dump(output, f, indent=2)
    
    print(f"\nStatus saved to: {output_file}")

if __name__ == "__main__":
    main()

# Made with Bob
