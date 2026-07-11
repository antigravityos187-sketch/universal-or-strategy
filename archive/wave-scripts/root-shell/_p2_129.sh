#!/bin/bash
# Building-Blocks Method: Copied from _p1_5_002.sh (successful pattern)
# Changes: phase1 -> phase2, scope -> architecture
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_hXrAgTSbvVHGpmUxqohSv8SirgVeXpEBSoF1wyb8xBQz2PBMmgyKfozT2kJP5RuCbqBVsW91Z6pp8auuGBjb53P_2qJE8WD7whnc8nYcahv4AevV8ekHvP1K4zbYggeoSvZ2'
mkdir -p docs/brain/EPIC-W7-129
mkdir -p logs/wave7/phase2

if [ ! -f "docs/brain/EPIC-W7-129/01-scope-boundary.md" ]; then
    echo "BLOCKED: Phase 1.5 not complete"
    exit 1
fi

cat > /tmp/phase2_msg_129.txt << 'EOFMSG'
Execute Phase 2 (Architecture Planning) for EPIC-W7-129.

CRITICAL FILE I/O PROTOCOL:
1. NEVER use write_to_file, read_file, or run_shell_command tools
2. ALWAYS use execute_command tool with cat > file
3. ALWAYS set cwd parameter to /home/malhitticrypto/universal-or-strategy

Input: docs/brain/EPIC-W7-129/01-scope-boundary.md

Required Actions:
1. Read scope boundary validation
2. Query Jane Street KB for extraction patterns: python scripts/query_kb.py "complexity reduction"
3. Design extraction architecture (method splitting, parameter reduction, FSM patterns)
4. Write docs/brain/EPIC-W7-129/02-architecture-plan.md using execute_command
5. Update manifest.json using execute_command
6. Verify both files exist using execute_command

Success Criteria:
- 02-architecture-plan.md exists and contains architecture design
- manifest.json updated to show phase 2 completed
EOFMSG

bob --yolo --chat-mode plan "$(cat /tmp/phase2_msg_129.txt)" 2>&1 | tee logs/wave7/phase2/EPIC-W7-129.log
echo "DONE_EXIT=$?"

# Made with Bob
