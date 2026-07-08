#!/usr/bin/env python3
"""
Check Bob Shell API balances for all APIs in docs/API/

Usage:
    python scripts/wave2/check_api_balances.py
"""

import json
import subprocess
from pathlib import Path

API_DIR = Path("docs/API")


def check_balance(api_key: str) -> dict:
    """Check balance for a single API key using bob CLI."""
    try:
        # Run bob with the API key to check balance
        result = subprocess.run(
            ["bob", "--api-key", api_key, "--check-balance"],
            capture_output=True,
            text=True,
            timeout=10
        )
        
        # Parse output for balance info
        # This is a placeholder - actual command may differ
        output = result.stdout + result.stderr
        
        # Try to extract balance from output
        # Format may vary - adjust based on actual bob CLI output
        if "balance" in output.lower():
            # Extract number from output
            import re
            match = re.search(r'(\d+)\s*bobcoins?', output, re.IGNORECASE)
            if match:
                return {
                    "balance": int(match.group(1)),
                    "status": "success",
                    "raw_output": output
                }
        
        return {
            "balance": None,
            "status": "unknown",
            "raw_output": output
        }
        
    except subprocess.TimeoutExpired:
        return {"balance": None, "status": "timeout", "raw_output": ""}
    except Exception as e:
        return {"balance": None, "status": "error", "error": str(e), "raw_output": ""}


def main():
    print("\n═══ Bob Shell API Balance Check ═══\n")
    
    api_files = sorted(API_DIR.glob("*.json"))
    if not api_files:
        print(f"[ERROR] No API files found in {API_DIR}")
        return
    
    print(f"Found {len(api_files)} API keys to check:\n")
    
    results = []
    for idx, json_file in enumerate(api_files, 1):
        try:
            data = json.loads(json_file.read_text())
            api_key = data.get("apikey", "")
            name = data.get("name", json_file.stem)
            
            if not api_key:
                print(f"{idx}. {json_file.name}: [SKIP] No API key found")
                continue
            
            print(f"{idx}. {json_file.name} ({name})...")
            
            # Check balance
            result = check_balance(api_key)
            
            if result["status"] == "success" and result["balance"] is not None:
                balance = result["balance"]
                status = "✅ OK" if balance > 10 else "⚠️  LOW" if balance > 0 else "❌ NEGATIVE"
                print(f"   Balance: {balance} bobcoins {status}")
                results.append({
                    "file": json_file.name,
                    "name": name,
                    "balance": balance,
                    "status": status
                })
            else:
                print(f"   Status: {result['status']}")
                print(f"   Output: {result.get('raw_output', 'N/A')[:100]}")
                results.append({
                    "file": json_file.name,
                    "name": name,
                    "balance": "Unknown",
                    "status": "❓ UNKNOWN"
                })
            
            print()
            
        except Exception as e:
            print(f"{idx}. {json_file.name}: [ERROR] {e}\n")
    
    # Summary
    print("\n═══ Summary ═══\n")
    print(f"{'File':<40} {'Name':<30} {'Balance':<15} {'Status'}")
    print("─" * 100)
    
    total_balance = 0
    for r in results:
        balance_str = str(r['balance']) if isinstance(r['balance'], int) else r['balance']
        print(f"{r['file']:<40} {r['name']:<30} {balance_str:<15} {r['status']}")
        if isinstance(r['balance'], int):
            total_balance += r['balance']
    
    print("─" * 100)
    if total_balance > 0:
        print(f"{'TOTAL':<40} {'':<30} {total_balance:<15} bobcoins")
    
    print("\n[NOTE] If balances show as 'Unknown', the bob CLI may not support --check-balance")
    print("[NOTE] Alternative: Check balances via IBM Bob Shell web dashboard")
    print("[NOTE] Or use: bob --help to see available balance check commands")


if __name__ == "__main__":
    main()

# Made with Bob
