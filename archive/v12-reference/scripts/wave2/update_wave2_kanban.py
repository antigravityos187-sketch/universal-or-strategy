#!/usr/bin/env python3
"""
Update existing WAVE_2_KANBAN board with Phase 5 ticket progress.

This script reads the current Kanban board and updates Phase 5 ticket statuses
based on VM execution results.

Usage:
    # Find your vault first
    python scripts/wave2/update_wave2_kanban.py --find-vault
    
    # Then update (one-time)
    python scripts/wave2/update_wave2_kanban.py --vault-path "C:/path/to/V12-Agent-Vault"
    
    # Or watch mode (auto-update every 60 seconds)
    python scripts/wave2/update_wave2_kanban.py --vault-path "C:/path/to/V12-Agent-Vault" --watch
"""

import subprocess
import re
from pathlib import Path
from datetime import datetime
import argparse
import time
import sys

# Epic configuration for Phase 5
EPICS = {
    107: 6, 108: 5, 109: 4, 111: 3, 112: 6, 113: 5, 114: 1
}

def find_obsidian_vaults():
    """Find Obsidian vaults on the system."""
    print("Searching for Obsidian vaults...")
    
    # Common locations
    search_paths = [
        Path.home() / "Documents" / "Obsidian",
        Path.home() / "Obsidian",
        Path("C:/WSGTA"),
    ]
    
    vaults = []
    for search_path in search_paths:
        if search_path.exists():
            for item in search_path.rglob(".obsidian"):
                if item.is_dir():
                    vault_path = item.parent
                    vaults.append(vault_path)
                    print(f"  Found: {vault_path}")
    
    return vaults

def run_gcloud_command(command):
    """Execute gcloud command and return output."""
    try:
        result = subprocess.run(
            command,
            shell=True,
            capture_output=True,
            text=True,
            timeout=30
        )
        return result.stdout.strip()
    except Exception as e:
        return f"ERROR: {e}"

def get_ticket_status(epic_id, ticket_num):
    """Get status of a specific ticket from VM."""
    # Check if verification file exists
    cmd = f'gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="test -f /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-{epic_id}/ticket-{ticket_num}-verification.md && echo EXISTS || echo MISSING" 2>/dev/null'
    output = run_gcloud_command(cmd)
    
    if "MISSING" in output or "ERROR" in output:
        return "pending"
    
    # Check verdict
    cmd = f'gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -i \'verdict\' /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-{epic_id}/ticket-{ticket_num}-verification.md 2>/dev/null | head -1"'
    verdict = run_gcloud_command(cmd)
    
    if "FAIL" in verdict:
        return "failed"
    elif "PASS" in verdict or "CONDITIONAL PASS" in verdict:
        return "complete"
    else:
        return "in-progress"

def get_all_phase5_status():
    """Get status of all Phase 5 tickets."""
    print("Fetching Phase 5 status from VM...")
    status = {}
    
    for epic_id, ticket_count in EPICS.items():
        epic_tickets = []
        for ticket_num in range(1, ticket_count + 1):
            ticket_status = get_ticket_status(epic_id, ticket_num)
            epic_tickets.append({
                "number": ticket_num,
                "status": ticket_status
            })
            print(f"  EPIC-{epic_id} T{ticket_num}: {ticket_status}")
        
        status[epic_id] = epic_tickets
    
    return status

