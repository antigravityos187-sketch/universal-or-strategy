#!/usr/bin/env python3
"""
API Balance Tracker for Wave 2 Autonomous Refactoring

Monitors bobcoin balances across all API keys and provides:
- Current balance status
- Epic-to-API assignment recommendations
- Low balance alerts
- Automatic tracker document updates
"""

import json
import re
import subprocess
from pathlib import Path
from typing import Dict, List, Tuple
from datetime import datetime

# Configuration
API_DIR = Path("docs/API")
TRACKER_FILE = Path("docs/workflow/API_BALANCE_TRACKER.md")
INITIAL_BALANCE = 160.0
LOW_BALANCE_THRESHOLD = 50.0
CRITICAL_BALANCE_THRESHOLD = 20.0

# Epic assignments (from roadmap)
EPIC_ASSIGNMENTS = {
    "EPIC-CCN-107": "b (2).json",
    "EPIC-CCN-108": "b.json",
    "EPIC-CCN-109": "bob (1).json",
    "EPIC-CCN-110": "bob (2).json",
    "EPIC-CCN-111": "bob (3).json",
    "EPIC-CCN-112": "bob (4).json",
    "EPIC-CCN-113": "bob (5).json",
    "EPIC-CCN-114": "bob (6).json",
    "EPIC-CCN-115": "bob.json",
}

RESERVE_API = "sean.carter.jr@atomicmail.io.json"


def load_api_keys() -> Dict[str, str]:
    """Load all API keys from docs/API/*.json"""
    api_keys = {}
    for json_file in API_DIR.glob("*.json"):
        with open(json_file, 'r') as f:
            data = json.load(f)
            api_keys[json_file.name] = data.get('apikey', '')
    return api_keys


def extract_costs_from_vm_logs(phase: str) -> Dict[str, Tuple[float, float]]:
    """
    Extract Cost and Balance from VM logs for a specific phase.
    
    Returns: Dict[epic_id, (cost, balance)]
    """
    cmd = [
        "gcloud", "compute", "ssh", "v12-test-golden-v2",
        "--zone=us-central1-a",
        "--command",
        f"grep -E 'Cost:.*Balance:|Cost: [0-9]' /home/malhitticrypto/universal-or-strategy/logs/{phase}/EPIC-CCN-*.log 2>/dev/null || true"
    ]
    
    try:
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=30)
        output = result.stdout
    except subprocess.TimeoutExpired:
        print(f"⚠️ Timeout extracting logs for {phase}")
        return {}
    except Exception as e:
        print(f"⚠️ Error extracting logs for {phase}: {e}")
        return {}
    
    costs = {}
    
    # Parse lines like:
    # logs/phase1/EPIC-CCN-107.log:Cost: 0.68 | Balance: 159.32
    # OR
    # logs/phase1/EPIC-CCN-107.log:Cost: 0.68
    
    for line in output.split('\n'):
        if not line.strip():
            continue
            
        # Extract epic ID
        epic_match = re.search(r'EPIC-CCN-(\d+)', line)
        if not epic_match:
            continue
        epic_id = f"EPIC-CCN-{epic_match.group(1)}"
        
        # Extract cost
        cost_match = re.search(r'Cost:\s*([0-9.]+)', line)
        if not cost_match:
            continue
        cost = float(cost_match.group(1))
        
        # Extract balance (optional)
        balance_match = re.search(r'Balance:\s*([0-9.]+)', line)
        balance = float(balance_match.group(1)) if balance_match else None
        
        costs[epic_id] = (cost, balance)
    
    return costs


def calculate_balances(phases: List[str]) -> Dict[str, Dict[str, float]]:
    """
    Calculate current balances for all APIs based on usage across phases.
    
    Returns: Dict[api_file, Dict[phase, cost]]
    """
    api_usage = {api: {} for api in EPIC_ASSIGNMENTS.values()}
    
    for phase in phases:
        phase_costs = extract_costs_from_vm_logs(phase)
        
        for epic_id, (cost, balance) in phase_costs.items():
            api_file = EPIC_ASSIGNMENTS.get(epic_id)
            if api_file:
                api_usage[api_file][phase] = cost
    
    return api_usage


def get_current_balances(api_usage: Dict[str, Dict[str, float]]) -> Dict[str, float]:
    """Calculate current balance for each API"""
    balances = {}
    
    for api_file, usage in api_usage.items():
        total_used = sum(usage.values())
        remaining = INITIAL_BALANCE - total_used
        balances[api_file] = remaining
    
    # Add reserve API
    balances[RESERVE_API] = INITIAL_BALANCE
    
    return balances


def check_thresholds(balances: Dict[str, float]) -> List[str]:
    """Check balance thresholds and return alerts"""
    alerts = []
    
    for api_file, balance in balances.items():
        if api_file == RESERVE_API:
            continue
            
        if balance < CRITICAL_BALANCE_THRESHOLD:
            alerts.append(f"🚨 CRITICAL: {api_file} balance {balance:.2f} < {CRITICAL_BALANCE_THRESHOLD}")
        elif balance < LOW_BALANCE_THRESHOLD:
            alerts.append(f"⚠️ WARNING: {api_file} balance {balance:.2f} < {LOW_BALANCE_THRESHOLD}")
    
    return alerts


