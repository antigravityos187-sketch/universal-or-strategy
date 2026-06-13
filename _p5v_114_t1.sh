#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_65hPWuoJAPhLQKgnKSePPDiqS5YRKW1XDF1LM8kRporvu9XTpgAaY4WYvJgAe72VzRDARKEQzqzMei9UqCj28buk_2Astcnxpem897Pn91xpJXnKY6N7dMhDXAriwNtncfzsB'
mkdir -p docs/brain/EPIC-CCN-114
mkdir -p logs/phase5v

cat > /tmp/phase5v_msg_114_t1.txt << 'EOFMSG'
You are performing INDEPENDENT VALIDATION (Tier 2) for TICKET-1 of EPIC-CCN-114.

**Input**: Read `docs/brain/EPIC-CCN-114/ticket-1-completion.md` and original ticket spec

**Task**: Independent adversarial review of TICKET-1 implementation.

**Steps**: 1) Read completion report 2) Verify against spec 3) Run tests independently 4) Check quality 5) Provide PASS/FAIL verdict

**Output**: `docs/brain/EPIC-CCN-114/ticket-1-verification.md` with verdict and detailed findings

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 5.1.V (Independent Ticket Validation)
EOFMSG

bob --yolo --chat-mode advanced "$(cat /tmp/phase5v_msg_114_t1.txt)" 2>&1 | tee logs/phase5v/EPIC-CCN-114-T1-VALIDATION.log
echo "DONE_EXIT=$?"

# Made with Bob
