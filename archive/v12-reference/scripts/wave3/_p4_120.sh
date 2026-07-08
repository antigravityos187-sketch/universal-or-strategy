#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_5eZYFvHuinQHMnDWNZDZ7ciMX4oiUBsfkVyscGyoEahtNto1a7KNWHo5BFmoN4uPy8rbBYJrUsBtnshvB12nrYQJ_7tiXqEriChoWjAwta66uaZ76JKhxrqiQb6mR5C7AZQyo'
mkdir -p docs/brain/EPIC-CCN-120
mkdir -p logs/phase4

cat > /tmp/phase4_msg_120.txt << 'EOFMSG'
You are executing Phase 4 (Ticket Generation) for EPIC-CCN-120.

**Input Artifacts**: 
- Read `docs/brain/EPIC-CCN-120/02-architecture-plan.md` for extraction plan
- Read `docs/brain/EPIC-CCN-120/03-audit-report.md` for audit results

**Your Task**: Generate detailed implementation tickets for surgical extraction.

**Output Requirements**:
1. Create `docs/brain/EPIC-CCN-120/04-tickets.md` with:
   - Ticket breakdown (one ticket per extraction target)
   - Each ticket includes:
     * Method signature
     * Extraction steps (numbered, surgical)
     * Test requirements
     * Verification criteria
     * Estimated complexity reduction
   - Execution order (dependencies)
   - Success criteria per ticket

2. Update `docs/brain/EPIC-CCN-120/manifest.json`:
   - Set phase "4" status to "completed"
   - Add "04-tickets.md" to outputs

**MANDATORY REPORTING**:
After completing all tasks, you MUST report:
1. Bobcoins used this session: [X.XX]
2. Remaining balance in API key: [Y.YY]
Format: "Cost: X.XX | Balance: Y.YY"

**Critical Rules**:
- Use execute_command with printf for file creation (SSH-safe)
- Verify files exist with ls -lh before completion
- Each ticket must be independently executable
- Target complexity <= 8 per extracted method
- Include rollback steps for each ticket
- Verify no scope creep (single-method boundary)

**Phase**: 4 (Ticket Generation)
EOFMSG

bob --yolo --chat-mode plan "$(cat /tmp/phase4_msg_120.txt)" 2>&1 | tee logs/phase4/EPIC-CCN-120.log
echo "DONE_EXIT=$?"

# Made with Bob