def update_kanban_board(vault_path, status):
    """Update the existing WAVE_2_KANBAN board."""
    kanban_file = Path(vault_path) / "WAVE_2_KANBAN.md"
    
    if not kanban_file.exists():
        print(f"ERROR: Kanban file not found: {kanban_file}")
        print("   Looking for WAVE_2_KANBAN.md in vault...")
        return False
    
    # Read existing content
    content = kanban_file.read_text(encoding='utf-8')
    
    # Find or create Phase 5 section
    phase5_pattern = r'## Phase 5: Tickets.*?(?=\n##|\Z)'
    
    # Generate Phase 5 cards
    phase5_cards = []
    for epic_id, tickets in sorted(status.items()):
        for ticket in tickets:
            ticket_num = ticket["number"]
            ticket_status = ticket["status"]
            
            # Status emoji
            if ticket_status == "complete":
                status_emoji = "[DONE]"
            elif ticket_status == "failed":
                status_emoji = "[FAIL]"
            elif ticket_status == "in-progress":
                status_emoji = "[WORK]"
            else:
                status_emoji = "[PEND]"
            
            card = f"- [ ] {status_emoji} EPIC-CCN-{epic_id} T{ticket_num} ({ticket_status})"
            phase5_cards.append(card)
    
    # Count by status
    complete_count = sum(1 for epic in status.values() for t in epic if t["status"] == "complete")
    failed_count = sum(1 for epic in status.values() for t in epic if t["status"] == "failed")
    in_progress_count = sum(1 for epic in status.values() for t in epic if t["status"] == "in-progress")
    pending_count = sum(1 for epic in status.values() for t in epic if t["status"] == "pending")
    total_count = sum(len(tickets) for tickets in status.values())
    
    # Build Phase 5 section
    phase5_section = f"""## Phase 5: Tickets

**Progress**: {complete_count}/{total_count} complete | {failed_count} failed | {in_progress_count} in progress | {pending_count} pending
**Last Updated**: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}

{chr(10).join(phase5_cards)}

"""
    
    # Replace or append Phase 5 section
    if re.search(phase5_pattern, content, re.DOTALL):
        # Replace existing section
        new_content = re.sub(phase5_pattern, phase5_section.rstrip(), content, flags=re.DOTALL)
    else:
        # Append new section
        new_content = content.rstrip() + "\n\n" + phase5_section
    
    # Write back
    kanban_file.write_text(new_content, encoding='utf-8')
    print(f"SUCCESS: Updated {kanban_file}")
    print(f"   Progress: {complete_count}/{total_count} tickets ({complete_count * 100 // total_count}%)")
    
    return True

def main():
    parser = argparse.ArgumentParser(description="Update WAVE_2_KANBAN with Phase 5 progress")
    parser.add_argument("--vault-path", help="Path to Obsidian vault")
    parser.add_argument("--find-vault", action="store_true", help="Find Obsidian vaults on system")
    parser.add_argument("--watch", action="store_true", help="Watch mode - continuously update")
    parser.add_argument("--interval", type=int, default=60, help="Update interval in seconds (default: 60)")
    
    args = parser.parse_args()
    
    # Find vaults if requested
    if args.find_vault:
        vaults = find_obsidian_vaults()
        if not vaults:
            print("ERROR: No Obsidian vaults found")
        return
    
    # Require vault path
    if not args.vault_path:
        print("ERROR: --vault-path required")
        print("\nUsage:")
        print("  python update_wave2_kanban.py --find-vault")
        print("  python update_wave2_kanban.py --vault-path 'C:/path/to/vault'")
        return
    
    vault_path = Path(args.vault_path)
    if not vault_path.exists():
        print(f"ERROR: Vault path not found: {vault_path}")
        return
    
    print(f"Starting WAVE_2_KANBAN updater")
    print(f"Vault: {vault_path}")
    
    if args.watch:
        print(f"Watch mode - updating every {args.interval} seconds")
        print("   Press Ctrl+C to stop\n")
        
        try:
            while True:
                print(f"[{datetime.now().strftime('%H:%M:%S')}]")
                status = get_all_phase5_status()
                update_kanban_board(vault_path, status)
                print(f"   Sleeping {args.interval}s...\n")
                time.sleep(args.interval)
        except KeyboardInterrupt:
            print("\nStopped")
    else:
        status = get_all_phase5_status()
        if update_kanban_board(vault_path, status):
            print("SUCCESS: Done!")
        else:
            print("ERROR: Update failed")

if __name__ == "__main__":
    main()

# Made with Bob
