#!/usr/bin/env python3
"""Identify failed epics from Wave 4 Phase 5 execution."""

# All epics that should have been executed (001-080)
all_epics = [f"EPIC-CCN-{i:03d}" for i in range(1, 81)]

# Completed epics from VM output
completed_epics = [
    "EPIC-CCN-001", "EPIC-CCN-002", "EPIC-CCN-004", "EPIC-CCN-008",
    "EPIC-CCN-011", "EPIC-CCN-012", "EPIC-CCN-013", "EPIC-CCN-014",
    "EPIC-CCN-017", "EPIC-CCN-019", "EPIC-CCN-020", "EPIC-CCN-022",
    "EPIC-CCN-024", "EPIC-CCN-025", "EPIC-CCN-026", "EPIC-CCN-028",
    "EPIC-CCN-029", "EPIC-CCN-032", "EPIC-CCN-036", "EPIC-CCN-038",
    "EPIC-CCN-041", "EPIC-CCN-043", "EPIC-CCN-044", "EPIC-CCN-046",
    "EPIC-CCN-049", "EPIC-CCN-050", "EPIC-CCN-051", "EPIC-CCN-052",
    "EPIC-CCN-054", "EPIC-CCN-056", "EPIC-CCN-057", "EPIC-CCN-058",
    "EPIC-CCN-059", "EPIC-CCN-061", "EPIC-CCN-062", "EPIC-CCN-063",
    "EPIC-CCN-064", "EPIC-CCN-065", "EPIC-CCN-067", "EPIC-CCN-070",
    "EPIC-CCN-074", "EPIC-CCN-075", "EPIC-CCN-077", "EPIC-CCN-078",
]

# Find failed epics
failed_epics = [epic for epic in all_epics if epic not in completed_epics]

print("=== WAVE 4 PHASE 5 RECOVERY ANALYSIS ===\n")
print(f"Total Epics: {len(all_epics)}")
print(f"Completed: {len(completed_epics)} ({len(completed_epics)/len(all_epics)*100:.1f}%)")
print(f"Failed: {len(failed_epics)} ({len(failed_epics)/len(all_epics)*100:.1f}%)\n")

print("=== FAILED EPICS (Need Recovery) ===")
for epic in failed_epics:
    print(epic)

print(f"\n=== RECOVERY COMMAND ===")
print(f"Total epics to recover: {len(failed_epics)}")
print(f"\nFailed epic IDs for recovery script:")
print('FAILED_EPICS=("' + '" "'.join([e.split('-')[2] for e in failed_epics]) + '")')

# Made with Bob
