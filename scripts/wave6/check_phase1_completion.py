#!/usr/bin/env python3
"""Check Wave 6 Phase 1 completion status."""

from pathlib import Path

completed = 0
pending = []

for i in range(1, 25):
    epic_id = f'EPIC-CCN-{i:03d}'
    scope_file = Path(f'docs/brain/{epic_id}/00-scope.md')
    if scope_file.exists():
        completed += 1
    else:
        pending.append(epic_id)

print(f'Wave 6 Phase 1 Status: {completed}/24 complete')
if pending:
    print(f'Pending: {", ".join(pending)}')
else:
    print('✅ ALL 24 EPICS COMPLETE!')

# Made with Bob
