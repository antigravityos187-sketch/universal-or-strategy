#!/usr/bin/env python3
"""Check discrepancy between Phase 5 report (79/80) and roadmap (8/66)"""

import json
from pathlib import Path

def check_discrepancy():
    """Compare different roadmap files to understand the discrepancy"""
    
    roadmap_files = [
        'epic_roadmap.json',
        'epic_roadmap_wave4_fresh.json',
        'epic_roadmap_wave4_phase4_complete.json'
    ]
    
    print("=" * 80)
    print("WAVE 4 ROADMAP DISCREPANCY ANALYSIS")
    print("=" * 80)
    
    for filename in roadmap_files:
        filepath = Path(filename)
        if not filepath.exists():
            print(f"\n❌ {filename}: NOT FOUND")
            continue
        
        with open(filepath) as f:
            epics = json.load(f)
        
        # Filter Wave 4 epics (EPIC-CCN-001 through EPIC-CCN-080)
        wave4_epics = [e for e in epics if e['epic_number'].startswith('EPIC-CCN-') 
                       and 1 <= int(e['epic_number'].split('-')[-1]) <= 80]
        
        # Count by status
        complete = len([e for e in wave4_epics if e.get('status') == 'complete'])
        pending = len([e for e in wave4_epics if e.get('status') == 'pending'])
        other = len(wave4_epics) - complete - pending
        
        print(f"\n📄 {filename}")
        print(f"   Total Wave 4 Epics: {len(wave4_epics)}")
        print(f"   Complete: {complete}")
        print(f"   Pending: {pending}")
        print(f"   Other: {other}")
        
        # Show complete epics
        if complete > 0:
            complete_list = [e['epic_number'] for e in wave4_epics if e.get('status') == 'complete']
            print(f"   Complete epics: {', '.join(sorted(complete_list))}")
    
    # Check if there's a VM-specific roadmap
    print("\n" + "=" * 80)
    print("CHECKING FOR VM ROADMAP")
    print("=" * 80)
    
    # The Phase 5 report says 79/80 complete, which suggests there's a different
    # roadmap file on the VM that was updated during Wave 4 execution
    print("\n⚠️  HYPOTHESIS:")
    print("   The Phase 5 completion report (79/80) refers to a roadmap file")
    print("   that was updated ON THE VM during Wave 4 execution.")
    print("   The local roadmap files are STALE and don't reflect VM progress.")
    print("\n   To get accurate status, we need to:")
    print("   1. Check if VM is still running")
    print("   2. Sync the updated roadmap from VM to local")
    print("   3. OR check docs/brain/EPIC-CCN-*/06-verification-report.md files")
    
    # Check for verification reports (Phase 6 output)
    print("\n" + "=" * 80)
    print("CHECKING FOR PHASE 6 VERIFICATION REPORTS")
    print("=" * 80)
    
    brain_dir = Path('docs/brain')
    if brain_dir.exists():
        verification_reports = list(brain_dir.glob('EPIC-CCN-*/06-verification-report.md'))
        print(f"\nFound {len(verification_reports)} Phase 6 verification reports")
        
        if verification_reports:
            print("\nEpics with Phase 6 verification:")
            for report in sorted(verification_reports)[:10]:
                epic_id = report.parent.name
                print(f"   - {epic_id}")
            if len(verification_reports) > 10:
                print(f"   ... and {len(verification_reports) - 10} more")
    
    # Check for Phase 5 completion files
    print("\n" + "=" * 80)
    print("CHECKING FOR PHASE 5 COMPLETION FILES")
    print("=" * 80)
    
    if brain_dir.exists():
        completion_files = list(brain_dir.glob('EPIC-CCN-*/ticket-*-completion.md'))
        epics_with_completions = set(f.parent.name for f in completion_files)
        print(f"\nFound {len(epics_with_completions)} epics with Phase 5 completion files")
        
        if epics_with_completions:
            print("\nEpics with Phase 5 completions:")
            for epic_id in sorted(epics_with_completions)[:10]:
                print(f"   - {epic_id}")
            if len(epics_with_completions) > 10:
                print(f"   ... and {len(epics_with_completions) - 10} more")

if __name__ == '__main__':
    check_discrepancy()

# Made with Bob
