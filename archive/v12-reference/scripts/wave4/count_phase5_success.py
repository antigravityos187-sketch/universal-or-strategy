#!/usr/bin/env python3
"""Count Phase 5 successful epics."""

from pathlib import Path

# Check which epics have Phase 5 completion files
successful = []
for i in range(1, 81):
    if i == 16:  # Skip EPIC-CCN-016 (deferred)
        continue
    
    epic_id = f"EPIC-CCN-{i:03d}"
    brain_dir = Path(f"docs/brain/{epic_id}")
    
    # Check for any Phase 5 completion file
    has_05 = list(brain_dir.glob("05-*.md"))
    has_tickets = list(brain_dir.glob("ticket-*-completion.md"))
    
    if has_05 or has_tickets:
        successful.append(epic_id)
        print(f"{epic_id}: ✅ ({len(has_05)} 05-*.md, {len(has_tickets)} ticket-*.md)")
    else:
        print(f"{epic_id}: ❌ NO PHASE 5 FILES")

print()
print(f"=== PHASE 5 SUMMARY ===")
print(f"Successful: {len(successful)}/79 ({100*len(successful)/79:.1f}%)")
print(f"Failed: {79 - len(successful)}/79 ({100*(79-len(successful))/79:.1f}%)")

# Made with Bob
