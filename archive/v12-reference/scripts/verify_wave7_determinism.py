#!/usr/bin/env python3
"""
Verify Wave 7 determinism guarantees.

Checks all Wave 7 epics for:
1. Dependencies satisfied (happens-before)
2. State hash consistency
3. No concurrent conflicts

Usage:
    python scripts/verify_wave7_determinism.py
    python scripts/verify_wave7_determinism.py EPIC-W7-001  # Verify specific epic
"""

import sys
from pathlib import Path
from typing import List, Tuple
from scripts.lamport_clock import get_workflow


def get_wave7_epics() -> List[str]:
    """
    Get all Wave 7 epic IDs from event log.
    
    Returns:
        List of unique epic IDs
    """
    wave7_log = Path(".lamport/wave7/event_log.jsonl")
    if not wave7_log.exists():
        print("No Wave 7 event log found. Run filter_wave7_events.py first.")
        return []
    
    import json
    epic_ids = set()
    with open(wave7_log, 'r') as f:
        for line in f:
            event = json.loads(line.strip())
            epic_id = event.get('epic_id')
            if epic_id:
                epic_ids.add(epic_id)
    
    return sorted(epic_ids)


def verify_epic(epic_id: str) -> Tuple[bool, List[str]]:
    """
    Verify determinism for a single epic.
    
    Args:
        epic_id: Epic identifier
    
    Returns:
        (is_deterministic, issues) tuple
    """
    workflow = get_workflow()
    issues = []
    
    # Get all events for this epic
    events = workflow.get_event_log(epic_id)
    if not events:
        issues.append(f"No events found for {epic_id}")
        return False, issues
    
    # Get unique phases
    phases = sorted(set(e['phase'] for e in events))
    
    # Verify each phase
    for phase in phases:
        is_deterministic, reason = workflow.verify_determinism(epic_id, phase)
        if not is_deterministic:
            issues.append(f"Phase {phase}: {reason}")
    
    return len(issues) == 0, issues


def verify_all_epics() -> Tuple[int, int, List[Tuple[str, List[str]]]]:
    """
    Verify determinism for all Wave 7 epics.
    
    Returns:
        (total_epics, deterministic_epics, failed_epics) tuple
    """
    epic_ids = get_wave7_epics()
    if not epic_ids:
        return 0, 0, []
    
    deterministic_count = 0
    failed_epics = []
    
    for epic_id in epic_ids:
        is_deterministic, issues = verify_epic(epic_id)
        if is_deterministic:
            deterministic_count += 1
        else:
            failed_epics.append((epic_id, issues))
    
    return len(epic_ids), deterministic_count, failed_epics


def print_results(total: int, deterministic: int, failed: List[Tuple[str, List[str]]]):
    """
    Print verification results.
    
    Args:
        total: Total epics checked
        deterministic: Number of deterministic epics
        failed: List of (epic_id, issues) tuples
    """
    print("\n" + "="*60)
    print("WAVE 7 DETERMINISM VERIFICATION")
    print("="*60)
    
    print(f"\nSummary:")
    print(f"  Total Epics: {total}")
    print(f"  Deterministic: {deterministic} ({deterministic/total*100:.1f}%)")
    print(f"  Non-Deterministic: {len(failed)} ({len(failed)/total*100:.1f}%)")
    
    if failed:
        print(f"\nNon-Deterministic Epics:")
        for epic_id, issues in failed:
            print(f"\n  {epic_id}:")
            for issue in issues:
                print(f"    ❌ {issue}")
    else:
        print(f"\n✅ All epics are deterministic!")
    
    print("\n" + "="*60)


def main():
    """Main entry point."""
    if len(sys.argv) > 1:
        # Verify specific epic
        epic_id = sys.argv[1]
        print(f"Verifying {epic_id}...")
        
        is_deterministic, issues = verify_epic(epic_id)
        
        if is_deterministic:
            print(f"✅ {epic_id} is deterministic")
        else:
            print(f"❌ {epic_id} is NOT deterministic:")
            for issue in issues:
                print(f"  - {issue}")
        
        sys.exit(0 if is_deterministic else 1)
    
    # Verify all epics
    print("Verifying all Wave 7 epics...")
    total, deterministic, failed = verify_all_epics()
    
    if total == 0:
        print("No Wave 7 epics found")
        sys.exit(0)
    
    print_results(total, deterministic, failed)
    
    # Exit with error if any epics are non-deterministic
    sys.exit(0 if len(failed) == 0 else 1)


if __name__ == "__main__":
    main()

# Made with Bob
