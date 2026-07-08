#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu'
mkdir -p docs/brain/EPIC-CCN-107
mkdir -p logs/phase5v

cat > /tmp/phase5v_msg_107_t5.txt << 'EOFMSG'
You are performing INDEPENDENT VALIDATION (Tier 2) for TICKET-5 of EPIC-CCN-107.

**Input**: Read `docs/brain/EPIC-CCN-107/ticket-5-completion.md` and original ticket spec

**Task**: Independent adversarial review of TICKET-5 implementation.

**Steps**: 1) Read completion report 2) Verify against spec 3) Run tests independently 4) Check quality 5) Provide PASS/FAIL verdict

**Output**: `docs/brain/EPIC-CCN-107/ticket-5-verification.md` with verdict and detailed findings

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 5.5.V (Independent Ticket Validation)
EOFMSG

bob --yolo --chat-mode advanced "$(cat /tmp/phase5v_msg_107_t5.txt)" 2>&1 | tee logs/phase5v/EPIC-CCN-107-T5-VALIDATION.log
echo "DONE_EXIT=$?"

# Made with Bob
