#!/usr/bin/env python3
"""
Add missing top-level fields to all manifests for V12.52 compliance.

Required fields:
- description: Human-readable epic description
- status: Epic status (pending/in_progress/completed/failed)
- created_at: ISO 8601 timestamp
- dependencies: Top-level dependencies dict (empty for now)
"""

import json
import os
from datetime import datetime, timezone
from pathlib import Path

def add_missing_fields():
    """Add missing top-level fields to all manifests."""
    brain_dir = Path("docs/brain")
    fixed_count = 0
    
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
        
        # Add description if missing
        if 'description' not in manifest:
            method = manifest.get('method', 'Unknown')
            file = manifest.get('file', 'Unknown')
            cyc = manifest.get('complexity', manifest.get('complexity_before', 'Unknown'))
            manifest['description'] = f"Reduce complexity of {method} in {file} (CYC: {cyc})"
            modified = True
        
        # Add status if missing
        if 'status' not in manifest:
            # Infer status from phases
            phases = manifest.get('phases', {})
            if all(p.get('status') == 'completed' for p in phases.values()):
                manifest['status'] = 'completed'
            elif any(p.get('status') == 'in_progress' for p in phases.values()):
                manifest['status'] = 'in_progress'
            elif any(p.get('status') == 'failed' for p in phases.values()):
                manifest['status'] = 'failed'
            else:
                manifest['status'] = 'pending'
            modified = True
        
        # Add created_at if missing
        if 'created_at' not in manifest:
            # Use earliest phase created_at or current time
            earliest = None
            for phase_data in manifest.get('phases', {}).values():
                phase_created = phase_data.get('created_at')
                if phase_created:
                    if earliest is None or phase_created < earliest:
                        earliest = phase_created
            
            manifest['created_at'] = earliest or datetime.now(timezone.utc).isoformat()
            modified = True
        
        # Add top-level dependencies if missing
        if 'dependencies' not in manifest:
            manifest['dependencies'] = {}
            modified = True
        
        # Save if modified
        if modified:
            with open(manifest_path, 'w', encoding='utf-8') as f:
                json.dump(manifest, f, indent=2, ensure_ascii=True)
            fixed_count += 1
            print(f"[OK] Fixed {epic_id}")
    
    print(f"\n[OK] Added missing fields to {fixed_count} manifests")

if __name__ == '__main__':
    add_missing_fields()

# Made with Bob
