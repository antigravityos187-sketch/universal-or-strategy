#!/usr/bin/env python3
"""Analyze Wave 4 completion status from epic_roadmap.json"""

import json
from pathlib import Path
from collections import defaultdict

def analyze_wave4_status():
    """Analyze Wave 4 (EPIC-CCN-001 through EPIC-CCN-080) completion status"""
    
    roadmap_path = Path('epic_roadmap.json')
    if not roadmap_path.exists():
        print(f"ERROR: {roadmap_path} not found")
        return
    
    with open(roadmap_path) as f:
        epics = json.load(f)
    
    # Filter Wave 4 epics (EPIC-CCN-001 through EPIC-CCN-080)
    wave4_epics = [e for e in epics if e['epic_number'].startswith('EPIC-CCN-') 
                   and 1 <= int(e['epic_number'].split('-')[-1]) <= 80]
    
    # Analyze by status
    status_counts = defaultdict(list)
    for epic in wave4_epics:
        status = epic.get('status', 'pending')
        status_counts[status].append(epic['epic_number'])
    
    # Print summary
    print("=" * 80)
    print("WAVE 4 COMPLETION STATUS (EPIC-CCN-001 through EPIC-CCN-080)")
    print("=" * 80)
    print(f"\nTotal Epics: {len(wave4_epics)}")
    print(f"\nStatus Breakdown:")
    print("-" * 80)
    
    for status in sorted(status_counts.keys()):
        epic_list = status_counts[status]
        count = len(epic_list)
        percentage = (count / len(wave4_epics)) * 100
        print(f"\n{status.upper()}: {count}/{len(wave4_epics)} ({percentage:.1f}%)")
        
        # Show first 10 and last 10 if more than 20
        if count <= 20:
            for epic_id in sorted(epic_list):
                print(f"  - {epic_id}")
        else:
            print(f"  First 10:")
            for epic_id in sorted(epic_list)[:10]:
                print(f"    - {epic_id}")
            print(f"  ... ({count - 20} more) ...")
            print(f"  Last 10:")
            for epic_id in sorted(epic_list)[-10:]:
                print(f"    - {epic_id}")
    
    # Identify incomplete epics
    incomplete_statuses = ['pending', 'in_progress', 'blocked', 'deferred', 'failed']
    incomplete = []
    for status in incomplete_statuses:
        incomplete.extend(status_counts.get(status, []))
    
    print("\n" + "=" * 80)
    print(f"INCOMPLETE EPICS: {len(incomplete)}/{len(wave4_epics)}")
    print("=" * 80)
    
    if incomplete:
        print("\nEpics requiring completion:")
        for epic_id in sorted(incomplete):
            epic = next(e for e in wave4_epics if e['epic_number'] == epic_id)
            status = epic.get('status', 'pending')
            method = epic.get('method', 'unknown')
            file = epic.get('file', 'unknown')
            cyc = epic.get('cyclomatic', 0)
            print(f"\n  {epic_id} ({status.upper()})")
            print(f"    Method: {method}")
            print(f"    File: {file}")
            print(f"    Complexity: {cyc}")
    else:
        print("\n✅ ALL WAVE 4 EPICS COMPLETE!")
    
    # Check for local execution requirements
    print("\n" + "=" * 80)
    print("LOCAL EXECUTION ANALYSIS")
    print("=" * 80)
    
    # Epics that might require local execution (based on file path)
    local_required = []
    for epic_id in incomplete:
        epic = next(e for e in wave4_epics if e['epic_number'] == epic_id)
        file = epic.get('file', '')
        # Check if file is in src/ (requires NinjaTrader)
        if file.startswith('src/'):
            local_required.append(epic_id)
    
    if local_required:
        print(f"\nEpics likely requiring local Windows/NT8 execution: {len(local_required)}")
        for epic_id in sorted(local_required):
            epic = next(e for e in wave4_epics if e['epic_number'] == epic_id)
            print(f"  - {epic_id}: {epic.get('file', 'unknown')}")
    else:
        print("\n✅ No epics require local execution (all in src/)")
    
    # Summary for user
    print("\n" + "=" * 80)
    print("PATH TO 80/80 COMPLETION")
    print("=" * 80)
    
    complete_count = len(status_counts.get('complete', []))
    remaining = len(wave4_epics) - complete_count
    
    print(f"\nCurrent: {complete_count}/80 complete ({(complete_count/80)*100:.1f}%)")
    print(f"Remaining: {remaining} epics")
    
    if remaining == 0:
        print("\n🎉 WAVE 4 IS COMPLETE! All 80/80 epics done!")
    else:
        print(f"\nTo reach 80/80:")
        print(f"  1. Complete {len(local_required)} epics requiring local execution")
        print(f"  2. Complete {remaining - len(local_required)} epics on VM")
        print(f"\nEstimated time: {remaining * 30} minutes ({remaining * 30 / 60:.1f} hours)")

if __name__ == '__main__':
    analyze_wave4_status()

# Made with Bob
