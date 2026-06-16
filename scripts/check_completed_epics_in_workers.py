#!/usr/bin/env python3
"""Check which Wave 1 epics are already completed in worker worktrees."""

import os

# Wave 1 epic IDs
wave1_epics = [21, 22, 23, 24, 25, 26, 27, 28, 29, 30]

# Worker worktree paths
workers = {
    'worker-1': 'C:/WSGTA/universal-or-epic-cluster-1',
    'worker-2': 'C:/WSGTA/universal-or-epic-cluster-2',
    'worker-3': 'C:/WSGTA/universal-or-epic-cluster-3',
    'worker-4': 'C:/WSGTA/universal-or-epic-cluster-4'
}

print("Wave 1 Epic Status Across Workers")
print("=" * 80)

# Track which epics are found in which workers
epic_locations = {}
for epic_num in wave1_epics:
    epic_locations[epic_num] = []

# Check each worker
for worker_name, worker_path in workers.items():
    brain_path = os.path.join(worker_path, 'docs', 'brain')
    if os.path.exists(brain_path):
        for epic_num in wave1_epics:
            epic_dir = f'EPIC-CCN-{epic_num}'
            epic_path = os.path.join(brain_path, epic_dir)
            if os.path.exists(epic_path):
                epic_locations[epic_num].append(worker_name)

# Print results
print("\nEpic Distribution:")
print("-" * 80)
for epic_num in wave1_epics:
    locations = epic_locations[epic_num]
    if locations:
        status = f"FOUND in {', '.join(locations)}"
    else:
        status = "NOT FOUND (needs work)"
    print(f"EPIC-CCN-{epic_num}: {status}")

# Summary
completed = [e for e in wave1_epics if epic_locations[e]]
pending = [e for e in wave1_epics if not epic_locations[e]]

print("\n" + "=" * 80)
print(f"Completed: {len(completed)}/10 epics")
print(f"Pending: {len(pending)}/10 epics")

if pending:
    print(f"\nEpics needing work: {', '.join([f'EPIC-CCN-{e}' for e in pending])}")
else:
    print("\nAll Wave 1 epics are complete!")

# Made with Bob
