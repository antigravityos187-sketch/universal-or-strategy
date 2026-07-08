#!/usr/bin/env python3
"""
Generate Wave 4 Phase 1 scripts using slash commands.
Updated from Wave 3 to use /epic-intake instead of --chat-mode plan.
"""

import json
from pathlib import Path

# Wave 4 epic definitions (126-135)
WAVE4_EPICS = [
    {"id": 126, "method": "HandleOrderRejection", "file": "V12_002.Orders.Callbacks.Execution.cs", "cyc": 18},
    {"id": 127, "method": "ProcessFleetAccountUpdate", "file": "V12_002.SIMA.Execution.cs", "cyc": 17},
    {"id": 128, "method": "ValidatePositionReconciliation", "file": "V12_002.Orders.Reconciliation.cs", "cyc": 17},
    {"id": 129, "method": "HandleMasterOrderFill", "file": "V12_002.Orders.Callbacks.Master.cs", "cyc": 16},
    {"id": 130, "method": "ProcessRMAPriorityQueue", "file": "V12_002.SIMA.Execution.cs", "cyc": 15},
    {"id": 131, "method": "AuditFleetPositionState", "file": "V12_002.REAPER.Audit.cs", "cyc": 15},
    {"id": 132, "method": "HandleStopLimitSync", "file": "V12_002.Orders.Management.StopSync.cs", "cyc": 14},
    {"id": 133, "method": "ProcessAccountOrderQueue", "file": "V12_002.Orders.Callbacks.AccountOrders.cs", "cyc": 13},
    {"id": 134, "method": "ValidateOrderModification", "file": "V12_002.Orders.Validation.cs", "cyc": 12},
    {"id": 135, "method": "HandlePositionFlattening", "file": "V12_002.SIMA.Flatten.cs", "cyc": 11},
]

# API key mapping (reuse Wave 3 allocation)
API_KEYS = {
    126: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    127: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    128: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    129: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    130: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    131: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    132: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    133: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    134: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    135: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
}

def generate_phase1_script(epic_id: int, method: str, file: str, cyc: int, api_key: str) -> str:
    """Generate Phase 1 script using /epic-intake slash command."""
    
    script = f"""#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/EPIC-CCN-{epic_id}
mkdir -p logs/phase1

bob --yolo /epic-intake EPIC-CCN-{epic_id} 2>&1 | tee logs/phase1/EPIC-CCN-{epic_id}.log
echo "DONE_EXIT=$?"

# Made with Bob
"""
    return script

def generate_launcher_script(epic_ids: list[int]) -> str:
    """Generate launcher script for all Phase 1 epics."""
    
    script = """#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy

echo "Launching Wave 4 Phase 1 (Scope + Boundary) for 10 epics..."
echo "Start time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"

"""
    
    for epic_id in epic_ids:
        script += f"""
# Launch EPIC-CCN-{epic_id}
chmod +x _p1_{epic_id}.sh
screen -dmS "p1-{epic_id}" bash -l "_p1_{epic_id}.sh"
echo "Launched EPIC-CCN-{epic_id} in screen session p1-{epic_id}"
"""
    
    script += """
echo ""
echo "All 10 Phase 1 sessions launched!"
echo "Monitor with: screen -ls"
echo "Attach to session: screen -r p1-126"
echo "Check logs: tail -f logs/phase1/EPIC-CCN-126.log"
"""
    
    return script

def main():
    """Generate all Wave 4 Phase 1 scripts."""
    
    output_dir = Path("scripts/wave4")
    output_dir.mkdir(parents=True, exist_ok=True)
    
    print("Generating Wave 4 Phase 1 scripts...")
    print(f"Output directory: {output_dir}")
    print()
    
    epic_ids = []
    
    for epic in WAVE4_EPICS:
        epic_id = epic["id"]
        method = epic["method"]
        file = epic["file"]
        cyc = epic["cyc"]
        api_key = API_KEYS[epic_id]
        
        script_content = generate_phase1_script(epic_id, method, file, cyc, api_key)
        script_path = output_dir / f"_p1_{epic_id}.sh"
        
        script_path.write_text(script_content)
        print(f"[OK] Generated: {script_path}")
        
        epic_ids.append(epic_id)
    
    # Generate launcher script
    launcher_content = generate_launcher_script(epic_ids)
    launcher_path = output_dir / "launch_phase1_all_screen.sh"
    launcher_path.write_text(launcher_content)
    print(f"[OK] Generated: {launcher_path}")
    
    print()
    print(f"[OK] Generated {len(WAVE4_EPICS)} Phase 1 scripts + 1 launcher")
    print()
    print("Next steps:")
    print("1. Upload to VM: gcloud compute scp scripts/wave4/_p1_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a")
    print("2. Upload launcher: gcloud compute scp scripts/wave4/launch_phase1_all_screen.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a")
    print("3. Launch: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='chmod +x /home/malhitticrypto/universal-or-strategy/launch_phase1_all_screen.sh && /home/malhitticrypto/universal-or-strategy/launch_phase1_all_screen.sh'")

if __name__ == "__main__":
    main()