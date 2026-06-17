#!/usr/bin/env python3
"""
Add missing 'mode' field to all phases in manifests for V12.52 compliance.

Standard phase-to-mode mapping:
- Phase 0: ask (hotspot analysis)
- Phase 1: plan (scope definition)
- Phase 1.5: plan (scope boundary validation)
- Phase 2: plan (architecture planning)
- Phase 3: advanced (DNA & PR audit - needs MCP tools)
- Phase 4: plan (ticket generation)
- Phase 5.X: v12-engineer (ticket execution with Bob CLI)
- Phase 5.X.V: advanced (verification - needs MCP tools)
- Phase 6: advanced (final review - needs MCP tools)
"""

import json
from pathlib import Path

# Standard phase-to-mode mapping
PHASE_MODES = {
    "0": "ask",
    "1": "plan",
    "1.5": "plan",
    "2": "plan",
    "3": "advanced",
    "4": "plan",
    "6": "advanced"
}

def add_missing_modes():
    """Add missing mode field to all phases."""
    brain_dir = Path("docs/brain")
    fixed_count = 0
    phase_count = 0
    
    # Get all epic directories
    epic_dirs = sorted([d for d in brain_dir.iterdir() if d.is_dir() and d.name.startswith("EPIC-CCN-")])
    
    for epic_dir in epic_dirs:
        manifest_path = epic_dir / "manifest.json"
        if not manifest_path.exists():
            continue
        
        # Load manifest
        with open(manifest_path, 'r', encoding='utf-8') as f:
            manifest = json.load(f)
        
        epic_id = manifest.get('epic_id', epic_dir.name)
        modified = False
        
        # Process each phase
        for phase_id, phase_data in manifest.get('phases', {}).items():
            if 'mode' not in phase_data:
                # Determine mode based on phase ID
                if phase_id.startswith("5.") and phase_id.endswith(".V"):
                    # Verification phase
                    mode = "advanced"
                elif phase_id.startswith("5."):
                    # Ticket execution phase
                    mode = "v12-engineer"
                else:
                    # Standard phase
                    mode = PHASE_MODES.get(phase_id, "plan")
                
                phase_data['mode'] = mode
                modified = True
                phase_count += 1
        
        # Save if modified
        if modified:
            with open(manifest_path, 'w', encoding='utf-8') as f:
                json.dump(manifest, f, indent=2, ensure_ascii=True)
            fixed_count += 1
            print(f"[OK] Fixed {epic_id}")
    
    print(f"\n[OK] Added mode to {phase_count} phases across {fixed_count} manifests")

if __name__ == '__main__':
    add_missing_modes()

# Made with Bob
