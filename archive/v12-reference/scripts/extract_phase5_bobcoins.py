#!/usr/bin/env python3
"""
Extract bobcoin usage from Wave 4 Phase 5 logs.
Generates CSV report with per-epic and per-API breakdown.
"""

import re
import json
from pathlib import Path
from collections import defaultdict

def extract_bobcoins_from_log(log_path):
    """Extract bobcoin usage from a single log file."""
    epic_id = log_path.stem  # e.g., "EPIC-CCN-001"
    
    bobcoins_used = []
    api_used = None
    
    with open(log_path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()
        
        # Pattern 1: "Cost: X.XX | Balance: Y.YY"
        cost_matches = re.findall(r'Cost:\s*(\d+\.?\d*)\s*\|\s*Balance:\s*(\d+\.?\d*)', content)
        
        # Pattern 2: "Cost: X" (without balance)
        simple_cost_matches = re.findall(r'Cost:\s*(\d+\.?\d*)', content)
        
        # Extract API name (from script or log)
        api_match = re.search(r'API_KEY="([^"]+)"', content)
        if api_match:
            api_key = api_match.group(1)
            # Extract API name from key (e.g., "bob_prod_bob-admin_...")
            if 'bob-admin' in api_key:
                api_used = 'bob-admin'
            elif 'jessica' in api_key:
                api_used = 'jessica'
            elif 'mikethelife' in api_key:
                api_used = 'mikethelife'
            else:
                api_used = 'unknown'
        
        # Collect all cost values
        if cost_matches:
            bobcoins_used = [float(cost) for cost, _ in cost_matches]
        elif simple_cost_matches:
            bobcoins_used = [float(cost) for cost in simple_cost_matches]
    
    # Use the last (final) cost value
    final_cost = bobcoins_used[-1] if bobcoins_used else 0.0
    
    return {
        'epic_id': epic_id,
        'bobcoins_used': final_cost,
        'api_used': api_used or 'unknown',
        'success': final_cost > 0  # If bobcoins were used, likely succeeded
    }

def main():
    logs_dir = Path('logs/phase5')
    
    if not logs_dir.exists():
        print(f"ERROR: {logs_dir} not found")
        return
    
    # Process all Wave 4 logs (EPIC-CCN-001 through EPIC-CCN-080)
    results = []
    api_totals = defaultdict(float)
    
    for i in range(1, 81):
        epic_num = f"{i:03d}"
        log_file = logs_dir / f"EPIC-CCN-{epic_num}.log"
        
        if log_file.exists():
            result = extract_bobcoins_from_log(log_file)
            results.append(result)
            api_totals[result['api_used']] += result['bobcoins_used']
        else:
            # Epic not found (e.g., EPIC-CCN-016 failed)
            results.append({
                'epic_id': f"EPIC-CCN-{epic_num}",
                'bobcoins_used': 0.0,
                'api_used': 'N/A',
                'success': False
            })
    
    # Write CSV report
    csv_path = Path('WAVE4_PHASE5_BOBCOIN_USAGE.csv')
    with open(csv_path, 'w') as f:
        f.write("Epic,Bobcoins_Used,API_Used,Success\n")
        for r in results:
            f.write(f"{r['epic_id']},{r['bobcoins_used']:.2f},{r['api_used']},{r['success']}\n")
    
    # Calculate statistics
    total_bobcoins = sum(r['bobcoins_used'] for r in results)
    successful_epics = sum(1 for r in results if r['success'])
    avg_per_epic = total_bobcoins / successful_epics if successful_epics > 0 else 0
    
    # Write summary report
    summary_path = Path('WAVE4_PHASE5_BOBCOIN_SUMMARY.txt')
    with open(summary_path, 'w') as f:
        f.write("Wave 4 Phase 5 Bobcoin Usage Summary\n")
        f.write("=" * 50 + "\n\n")
        f.write(f"Total Epics: 80\n")
        f.write(f"Successful Epics: {successful_epics}\n")
        f.write(f"Failed Epics: {80 - successful_epics}\n\n")
        f.write(f"Total Bobcoins Used: {total_bobcoins:.2f}\n")
        f.write(f"Average per Epic: {avg_per_epic:.2f}\n\n")
        f.write("Per-API Breakdown:\n")
        for api, total in sorted(api_totals.items()):
            f.write(f"  {api}: {total:.2f} bobcoins\n")
        f.write("\n")
        f.write(f"Budget: 800-1,600 bobcoins\n")
        f.write(f"Status: {'WITHIN BUDGET' if total_bobcoins <= 1600 else 'OVER BUDGET'}\n")
    
    print(f"[OK] CSV report: {csv_path}")
    print(f"[OK] Summary report: {summary_path}")
    print(f"\nTotal Bobcoins: {total_bobcoins:.2f}")
    print(f"Successful Epics: {successful_epics}/80")
    print(f"Average per Epic: {avg_per_epic:.2f}")

if __name__ == '__main__':
    main()

# Made with Bob
