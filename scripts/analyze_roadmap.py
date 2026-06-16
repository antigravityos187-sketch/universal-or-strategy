#!/usr/bin/env python3
"""Analyze epic roadmap status."""

import json
from pathlib import Path

def analyze_roadmap():
    roadmap_path = Path("epic_roadmap.json")
    data = json.loads(roadmap_path.read_text())
    
    total = len(data)
    completed = sum(1 for e in data if e.get("status") == "complete")
    pending = total - completed
    
    print(f"Total epics: {total}")
    print(f"Completed: {completed}")
    print(f"Pending: {pending}")
    print(f"\nFirst 10 pending epics:")
    
    pending_epics = [e for e in data if e.get("status") != "complete"]
    for i, epic in enumerate(pending_epics[:10], 1):
        print(f"{i}. {epic['epic_number']}: {epic['method']} (CYC={epic['cyclomatic']})")

if __name__ == "__main__":
    analyze_roadmap()

# Made with Bob
