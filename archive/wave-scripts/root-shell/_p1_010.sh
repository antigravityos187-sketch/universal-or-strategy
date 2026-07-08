#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_aRSjzM4xwaEhbcjDdViPqh3giwmvtQksbGerdHvRxq8MPyN2X7KHUU9q6H9DYDBj2YaJwhkgDci2HcT1gRbS9d6_9MHxQ1wMuJVJYeJG2gbRe4NCDCAdf2GBd4wKLhQMg1hS'
mkdir -p docs/brain/EPIC-W7-010
mkdir -p logs/wave7/phase1

if [ ! -f "docs/brain/EPIC-W7-010/00-hotspots.md" ]; then
    echo "BLOCKED: Phase 0 not complete"
    exit 1
fi

cat > /tmp/phase1_msg_010.txt << 'EOFMSG'
Execute Phase 1 (Scope Definition) for EPIC-W7-010.

CRITICAL FILE I/O PROTOCOL:
1. NEVER use write_to_file, read_file, or run_shell_command tools
2. ALWAYS use execute_command tool with cat > file
3. ALWAYS set cwd parameter to /home/malhitticrypto/universal-or-strategy

Input: docs/brain/EPIC-W7-010/00-hotspots.md

Required Actions:
1. Read hotspot analysis
2. Define extraction scope (IN SCOPE vs OUT OF SCOPE)
3. Write docs/brain/EPIC-W7-010/00-scope.md using execute_command
4. Update manifest.json using execute_command
5. Verify both files exist using execute_command

Success Criteria:
- 00-scope.md exists and contains scope definition
- manifest.json updated to show phase 1 completed
EOFMSG

bob --yolo --chat-mode v12-phase1-scope "$(cat /tmp/phase1_msg_010.txt)" 2>&1 | tee logs/wave7/phase1/EPIC-W7-010.log
echo "DONE_EXIT=$?"
