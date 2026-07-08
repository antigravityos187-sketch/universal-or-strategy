#!/usr/bin/env python3
"""
API Balance Tracker - Automated bobcoin usage tracking
Maintains local state file to track API usage without manual dashboard checks
"""

import json
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Tuple

# Paths
REPO_ROOT = Path(__file__).parent.parent.parent
API_DIR = REPO_ROOT / "docs" / "API"
TRACKER_FILE = REPO_ROOT / "docs" / "workflow" / "api_balance_state.json"

# Constants
INITIAL_BALANCE = 160  # Fresh API balance
MIN_SAFE_BALANCE = 10  # Minimum to keep positive


def load_tracker_state() -> Dict:
    """Load current tracker state, initialize if doesn't exist"""
    if TRACKER_FILE.exists():
        return json.loads(TRACKER_FILE.read_text())
    
    # Initialize with all APIs at 160 bobcoins
    api_files = sorted(API_DIR.glob("*.json"))
    state = {
        "last_updated": datetime.utcnow().isoformat(),
        "apis": {}
    }
    
    for api_file in api_files:
        api_data = json.loads(api_file.read_text())
        state["apis"][api_file.name] = {
            "name": api_data.get("name", api_file.stem),
            "balance": INITIAL_BALANCE,
            "used": 0,
            "history": []
        }
    
    return state


def save_tracker_state(state: Dict):
    """Save tracker state to file"""
    state["last_updated"] = datetime.utcnow().isoformat()
    TRACKER_FILE.write_text(json.dumps(state, indent=2))
    print(f"Tracker state saved to {TRACKER_FILE}")


def record_usage(api_file: str, epic_id: str, bobcoins_used: float, phase: str = "unknown"):
    """Record bobcoin usage for an API"""
    state = load_tracker_state()
    
    if api_file not in state["apis"]:
        print(f"ERROR: API {api_file} not found in tracker")
        return
    
    api = state["apis"][api_file]
    api["balance"] -= bobcoins_used
    api["used"] += bobcoins_used
    api["history"].append({
        "timestamp": datetime.utcnow().isoformat(),
        "epic_id": epic_id,
        "phase": phase,
        "bobcoins": bobcoins_used,
        "balance_after": api["balance"]
    })
    
    save_tracker_state(state)
    
    # Warnings
    if api["balance"] < MIN_SAFE_BALANCE:
        print(f"WARNING: {api_file} balance LOW: {api['balance']:.2f} bobcoins")
    elif api["balance"] < 0:
        print(f"CRITICAL: {api_file} NEGATIVE: {api['balance']:.2f} bobcoins")
    else:
        print(f"OK: {api_file}: {bobcoins_used:.2f} used, {api['balance']:.2f} remaining")


def print_summary():
    """Print summary of all API balances"""
    state = load_tracker_state()
    
    print("\n" + "="*70)
    print("API BALANCE SUMMARY")
    print("="*70)
    print(f"Last Updated: {state['last_updated']}")
    print()
    
    total_balance = 0
    total_used = 0
    low_balance_apis = []
    
    for api_file, data in sorted(state["apis"].items()):
        balance = data["balance"]
        used = data["used"]
        total_balance += balance
        total_used += used
        
        status = "OK"
        if balance < 0:
            status = "CRITICAL"
            low_balance_apis.append((api_file, balance))
        elif balance < MIN_SAFE_BALANCE:
            status = "WARNING"
            low_balance_apis.append((api_file, balance))
        
        print(f"{status:8s} {api_file:40s} | Balance: {balance:6.2f} | Used: {used:6.2f}")
    
    print()
    print(f"TOTAL: {total_balance:.2f} bobcoins remaining ({total_used:.2f} used)")
    print("="*70)
    
    if low_balance_apis:
        print("\nLOW BALANCE ALERTS:")
        for api_file, balance in low_balance_apis:
            print(f"   - {api_file}: {balance:.2f} bobcoins")
    
    print()


def estimate_phase_budget(phase: str, num_epics: int = 9) -> float:
    """Estimate bobcoin budget for a phase"""
    estimates = {
        "0": 3,    # Hotspot analysis (based on actual v4 usage)
        "1": 3,    # Scope definition
        "1.5": 2,  # Boundary validation
        "2": 5,    # Architecture planning
        "3": 5,    # DNA audit
        "4": 5,    # Ticket generation
        "5": 35,   # Implementation (most expensive)
        "6": 10,   # Final review
    }
    
    per_epic = estimates.get(phase, 10)
    return per_epic * num_epics


def check_phase_feasibility(phase: str, num_epics: int = 9) -> bool:
    """Check if we have enough bobcoins for a phase"""
    state = load_tracker_state()
    total_balance = sum(data["balance"] for data in state["apis"].values())
    
    estimated_cost = estimate_phase_budget(phase, num_epics)
    safety_margin = total_balance * 0.1  # 10% safety
    
    print(f"\nPhase {phase} Feasibility Check:")
    print(f"  Estimated Cost: {estimated_cost:.2f} bobcoins")
    print(f"  Available: {total_balance:.2f} bobcoins")
    print(f"  Safety Margin: {safety_margin:.2f} bobcoins (10%)")
    print(f"  After Phase: {total_balance - estimated_cost:.2f} bobcoins")
    
    if total_balance < estimated_cost:
        print(f"  INSUFFICIENT BUDGET")
        return False
    elif total_balance - estimated_cost < safety_margin:
        print(f"  WARNING: Low safety margin")
        return True
    else:
        print(f"  SUFFICIENT BUDGET")
        return True


if __name__ == "__main__":
    import sys
    
    if len(sys.argv) < 2:
        print("Usage:")
        print("  python api_balance_tracker.py summary")
        print("  python api_balance_tracker.py record <api_file> <epic_id> <bobcoins> [phase]")
        print("  python api_balance_tracker.py check <phase> [num_epics]")
        sys.exit(1)
    
    command = sys.argv[1]
    
    if command == "summary":
        print_summary()
    
    elif command == "record":
        if len(sys.argv) < 5:
            print("Usage: record <api_file> <epic_id> <bobcoins> [phase]")
            sys.exit(1)
        api_file = sys.argv[2]
        epic_id = sys.argv[3]
        bobcoins = float(sys.argv[4])
        phase = sys.argv[5] if len(sys.argv) > 5 else "unknown"
        record_usage(api_file, epic_id, bobcoins, phase)
    
    elif command == "check":
        if len(sys.argv) < 3:
            print("Usage: check <phase> [num_epics]")
            sys.exit(1)
        phase = sys.argv[2]
        num_epics = int(sys.argv[3]) if len(sys.argv) > 3 else 9
        feasible = check_phase_feasibility(phase, num_epics)
        sys.exit(0 if feasible else 1)
    
    else:
        print(f"Unknown command: {command}")
        sys.exit(1)

# Made with Bob
