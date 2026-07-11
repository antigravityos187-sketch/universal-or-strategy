#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_c8SKNdvWX47LjEA1771m3PtSTg5Rd95DFurnpmpuoEEBD4Q1DAwe9UibFmH1wSeyL5u2MwZFGWDZPbbS5iPh8jC_ESknTx4s3SD4zbfW5Gu6sHTNPA5AYwSnsWy9uS5rkpKu'
mkdir -p docs/brain/EPIC-W7-091
mkdir -p logs/wave7/phase1

if [ ! -f "docs/brain/EPIC-W7-091/00-hotspots.md" ]; then
    echo "BLOCKED: Phase 0 not complete"
    exit 1
fi

cat > /tmp/phase1_msg_091.txt << 'EOFMSG'
Execute Phase 1 (Scope Definition) for EPIC-W7-091.

CRITICAL FILE I/O PROTOCOL:
1. NEVER use write_to_file, read_file, or run_shell_command tools
2. ALWAYS use execute_command tool with cat > file
3. ALWAYS set cwd parameter to /home/malhitticrypto/universal-or-strategy

Input: docs/brain/EPIC-W7-091/00-hotspots.md

Required Actions:
1. Read hotspot analysis
2. Define extraction scope (IN SCOPE vs OUT OF SCOPE)
3. Write docs/brain/EPIC-W7-091/00-scope.md using execute_command
4. Update manifest.json using execute_command
5. Verify both files exist using execute_command

Success Criteria:
- 00-scope.md exists with IN SCOPE and OUT OF SCOPE sections
- manifest.json updated with phase1 status
- No use of write_to_file or read_file tools

EOFMSG

~/.npm-global/bin/bob --yolo --chat-mode v12-phase1-scope "$(cat /tmp/phase1_msg_091.txt)"
DONE_EXIT=$?
echo "Phase 1 complete for EPIC-W7-091 (exit: $DONE_EXIT)"
exit $DONE_EXIT
