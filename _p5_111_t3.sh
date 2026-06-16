#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_5eZYFvHuinQHMnDWNZDZ7ciMX4oiUBsfkVyscGyoEahtNto1a7KNWHo5BFmoN4uPy8rbBYJrUsBtnshvB12nrYQJ_7tiXqEriChoWjAwta66uaZ76JKhxrqiQb6mR5C7AZQyo'
mkdir -p docs/brain/EPIC-CCN-111
mkdir -p logs/phase5

cat > /tmp/phase5_msg_111_t3.txt << 'EOFMSG'
You are executing TICKET-3 for EPIC-CCN-111.

**Input**: Read `docs/brain/EPIC-CCN-111/04-tickets.md`, locate TICKET-3

**Task**: Execute TICKET-3 with self-validation (Tier 1).

**Steps**: 1) Read ticket spec 2) Implement code 3) Write tests 4) Run tests 5) Self-validate 6) Create completion report

**Output**: `docs/brain/EPIC-CCN-111/ticket-3-completion.md` with self-validation results

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 5.3 (Ticket Execution + Self-Validation)
EOFMSG

bob --yolo --chat-mode v12-engineer "$(cat /tmp/phase5_msg_111_t3.txt)" 2>&1 | tee logs/phase5/EPIC-CCN-111-T3.log
echo "DONE_EXIT=$?"

# Made with Bob
