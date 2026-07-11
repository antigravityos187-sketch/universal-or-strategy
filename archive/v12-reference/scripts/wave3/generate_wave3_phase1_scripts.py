#!/usr/bin/env python3
"""
Generate Wave 3 Phase 1 scripts by copying Wave 2 Phase 1 pattern.
Building-blocks methodology: Copy working scripts, change only what's necessary.
"""

import json
from pathlib import Path

# Wave 3 epic definitions (from WAVE3_EPIC_ROADMAP.md)
WAVE3_EPICS = [
    {"id": 116, "method": "PropagateMaster_IdentifyMove", "file": "V12_002.Orders.Callbacks.Propagation.cs", "cyc": 18},
    {"id": 117, "method": "HandleFlatPosition_CleanupActivePositions", "file": "V12_002.Orders.Callbacks.Execution.cs", "cyc": 17},
    {"id": 118, "method": "SyncLimitTarget", "file": "V12_002.Orders.Management.StopSync.cs", "cyc": 17},
    {"id": 119, "method": "EmergencyFlattenSingleFleetAccount", "file": "V12_002.SIMA.Flatten.cs", "cyc": 16},
    {"id": 120, "method": "AuditMaster_HandleNakedPosition", "file": "V12_002.REAPER.Audit.cs", "cyc": 15},
    {"id": 121, "method": "ProcessQueuedAccountOrder", "file": "V12_002.Orders.Callbacks.AccountOrders.cs", "cyc": 15},
    {"id": 122, "method": "ProcessSingleFleetRMAAccount", "file": "V12_002.SIMA.Execution.cs", "cyc": 14},
    {"id": 123, "method": "HandleMasterOrderUpdate", "file": "V12_002.Orders.Callbacks.Master.cs", "cyc": 13},
    {"id": 124, "method": "ValidateOrderPlacement", "file": "V12_002.Orders.Validation.cs", "cyc": 12},
    {"id": 125, "method": "ReconcilePositionState", "file": "V12_002.Orders.Reconciliation.cs", "cyc": 11},
]

# API key mapping (reuse Wave 2 allocation - same as Phase 0)
API_KEYS = {
    116: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    117: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    118: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    119: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    120: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    121: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    122: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    123: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    124: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
    125: "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu",  # b (2).json
}

def generate_phase1_script(epic_id: int, method: str, file: str, cyc: int, api_key: str) -> str:
    """Generate Phase 1 script by copying Wave 2 template pattern."""
    
    script = f"""#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/EPIC-CCN-{epic_id}
mkdir -p logs/phase1

cat > /tmp/phase1_msg_{epic_id}.txt << 'EOFMSG'
You are executing Phase 1 (Scope + Boundary) for EPIC-CCN-{epic_id}.

**IMPORTANT**: Phase 1 now combines Scope Definition AND Boundary Validation (V12.25 10-phase workflow).

**Input Artifact**: Read `docs/brain/EPIC-CCN-{epic_id}/00-hotspots.md` for hotspot analysis.

**Your Task**: Define the extraction scope AND validate boundary constraints.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-{epic_id}/00-scope.md` with:
   - Target method details
   - Extraction strategy (what to extract, what to keep)
   - Boundary definition (single method only, no scope creep)
   - Success criteria (target complexity <= 8, Jane Street alignment)
   - Risk assessment
   - **Boundary Validation Section**:
     * Confirm extraction stays within single method
     * List any dependencies that would violate boundary
     * Explicit statement: "Boundary validated: YES/NO"

2. Update `docs/brain/EPIC-CCN-{epic_id}/manifest.json`:
   - Set phase "1" status to "completed"
   - Add "00-scope.md" to outputs

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Keep scope to single method (V12.23 No Scope Creep Protocol)
- Target complexity <= 8 (Jane Street HFT alignment, NOT 15)
- **MANDATORY**: Boundary validation must explicitly confirm single-method scope

**Phase**: 1 (Scope + Boundary)
**Target Complexity**: <= 8 (Jane Street standard)
EOFMSG

bob --yolo --chat-mode plan "$(cat /tmp/phase1_msg_{epic_id}.txt)" 2>&1 | tee logs/phase1/EPIC-CCN-{epic_id}.log
echo "DONE_EXIT=$?"

# Made with Bob
"""
    return script

def generate_launcher_script(epic_ids: list[int]) -> str:
    """Generate launcher script for all Phase 1 epics."""
    
    script = """#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy

echo "Launching Wave 3 Phase 1 (Scope + Boundary) for 10 epics..."
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
echo "Attach to session: screen -r p1-116"
echo "Check logs: tail -f logs/phase1/EPIC-CCN-116.log"
"""
    
    return script

def main():
    """Generate all Wave 3 Phase 1 scripts."""
    
    output_dir = Path("scripts/wave3")
    output_dir.mkdir(parents=True, exist_ok=True)
    
    print("Generating Wave 3 Phase 1 scripts...")
    print(f"Output directory: {output_dir}")
    print()
    
    epic_ids = []
    
    for epic in WAVE3_EPICS:
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
    print(f"[OK] Generated {len(WAVE3_EPICS)} Phase 1 scripts + 1 launcher")
    print()
    print("Next steps:")
    print("1. Upload to VM: gcloud compute scp scripts/wave3/_p1_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a")
    print("2. Upload launcher: gcloud compute scp scripts/wave3/launch_phase1_all_screen.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a")
    print("3. Launch: gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command='chmod +x /home/malhitticrypto/universal-or-strategy/launch_phase1_all_screen.sh && /home/malhitticrypto/universal-or-strategy/launch_phase1_all_screen.sh'")

if __name__ == "__main__":
    main()