def recommend_reassignments(balances: Dict[str, float]) -> List[Tuple[str, str, str]]:
    """
    Recommend epic reassignments for low-balance APIs.
    
    Returns: List[(epic_id, old_api, new_api)]
    """
    recommendations = []
    
    # Find APIs below threshold
    low_apis = [api for api, bal in balances.items() 
                if api != RESERVE_API and bal < LOW_BALANCE_THRESHOLD]
    
    if not low_apis:
        return recommendations
    
    # Find epics assigned to low APIs
    for epic_id, api_file in EPIC_ASSIGNMENTS.items():
        if api_file in low_apis:
            # Recommend reserve API
            recommendations.append((epic_id, api_file, RESERVE_API))
    
    return recommendations


def format_status_table(api_usage: Dict[str, Dict[str, float]], 
                       balances: Dict[str, float]) -> str:
    """Format current status as markdown table"""
    lines = [
        "| API Key File | Epic | Initial | Phase 0 | Phase 1 | Phase 1.5 | Phase 2 | Remaining | Status |",
        "|--------------|------|---------|---------|---------|-----------|---------|-----------|--------|"
    ]
    
    # Reverse lookup: api -> epic
    api_to_epic = {v: k for k, v in EPIC_ASSIGNMENTS.items()}
    
    for api_file in sorted(api_usage.keys()):
        epic = api_to_epic.get(api_file, "N/A")
        usage = api_usage[api_file]
        balance = balances[api_file]
        
        # Determine status
        if balance >= 100:
            status = "✅ Healthy"
        elif balance >= LOW_BALANCE_THRESHOLD:
            status = "⚠️ Monitor"
        elif balance >= CRITICAL_BALANCE_THRESHOLD:
            status = "🔶 Caution"
        else:
            status = "🔴 Critical"
        
        # Format costs
        p0 = f"-{usage.get('phase0', 0):.2f}" if 'phase0' in usage else "TBD"
        p1 = f"-{usage.get('phase1', 0):.2f}" if 'phase1' in usage else "TBD"
        p1_5 = f"-{usage.get('phase1_5', 0):.2f}" if 'phase1_5' in usage else "TBD"
        p2 = f"-{usage.get('phase2', 0):.2f}" if 'phase2' in usage else "TBD"
        
        lines.append(
            f"| {api_file} | {epic} | {INITIAL_BALANCE:.2f} | {p0} | {p1} | {p1_5} | {p2} | {balance:.2f} | {status} |"
        )
    
    # Add reserve
    lines.append(
        f"| {RESERVE_API} | RESERVE | {INITIAL_BALANCE:.2f} | 0 | 0 | 0 | 0 | {balances[RESERVE_API]:.2f} | 🔒 Reserve |"
    )
    
    return "\n".join(lines)


def main():
    """Main execution"""
    print("=" * 60)
    print("API Balance Tracker - Wave 2 Autonomous Refactoring")
    print("=" * 60)
    print()
    
    # Load API keys
    print("📋 Loading API keys...")
    api_keys = load_api_keys()
    print(f"   Found {len(api_keys)} API keys")
    print()
    
    # Calculate balances
    print("💰 Calculating balances...")
    phases = ['phase0', 'phase1', 'phase1_5']
    api_usage = calculate_balances(phases)
    balances = get_current_balances(api_usage)
    print()
    
    # Display status
    print("📊 Current Status:")
    print()
    print(format_status_table(api_usage, balances))
    print()
    
    # Check thresholds
    print("🔍 Threshold Checks:")
    alerts = check_thresholds(balances)
    if alerts:
        for alert in alerts:
            print(f"   {alert}")
    else:
        print("   ✅ All APIs above threshold")
    print()
    
    # Recommendations
    print("💡 Recommendations:")
    recommendations = recommend_reassignments(balances)
    if recommendations:
        print("   ⚠️ Reassignment needed:")
        for epic_id, old_api, new_api in recommendations:
            print(f"      {epic_id}: {old_api} → {new_api}")
    else:
        print("   ✅ No reassignments needed")
    print()
    
    # Summary
    total_used = sum(sum(usage.values()) for usage in api_usage.values())
    total_remaining = sum(balances.values())
    print("📈 Summary:")
    print(f"   Total Budget: {len(api_keys) * INITIAL_BALANCE:.2f} bobcoins")
    print(f"   Total Used: {total_used:.2f} bobcoins")
    print(f"   Total Remaining: {total_remaining:.2f} bobcoins")
    print(f"   Usage: {(total_used / (len(api_keys) * INITIAL_BALANCE) * 100):.1f}%")
    print()
    
    print("=" * 60)
    print("✅ Balance tracking complete")
    print("=" * 60)


if __name__ == "__main__":
    main()

# Made with Bob
