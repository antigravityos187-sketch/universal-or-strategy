#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_5eZYFvHuinQHMnDWNZDZ7ciMX4oiUBsfkVyscGyoEahtNto1a7KNWHo5BFmoN4uPy8rbBYJrUsBtnshvB12nrYQJ_7tiXqEriChoWjAwta66uaZ76JKhxrqiQb6mR5C7AZQyo'
mkdir -p docs/brain/EPIC-CCN-111
mkdir -p logs/phase5v

cat > /tmp/phase5v_msg_111_t3.txt << 'EOFMSG'
You are performing INDEPENDENT VALIDATION (Tier 2) for TICKET-3 of EPIC-CCN-111.

**Input**: Read `docs/brain/EPIC-CCN-111/ticket-3-completion.md` and original ticket spec

**Task**: Independent adversarial review of TICKET-3 implementation.

**Steps**: 1) Read completion report 2) Verify against spec 3) Run tests independently 4) Check quality 5) Provide PASS/FAIL verdict

**Output**: `docs/brain/EPIC-CCN-111/ticket-3-verification.md` with verdict and detailed findings

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 5.3.V (Independent Ticket Validation)
EOFMSG

bob --yolo --chat-mode advanced "$(cat /tmp/phase5v_msg_111_t3.txt)" 2>&1 | tee logs/phase5v/EPIC-CCN-111-T3-VALIDATION.log
echo "DONE_EXIT=$?"

# Made with Bob
