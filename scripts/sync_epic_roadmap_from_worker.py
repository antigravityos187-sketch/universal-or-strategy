#!/usr/bin/env python3
"""
Sync epic_roadmap.json with completed epics from worker git logs.

Usage:
    python scripts/sync_epic_roadmap_from_worker.py --worker-path C:/WSGTA/universal-or-epic-cluster-1
"""

import json
import re
import subprocess
import sys
from pathlib import Path
from datetime import datetime
from typing import List, Dict, Optional

def get_completed_epics_from_git(worker_path: str) -> List[Dict[str, str]]:
    """Extract completed epic info from git log."""
    try:
        # Get git log from worker worktree
        result = subprocess.run(
            ["git", "log", "--oneline", "--all"],
            cwd=worker_path,
            capture_output=True,
            text=True,
            check=True
        )
        
        completed = []
        epic_pattern = re.compile(r'EPIC-CCN-(\d+)', re.IGNORECASE)
        
        for line in result.stdout.splitlines():
            match = epic_pattern.search(line)
            if match:
                epic_num = match.group(1)
                epic_id = f"EPIC-CCN-{epic_num}"
                
                # Extract commit message
                commit_msg = line.split(' ', 1)[1] if ' ' in line else line
                
                # Try to extract final CYC from commit message
                cyc_match = re.search(r'CYC[:\s]+(\d+)\s*[-=>]+\s*(\d+)', commit_msg, re.IGNORECASE)
                final_cyc = int(cyc_match.group(2)) if cyc_match else None
                
                completed.append({
                    'epic_id': epic_id,
                    'epic_number': int(epic_num),
                    'commit_msg': commit_msg,
                    'final_cyc': final_cyc
                })
        
        # Deduplicate by epic_number (keep first occurrence = most recent)
        seen = set()
        unique_completed = []
        for epic in completed:
            if epic['epic_number'] not in seen:
                seen.add(epic['epic_number'])
                unique_completed.append(epic)
        
        return sorted(unique_completed, key=lambda x: x['epic_number'])
        
    except subprocess.CalledProcessError as e:
        print(f"Error running git log: {e}", file=sys.stderr)
        return []

def update_roadmap(roadmap_path: str, completed_epics: List[Dict[str, str]]) -> Dict:
    """Update epic_roadmap.json with completion status."""
    with open(roadmap_path, 'r') as f:
        roadmap = json.load(f)
    
    completed_map = {e['epic_id']: e for e in completed_epics}
    
    updated_count = 0
    for epic in roadmap:
        epic_id = epic.get('epic_number')
        if epic_id in completed_map:
            completed_info = completed_map[epic_id]
            
            # Update status
            if epic.get('status') != 'complete':
                epic['status'] = 'complete'
                epic['completion_date'] = datetime.now().strftime('%Y-%m-%d')
                
                # Update final_cyc if available
                if completed_info['final_cyc']:
                    epic['final_cyc'] = completed_info['final_cyc']
                
                updated_count += 1
                print(f"[OK] Marked {epic_id} as complete (final CYC: {epic.get('final_cyc', 'N/A')})")
    
    return {
        'roadmap': roadmap,
        'updated_count': updated_count,
        'total_completed': len([e for e in roadmap if e.get('status') == 'complete'])
    }

def main():
    import argparse
    parser = argparse.ArgumentParser(description='Sync epic roadmap with worker git log')
    parser.add_argument('--worker-path', required=True, help='Path to worker worktree')
    parser.add_argument('--roadmap', default='epic_roadmap.json', help='Path to epic_roadmap.json')
    parser.add_argument('--dry-run', action='store_true', help='Show changes without writing')
    args = parser.parse_args()
    
    print(f"[SCAN] Scanning git log in {args.worker_path}...")
    completed_epics = get_completed_epics_from_git(args.worker_path)
    
    if not completed_epics:
        print("[ERROR] No completed epics found in git log")
        return 1
    
    print(f"\n[OK] Found {len(completed_epics)} completed epics:")
    for epic in completed_epics:
        cyc_info = f" (CYC: {epic['final_cyc']})" if epic['final_cyc'] else ""
        print(f"  - {epic['epic_id']}{cyc_info}")
    
    print(f"\n[UPDATE] Updating {args.roadmap}...")
    result = update_roadmap(args.roadmap, completed_epics)
    
    if args.dry_run:
        print(f"\n[DRY-RUN] Would update {result['updated_count']} epics")
        print(f"   Total completed: {result['total_completed']}/{len(result['roadmap'])}")
        return 0
    
    # Write updated roadmap
    with open(args.roadmap, 'w') as f:
        json.dump(result['roadmap'], f, indent=2)
    
    print(f"\n[OK] Updated {result['updated_count']} epics")
    print(f"   Total completed: {result['total_completed']}/{len(result['roadmap'])} ({result['total_completed']/len(result['roadmap'])*100:.1f}%)")
    
    return 0

if __name__ == '__main__':
    sys.exit(main())

# Made with Bob
