#!/usr/bin/env python3
"""
Session Continuity Manager for V12 Universal OR Strategy

Extends session_snapshot.py with:
- Auto-snapshot: Automatically save snapshots after N tokens consumed
- Restore: Load and resume from last checkpoint
- Merge: Combine multiple snapshots for long-running tasks
- Prune: Remove old snapshots (keep last N)

Part of Jane Street Cyborg Transformation (Phase 4).
"""

import argparse
import json
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Any, Dict, List, Optional

# Import the base SessionSnapshot class
from session_snapshot import SessionSnapshot, get_session_path


class SessionContinuity:
    """Manages session continuity with auto-snapshots and restoration."""
    
    def __init__(self, session_id: str):
        self.session_id = session_id
        self.base_path = get_session_path(session_id)
        self.snapshot_dir = self.base_path.parent / f"snapshots_{session_id}"
        self.snapshot_dir.mkdir(parents=True, exist_ok=True)
    
    def auto_snapshot(self, tokens_consumed: int, checkpoint_interval: int = 50000) -> bool:
        """
        Automatically create snapshot if token threshold crossed.
        
        Args:
            tokens_consumed: Total tokens consumed in session
            checkpoint_interval: Tokens between snapshots (default 50k)
        
        Returns:
            True if snapshot created, False otherwise
        """
        # Load current session
        snapshot = SessionSnapshot(self.base_path)
        if not snapshot.load():
            print(f"Error: Session not found: {self.session_id}", file=sys.stderr)
            return False
        
        # Calculate checkpoint number
        checkpoint_num = tokens_consumed // checkpoint_interval
        
        # Check if we've crossed a checkpoint boundary
        last_checkpoint = snapshot.data.get("last_checkpoint", 0)
        if checkpoint_num <= last_checkpoint:
            # No new checkpoint needed
            return False
        
        # Create checkpoint snapshot
        checkpoint_path = self.snapshot_dir / f"checkpoint_{checkpoint_num:03d}.json"
        
        checkpoint_data = {
            "session_id": self.session_id,
            "checkpoint": checkpoint_num,
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "tokens_consumed": tokens_consumed,
            "tokens_at_checkpoint": checkpoint_num * checkpoint_interval,
            "files_read": snapshot.data.get("files_read", []),
            "symbols_explored": snapshot.data.get("symbols_explored", []),
            "searches_performed": snapshot.data.get("searches_performed", []),
            "context_summary": self._generate_context_summary(snapshot.data)
        }
        
        try:
            with open(checkpoint_path, 'w', encoding='utf-8') as f:
                json.dump(checkpoint_data, f, indent=2, ensure_ascii=True)
            
            # Update session with last checkpoint
            snapshot.data["last_checkpoint"] = checkpoint_num
            snapshot.save()
            
            print(f"Auto-snapshot created: checkpoint_{checkpoint_num:03d}.json")
            print(f"  Tokens: {tokens_consumed:,} ({checkpoint_num * checkpoint_interval:,} at checkpoint)")
            print(f"  Files read: {len(checkpoint_data['files_read'])}")
            print(f"  Symbols explored: {len(checkpoint_data['symbols_explored'])}")
            
            return True
        except IOError as e:
            print(f"Error creating checkpoint: {e}", file=sys.stderr)
            return False
    
    def _generate_context_summary(self, session_data: Dict[str, Any]) -> str:
        """Generate a human-readable context summary for the snapshot."""
        files_read = session_data.get("files_read", [])
        symbols = session_data.get("symbols_explored", [])
        searches = session_data.get("searches_performed", [])
        
        # Get most recent files (last 5)
        recent_files = [f["path"] for f in files_read[-5:]] if files_read else []
        
        # Get most recent symbols (last 5)
        recent_symbols = [s["symbol_name"] for s in symbols[-5:]] if symbols else []
        
        # Get most recent searches (last 3)
        recent_searches = [s["query"] for s in searches[-3:]] if searches else []
        
        summary_parts = []
        
        if recent_files:
            summary_parts.append(f"Recent files: {', '.join(recent_files)}")
        
        if recent_symbols:
            summary_parts.append(f"Recent symbols: {', '.join(recent_symbols)}")
        
        if recent_searches:
            summary_parts.append(f"Recent searches: {', '.join(recent_searches)}")
        
        return " | ".join(summary_parts) if summary_parts else "No activity yet"
    
    def restore(self, checkpoint_num: Optional[int] = None) -> Optional[Dict[str, Any]]:
        """
        Restore session from a checkpoint.
        
        Args:
            checkpoint_num: Specific checkpoint to restore (None = latest)
        
        Returns:
            Checkpoint data if found, None otherwise
        """
        if checkpoint_num is not None:
            # Restore specific checkpoint
            checkpoint_path = self.snapshot_dir / f"checkpoint_{checkpoint_num:03d}.json"
        else:
            # Find latest checkpoint
            checkpoints = sorted(self.snapshot_dir.glob("checkpoint_*.json"))
            if not checkpoints:
                print(f"No checkpoints found for session: {self.session_id}", file=sys.stderr)
                return None
            checkpoint_path = checkpoints[-1]
        
        if not checkpoint_path.exists():
            print(f"Checkpoint not found: {checkpoint_path}", file=sys.stderr)
            return None
        
        try:
            with open(checkpoint_path, 'r', encoding='utf-8') as f:
                checkpoint_data = json.load(f)
            
            print(f"Restored checkpoint: {checkpoint_path.name}")
            print(f"  Timestamp: {checkpoint_data['timestamp']}")
            print(f"  Tokens consumed: {checkpoint_data['tokens_consumed']:,}")
            print(f"  Files read: {len(checkpoint_data['files_read'])}")
            print(f"  Symbols explored: {len(checkpoint_data['symbols_explored'])}")
            print(f"\nContext summary:")
            print(f"  {checkpoint_data['context_summary']}")
            
            return checkpoint_data
        except (json.JSONDecodeError, IOError) as e:
            print(f"Error loading checkpoint: {e}", file=sys.stderr)
            return None
    
    def list_checkpoints(self) -> List[Dict[str, Any]]:
        """List all checkpoints for this session."""
        checkpoints = []
        
        for checkpoint_path in sorted(self.snapshot_dir.glob("checkpoint_*.json")):
            try:
                with open(checkpoint_path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                checkpoints.append({
                    "checkpoint": data["checkpoint"],
                    "timestamp": data["timestamp"],
                    "tokens": data["tokens_consumed"],
                    "files": len(data["files_read"]),
                    "symbols": len(data["symbols_explored"]),
                    "path": str(checkpoint_path)
                })
            except (json.JSONDecodeError, IOError):
                continue
        
        return checkpoints
    
    def merge(self, checkpoint_nums: List[int]) -> Optional[Dict[str, Any]]:
        """
        Merge multiple checkpoints into a single consolidated snapshot.
        
        Args:
            checkpoint_nums: List of checkpoint numbers to merge
        
        Returns:
            Merged checkpoint data if successful, None otherwise
        """
        if not checkpoint_nums:
            print("Error: No checkpoints specified for merge", file=sys.stderr)
            return None
        
        merged_data = {
            "session_id": self.session_id,
            "merged_checkpoints": checkpoint_nums,
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "files_read": [],
            "symbols_explored": [],
            "searches_performed": [],
            "tokens_consumed": 0
        }
        
        # Merge checkpoints in order
        for checkpoint_num in sorted(checkpoint_nums):
            checkpoint_path = self.snapshot_dir / f"checkpoint_{checkpoint_num:03d}.json"
            
            if not checkpoint_path.exists():
                print(f"Warning: Checkpoint {checkpoint_num} not found, skipping", file=sys.stderr)
                continue
            
            try:
                with open(checkpoint_path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                
                # Merge data (deduplicate files/symbols by path/id)
                existing_files = {f["path"] for f in merged_data["files_read"]}
                for file_entry in data.get("files_read", []):
                    if file_entry["path"] not in existing_files:
                        merged_data["files_read"].append(file_entry)
                        existing_files.add(file_entry["path"])
                
                existing_symbols = {s["symbol_id"] for s in merged_data["symbols_explored"]}
                for symbol_entry in data.get("symbols_explored", []):
                    if symbol_entry["symbol_id"] not in existing_symbols:
                        merged_data["symbols_explored"].append(symbol_entry)
                        existing_symbols.add(symbol_entry["symbol_id"])
                
                # Append all searches (no deduplication)
                merged_data["searches_performed"].extend(data.get("searches_performed", []))
                
                # Track highest token count
                merged_data["tokens_consumed"] = max(
                    merged_data["tokens_consumed"],
                    data.get("tokens_consumed", 0)
                )
            except (json.JSONDecodeError, IOError) as e:
                print(f"Error loading checkpoint {checkpoint_num}: {e}", file=sys.stderr)
                continue
        
        # Save merged snapshot
        merged_path = self.snapshot_dir / f"merged_{'-'.join(map(str, checkpoint_nums))}.json"
        try:
            with open(merged_path, 'w', encoding='utf-8') as f:
                json.dump(merged_data, f, indent=2, ensure_ascii=True)
            
            print(f"Merged checkpoint created: {merged_path.name}")
            print(f"  Checkpoints merged: {len(checkpoint_nums)}")
            print(f"  Total files: {len(merged_data['files_read'])}")
            print(f"  Total symbols: {len(merged_data['symbols_explored'])}")
            print(f"  Total searches: {len(merged_data['searches_performed'])}")
            
            return merged_data
        except IOError as e:
            print(f"Error saving merged checkpoint: {e}", file=sys.stderr)
            return None
    
    def prune(self, keep_last: int = 5) -> int:
        """
        Remove old checkpoints, keeping only the last N.
        
        Args:
            keep_last: Number of recent checkpoints to keep (default 5)
        
        Returns:
            Number of checkpoints deleted
        """
        checkpoints = sorted(self.snapshot_dir.glob("checkpoint_*.json"))
        
        if len(checkpoints) <= keep_last:
            print(f"No pruning needed: {len(checkpoints)} checkpoints (keeping {keep_last})")
            return 0
        
        to_delete = checkpoints[:-keep_last]
        deleted_count = 0
        
        for checkpoint_path in to_delete:
            try:
                checkpoint_path.unlink()
                deleted_count += 1
                print(f"Deleted: {checkpoint_path.name}")
            except OSError as e:
                print(f"Error deleting {checkpoint_path.name}: {e}", file=sys.stderr)
        
        print(f"Pruned {deleted_count} old checkpoints (kept last {keep_last})")
        return deleted_count


def main():
    """Main CLI entry point."""
    parser = argparse.ArgumentParser(
        description="Manage session continuity with auto-snapshots"
    )
    
    subparsers = parser.add_subparsers(dest="command", help="Command to execute")
    
    # auto-snapshot command
    auto_parser = subparsers.add_parser("auto-snapshot", 
                                       help="Create checkpoint if threshold crossed")
    auto_parser.add_argument("session_id", help="Session identifier")
    auto_parser.add_argument("tokens", type=int, help="Total tokens consumed")
    auto_parser.add_argument("--interval", type=int, default=50000,
                           help="Checkpoint interval in tokens (default 50000)")
    
    # restore command
    restore_parser = subparsers.add_parser("restore", 
                                          help="Restore from checkpoint")
    restore_parser.add_argument("session_id", help="Session identifier")
    restore_parser.add_argument("--checkpoint", type=int,
                              help="Specific checkpoint number (default: latest)")
    
    # list command
    list_parser = subparsers.add_parser("list", 
                                       help="List all checkpoints")
    list_parser.add_argument("session_id", help="Session identifier")
    
    # merge command
    merge_parser = subparsers.add_parser("merge", 
                                        help="Merge multiple checkpoints")
    merge_parser.add_argument("session_id", help="Session identifier")
    merge_parser.add_argument("checkpoints", type=int, nargs="+",
                            help="Checkpoint numbers to merge")
    
    # prune command
    prune_parser = subparsers.add_parser("prune", 
                                        help="Remove old checkpoints")
    prune_parser.add_argument("session_id", help="Session identifier")
    prune_parser.add_argument("--keep", type=int, default=5,
                            help="Number of recent checkpoints to keep (default 5)")
    
    args = parser.parse_args()
    
    if not args.command:
        parser.print_help()
        return 2
    
    continuity = SessionContinuity(args.session_id)
    
    try:
        if args.command == "auto-snapshot":
            if continuity.auto_snapshot(args.tokens, args.interval):
                return 0
            return 1
        
        elif args.command == "restore":
            checkpoint_data = continuity.restore(args.checkpoint)
            if checkpoint_data:
                return 0
            return 1
        
        elif args.command == "list":
            checkpoints = continuity.list_checkpoints()
            if checkpoints:
                print(f"\nCheckpoints for session: {args.session_id}")
                print("-" * 80)
                for cp in checkpoints:
                    print(f"Checkpoint {cp['checkpoint']:3d}: {cp['timestamp']}")
                    print(f"  Tokens: {cp['tokens']:,} | Files: {cp['files']} | Symbols: {cp['symbols']}")
                print(f"\nTotal: {len(checkpoints)} checkpoints")
                return 0
            print(f"No checkpoints found for session: {args.session_id}")
            return 1
        
        elif args.command == "merge":
            merged_data = continuity.merge(args.checkpoints)
            if merged_data:
                return 0
            return 1
        
        elif args.command == "prune":
            deleted = continuity.prune(args.keep)
            return 0 if deleted >= 0 else 1
        
    except Exception as e:
        print(f"Error: {e}", file=sys.stderr)
        return 2
    
    return 2


if __name__ == "__main__":
    sys.exit(main())

# Made with Bob