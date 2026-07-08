#!/usr/bin/env python3
"""
Generate Wave 7 statistics from event log.

Computes statistics for Wave 7 execution including:
- Total/completed/failed epics
- Phase completion counts
- Average CYC reduction
- Total bobcoin usage

Usage:
    python scripts/generate_wave7_stats.py
"""

import json
from pathlib import Path
from typing import Dict, List
from datetime import datetime
from collections import defaultdict


def load_wave7_events() -> List[Dict]:
    """
    Load Wave 7 events from wave-specific log.
    
    Returns:
        List of Wave 7 events
    """
    wave7_log = Path(".lamport/wave7/event_log.jsonl")
    if not wave7_log.exists():
        print("No Wave 7 event log found. Run filter_wave7_events.py first.")
        return []
    
    events = []
    with open(wave7_log, 'r') as f:
        for line in f:
            events.append(json.loads(line.strip()))
    
    return events


def compute_statistics(events: List[Dict]) -> Dict:
    """
    Compute Wave 7 statistics from events.
    
    Args:
        events: List of Wave 7 events
    
    Returns:
        Statistics dictionary
    """
    # Initialize stats
    stats = {
        'wave_id': 'wave7',
        'start_time': None,
        'end_time': None,
        'total_epics': 180,  # Wave 7 target
        'completed_epics': 0,
        'failed_epics': 0,
        'in_progress_epics': 0,
        'total_events': len(events),
        'phases_completed': defaultdict(int),
        'avg_cyc_reduction': 0.0,
        'total_bobcoins': 0.0,
        'epic_status': {}
    }
    
    if not events:
        return stats
    
    # Find start/end times
    timestamps = [e['timestamp'] for e in events if 'timestamp' in e and e['timestamp']]
    if timestamps:
        stats['start_time'] = min(timestamps)
        stats['end_time'] = max(timestamps)
    
    # Track epic status
    epic_phases = defaultdict(set)
    epic_failures = defaultdict(int)
    cyc_reductions = []
    bobcoin_totals = []
    
    for event in events:
        epic_id = event.get('epic_id')
        phase = event.get('phase')
        event_type = event.get('event_type')
        status = event.get('status')
        data = event.get('data', {})
        
        # Track phase completions
        if event_type == 'phase_complete' and status == 'completed':
            stats['phases_completed'][phase] += 1
            epic_phases[epic_id].add(phase)
        
        # Track failures
        if event_type == 'phase_fail' or status == 'failed':
            epic_failures[epic_id] += 1
        
        # Track CYC reductions (from Phase 6 final review)
        if phase == '6' and event_type == 'phase_complete':
            cyc_before = data.get('cyc_before', 0)
            cyc_after = data.get('cyc_after', 0)
            if cyc_before > 0:
                reduction = cyc_before - cyc_after
                cyc_reductions.append(reduction)
        
        # Track bobcoin usage
        bobcoins = data.get('bobcoins', 0.0)
        if bobcoins > 0:
            bobcoin_totals.append(bobcoins)
    
    # Compute epic status
    for epic_id in epic_phases:
        phases = epic_phases[epic_id]
        failures = epic_failures.get(epic_id, 0)
        
        if '6' in phases:
            stats['completed_epics'] += 1
            stats['epic_status'][epic_id] = 'completed'
        elif failures > 0:
            stats['failed_epics'] += 1
            stats['epic_status'][epic_id] = 'failed'
        else:
            stats['in_progress_epics'] += 1
            stats['epic_status'][epic_id] = 'in_progress'
    
    # Compute averages
    if cyc_reductions:
        stats['avg_cyc_reduction'] = sum(cyc_reductions) / len(cyc_reductions)
    
    if bobcoin_totals:
        stats['total_bobcoins'] = sum(bobcoin_totals)
    
    # Convert defaultdict to regular dict for JSON serialization
    stats['phases_completed'] = dict(stats['phases_completed'])
    
    return stats


def write_statistics(stats: Dict):
    """
    Write statistics to JSON file.
    
    Args:
        stats: Statistics dictionary
    """
    wave7_dir = Path(".lamport/wave7")
    wave7_dir.mkdir(parents=True, exist_ok=True)
    
    stats_file = wave7_dir / "stats.json"
    with open(stats_file, 'w') as f:
        json.dump(stats, f, indent=2)
    
    print(f"Wrote statistics to {stats_file}")


def print_summary(stats: Dict):
    """
    Print human-readable summary.
    
    Args:
        stats: Statistics dictionary
    """
    print("\n" + "="*60)
    print("WAVE 7 STATISTICS")
    print("="*60)
    
    print(f"\nOverview:")
    print(f"  Total Epics: {stats['total_epics']}")
    print(f"  Completed: {stats['completed_epics']} ({stats['completed_epics']/stats['total_epics']*100:.1f}%)")
    print(f"  In Progress: {stats['in_progress_epics']}")
    print(f"  Failed: {stats['failed_epics']}")
    print(f"  Total Events: {stats['total_events']}")
    
    if stats['start_time']:
        print(f"\nTimeline:")
        print(f"  Start: {stats['start_time']}")
        print(f"  End: {stats['end_time']}")
    
    print(f"\nPhase Completions:")
    for phase in sorted(stats['phases_completed'].keys()):
        count = stats['phases_completed'][phase]
        print(f"  Phase {phase}: {count}")
    
    if stats['avg_cyc_reduction'] > 0:
        print(f"\nComplexity Reduction:")
        print(f"  Average CYC Reduction: {stats['avg_cyc_reduction']:.1f}")
    
    if stats['total_bobcoins'] > 0:
        print(f"\nCost:")
        print(f"  Total Bobcoins: ${stats['total_bobcoins']:.2f}")
    
    print("\n" + "="*60)


def main():
    """Main entry point."""
    print("Loading Wave 7 events...")
    events = load_wave7_events()
    
    if not events:
        print("No events to process")
        return
    
    print(f"Computing statistics from {len(events)} events...")
    stats = compute_statistics(events)
    
    write_statistics(stats)
    print_summary(stats)


if __name__ == "__main__":
    main()

# Made with Bob
