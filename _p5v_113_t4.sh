#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_3vzs4jptuwZ7Z63gqpyn3aNy89ozwWyanh2aNB7TQDa22rfmiRJXWCUivJphxYNLAoT8nJMEYmUxaTgWA5Z8URUd_F6U16mpCReKejNsSHgrd7VxPEHuX8sedjJm4hrV7srcQ'
mkdir -p docs/brain/EPIC-CCN-113
mkdir -p logs/phase5v

cat > /tmp/phase5v_msg_113_t4.txt << 'EOFMSG'
You are performing INDEPENDENT VALIDATION (Tier 2) for TICKET-4 of EPIC-CCN-113.

**Input**: Read `docs/brain/EPIC-CCN-113/ticket-4-completion.md` and original ticket spec

**Task**: Independent adversarial review of TICKET-4 implementation.

**Steps**: 1) Read completion report 2) Verify against spec 3) Run tests independently 4) Check quality 5) Provide PASS/FAIL verdict

**Output**: `docs/brain/EPIC-CCN-113/ticket-4-verification.md` with verdict and detailed findings

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 5.4.V (Independent Ticket Validation)
EOFMSG

bob --yolo --chat-mode advanced "$(cat /tmp/phase5v_msg_113_t4.txt)" 2>&1 | tee logs/phase5v/EPIC-CCN-113-T4-VALIDATION.log
echo "DONE_EXIT=$?"

# Made with Bob
