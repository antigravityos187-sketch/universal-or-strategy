#!/bin/bash
# Building-Blocks Method: Copied from _p1_5_002.sh (successful pattern)
# Changes: phase1 -> phase2, scope -> architecture
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_c8SKNdvWX47LjEA1771m3PtSTg5Rd95DFurnpmpuoEEBD4Q1DAwe9UibFmH1wSeyL5u2MwZFGWDZPbbS5iPh8jC_ESknTx4s3SD4zbfW5Gu6sHTNPA5AYwSnsWy9uS5rkpKu'
mkdir -p docs/brain/EPIC-W7-105
mkdir -p logs/wave7/phase2

if [ ! -f "docs/brain/EPIC-W7-105/01-scope-boundary.md" ]; then
    echo "BLOCKED: Phase 1.5 not complete"
    exit 1
fi

cat > /tmp/phase2_msg_105.txt << 'EOFMSG'
Execute Phase 2 (Architecture Planning) for EPIC-W7-105.

CRITICAL FILE I/O PROTOCOL:
1. NEVER use write_to_file, read_file, or run_shell_command tools
2. ALWAYS use execute_command tool with cat > file
3. ALWAYS set cwd parameter to /home/malhitticrypto/universal-or-strategy

Input: docs/brain/EPIC-W7-105/01-scope-boundary.md

Required Actions:
1. Read scope boundary validation
2. Query Jane Street KB for extraction patterns: python scripts/query_kb.py "complexity reduction"
3. Design extraction architecture (method splitting, parameter reduction, FSM patterns)
4. Write docs/brain/EPIC-W7-105/02-architecture-plan.md using execute_command
5. Update manifest.json using execute_command
6. Verify both files exist using execute_command

Success Criteria:
- 02-architecture-plan.md exists and contains architecture design
- manifest.json updated to show phase 2 completed
EOFMSG

bob --yolo --chat-mode plan "$(cat /tmp/phase2_msg_105.txt)" 2>&1 | tee logs/wave7/phase2/EPIC-W7-105.log
echo "DONE_EXIT=$?"

# Made with Bob
