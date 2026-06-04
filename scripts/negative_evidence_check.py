#!/usr/bin/env python3
"""
Negative Evidence Check - Query negative_evidence.json for known non-implementations

Usage:
    python scripts/negative_evidence_check.py "feature name"
    mise run negative-evidence-check "feature name"

Exit Codes:
    0 - Found negative evidence (feature confirmed as not implemented)
    1 - No match found (feature may exist or hasn't been searched)
    2 - Error (file missing, invalid JSON, etc.)
"""

import argparse
import json
import sys
from datetime import datetime, timezone
from pathlib import Path


def parse_iso_timestamp(timestamp_str):
    """Parse ISO-8601 timestamp string to datetime object."""
    try:
        return datetime.fromisoformat(timestamp_str.replace('Z', '+00:00'))
    except (ValueError, AttributeError):
        return None


def check_freshness(searched_at_str, max_age_days=7):
    """Check if evidence is fresh (within max_age_days)."""
    searched_at = parse_iso_timestamp(searched_at_str)
    if not searched_at:
        return False, "Invalid timestamp"
    
    now = datetime.now(timezone.utc)
    age_days = (now - searched_at).days
    
    if age_days > max_age_days:
        return False, f"Evidence is {age_days} days old (threshold: {max_age_days} days)"
    
    return True, f"Evidence is {age_days} days old (fresh)"


def search_negative_evidence(query, evidence_file):
    """Search for negative evidence matching the query."""
    try:
        with open(evidence_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
    except FileNotFoundError:
        return {
            "status": "error",
            "message": f"Negative evidence file not found: {evidence_file}",
            "exit_code": 2
        }
    except json.JSONDecodeError as e:
        return {
            "status": "error",
            "message": f"Invalid JSON in {evidence_file}: {e}",
            "exit_code": 2
        }
    
    query_lower = query.lower()
    matches = []
    
    for entry in data.get("entries", []):
        entry_query = entry.get("query", "").lower()
        if query_lower in entry_query or entry_query in query_lower:
            is_fresh, freshness_msg = check_freshness(entry.get("searched_at", ""))
            
            match_result = {
                "query": entry.get("query"),
                "verdict": entry.get("verdict"),
                "search_type": entry.get("search_type"),
                "searched_at": entry.get("searched_at"),
                "searched_by": entry.get("searched_by"),
                "related_files": entry.get("related_files", []),
                "notes": entry.get("notes", ""),
                "is_fresh": is_fresh,
                "freshness_message": freshness_msg
            }
            matches.append(match_result)
    
    if matches:
        return {
            "status": "found",
            "query": query,
            "matches": matches,
            "match_count": len(matches),
            "exit_code": 0
        }
    else:
        return {
            "status": "not_found",
            "query": query,
            "message": "No negative evidence found for this query. Feature may exist or hasn't been searched yet.",
            "exit_code": 1
        }


def main():
    parser = argparse.ArgumentParser(
        description="Check if a feature has negative evidence (confirmed non-implementation)",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
    python scripts/negative_evidence_check.py "CSRF"
    python scripts/negative_evidence_check.py "rate limiting"
    mise run negative-evidence-check "authentication"
        """
    )
    parser.add_argument(
        "query",
        help="Feature name or search term to check"
    )
    parser.add_argument(
        "--json",
        action="store_true",
        help="Output results as JSON"
    )
    parser.add_argument(
        "--file",
        default="docs/brain/negative_evidence.json",
        help="Path to negative evidence file (default: docs/brain/negative_evidence.json)"
    )
    
    args = parser.parse_args()
    
    # Resolve file path relative to project root
    project_root = Path(__file__).parent.parent
    evidence_file = project_root / args.file
    
    result = search_negative_evidence(args.query, evidence_file)
    
    if args.json:
        print(json.dumps(result, indent=2))
    else:
        # Human-readable output
        if result["status"] == "found":
            print(f"NEGATIVE EVIDENCE FOUND for '{args.query}'")
            print(f"Matches: {result['match_count']}")
            print()
            
            for i, match in enumerate(result["matches"], 1):
                print(f"Match {i}:")
                print(f"  Query: {match['query']}")
                print(f"  Verdict: {match['verdict']}")
                print(f"  Search Type: {match['search_type']}")
                print(f"  Searched By: {match['searched_by']}")
                print(f"  Searched At: {match['searched_at']}")
                print(f"  Freshness: {match['freshness_message']}")
                
                if not match['is_fresh']:
                    print(f"  WARNING: Evidence may be stale. Consider re-searching.")
                
                if match['related_files']:
                    print(f"  Related Files: {', '.join(match['related_files'])}")
                
                if match['notes']:
                    print(f"  Notes: {match['notes']}")
                print()
            
            print("CONCLUSION: This feature is confirmed as NOT IMPLEMENTED.")
            print("Do NOT waste time re-searching with different terms.")
            
        elif result["status"] == "not_found":
            print(f"NO NEGATIVE EVIDENCE for '{args.query}'")
            print(result["message"])
            print()
            print("This means:")
            print("  - Feature may exist (use jcodemunch search_symbols to find it)")
            print("  - Feature hasn't been searched yet (search and record result)")
            
        else:  # error
            print(f"ERROR: {result['message']}", file=sys.stderr)
    
    sys.exit(result["exit_code"])


if __name__ == "__main__":
    main()

# Made with Bob
