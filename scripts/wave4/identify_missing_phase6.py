#!/usr/bin/env python3
"""Identify epics missing Phase 6 verification reports."""

from pathlib import Path

# Check which epics are missing 06-*.md files
missing = []
for i in range(1, 81):
    if i == 16:  # Skip EPIC-CCN-016 (deferred)
        continue
    
    epic_id = f"EPIC-CCN-{i:03d}"
    brain_dir = Path(f"docs/brain/{epic_id}")
    
    # Check for any 06-*.md file
    reports = list(brain_dir.glob("06-*.md"))
    if not reports:
        missing.append(epic_id)

print(f"=== MISSING PHASE 6 REPORTS ===")
print(f"Total missing: {len(missing)}/79")
print()
for epic in missing:
    print(epic)

print()
print(f"=== SUMMARY ===")
print(f"Complete: {79 - len(missing)}/79 ({100*(79-len(missing))/79:.1f}%)")
print(f"Missing: {len(missing)}/79 ({100*len(missing)/79:.1f}%)")

# Made with Bob
