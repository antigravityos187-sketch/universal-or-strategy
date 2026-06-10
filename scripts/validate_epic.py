#!/usr/bin/env python3
"""
Epic Validation Script - V12 Universal OR Strategy
Prevents phantom epic execution by validating against epic_roadmap.json
"""

import json
import sys
import io
import subprocess
from pathlib import Path
from datetime import datetime
from typing import Optional, Dict, List

# Fix Windows console encoding
if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')

ROADMAP_PATH = Path("epic_roadmap.json")

def load_roadmap() -> List[Dict]:
    """Load epic_roadmap.json"""
    if not ROADMAP_PATH.exists():
        raise FileNotFoundError(f"Roadmap not found: {ROADMAP_PATH}")
    
    with open(ROADMAP_PATH, 'r', encoding='utf-8') as f:
        return json.load(f)

def save_roadmap(roadmap: List[Dict]) -> None:
    """Save updated roadmap"""
    with open(ROADMAP_PATH, 'w', encoding='utf-8') as f:
        json.dump(roadmap, f, indent=2, ensure_ascii=False)

def validate_epic_exists(epic_id: str) -> bool:
    """Verify epic exists in roadmap"""
    roadmap = load_roadmap()
    return any(e['epic_number'] == epic_id for e in roadmap)

def get_epic_details(epic_id: str) -> Optional[Dict]:
    """Get epic details from roadmap"""
    roadmap = load_roadmap()
    for epic in roadmap:
        if epic['epic_number'] == epic_id:
            return epic
    return None

def get_next_epic() -> Optional[Dict]:
    """Get next pending epic from roadmap (status != 'complete', not assigned)"""
    roadmap = load_roadmap()
    for epic in roadmap:
        if epic.get('status') != 'complete' and not epic.get('assigned_to'):
            return epic
    return None

def claim_epic(epic_id: str, worker_id: str) -> Dict:
    """
    Atomically claim epic for worker using git pull + commit + push
    Raises ValueError if epic already assigned or not found
    """
    # Pull latest roadmap to avoid race conditions
    try:
        subprocess.run(
            ["git", "pull", "origin", "gitbutler/workspace", "--rebase"],
            check=True,
            capture_output=True,
            text=True
        )
    except subprocess.CalledProcessError as e:
        print(f"[WARN] Git pull failed: {e.stderr}")
        print("[INFO] Continuing with local roadmap (may cause conflicts)")
    
    roadmap = load_roadmap()
    
    for epic in roadmap:
        if epic['epic_number'] == epic_id:
            # Check if already assigned
            if epic.get('assigned_to'):
                raise ValueError(
                    f"Epic {epic_id} already assigned to {epic['assigned_to']} "
                    f"at {epic.get('lock_timestamp', 'unknown time')}"
                )
            
            # Claim epic
            epic['assigned_to'] = worker_id
            epic['lock_timestamp'] = datetime.utcnow().isoformat() + 'Z'
            
            # Save roadmap
            save_roadmap(roadmap)
            
            # Commit and push atomically
            try:
                subprocess.run(["git", "add", "epic_roadmap.json"], check=True)
                subprocess.run(
                    ["git", "commit", "-m", f"lock: Claim {epic_id} for {worker_id}"],
                    check=True
                )
                subprocess.run(
                    ["git", "push", "origin", "gitbutler/workspace"],
                    check=True
                )
                print(f"[OK] Epic {epic_id} claimed by {worker_id} (pushed to git)")
            except subprocess.CalledProcessError as e:
                print(f"[ERROR] Failed to push claim: {e}")
                print("[WARN] Epic claimed locally but not synced to git")
            
            return epic
    
    raise ValueError(f"Epic {epic_id} not found in roadmap")

def release_epic(epic_id: str) -> None:
    """Release epic lock (on completion or failure)"""
    roadmap = load_roadmap()
    
    for epic in roadmap:
        if epic['epic_number'] == epic_id:
            epic.pop('assigned_to', None)
            epic.pop('lock_timestamp', None)
            save_roadmap(roadmap)
            print(f"[OK] Epic {epic_id} lock released")
            return
    
    raise ValueError(f"Epic {epic_id} not found in roadmap")

