#!/usr/bin/env python3
"""Find missing Phase 0 scripts."""

import os

missing = []
for i in range(1, 81):
    epic_id = f'EPIC-CCN-{i:03d}'
    script_path = f'scripts/wave6/_p0_epic_ccn_{i:03d}.sh'
    if not os.path.exists(script_path):
        missing.append(epic_id)

if missing:
    print(f"Missing Phase 0 scripts: {', '.join(missing)}")
else:
    print("All Phase 0 scripts present")

# Made with Bob
