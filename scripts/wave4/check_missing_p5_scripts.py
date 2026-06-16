#!/usr/bin/env python3
"""Check which Phase 5 scripts exist locally."""

from pathlib import Path

failed_epics = ['003', '015', '030', '031', '033', '042', '055']

print("=== Phase 5 Script Check (Local) ===\n")

for num in failed_epics:
    script_path = Path(f'scripts/wave4/_p5_{num}.sh')
    if script_path.exists():
        size = script_path.stat().st_size
        print(f"[OK] EXISTS: {script_path} ({size} bytes)")
    else:
        print(f"[X] MISSING: {script_path}")

print(f"\n=== Summary ===")
existing = [num for num in failed_epics if Path(f'scripts/wave4/_p5_{num}.sh').exists()]
print(f"Existing: {len(existing)}/{len(failed_epics)}")
if existing:
    print(f"  {', '.join(existing)}")

missing = [num for num in failed_epics if not Path(f'scripts/wave4/_p5_{num}.sh').exists()]
if missing:
    print(f"Missing: {len(missing)}/{len(failed_epics)}")
    print(f"  {', '.join(missing)}")

# Made with Bob
