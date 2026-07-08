#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_4tdFq99zrsvGGgqpLmsaDid9QqycnQT74EtvTFttZpWcJdWW5L3VEQuCTsQxM1GTWDCd8HWkPW9jcWPFqYp5hW9v_8TSHVEQRkt3DbE6zuqMQHoajMzLtuUUYdUxTxSrofQMg'
mkdir -p docs/brain/EPIC-W7-158
mkdir -p logs/wave7/phase1

if [ ! -f "docs/brain/EPIC-W7-158/00-hotspots.md" ]; then
    echo "BLOCKED: Phase 0 not complete"
    exit 1
fi

cat > /tmp/phase1_5_msg_001.txt << 'EOFMSG'
Execute Phase 1 (Scope Definition) for EPIC-W7-158.

CRITICAL FILE I/O PROTOCOL:
1. NEVER use write_to_file, read_file, or run_shell_command tools
2. ALWAYS use execute_command tool with cat > file
3. ALWAYS set cwd parameter to /home/malhitticrypto/universal-or-strategy

Input: docs/brain/EPIC-W7-158/00-hotspots.md

Required Actions:
1. Read hotspot analysis
2. Define extraction scope (IN SCOPE vs OUT OF SCOPE)
3. Write docs/brain/EPIC-W7-158/01-scope-boundary.md using execute_command
4. Update manifest.json using execute_command
5. Verify both files exist using execute_command

Success Criteria:
- 01-scope-boundary.md exists and contains scope definition
- manifest.json updated to show phase 1 completed
EOFMSG

bob --yolo --chat-mode v12-phase1-scope "$(cat /tmp/phase1_5_msg_001.txt)" 2>&1 | tee logs/wave7/phase1/EPIC-W7-158.log
echo "DONE_EXIT=$?"
