#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Wave 3 Phase 3 Script Generator
Generates Phase 3 (DNA & PR Audit) scripts for all 10 epics

CRITICAL: Uses building-blocks methodology - copies Phase 2 pattern
"""

import os
import json

# Load API key from JSON (CRITICAL: Must match Phase 2 pattern)
with open('docs/API/b (2).json', 'r') as f:
    api_data = json.load(f)
    API_KEY = api_data['apikey']

# Epic configuration
EPICS = [116, 117, 118, 119, 120, 121, 122, 123, 124, 125]

def generate_phase3_script(epic_id: int) -> str:
    """Generate Phase 3 script by copying Phase 2 pattern"""
    
    # Use single API key for all epics (same as Phase 2)
    api_key = API_KEY
    
    # CRITICAL: Copy Phase 2 pattern exactly, only change phase-specific parts
    # ASCII-ONLY: Replace Unicode characters with ASCII equivalents
    script = f"""#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/EPIC-CCN-{epic_id}
mkdir -p logs/phase3

cat > /tmp/phase3_msg_{epic_id}.txt << 'EOFMSG'
You are executing Phase 3 (DNA & PR Audit) for EPIC-CCN-{epic_id}.

**Context**: You have completed Phase 2 (Architecture Planning). The implementation plan exists in docs/brain/EPIC-CCN-{epic_id}/.

**Your Task**: Perform V12 DNA compliance checks and PR hygiene validation.

**Required Checks**:
1. Correctness by Construction (no invalid states possible)
2. Lock-Free Actor Pattern (zero locks, atomic operations only)
3. ASCII-Only Compliance (no Unicode/emoji in string literals)
4. Jane Street Alignment (cognitive simplicity, CYC <=8)
5. PR Hygiene (diff < 10k characters, single-file scope)
6. Thread Safety (H13-FIX pattern, atomic ops)
7. Exception Safety (robust error handling)
8. Deduplication (in-flight flag prevents duplicates)

**Output File**: docs/brain/EPIC-CCN-{epic_id}/03-audit-report.md

**Format**:
```markdown
# Phase 3: DNA & PR Audit - EPIC-CCN-{epic_id}

## Compliance Summary
| Principle | Status | Notes |
|-----------|--------|-------|
| Correctness by Construction | [OK]/[FAIL] | ... |
| Lock-Free Actor Pattern | [OK]/[FAIL] | ... |
...

## Adversarial Review
- Race conditions: ...
- Collection safety: ...
- Exception handling: ...

## Verdict
[OK] PASS / [FAIL] FAIL

## Next Phase
Phase 4 (Ticket Generation) / BLOCKED
```

**CRITICAL**: Use jCodemunch tools to analyze the implementation plan. Verify all V12 DNA principles.
EOFMSG

bob --yolo /epic-scan EPIC-CCN-{epic_id} 2>&1 | tee logs/phase3/EPIC-CCN-{epic_id}.log

echo "DONE_EXIT=$?"
"""
    
    return script

def generate_launcher_script() -> str:
    """Generate launcher script for all Phase 3 epics"""
    
    launcher = """#!/bin/bash
# Wave 3 Phase 3 Launcher
# Launches all 10 epics in parallel using screen sessions

set -e
cd /home/malhitticrypto/universal-or-strategy

echo "Starting Wave 3 Phase 3 (DNA & PR Audit) for 10 epics..."
echo "Estimated time: 10 minutes per epic (parallel execution)"

"""
    
    for epic_id in EPICS:
        launcher += f"""
echo "Launching EPIC-CCN-{epic_id} Phase 3..."
screen -dmS p3-{epic_id} bash -l -c './_p3_{epic_id}.sh 2>&1 | tee logs/phase3/EPIC-CCN-{epic_id}.log'
sleep 2
"""
    
    launcher += """
echo ""
echo "All Phase 3 sessions launched!"
echo "Monitor with: screen -ls"
echo "Attach to session: screen -r p3-116"
echo "Detach from session: Ctrl+A, then D"
echo ""
echo "Expected completion: 10-15 minutes"
echo "Verify completion: screen -ls (should show 'No Sockets found')"
"""
    
    return launcher

def main():
    """Generate all Phase 3 scripts"""
    
    print("Generating Wave 3 Phase 3 scripts...")
    print(f"Epics: {EPICS}")
    
    # Create output directory
    os.makedirs("scripts/wave3", exist_ok=True)
    
    # Generate individual epic scripts
    for epic_id in EPICS:
        script_content = generate_phase3_script(epic_id)
        script_path = f"scripts/wave3/_p3_{epic_id}.sh"
        
        with open(script_path, 'w', newline='\n') as f:
            f.write(script_content)
        
        print(f"  Created: {script_path}")
    
    # Generate launcher script
    launcher_content = generate_launcher_script()
    launcher_path = "scripts/wave3/launch_phase3_all_screen.sh"
    
    with open(launcher_path, 'w', newline='\n') as f:
        f.write(launcher_content)
    
    print(f"  Created: {launcher_path}")
    
    print("\nPhase 3 scripts generated successfully!")
    print("\nNext steps:")
    print("1. Upload scripts to VM:")
    print("   gcloud compute scp scripts/wave3/_p3_*.sh scripts/wave3/launch_phase3_all_screen.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a")
    print("2. Fix line endings and permissions:")
    print("   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command=\"cd /home/malhitticrypto/universal-or-strategy && for f in _p3_*.sh launch_phase3_all_screen.sh; do sed -i 's/\\r$//' \\$f; chmod +x \\$f; done\"")
    print("3. Launch Phase 3:")
    print("   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command=\"cd /home/malhitticrypto/universal-or-strategy && ./launch_phase3_all_screen.sh\"")
    print("4. Wait 10-15 minutes for completion")
    print("5. Run verification:")
    print("   .\\scripts\\verify_phase_completion.ps1 -Phase 3 -Epics 116,117,118,119,120,121,122,123,124,125")

if __name__ == "__main__":
    main()

# Made with Bob
