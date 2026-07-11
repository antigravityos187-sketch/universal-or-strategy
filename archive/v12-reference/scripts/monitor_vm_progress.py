#!/usr/bin/env python3
"""
Monitor VM progress and update Obsidian Kanban board.
Polls VM every 5 minutes via gcloud SSH.
"""

import subprocess
import json
import time
import os
from pathlib import Path
from datetime import datetime

# Configuration
VM_NAME = "v12-epic-executor"
PROJECT_ID = "project-14c86305-3cba-493f-a73"
ZONE = "us-central1-a"
KANBAN_PATH = Path(r"C:\Users\Mohammed Khalid\Documents\V12-Agent-Vault\WAVE_2_KANBAN.md")
POLL_INTERVAL = 300  # 5 minutes

# Epic configuration
EPICS = [
    {
        "epic_id": "EPIC-CCN-164",
        "method": "IsCommandForThisInstrument",
        "cyc": 36,
        "target": 8
    },
    {
        "epic_id": "EPIC-CCN-107",
        "method": "HydrateFromOpenPositions",
        "cyc": 31,
        "target": 8
    }
]

# Phase names for Kanban columns
PHASES = [
    "Pending",
    "Phase 0: Hotspot",
    "Phase 1: Scope",
    "Phase 1.5: Boundary",
    "Phase 2: Architecture",
    "Phase 3: Audit",
    "Phase 4: Tickets",
    "Phase 5: Execute",
    "Phase 5.5: Verify",
    "Phase 6: Review",
    "Complete"
]

def run_ssh_command(command):
    """Execute command on VM via gcloud SSH."""
    try:
        result = subprocess.run(
            [
                "gcloud", "compute", "ssh", VM_NAME,
                f"--project={PROJECT_ID}",
                f"--zone={ZONE}",
                f"--command={command}"
            ],
            capture_output=True,
            text=True,
            timeout=30
        )
        return result.returncode == 0, result.stdout, result.stderr
    except subprocess.TimeoutExpired:
        return False, "", "SSH command timed out"
    except Exception as e:
        return False, "", str(e)

def check_vm_status():
    """Check if VM is running and accessible."""
    success, stdout, stderr = run_ssh_command("echo 'VM accessible'")
    return success

def get_epic_status(epic_id):
    """Get epic status from manifest.json on VM."""
    manifest_path = f"/home/malhitticrypto/universal-or-strategy/docs/brain/{epic_id}/manifest.json"
    success, stdout, stderr = run_ssh_command(f"cat {manifest_path}")
    
    if not success:
        return {
            "phase": "Pending",
            "status": "not_started",
            "error": stderr
        }
    
    try:
        manifest = json.loads(stdout)
        
        # Determine current phase
        phases_order = [
            "phase_0", "phase_1", "phase_1_5", "phase_2",
            "phase_3", "phase_4", "phase_5", "phase_5_5", "phase_6"
        ]
        
        current_phase = "Pending"
        for phase in phases_order:
            if phase in manifest and manifest[phase].get("status") == "in_progress":
                phase_map = {
                    "phase_0": "Phase 0: Hotspot",
                    "phase_1": "Phase 1: Scope",
                    "phase_1_5": "Phase 1.5: Boundary",
                    "phase_2": "Phase 2: Architecture",
                    "phase_3": "Phase 3: Audit",
                    "phase_4": "Phase 4: Tickets",
                    "phase_5": "Phase 5: Execute",
                    "phase_5_5": "Phase 5.5: Verify",
                    "phase_6": "Phase 6: Review"
                }
                current_phase = phase_map.get(phase, "Pending")
                break
            elif phase in manifest and manifest[phase].get("status") == "completed":
                continue
            else:
                break
        
        # Check if complete
        if "phase_6" in manifest and manifest["phase_6"].get("status") == "completed":
            current_phase = "Complete"
        
        return {
            "phase": current_phase,
            "status": manifest.get("status", "unknown"),
            "manifest": manifest
        }
    except json.JSONDecodeError:
        return {
            "phase": "Pending",
            "status": "error",
            "error": "Invalid manifest JSON"
        }

def create_epic_card(epic, status_info):
    """Create Kanban card text for an epic."""
    phase = status_info.get("phase", "Pending")
    status = status_info.get("status", "unknown")
    
    # Status emoji
    status_emoji = {
        "not_started": "⏳",
        "in_progress": "🔄",
        "completed": "✅",
        "failed": "❌",
        "unknown": "❓"
    }.get(status, "❓")
    
    card = f"- [ ] {epic['epic_id']} (CYC {epic['cyc']}→{epic['target']})<br>{epic['method']}<br>{status_emoji} {status.replace('_', ' ').title()}"
    
    return card

def update_kanban_board(epic_statuses):
    """Update Obsidian Kanban board with current epic statuses."""
    
    # Build Kanban content
    lines = [
        "---",
        "",
        "kanban-plugin: basic",
        "",
        "---",
        ""
    ]
    
    # Create columns for each phase
    for phase in PHASES:
        lines.append(f"## {phase}")
        lines.append("")
        
        # Add epics in this phase
        for epic in EPICS:
            status_info = epic_statuses.get(epic["epic_id"], {"phase": "Pending", "status": "not_started"})
            if status_info["phase"] == phase:
                card = create_epic_card(epic, status_info)
                lines.append(card)
                lines.append("")
        
        lines.append("")
    
    # Add metadata
    lines.extend([
        "",
        "%% kanban:settings",
        "```",
        '{"kanban-plugin":"basic","list-collapse":[false,false,false,false,false,false,false,false,false,false,false]}',
        "```",
        "%%"
    ])
    
    # Write to file
    content = "\n".join(lines)
    KANBAN_PATH.write_text(content, encoding="utf-8")
    
    print(f"✅ Kanban board updated at {datetime.now().strftime('%H:%M:%S')}")

def main():
    """Main monitoring loop."""
    print("🚀 Starting VM progress monitor...")
    print(f"📊 Kanban board: {KANBAN_PATH}")
    print(f"⏱️  Poll interval: {POLL_INTERVAL}s ({POLL_INTERVAL//60} minutes)")
    print()
    
    iteration = 0
    while True:
        iteration += 1
        print(f"\n{'='*60}")
        print(f"Iteration {iteration} - {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        print(f"{'='*60}")
        
        # Check VM status
        print("Checking VM status...")
        if not check_vm_status():
            print("❌ VM not accessible - will retry next iteration")
            epic_statuses = {epic["epic_id"]: {"phase": "Pending", "status": "not_started"} for epic in EPICS}
        else:
            print("✅ VM accessible")
            
            # Get status for each epic
            epic_statuses = {}
            for epic in EPICS:
                print(f"  Checking {epic['epic_id']}...")
                status = get_epic_status(epic["epic_id"])
                epic_statuses[epic["epic_id"]] = status
                print(f"    Phase: {status['phase']}, Status: {status.get('status', 'unknown')}")
        
        # Update Kanban board
        print("\nUpdating Kanban board...")
        update_kanban_board(epic_statuses)
        
        # Check if all complete
        all_complete = all(
            status.get("phase") == "Complete" 
            for status in epic_statuses.values()
        )
        
        if all_complete:
            print("\n🎉 All epics complete! Monitoring stopped.")
            break
        
        # Wait for next iteration
        print(f"\n⏳ Waiting {POLL_INTERVAL}s until next check...")
        time.sleep(POLL_INTERVAL)

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n⚠️  Monitoring stopped by user")
    except Exception as e:
        print(f"\n\n❌ Error: {e}")
        raise

# Made with Bob
