#!/usr/bin/env python3
"""
Verify jCodemunch Index Freshness

Prevents EPIC-CCN-1 failure mode where 19-day-old stale index caused
tickets 02-07 to target obsolete code.

Usage:
    python scripts/verify_index_freshness.py [--max-age-days 7]

Returns:
    Exit 0: Index is fresh
    Exit 1: Index is stale (needs refresh)
"""

import json
import os
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Dict, List
import subprocess


def get_git_head_timestamp() -> datetime:
    """Get timestamp of current git HEAD commit."""
    result = subprocess.run(
        ["git", "log", "-1", "--format=%cI"],
        capture_output=True,
        text=True,
        check=True
    )
    return datetime.fromisoformat(result.stdout.strip())


def get_graphify_timestamp() -> datetime:
    """Get timestamp of graphify-out/graph.json."""
    graph_path = Path("graphify-out/graph.json")
    if not graph_path.exists():
        raise FileNotFoundError("graphify-out/graph.json not found - index never created")
    
    stat = graph_path.stat()
    return datetime.fromtimestamp(stat.st_mtime, tz=timezone.utc)


def get_modified_files_since(since_timestamp: datetime) -> List[str]:
    """Get list of files modified since given timestamp."""
    since_str = since_timestamp.strftime("%Y-%m-%d %H:%M:%S")
    result = subprocess.run(
        ["git", "log", f"--since={since_str}", "--name-only", "--pretty=format:", "--", "src/"],
        capture_output=True,
        text=True,
        check=True
    )
    
    files = [f for f in result.stdout.split("\n") if f.strip()]
    return sorted(set(files))


def verify_index_freshness(max_age_days: int = 7) -> Dict:
    """
    Verify jCodemunch index is fresh.
    
    Args:
        max_age_days: Maximum acceptable age in days
        
    Returns:
        {
            "fresh": bool,
            "index_age_days": float,
            "last_indexed": str (ISO timestamp),
            "git_head": str (ISO timestamp),
            "stale_files": list[str],
            "action_required": str
        }
    """
    try:
        # Get timestamps
        git_head = get_git_head_timestamp()
        index_time = get_graphify_timestamp()
        
        # Calculate age
        age_delta = git_head - index_time
        age_days = age_delta.total_seconds() / 86400
        
        # Get stale files
        stale_files = get_modified_files_since(index_time)
        
        # Determine freshness
        fresh = age_days <= max_age_days and len(stale_files) == 0
        
        result = {
            "fresh": fresh,
            "index_age_days": round(age_days, 2),
            "last_indexed": index_time.isoformat(),
            "git_head": git_head.isoformat(),
            "stale_files": stale_files,
            "stale_file_count": len(stale_files),
            "action_required": "ok" if fresh else "reindex"
        }
        
        return result
        
    except FileNotFoundError as e:
        return {
            "fresh": False,
            "error": str(e),
            "action_required": "initial_index"
        }
    except subprocess.CalledProcessError as e:
        return {
            "fresh": False,
            "error": f"Git command failed: {e}",
            "action_required": "check_git"
        }


def main():
    import argparse
    
    parser = argparse.ArgumentParser(description="Verify jCodemunch index freshness")
    parser.add_argument(
        "--max-age-days",
        type=int,
        default=7,
        help="Maximum acceptable age in days (default: 7)"
    )
    parser.add_argument(
        "--json",
        action="store_true",
        help="Output JSON format"
    )
    
    args = parser.parse_args()
    
    result = verify_index_freshness(args.max_age_days)
    
    if args.json:
        print(json.dumps(result, indent=2))
    else:
        # Human-readable output
        if result.get("error"):
            print(f"❌ ERROR: {result['error']}")
            print(f"Action: {result['action_required']}")
            sys.exit(1)
        
        if result["fresh"]:
            print(f"✅ INDEX FRESH")
            print(f"   Age: {result['index_age_days']} days")
            print(f"   Last indexed: {result['last_indexed']}")
            print(f"   Git HEAD: {result['git_head']}")
            sys.exit(0)
        else:
            print(f"❌ INDEX STALE")
            print(f"   Age: {result['index_age_days']} days (max: {args.max_age_days})")
            print(f"   Last indexed: {result['last_indexed']}")
            print(f"   Git HEAD: {result['git_head']}")
            print(f"   Stale files: {result['stale_file_count']}")
            if result['stale_files']:
                print(f"\n   Modified since index:")
                for f in result['stale_files'][:10]:  # Show first 10
                    print(f"     - {f}")
                if len(result['stale_files']) > 10:
                    print(f"     ... and {len(result['stale_files']) - 10} more")
            print(f"\n   Action: {result['action_required']}")
            sys.exit(1)


if __name__ == "__main__":
    main()

# Made with Bob
