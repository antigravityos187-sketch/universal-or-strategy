#!/usr/bin/env python3
"""
Generate Phase 4 recovery scripts using building-blocks method.

Usage:
    python scripts/wave4/generate_phase4_recovery.py

Generates recovery scripts for failed Phase 4 epics:
- EPIC-CCN-044 (missing Phase 2/3)
- EPIC-CCN-065 (critical error)
- EPIC-CCN-074 (MCP connection error)
"""

import os
import shutil
from pathlib import Path

# Failed epics from Wave 4 Phase 4
FAILED_EPICS = [
    "EPIC-CCN-044",
    "EPIC-CCN-065", 
    "EPIC-CCN-074"
]

def generate_recovery_scripts():
    """Generate recovery scripts using building-blocks method."""
    
    # 1. Use working script as template (EPIC-CCN-001)
    template_path = Path("scripts/wave4/_p4_001.sh")
    
    if not template_path.exists():
        print(f"ERROR: Template script not found: {template_path}")
        return False
    
    print(f"Using template: {template_path}")
    
    # 2. For each failed epic, copy and modify
    for epic_id in FAILED_EPICS:
        epic_num = epic_id.split("-")[-1]  # Extract "044" from "EPIC-CCN-044"
        
        # Recovery script path
        recovery_script = Path(f"scripts/wave4/_p4_{epic_num}_recovery.sh")
        
        print(f"\nGenerating recovery script for {epic_id}...")
        
        # 3. Copy template
        shutil.copy(template_path, recovery_script)
        print(f"  Copied template to {recovery_script}")
        
        # 4. Find-and-replace epic ID only
        with open(recovery_script, 'r') as f:
            content = f.read()
        
        # Replace EPIC-CCN-001 with target epic ID
        content = content.replace("EPIC-CCN-001", epic_id)
        content = content.replace("001", epic_num)
        
        with open(recovery_script, 'w') as f:
            f.write(content)
        
        print(f"  Replaced EPIC-CCN-001 -> {epic_id}")
        print(f"  [OK] Created: {recovery_script}")
    
    # 5. Generate recovery launcher
    generate_recovery_launcher()
    
    return True

def generate_recovery_launcher():
    """Generate launcher script for recovery epics."""
    
    launcher_path = Path("scripts/wave4/launch_phase4_recovery.sh")
    
    print(f"\nGenerating recovery launcher: {launcher_path}")
    
    # Building-blocks: Copy from launch_phase4_test.sh pattern
    launcher_content = """#!/bin/bash
# Wave 4 Phase 4 Recovery Launcher
# Generated: 2026-06-15
# Epics: EPIC-CCN-044, 065, 074

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/../.."

echo "=== Wave 4 Phase 4 Recovery ==="
echo "Recovering 3 failed epics..."
echo ""

# Constant delay (12 seconds)
DELAY=12

# Launch recovery scripts
for EPIC_NUM in 044 065 074; do
    SCRIPT="./scripts/wave4/_p4_${EPIC_NUM}_recovery.sh"
    
    if [ ! -f "$SCRIPT" ]; then
        echo "ERROR: Recovery script not found: $SCRIPT"
        exit 1
    fi
    
    echo "Launching EPIC-CCN-${EPIC_NUM} recovery..."
    screen -dmS "p4-recovery-${EPIC_NUM}" bash -l -c "$SCRIPT" | tee "logs/phase4/EPIC-CCN-${EPIC_NUM}_recovery.log"
    
    echo "  Screen session: p4-recovery-${EPIC_NUM}"
    echo "  Waiting ${DELAY}s before next launch..."
    sleep $DELAY
done

echo ""
echo "=== Recovery Launch Complete ==="
echo "3 recovery sessions started"
echo ""
echo "Monitor with:"
echo "  screen -ls | grep p4-recovery"
echo "  ls docs/brain/EPIC-CCN-{044,065,074}/04-tickets.md"
echo ""
"""
    
    with open(launcher_path, 'w') as f:
        f.write(launcher_content)
    
    # Make executable
    os.chmod(launcher_path, 0o755)
    
    print(f"  [OK] Created: {launcher_path}")
    print(f"  [OK] Made executable")

def main():
    """Main entry point."""
    print("Wave 4 Phase 4 Recovery Script Generator")
    print("=" * 50)
    print("")
    
    success = generate_recovery_scripts()
    
    if success:
        print("\n" + "=" * 50)
        print("[SUCCESS] Recovery scripts generated successfully!")
        print("")
        print("Next steps:")
        print("1. Upload to VM:")
        print("   gcloud compute scp scripts/wave4/_p4_*_recovery.sh v12-test-golden-v2:~/universal-or-strategy/scripts/wave4/ --zone=us-central1-a")
        print("   gcloud compute scp scripts/wave4/launch_phase4_recovery.sh v12-test-golden-v2:~/universal-or-strategy/scripts/wave4/ --zone=us-central1-a")
        print("")
        print("2. Set permissions:")
        print("   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command=\"cd universal-or-strategy/scripts/wave4 && chmod +x _p4_*_recovery.sh launch_phase4_recovery.sh\"")
        print("")
        print("3. Execute recovery:")
        print("   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command=\"cd universal-or-strategy && ./scripts/wave4/launch_phase4_recovery.sh\"")
        print("")
    else:
        print("\n[ERROR] Recovery script generation failed!")
        return 1
    
    return 0

if __name__ == "__main__":
    exit(main())

# Made with Bob
