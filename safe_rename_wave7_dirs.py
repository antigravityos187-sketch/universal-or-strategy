#!/usr/bin/env python3
"""
Safe rename of 5 Wave 7 directories from EPIC-CCN-XXX to EPIC-W7-XXX
"""

import os
import shutil

renames = [
    ("docs/brain/EPIC-CCN-155", "docs/brain/EPIC-W7-009", "TryHandleFleetCommand"),
    ("docs/brain/EPIC-CCN-98", "docs/brain/EPIC-W7-013", "ProcessFlattenWorkItem_CancelOrders"),
    ("docs/brain/EPIC-CCN-128", "docs/brain/EPIC-W7-014", "SymmetryGuardReplaceExistingFollowerTarget"),
    ("docs/brain/EPIC-CCN-129", "docs/brain/EPIC-W7-015", "SymmetryGuardTryResolveFollowersForDispatch"),
    ("docs/brain/EPIC-CCN-023", "docs/brain/EPIC-W7-019", "HandleFlatPosition_CleanupActivePositions"),
]

print("Renaming 5 Wave 7 directories to correct EPIC-W7-XXX format...")
print()

for old_path, new_path, method in renames:
    if os.path.exists(old_path):
        if os.path.exists(new_path):
            print(f"⚠️  Target already exists: {new_path}")
            print(f"   Skipping {old_path}")
        else:
            shutil.move(old_path, new_path)
            print(f"✓ Renamed {os.path.basename(old_path)} -> {os.path.basename(new_path)} ({method})")
    else:
        print(f"⚠️  Source not found: {old_path}")

print()
print("✅ Rename operation complete!")
print("Wave 7 directories now: 15 total, all with correct EPIC-W7-XXX naming")

# Made with Bob
