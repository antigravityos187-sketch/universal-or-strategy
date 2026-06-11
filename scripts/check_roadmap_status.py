#!/usr/bin/env python3
"""Check epic roadmap status."""

import json
from pathlib import Path

def main():
    roadmap_path = Path("epic_roadmap.json")
    
    with open(roadmap_path) as f:
        data = json.load(f)
    
    completed = [e for e in data if e.get('status') == 'completed']
    in_progress = [e for e in data if e.get('status') == 'in_progress']
    pending = [e for e in data if e.get('status', 'pending') == 'pending']
    
    print(f"Total EPICs: {len(data)}")
    print(f"Completed: {len(completed)}")
    print(f"In Progress: {len(in_progress)}")
    print(f"Pending: {len(pending)}")
    print(f"\nProgress: {len(completed)}/{len(data)} ({100*len(completed)/len(data):.1f}%)")
    
    if completed:
        print(f"\nCompleted EPICs (showing first 15):")
        for e in completed[:15]:
            print(f"  {e['epic_number']}: {e['method']}")
    
    if in_progress:
        print(f"\nIn Progress EPICs:")
        for e in in_progress:
            print(f"  {e['epic_number']}: {e['method']} (File: {e['file']})")
    
    if pending:
        print(f"\nNext Pending EPICs (showing first 5):")
        for e in pending[:5]:
            print(f"  {e['epic_number']}: {e['method']} (CYC: {e['cyclomatic']})")

if __name__ == "__main__":
    main()

# Made with Bob
