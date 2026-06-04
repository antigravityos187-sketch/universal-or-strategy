#!/usr/bin/env python3
"""
Session Snapshot Manager for V12 Universal OR Strategy

Tracks what files/symbols agents have read in a session to prevent redundant
reads and enable budget-aware exploration.

Part of Jane Street Cyborg Transformation (Phase 1.2).
"""

import argparse
import json
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Any, Dict, List, Optional


class SessionSnapshot:
    """Manages session snapshot state for budget-aware exploration."""

    def __init__(self, snapshot_path: Path):
        self.snapshot_path = snapshot_path
        self.data: Dict[str, Any] = {}

    def load(self) -> bool:
        """Load session snapshot from disk. Returns True if loaded successfully."""
        if not self.snapshot_path.exists():
            return False
        
        try:
            with open(self.snapshot_path, 'r', encoding='utf-8') as f:
                self.data = json.load(f)
            return True
        except (json.JSONDecodeError, IOError) as e:
            print(f"Error loading session: {e}", file=sys.stderr)
            return False

    def save(self) -> bool:
        """Save session snapshot to disk. Returns True if saved successfully."""
        try:
            self.snapshot_path.parent.mkdir(parents=True, exist_ok=True)
            with open(self.snapshot_path, 'w', encoding='utf-8') as f:
                json.dump(self.data, f, indent=2, ensure_ascii=True)
            return True
        except IOError as e:
            print(f"Error saving session: {e}", file=sys.stderr)
            return False

    def init(self, session_id: str, agent: str, task: str) -> bool:
        """Initialize a new session snapshot."""
        now = datetime.now(timezone.utc).isoformat()
        
        self.data = {
            "version": "1.0",
            "session_id": session_id,
            "started_at": now,
            "last_updated": now,
            "agent": agent,
            "task_description": task,
            "files_read": [],
            "symbols_explored": [],
            "searches_performed": [],
            "token_budget": {
                "initial": 200000,
                "consumed": 0,
                "remaining": 200000,
                "budget_warnings": []
            }
        }
        
        return self.save()

    def record_read(self, file_path: str, read_type: str, 
                   symbols: Optional[List[str]] = None,
                   line_range: str = "") -> bool:
        """Record a file read operation."""
        if not self.load():
            print("Error: Session not found. Use 'init' first.", file=sys.stderr)
            return False
        
        now = datetime.now(timezone.utc).isoformat()
        
        read_entry = {
            "path": file_path,
            "read_at": now,
            "read_type": read_type,
            "symbols_accessed": symbols or [],
            "line_range": line_range
        }
        
        self.data["files_read"].append(read_entry)
        self.data["last_updated"] = now
        
        return self.save()

    def record_symbol(self, symbol_id: str, symbol_name: str, file_path: str) -> bool:
        """Record a symbol exploration."""
        if not self.load():
            print("Error: Session not found. Use 'init' first.", file=sys.stderr)
            return False
        
        now = datetime.now(timezone.utc).isoformat()
        
        symbol_entry = {
            "symbol_id": symbol_id,
            "symbol_name": symbol_name,
            "file": file_path,
            "explored_at": now
        }
        
        self.data["symbols_explored"].append(symbol_entry)
        self.data["last_updated"] = now
        
        return self.save()

    def record_search(self, query: str, search_type: str, result_count: int) -> bool:
        """Record a search operation."""
        if not self.load():
            print("Error: Session not found. Use 'init' first.", file=sys.stderr)
            return False
        
        now = datetime.now(timezone.utc).isoformat()
        
        search_entry = {
            "query": query,
            "search_type": search_type,
            "result_count": result_count,
            "searched_at": now
        }
        
        self.data["searches_performed"].append(search_entry)
        self.data["last_updated"] = now
        
        return self.save()

    def update_budget(self, consumed: int) -> bool:
        """Update token budget consumption."""
        if not self.load():
            print("Error: Session not found. Use 'init' first.", file=sys.stderr)
            return False
        
        now = datetime.now(timezone.utc).isoformat()
        
        budget = self.data["token_budget"]
        budget["consumed"] = consumed
        budget["remaining"] = budget["initial"] - consumed
        
        # Add budget warning if consumption exceeds 80%
        if consumed > budget["initial"] * 0.8:
            warning = f"Budget warning at {now}: {consumed}/{budget['initial']} tokens consumed"
            if warning not in budget["budget_warnings"]:
                budget["budget_warnings"].append(warning)
        
        self.data["last_updated"] = now
        
        return self.save()

    def check_read(self, file_path: str) -> bool:
        """Check if a file has already been read. Returns True if read."""
        if not self.load():
            return False
        
        for read_entry in self.data.get("files_read", []):
            if read_entry["path"] == file_path:
                return True
        
        return False

    def get_state(self) -> Optional[Dict[str, Any]]:
        """Get current session state."""
        if not self.load():
            return None
        return self.data


