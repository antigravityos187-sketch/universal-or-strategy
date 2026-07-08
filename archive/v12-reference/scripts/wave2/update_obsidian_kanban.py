#!/usr/bin/env python3
"""
Update Obsidian Kanban board with Wave 2 Phase 5 progress.

This script polls the GCP VM for execution status and updates a local
Obsidian Kanban board in real-time.

Usage:
    python update_obsidian_kanban.py --vault-path "C:/Users/Mohammed Khalid/Documents/Obsidian/V12-Vault"
    
Or run continuously:
    python update_obsidian_kanban.py --vault-path "C:/path/to/vault" --watch --interval 60
"""

import subprocess
import json
import re
from pathlib import Path
from datetime import datetime
import argparse
import time

# Epic configuration
EPICS = {
    107: {"tickets": 6, "name": "EPIC-CCN-107"},
    108: {"tickets": 5, "name": "EPIC-CCN-108"},
    109: {"tickets": 4, "name": "EPIC-CCN-109"},
    111: {"tickets": 3, "name": "EPIC-CCN-111"},
    112: {"tickets": 6, "name": "EPIC-CCN-112"},
    113: {"tickets": 5, "name": "EPIC-CCN-113"},
    114: {"tickets": 1, "name": "EPIC-CCN-114"},
}

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
        return result.stdout
    except Exception as e:
        print(f"Error running command: {e}")
        return ""

def get_epic_status(epic_id):
    """Get status of an epic from VM."""
    # Check if epic has status file (means it's blocked)
    cmd = f'gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cat /tmp/epic_{epic_id}_status.txt 2>/dev/null || echo RUNNING"'
    output = run_gcloud_command(cmd)
    
    if "BLOCKED" in output:
        return "blocked"
    elif "RUNNING" in output:
        return "in-progress"
    else:
        return "complete"

def get_ticket_status(epic_id, ticket_num):
    """Get status of a specific ticket."""
    # Check if verification file exists
    cmd = f'gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="test -f /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-{epic_id}/ticket-{ticket_num}-verification.md && echo EXISTS || echo MISSING"'
    output = run_gcloud_command(cmd)
    
    if "MISSING" in output:
        return "pending"
    
    # Check verdict
    cmd = f'gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -i \'verdict\' /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-{epic_id}/ticket-{ticket_num}-verification.md | head -1"'
    verdict = run_gcloud_command(cmd)
    
    if "FAIL" in verdict:
        return "failed"
    elif "PASS" in verdict:
        return "complete"
    else:
        return "in-progress"

def get_all_status():
    """Get status of all epics and tickets."""
    status = {}
    
    for epic_id, epic_info in EPICS.items():
        epic_status = get_epic_status(epic_id)
        tickets = []
        
        for ticket_num in range(1, epic_info["tickets"] + 1):
            ticket_status = get_ticket_status(epic_id, ticket_num)
            tickets.append({
                "number": ticket_num,
                "status": ticket_status
            })
        
        status[epic_id] = {
            "name": epic_info["name"],
            "status": epic_status,
            "tickets": tickets
        }
    
    return status

def generate_kanban_markdown(status):
    """Generate Obsidian Kanban markdown from status."""
    
    # Count tickets by status
    pending = []
    in_progress = []
    complete = []
    failed = []
    
    for epic_id, epic_data in status.items():
        epic_name = epic_data["name"]
        
        for ticket in epic_data["tickets"]:
            ticket_name = f"{epic_name} T{ticket['number']}"
            ticket_status = ticket["status"]
            
            if ticket_status == "pending":
                pending.append(ticket_name)
            elif ticket_status == "in-progress":
                in_progress.append(ticket_name)
            elif ticket_status == "complete":
                complete.append(ticket_name)
            elif ticket_status == "failed":
                failed.append(ticket_name)
    
    # Generate markdown
    md = f"""---
kanban-plugin: basic
---

## 📋 Pending ({len(pending)})

"""
    for item in pending:
        md += f"- [ ] {item}\n"
    
    md += f"""

## 🔄 In Progress ({len(in_progress)})

"""
    for item in in_progress:
        md += f"- [ ] {item}\n"
    
    md += f"""

## ❌ Failed ({len(failed)})

"""
    for item in failed:
        md += f"- [ ] {item}\n"
    
    md += f"""

## ✅ Complete ({len(complete)})

"""
    for item in complete:
        md += f"- [x] {item}\n"
    
    md += f"""

---

**Last Updated**: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}

**Progress**: {len(complete)}/{len(complete) + len(pending) + len(in_progress) + len(failed)} tickets complete ({len(complete) * 100 // (len(complete) + len(pending) + len(in_progress) + len(failed))}%)
"""
    
    return md

def update_kanban_file(vault_path, markdown):
    """Update the Kanban file in Obsidian vault."""
    kanban_file = Path(vault_path) / "Wave 2 Phase 5 Progress.md"
    
    # Create parent directories if needed
    kanban_file.parent.mkdir(parents=True, exist_ok=True)
    
    # Write the file
    kanban_file.write_text(markdown, encoding='utf-8')
    print(f"✅ Updated Kanban board: {kanban_file}")

def main():
    parser = argparse.ArgumentParser(description="Update Obsidian Kanban with Wave 2 progress")
    parser.add_argument("--vault-path", required=True, help="Path to Obsidian vault")
    parser.add_argument("--watch", action="store_true", help="Watch mode - continuously update")
    parser.add_argument("--interval", type=int, default=60, help="Update interval in seconds (default: 60)")
    
    args = parser.parse_args()
    
    print(f"🚀 Starting Obsidian Kanban updater")
    print(f"📁 Vault path: {args.vault_path}")
    
    if args.watch:
        print(f"👀 Watch mode enabled - updating every {args.interval} seconds")
        print("Press Ctrl+C to stop")
        
        try:
            while True:
                print(f"\n⏰ {datetime.now().strftime('%H:%M:%S')} - Fetching status...")
                status = get_all_status()
                markdown = generate_kanban_markdown(status)
                update_kanban_file(args.vault_path, markdown)
                
                time.sleep(args.interval)
        except KeyboardInterrupt:
            print("\n\n👋 Stopped by user")
    else:
        print("📊 Fetching status (one-time update)...")
        status = get_all_status()
        markdown = generate_kanban_markdown(status)
        update_kanban_file(args.vault_path, markdown)
        print("✅ Done!")

if __name__ == "__main__":
    main()

# Made with Bob
