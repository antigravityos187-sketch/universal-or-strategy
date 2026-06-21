#!/usr/bin/env python3
"""
Validate Wave 6 scope and completion status.
Wave 6 = First 80 epics (EPIC-CCN-001 through EPIC-CCN-080)
"""

from pathlib import Path
import json

def validate_wave6_scope():
    """Validate Wave 6 scope (epics 1-80)."""
    
    print("=== Wave 6 Scope Validation ===\n")
    
    # Check Phase 0 completion
    phase0_complete = []
    phase0_missing = []
    
    for i in range(1, 81):
        epic_id = f'EPIC-CCN-{i:03d}'
        manifest_path = Path(f'docs/brain/{epic_id}/manifest.json')
        
        if not manifest_path.exists():
            phase0_missing.append(f"{epic_id} (no manifest)")
            continue
            
        try:
            manifest = json.load(open(manifest_path))
            if '0' in manifest.get('phases', {}) and manifest['phases']['0'].get('status') == 'completed':
                phase0_complete.append(epic_id)
            else:
                phase0_missing.append(f"{epic_id} (Phase 0 not complete)")
        except Exception as e:
            phase0_missing.append(f"{epic_id} (error: {e})")
    
    print(f"Phase 0 Status: {len(phase0_complete)}/80 complete")
    if phase0_missing:
        print(f"Phase 0 Missing ({len(phase0_missing)}):")
        for epic in phase0_missing:
            print(f"  - {epic}")
    print()
    
    # Check Phase 1 completion
    phase1_complete = []
    phase1_missing = []
    
    for i in range(1, 81):
        epic_id = f'EPIC-CCN-{i:03d}'
        scope_file = Path(f'docs/brain/{epic_id}/00-scope.md')
        
        if scope_file.exists():
            phase1_complete.append(epic_id)
        else:
            phase1_missing.append(epic_id)
    
    print(f"Phase 1 Status: {len(phase1_complete)}/80 complete")
    if phase1_missing:
        print(f"Phase 1 Missing ({len(phase1_missing)}):")
        for epic in phase1_missing:
            print(f"  - {epic}")
    print()
    
    # Check scripts
    phase0_scripts = list(Path('scripts/wave6').glob('_p0_epic_ccn_*.sh'))
    phase1_scripts = list(Path('scripts/wave6').glob('_p1_epic_ccn_*.sh'))
    
    print(f"Phase 0 Scripts: {len(phase0_scripts)}")
    print(f"Phase 1 Scripts: {len(phase1_scripts)}")
    print()
    
    # Find missing scripts
    missing_p0_scripts = []
    missing_p1_scripts = []
    
    for i in range(1, 81):
        p0_script = Path(f'scripts/wave6/_p0_epic_ccn_{i:03d}.sh')
        p1_script = Path(f'scripts/wave6/_p1_epic_ccn_{i:03d}.sh')
        
        if not p0_script.exists():
            missing_p0_scripts.append(f'EPIC-CCN-{i:03d}')
        if not p1_script.exists():
            missing_p1_scripts.append(f'EPIC-CCN-{i:03d}')
    
    if missing_p0_scripts:
        print(f"Missing Phase 0 Scripts ({len(missing_p0_scripts)}):")
        for epic in missing_p0_scripts:
            print(f"  - {epic}")
    
    if missing_p1_scripts:
        print(f"Missing Phase 1 Scripts ({len(missing_p1_scripts)}):")
        for epic in missing_p1_scripts:
            print(f"  - {epic}")
    
    print()
    
    # Summary
    print("=== Summary ===")
    print(f"Wave 6 Scope: 80 epics (EPIC-CCN-001 through EPIC-CCN-080)")
    print(f"Phase 0: {len(phase0_complete)}/80 complete ({len(phase0_missing)} missing)")
    print(f"Phase 1: {len(phase1_complete)}/80 complete ({len(phase1_missing)} missing)")
    print(f"Phase 0 Scripts: {len(phase0_scripts)}/80 ({len(missing_p0_scripts)} missing)")
    print(f"Phase 1 Scripts: {len(phase1_scripts)}/80 ({len(missing_p1_scripts)} missing)")
    
    # Check if EPIC-027 is intentionally excluded
    if 'EPIC-CCN-027' in missing_p0_scripts:
        print("\nNote: EPIC-CCN-027 missing Phase 0 script (intentionally excluded?)")

if __name__ == "__main__":
    validate_wave6_scope()

# Made with Bob
