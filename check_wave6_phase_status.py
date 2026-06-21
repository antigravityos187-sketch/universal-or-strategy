#!/usr/bin/env python3
"""Check which phases are complete for Wave 6 epics."""

import json
import os
from pathlib import Path

# Load epic roadmap
with open('epic_roadmap.json', 'r') as f:
    data = json.load(f)

wave6 = [e for e in data if e['epic_number'].startswith('EPIC-CCN-')]
complete = [e for e in wave6 if e.get('status') == 'complete']

print("=" * 80)
print("WAVE 6 PHASE STATUS CHECK")
print("=" * 80)

print(f"\nTotal Wave 6 Epics: {len(wave6)}")
print(f"Complete: {len(complete)}")
print(f"Pending: {len(wave6) - len(complete)}")

if complete:
    print("\nCompleted Epics:")
    for e in complete:
        print(f"  {e['epic_number']}: {e['method']}")

# Check brain directories for phase completion
print("\n" + "=" * 80)
print("PHASE COMPLETION STATUS (by brain directory)")
print("=" * 80)

brain_dir = Path('docs/brain')
phase_files = {
    'Phase 0': '00-hotspots.md',
    'Phase 1': '00-scope.md',
    'Phase 1.5': '01-scope-boundary.md',
    'Phase 2': '02-architecture-plan.md',
    'Phase 3': '03-audit-report.md',
    'Phase 4': '04-tickets.md',
    'Phase 5': 'ticket-*-completion.md',
    'Phase 5.V': 'ticket-*-verification.md',
    'Phase 6': '05-completion-report.md'
}

phase_counts = {phase: 0 for phase in phase_files.keys()}

for epic in wave6:
    epic_dir = brain_dir / epic['epic_number']
    if epic_dir.exists():
        for phase, pattern in phase_files.items():
            if '*' in pattern:
                # Glob pattern
                if list(epic_dir.glob(pattern)):
                    phase_counts[phase] += 1
            else:
                # Exact file
                if (epic_dir / pattern).exists():
                    phase_counts[phase] += 1

print("\nPhase Completion Counts:")
for phase, count in phase_counts.items():
    pct = (count * 100) // len(wave6) if wave6 else 0
    status = "[OK]" if count == len(wave6) else "[..]"
    print(f"  {status} {phase:12s}: {count:3d}/{len(wave6)} ({pct}%)")

# Recommendation
print("\n" + "=" * 80)
print("RECOMMENDATION")
print("=" * 80)

if phase_counts['Phase 0'] == 0:
    print("\n[X] START FROM PHASE 0")
    print("   No Phase 0 files found. Must start from beginning.")
elif phase_counts['Phase 0'] < len(wave6):
    print(f"\n[!] COMPLETE PHASE 0 FIRST")
    print(f"   {phase_counts['Phase 0']}/{len(wave6)} epics have Phase 0 complete")
    print(f"   Complete remaining {len(wave6) - phase_counts['Phase 0']} epics")
elif phase_counts['Phase 1'] < len(wave6):
    print(f"\n[!] COMPLETE PHASE 1 NEXT")
    print(f"   Phase 0: {phase_counts['Phase 0']}/{len(wave6)} [OK]")
    print(f"   Phase 1: {phase_counts['Phase 1']}/{len(wave6)} [..]")
    print(f"   Complete remaining {len(wave6) - phase_counts['Phase 1']} epics")
elif phase_counts['Phase 1.5'] < len(wave6):
    print(f"\n[OK] CONTINUE WITH PHASE 1.5")
    print(f"   Phase 0: {phase_counts['Phase 0']}/{len(wave6)} [OK]")
    print(f"   Phase 1: {phase_counts['Phase 1']}/{len(wave6)} [OK]")
    print(f"   Phase 1.5: {phase_counts['Phase 1.5']}/{len(wave6)} [..]")
    print(f"   Complete remaining {len(wave6) - phase_counts['Phase 1.5']} epics")
else:
    print(f"\n[OK] PHASES 0-1.5 COMPLETE")
    print(f"   Proceed to Phase 2")

print("\n" + "=" * 80)

# Made with Bob
