#!/usr/bin/env python3
"""
Manifest Migration Script: Pre-V12.52 → V12.52
Adds lamport_events array to manifests missing it.

Root Cause: Wave 6 Phase 0 used old manifest format (no Lamport clock).
Impact: 24 epics blocked at Phase 1 verification gate.
Fix: Backfill lamport_events from completed_at timestamps.

Usage:
  python3 scripts/migrate_manifests_v12_52.py --epic EPIC-CCN-001
  python3 scripts/migrate_manifests_v12_52.py --all
  python3 scripts/migrate_manifests_v12_52.py --dry-run
"""

import json
import sys
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Optional

def migrate_manifest(epic_id: str, dry_run: bool = False) -> bool:
    """
    Migrate a single manifest to V12.52 format.
    
    Returns:
        True if migration successful, False if skipped or failed
    """
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    
    if not manifest_path.exists():
        print(f"[X] {epic_id}: Manifest not found")
        return False
    
    # Load manifest
    with open(manifest_path, 'r') as f:
        manifest = json.load(f)
    
    # Check if already migrated
    if "lamport_events" in manifest and len(manifest["lamport_events"]) > 0:
        print(f"[OK] {epic_id}: Already migrated (has {len(manifest['lamport_events'])} events)")
        return False
    
    # Check if Phase 0 is complete
    phase_0 = manifest.get("phases", {}).get("0", {})
    if phase_0.get("status") != "completed":
        print(f"[SKIP] {epic_id}: Phase 0 not complete, skipping")
        return False
    
    # Extract Phase 0 metadata
    completed_at = phase_0.get("completed_at")
    started_at = phase_0.get("started_at")
    agent_id = phase_0.get("agent_id", "unknown")
    
    if not completed_at:
        print(f"[X] {epic_id}: Phase 0 missing completed_at timestamp")
        return False
    
    # Build lamport_events array
    lamport_events = []
    clock = 1
    
    # Event 1: phase_start (if we have started_at)
    if started_at:
        lamport_events.append({
            "event_type": "phase_start",
            "phase": "0",
            "agent_id": agent_id,
            "timestamp": started_at,
            "clock": clock,
            "status": "running",
            "epic_id": epic_id
        })
        clock += 1
    
    # Event 2: phase_complete
    lamport_events.append({
        "event_type": "phase_complete",
        "phase": "0",
        "agent_id": agent_id,
        "timestamp": completed_at,
        "clock": clock,
        "status": "completed",
        "epic_id": epic_id
    })
    
    # Add lamport_events to manifest
    manifest["lamport_events"] = lamport_events
    manifest["lamport_clock"] = clock
    
    if dry_run:
        print(f"[DRY-RUN] {epic_id}: Would add {len(lamport_events)} Lamport events")
        print(f"   Events: {[e['event_type'] for e in lamport_events]}")
        return True
    
    # Write updated manifest
    with open(manifest_path, 'w') as f:
        json.dump(manifest, f, indent=2)
    
    print(f"[OK] {epic_id}: Migrated (added {len(lamport_events)} events)")
    return True

def find_epics_needing_migration() -> List[str]:
    """Find all epics with completed Phase 0 but missing lamport_events."""
    brain_dir = Path("docs/brain")
    epics_needing_migration = []
    
    for epic_dir in sorted(brain_dir.glob("EPIC-CCN-*")):
        manifest_path = epic_dir / "manifest.json"
        if not manifest_path.exists():
            continue
        
        with open(manifest_path, 'r') as f:
            manifest = json.load(f)
        
        # Check if needs migration
        phase_0 = manifest.get("phases", {}).get("0", {})
        has_lamport = "lamport_events" in manifest and len(manifest["lamport_events"]) > 0
        
        if phase_0.get("status") == "completed" and not has_lamport:
            epics_needing_migration.append(epic_dir.name)
    
    return epics_needing_migration

def main():
    import argparse
    
    parser = argparse.ArgumentParser(description="Migrate manifests to V12.52 format")
    parser.add_argument("--epic", help="Migrate single epic (e.g., EPIC-CCN-001)")
    parser.add_argument("--all", action="store_true", help="Migrate all epics needing migration")
    parser.add_argument("--dry-run", action="store_true", help="Show what would be done without making changes")
    parser.add_argument("--list", action="store_true", help="List epics needing migration")
    
    args = parser.parse_args()
    
    if args.list:
        epics = find_epics_needing_migration()
        print(f"Found {len(epics)} epics needing migration:")
        for epic in epics:
            print(f"  - {epic}")
        return 0
    
    if args.epic:
        # Migrate single epic
        success = migrate_manifest(args.epic, dry_run=args.dry_run)
        return 0 if success else 1
    
    if args.all:
        # Migrate all epics
        epics = find_epics_needing_migration()
        print(f"Found {len(epics)} epics needing migration")
        print()
        
        migrated = 0
        for epic in epics:
            if migrate_manifest(epic, dry_run=args.dry_run):
                migrated += 1
        
        print()
        print(f"{'Would migrate' if args.dry_run else 'Migrated'} {migrated}/{len(epics)} epics")
        return 0
    
    # No arguments - show help
    parser.print_help()
    return 1

if __name__ == "__main__":
    sys.exit(main())

# Made with Bob