def list_assigned_epics() -> List[Dict]:
    """List all currently assigned epics"""
    roadmap = load_roadmap()
    return [e for e in roadmap if e.get('assigned_to')]

def list_pending_epics(limit: int = 10) -> List[Dict]:
    """List pending epics (not complete, not assigned)"""
    roadmap = load_roadmap()
    pending = [e for e in roadmap if e.get('status') != 'complete' and not e.get('assigned_to')]
    return pending[:limit]

def main():
    """CLI interface"""
    if len(sys.argv) < 2:
        print("Usage:")
        print("  python validate_epic.py <epic_id>           # Validate epic exists")
        print("  python validate_epic.py --next              # Get next pending epic")
        print("  python validate_epic.py --claim <epic_id> <worker_id>  # Claim epic")
        print("  python validate_epic.py --release <epic_id> # Release epic lock")
        print("  python validate_epic.py --assigned          # List assigned epics")
        print("  python validate_epic.py --pending [limit]   # List pending epics")
        sys.exit(1)
    
    command = sys.argv[1]
    
    try:
        if command == '--next':
            epic = get_next_epic()
            if epic:
                print(f"Next epic: {epic['epic_number']}")
                print(f"  Method: {epic['method']}")
                print(f"  File: {epic['file']}")
                print(f"  CYC: {epic['cyclomatic']}")
            else:
                print("No pending epics found")
                sys.exit(1)
        
        elif command == '--claim':
            if len(sys.argv) < 4:
                print("Usage: python validate_epic.py --claim <epic_id> <worker_id>")
                sys.exit(1)
            epic_id = sys.argv[2]
            worker_id = sys.argv[3]
            epic = claim_epic(epic_id, worker_id)
            print(f"  Method: {epic['method']}")
            print(f"  File: {epic['file']}")
            print(f"  CYC: {epic['cyclomatic']}")
        
        elif command == '--release':
            if len(sys.argv) < 3:
                print("Usage: python validate_epic.py --release <epic_id>")
                sys.exit(1)
            epic_id = sys.argv[2]
            release_epic(epic_id)
        
        elif command == '--assigned':
            assigned = list_assigned_epics()
            if assigned:
                print(f"Assigned epics ({len(assigned)}):")
                for epic in assigned:
                    print(f"  {epic['epic_number']}: {epic['method']} -> {epic['assigned_to']}")
            else:
                print("No assigned epics")
        
        elif command == '--pending':
            limit = int(sys.argv[2]) if len(sys.argv) > 2 else 10
            pending = list_pending_epics(limit)
            if pending:
                print(f"Pending epics (showing {len(pending)}):")
                for epic in pending:
                    print(f"  {epic['epic_number']}: {epic['method']} (CYC {epic['cyclomatic']})")
            else:
                print("No pending epics")
        
        else:
            # Validate epic exists
            epic_id = command
            if validate_epic_exists(epic_id):
                epic = get_epic_details(epic_id)
                print(f"[OK] {epic_id} exists in roadmap")
                print(f"  Method: {epic['method']}")
                print(f"  File: {epic['file']}")
                print(f"  CYC: {epic['cyclomatic']}")
                print(f"  Status: {epic.get('status', 'pending')}")
                if epic.get('assigned_to'):
                    print(f"  Assigned to: {epic['assigned_to']}")
            else:
                print(f"[ERROR] {epic_id} NOT FOUND in roadmap")
                print("\nDid you mean one of these?")
                # Show similar epic numbers
                roadmap = load_roadmap()
                epic_nums = [e['epic_number'] for e in roadmap[:10]]
                for num in epic_nums:
                    print(f"  {num}")
                sys.exit(1)
    
    except Exception as e:
        print(f"[ERROR] {e}")
        sys.exit(1)

if __name__ == '__main__':
    main()

# Made with Bob
