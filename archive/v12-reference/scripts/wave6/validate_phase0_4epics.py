#!/usr/bin/env python3
"""Validate Phase 0 completion for 4 blocked epics"""

import json
from pathlib import Path

BLOCKED_EPICS = ["001", "004", "016", "028"]

def main():
    print("=== Validating Phase 0 for 4 Blocked Epics ===\n")
    
    for epic_num in BLOCKED_EPICS:
        epic_id = f"EPIC-CCN-{epic_num}"
        manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
        
        print(f"--- {epic_id} ---")
        
        if not manifest_path.exists():
            print(f"❌ Manifest not found: {manifest_path}")
            print()
            continue
        
        try:
            with open(manifest_path) as f:
                manifest = json.load(f)
            
            # Check Phase 0 status
            phase0_status = manifest.get("phases", {}).get("0", {}).get("status", "unknown")
            print(f"Phase 0 status: {phase0_status}")
            
            # Check lamport_events
            lamport_events = manifest.get("lamport_events", [])
            print(f"Lamport events: {len(lamport_events)} total")
            
            # Check for Phase 0 completion event
            phase0_complete = any(
                e.get("event_type") == "phase_complete" and e.get("phase") == "0"
                for e in lamport_events
            )
            print(f"Phase 0 completion event: {'✅ Found' if phase0_complete else '❌ Missing'}")
            
            # Check if Phase 0 output exists
            hotspots_path = Path(f"docs/brain/{epic_id}/00-hotspots.md")
            print(f"Phase 0 output (00-hotspots.md): {'✅ Exists' if hotspots_path.exists() else '❌ Missing'}")
            
            # Summary
            if phase0_status == "completed" and phase0_complete and hotspots_path.exists():
                print(f"✅ {epic_id}: Phase 0 COMPLETE")
            else:
                print(f"❌ {epic_id}: Phase 0 INCOMPLETE or INVALID")
                if phase0_status != "completed":
                    print(f"   - Status is '{phase0_status}', not 'completed'")
                if not phase0_complete:
                    print(f"   - Missing Phase 0 completion event in lamport_events")
                if not hotspots_path.exists():
                    print(f"   - Missing 00-hotspots.md output file")
            
        except Exception as e:
            print(f"❌ Error reading manifest: {e}")
        
        print()

if __name__ == "__main__":
    main()

# Made with Bob
