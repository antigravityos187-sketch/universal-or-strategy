#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_V8sa2xf9tLezoczf9f7WZADcMhiUphzZPhDfRiMwx82Wxo1VtH3KMprtBvQFAmRYgECy254WHMSeWFxAuzBGzLj_2SQz2BrZKRs3WsotGTN56eL2Gthg4voAhcMZeefDi7wp'
mkdir -p docs/brain/EPIC-CCN-108
mkdir -p logs/phase5v

cat > /tmp/phase5v_msg_108_t2.txt << 'EOFMSG'
You are performing INDEPENDENT VALIDATION (Tier 2) for TICKET-2 of EPIC-CCN-108.

**Input**: Read `docs/brain/EPIC-CCN-108/ticket-2-completion.md` and original ticket spec

**Task**: Independent adversarial review of TICKET-2 implementation.

**Steps**: 1) Read completion report 2) Verify against spec 3) Run tests independently 4) Check quality 5) Provide PASS/FAIL verdict

**Output**: `docs/brain/EPIC-CCN-108/ticket-2-verification.md` with verdict and detailed findings

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 5.2.V (Independent Ticket Validation)
EOFMSG

bob --yolo --chat-mode advanced "$(cat /tmp/phase5v_msg_108_t2.txt)" 2>&1 | tee logs/phase5v/EPIC-CCN-108-T2-VALIDATION.log
echo "DONE_EXIT=$?"

# Made with Bob
