#!/usr/bin/env python3
"""
Generate epic roadmap from complexity audit output.
Creates epic_roadmap.json with all methods CYC > 8.
"""

import json
import re
import subprocess
from pathlib import Path
from typing import List, Dict, Any

def run_complexity_audit() -> str:
    """Run complexity audit and capture output."""
    result = subprocess.run(
        ['python', 'scripts/complexity_audit.py'],
        capture_output=True,
        text=True,
        cwd=Path.cwd()
    )
    return result.stdout

def parse_audit_output(output: str) -> List[Dict[str, Any]]:
    """Parse complexity audit output into epic entries."""
    epics = []
    current_file = None
    epic_number = 13  # Start from CCN-13 (first incomplete)
    
    # Pattern: | Method | LOC | Est. CYC | M5 Candidate? | Action |
    method_pattern = re.compile(
        r'\|\s*([^\|]+?)\s*\|\s*(\d+)\s*\|\s*(\d+)\s*\|[^\|]*\|\s*(REFACTOR|WATCH)'
    )
    
    # Pattern: === FILE: filename.cs ===
    file_pattern = re.compile(r'=== FILE: (.+?) ===')
    
    for line in output.split('\n'):
        # Check for file header
        file_match = file_pattern.search(line)
        if file_match:
            current_file = f"src/{file_match.group(1)}"
            continue
        
        # Check for method entry
        method_match = method_pattern.search(line)
        if method_match and current_file:
            method_name = method_match.group(1).strip()
            loc = int(method_match.group(2))
            cyc = int(method_match.group(3))
            action = method_match.group(4).strip()
            
            # Only include REFACTOR (CYC > 8)
            if action == 'REFACTOR' and cyc > 8:
                epics.append({
                    'epic_number': f'EPIC-CCN-{epic_number}',
                    'method': method_name,
                    'file': current_file,
                    'line': 0,  # Will be filled by actual line lookup
                    'cyclomatic': cyc,
                    'churn': 0,  # Placeholder
                    'hotspot_score': 0.0,  # Placeholder
                    'codescene_score': None,
                    'composite_score': 0.0,  # Placeholder
                    'codescene_issues': []
                })
                epic_number += 1
    
    return epics

def load_existing_roadmap() -> List[Dict[str, Any]]:
    """Load existing epic_roadmap.json if it exists."""
    roadmap_path = Path('epic_roadmap.json')
    if roadmap_path.exists():
        with open(roadmap_path, 'r') as f:
            return json.load(f)
    return []

def merge_roadmaps(existing: List[Dict[str, Any]], new: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
    """Merge existing and new roadmaps, preserving completed epics."""
    # Keep all completed epics
    completed = [e for e in existing if e.get('status') == 'complete']
    
    # Get highest completed epic number
    max_completed = 0
    for epic in completed:
        num = int(epic['epic_number'].split('-')[-1])
        max_completed = max(max_completed, num)
    
    # Renumber new epics to start after completed ones
    renumbered = []
    next_num = max_completed + 1
    for epic in new:
        epic['epic_number'] = f'EPIC-CCN-{next_num}'
        renumbered.append(epic)
        next_num += 1
    
    return completed + renumbered

def main():
    """Generate epic roadmap."""
    print("Running complexity audit...")
    audit_output = run_complexity_audit()
    
    print("Parsing audit results...")
    new_epics = parse_audit_output(audit_output)
    
    print(f"Found {len(new_epics)} methods with CYC > 8")
    
    print("Loading existing roadmap...")
    existing_epics = load_existing_roadmap()
    
    print("Merging roadmaps...")
    merged = merge_roadmaps(existing_epics, new_epics)
    
    print(f"Writing {len(merged)} epics to epic_roadmap.json...")
    with open('epic_roadmap.json', 'w') as f:
        json.dump(merged, f, indent=2)
    
    # Print summary
    completed = [e for e in merged if e.get('status') == 'complete']
    pending = [e for e in merged if e.get('status') != 'complete']
    
    print(f"\n=== EPIC ROADMAP SUMMARY ===")
    print(f"Total epics: {len(merged)}")
    print(f"Completed: {len(completed)}")
    print(f"Pending: {len(pending)}")
    print(f"\nNext epic: {pending[0]['epic_number'] if pending else 'None'}")
    print(f"Method: {pending[0]['method'] if pending else 'N/A'}")
    print(f"File: {pending[0]['file'] if pending else 'N/A'}")
    print(f"CYC: {pending[0]['cyclomatic'] if pending else 'N/A'}")

if __name__ == '__main__':
    main()

# Made with Bob
