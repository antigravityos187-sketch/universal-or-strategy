#!/usr/bin/env python3
"""
Wave 3 Phase 2 Script Generator
Building-Blocks Methodology: Copies Phase 2 template, modifies only epic-specific content
"""

import json
import os

# Load API key from JSON
with open('docs/API/b (2).json', 'r') as f:
    api_data = json.load(f)
    API_KEY = api_data['apikey']

# Wave 3 epic IDs
WAVE3_EPICS = list(range(116, 126))  # CCN-116 through CCN-125

# Phase 2 template (copied from _p2_107.sh)
PHASE2_TEMPLATE = """#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/EPIC-CCN-{epic_id}
mkdir -p logs/phase2

cat > /tmp/phase2_msg_{epic_id}.txt << 'EOFMSG'
You are executing Phase 2 (Architecture Planning) for EPIC-CCN-{epic_id}.

**Input Artifact**: Read `docs/brain/EPIC-CCN-{epic_id}/00-scope.md` for scope definition.

**Your Task**: Create detailed architecture plan for the extraction.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-{epic_id}/02-architecture-plan.md` with:
   - Method signatures (before/after)
   - Call graph analysis
   - Dependency mapping
   - Extraction sequence
   - Jane Street compliance checks
   - Risk mitigation strategies

2. Update `docs/brain/EPIC-CCN-{epic_id}/manifest.json`:
   - Set phase "2" status to "completed"
   - Add "02-architecture-plan.md" to outputs

**MANDATORY REPORTING**:
After completing all tasks, you MUST report:
1. Bobcoins used this session: [X.XX]
2. Remaining balance in API key: [Y.YY]
Format: "Cost: X.XX | Balance: Y.YY"

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Target complexity <= 8 (Jane Street alignment)
- Single method extraction only (V12.23 Protocol)

**Phase**: 2 (Architecture Planning)
EOFMSG

bob --yolo /epic-plan EPIC-CCN-{epic_id} 2>&1 | tee logs/phase2/EPIC-CCN-{epic_id}.log
echo "DONE_EXIT=$?"

# Made with Bob
"""

# Launcher template
LAUNCHER_TEMPLATE = """#!/bin/bash
# Wave 3 Phase 2 Launcher - Architecture Planning
# Launches all 10 epics in parallel using screen sessions

cd /home/malhitticrypto/universal-or-strategy

echo "Launching Wave 3 Phase 2 (Architecture Planning) for 10 epics..."
echo "Start time: $(date)"

{launch_commands}

echo "All Phase 2 sessions launched!"
echo "Check status with: screen -ls"
echo "View logs with: tail -f logs/phase2/EPIC-CCN-*.log"
"""

def generate_phase2_scripts():
    """Generate Phase 2 scripts for all Wave 3 epics"""
    
    # Create output directory
    os.makedirs('scripts/wave3', exist_ok=True)
    
    launch_commands = []
    
    for epic_id in WAVE3_EPICS:
        # Generate individual epic script
        script_content = PHASE2_TEMPLATE.format(
            api_key=API_KEY,
            epic_id=epic_id
        )
        
        script_path = f'scripts/wave3/_p2_{epic_id}.sh'
        with open(script_path, 'w', newline='\n') as f:  # Force LF line endings
            f.write(script_content)
        
        print(f"[OK] Generated: {script_path}")
        
        # Add to launcher
        launch_commands.append(
            f'screen -dmS p2-{epic_id} bash -l -c "./_p2_{epic_id}.sh 2>&1 | tee logs/phase2/EPIC-CCN-{epic_id}.log"'
        )
    
    # Generate launcher script
    launcher_content = LAUNCHER_TEMPLATE.format(
        launch_commands='\n'.join(launch_commands)
    )
    
    launcher_path = 'scripts/wave3/launch_phase2_all_screen.sh'
    with open(launcher_path, 'w', newline='\n') as f:  # Force LF line endings
        f.write(launcher_content)
    
    print(f"[OK] Generated: {launcher_path}")
    print(f"\nSummary:")
    print(f"   - Generated {len(WAVE3_EPICS)} Phase 2 scripts")
    print(f"   - Generated 1 launcher script")
    print(f"   - API Key: {API_KEY[:20]}... (reused from Phase 0/1)")
    print(f"\nNext Steps:")
    print(f"   1. Upload scripts to VM")
    print(f"   2. Fix line endings: sed -i 's/\\r$//' *.sh")
    print(f"   3. Make executable: chmod +x *.sh")
    print(f"   4. Launch: ./launch_phase2_all_screen.sh")

if __name__ == '__main__':
    generate_phase2_scripts()