def get_session_path(session_id: str) -> Path:
    """Get the path to a session snapshot file."""
    repo_root = Path(__file__).parent.parent
    return repo_root / "docs" / "brain" / f"session_snapshot_{session_id}.json"


def main():
    """Main CLI entry point."""
    parser = argparse.ArgumentParser(
        description="Manage session snapshots for budget-aware exploration"
    )
    
    subparsers = parser.add_subparsers(dest="command", help="Command to execute")
    
    # init command
    init_parser = subparsers.add_parser("init", help="Initialize a new session")
    init_parser.add_argument("session_id", help="Unique session identifier")
    init_parser.add_argument("agent", help="Agent name (e.g., 'Bob CLI')")
    init_parser.add_argument("task", help="Task description")
    
    # record-read command
    read_parser = subparsers.add_parser("record-read", help="Record a file read")
    read_parser.add_argument("session_id", help="Session identifier")
    read_parser.add_argument("file", help="File path")
    read_parser.add_argument("type", help="Read type (full|outline|symbol|context_bundle)")
    read_parser.add_argument("--symbols", nargs="*", help="Symbols accessed")
    read_parser.add_argument("--line-range", default="", help="Line range (start-end)")
    
    # record-symbol command
    symbol_parser = subparsers.add_parser("record-symbol", help="Record symbol access")
    symbol_parser.add_argument("session_id", help="Session identifier")
    symbol_parser.add_argument("symbol_id", help="jcodemunch symbol ID")
    symbol_parser.add_argument("name", help="Symbol name")
    symbol_parser.add_argument("file", help="File path")
    
    # record-search command
    search_parser = subparsers.add_parser("record-search", help="Record a search")
    search_parser.add_argument("session_id", help="Session identifier")
    search_parser.add_argument("query", help="Search query")
    search_parser.add_argument("type", help="Search type (search_symbols|search_text|find_references)")
    search_parser.add_argument("count", type=int, help="Result count")
    
    # update-budget command
    budget_parser = subparsers.add_parser("update-budget", help="Update token budget")
    budget_parser.add_argument("session_id", help="Session identifier")
    budget_parser.add_argument("consumed", type=int, help="Tokens consumed")
    
    # get command
    get_parser = subparsers.add_parser("get", help="Get session state")
    get_parser.add_argument("session_id", help="Session identifier")
    get_parser.add_argument("--json", action="store_true", help="Output as JSON")
    
    # check-read command
    check_parser = subparsers.add_parser("check-read", help="Check if file was read")
    check_parser.add_argument("session_id", help="Session identifier")
    check_parser.add_argument("file", help="File path to check")
    
    args = parser.parse_args()
    
    if not args.command:
        parser.print_help()
        return 2
    
    session_path = get_session_path(args.session_id)
    snapshot = SessionSnapshot(session_path)
    
    try:
        if args.command == "init":
            if snapshot.init(args.session_id, args.agent, args.task):
                print(f"Session initialized: {args.session_id}")
                return 0
            return 2
        
        elif args.command == "record-read":
            if snapshot.record_read(args.file, args.type, args.symbols, args.line_range):
                print(f"Recorded read: {args.file}")
                return 0
            return 2
        
        elif args.command == "record-symbol":
            if snapshot.record_symbol(args.symbol_id, args.name, args.file):
                print(f"Recorded symbol: {args.name}")
                return 0
            return 2
        
        elif args.command == "record-search":
            if snapshot.record_search(args.query, args.type, args.count):
                print(f"Recorded search: {args.query} ({args.count} results)")
                return 0
            return 2
        
        elif args.command == "update-budget":
            if snapshot.update_budget(args.consumed):
                remaining = 200000 - args.consumed
                print(f"Budget updated: {args.consumed} consumed, {remaining} remaining")
                return 0
            return 2
        
        elif args.command == "get":
            state = snapshot.get_state()
            if state:
                if args.json:
                    print(json.dumps(state, indent=2))
                else:
                    print(f"Session: {state['session_id']}")
                    print(f"Agent: {state['agent']}")
                    print(f"Task: {state['task_description']}")
                    print(f"Files read: {len(state['files_read'])}")
                    print(f"Symbols explored: {len(state['symbols_explored'])}")
                    print(f"Searches: {len(state['searches_performed'])}")
                    budget = state['token_budget']
                    print(f"Budget: {budget['consumed']}/{budget['initial']} tokens")
                return 0
            print(f"Error: Session not found: {args.session_id}", file=sys.stderr)
            return 1
        
        elif args.command == "check-read":
            if snapshot.check_read(args.file):
                print(f"File already read: {args.file}")
                return 0
            print(f"File not yet read: {args.file}")
            return 1
        
    except Exception as e:
        print(f"Error: {e}", file=sys.stderr)
        return 2
    
    return 2


if __name__ == "__main__":
    sys.exit(main())

# Made with Bob
