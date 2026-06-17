#!/usr/bin/env python3
"""
Migrate Wave 4 manifests to V12.52 schema.

V12.52 Schema Requirements:
- description (string): Epic description
- schema_version (string): "2.0"
- created_at (ISO timestamp): Epic creation time
- status (string): Epic status (pending/in_progress/completed/failed)
- phases[X].status (string): Phase status
- phases[X].created_at (ISO timestamp): Phase creation time
- phases[X].mode (string): Agent mode for phase
- phases[X].dependencies (list): Phase dependencies

Usage:
    python3 scripts/migrate_manifests_to_v12_52.py [epic_id]
    
    If epic_id is provided, migrates only that epic.
    Otherwise, migrates all epics in docs/brain/EPIC-CCN-*/
"""

import json
import os
import sys
from datetime import datetime
from pathlib import Path

# Phase mode mappings (from V12 workflow)
PHASE_MODES = {
    "0": "ask",           # Hotspot Analysis
    "1": "plan",          # Scope Definition
    "1.5": "plan",        # Scope Boundary
    "2": "plan",          # Architecture Planning
    "3": "advanced",      # DNA & PR Audit
    "4": "plan",          # Ticket Generation
    "5": "v12-engineer",  # Ticket Execution (Bob CLI)
    "5.V": "advanced",    # Ticket Verification
    "6": "advanced"       # Final Review
}

# Phase dependencies (from V12 workflow)
PHASE_DEPENDENCIES = {
    "0": [],
    "1": ["0"],
    "1.5": ["1"],
    "2": ["1.5"],
    "3": ["2"],
    "4": ["3"],
    "5": ["4"],
    "5.V": ["5"],
    "6": ["5.V"]
}

def migrate_manifest(manifest_path: Path, epic_id: str) -> bool:
    """Migrate a single manifest to V12.52 schema."""
    
    print(f"Migrating {epic_id}...")
    
    # Load existing manifest
    with open(manifest_path, 'r') as f:
        manifest = json.load(f)
    
    # Add top-level V12.52 fields
    if 'description' not in manifest:
        method = manifest.get('method', 'Unknown')
        cyc_before = manifest.get('complexity_before', 'N/A')
        manifest['description'] = f"Reduce complexity of {method} from CYC {cyc_before} to ≤8"
    
    if 'schema_version' not in manifest:
        manifest['schema_version'] = '2.0'
    
    if 'created_at' not in manifest:
        # Use earliest phase timestamp or current time
        earliest = None
        for phase_data in manifest.get('phases', {}).values():
            if 'created_at' in phase_data:
                if earliest is None or phase_data['created_at'] < earliest:
                    earliest = phase_data['created_at']
        manifest['created_at'] = earliest or datetime.utcnow().isoformat() + 'Z'
    
    if 'status' not in manifest:
        # Infer from epic_status or phases
        epic_status = manifest.get('epic_status', 'PENDING')
        if epic_status == 'COMPLETED':
            manifest['status'] = 'completed'
        elif epic_status == 'IN_PROGRESS':
            manifest['status'] = 'in_progress'
        elif epic_status == 'FAILED':
            manifest['status'] = 'failed'
        else:
            manifest['status'] = 'pending'
    
    # Always rebuild dependencies dict from phase dependencies
    # Format: {phase_id: [dependency_phase_ids]}
    dependencies = {}
    for phase_id, phase_data in manifest.get('phases', {}).items():
        if 'dependencies' in phase_data:
            dependencies[phase_id] = phase_data['dependencies']
        else:
            dependencies[phase_id] = PHASE_DEPENDENCIES.get(phase_id, [])
    manifest['dependencies'] = dependencies
    
    # Migrate phases
    phases = manifest.get('phases', {})
    for phase_id, phase_data in phases.items():
        # Add status if missing
        if 'status' not in phase_data:
            # Infer from old status field or outputs
            old_status = phase_data.get('status')
            if old_status == 'completed':
                phase_data['status'] = 'completed'
            elif old_status == 'deferred':
                phase_data['status'] = 'skipped'  # Map deferred -> skipped
            elif 'outputs' in phase_data or 'output' in phase_data:
                phase_data['status'] = 'completed'
            else:
                phase_data['status'] = 'pending'
        elif phase_data['status'] == 'deferred':
            # Fix existing deferred status
            phase_data['status'] = 'skipped'
        
        # Add created_at if missing
        if 'created_at' not in phase_data:
            phase_data['created_at'] = manifest['created_at']
        
        # Add mode if missing
        if 'mode' not in phase_data:
            phase_data['mode'] = PHASE_MODES.get(phase_id, 'plan')
        
        # Add dependencies if missing
        if 'dependencies' not in phase_data:
            phase_data['dependencies'] = PHASE_DEPENDENCIES.get(phase_id, [])
        
        # Normalize outputs field (some use 'output', some use 'outputs')
        if 'output' in phase_data and 'outputs' not in phase_data:
            phase_data['outputs'] = [phase_data['output']]
            del phase_data['output']
        elif 'outputs' not in phase_data:
            phase_data['outputs'] = []
    
    # Save migrated manifest
    with open(manifest_path, 'w') as f:
        json.dump(manifest, f, indent=2)
    
    print(f"✓ {epic_id} migrated successfully")
    return True

def main():
    """Main migration function."""
    
    # Get project root
    project_root = Path(__file__).parent.parent
    brain_dir = project_root / 'docs' / 'brain'
    
    # Get epic_id from command line or scan all
    if len(sys.argv) > 1:
        epic_id = sys.argv[1]
        epic_dirs = [brain_dir / epic_id]
    else:
        epic_dirs = sorted(brain_dir.glob('EPIC-CCN-*'))
    
    # Migrate each epic
    success_count = 0
    fail_count = 0
    
    for epic_dir in epic_dirs:
        if not epic_dir.is_dir():
            continue
        
        epic_id = epic_dir.name
        manifest_path = epic_dir / 'manifest.json'
        
        if not manifest_path.exists():
            print(f"⚠ {epic_id}: No manifest.json found, skipping")
            continue
        
        try:
            if migrate_manifest(manifest_path, epic_id):
                success_count += 1
            else:
                fail_count += 1
        except Exception as e:
            print(f"✗ {epic_id}: Migration failed: {e}")
            fail_count += 1
    
    # Summary
    print("\n" + "="*60)
    print(f"Migration complete: {success_count} succeeded, {fail_count} failed")
    print("="*60)
    
    return 0 if fail_count == 0 else 1

if __name__ == '__main__':
    sys.exit(main())

# Made with Bob
