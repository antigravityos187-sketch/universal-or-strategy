#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_3abxQUhB6oz3484pgXxkjkeZEXxTEJfFGwg4D5cY6GWrCXFjT6uUQhvtLz5n8dB5g9Pue31DVuLwR9wa34zrBNmT_DdGCwiky7h1JVUEzJZVTrDxZNUigAnSRPPdUEJNzeLZT'
mkdir -p docs/brain/EPIC-CCN-112
mkdir -p logs/phase5

cat > /tmp/phase5_msg_112_t1.txt << 'EOFMSG'
You are executing TICKET-1 for EPIC-CCN-112.

**Input**: Read `docs/brain/EPIC-CCN-112/04-tickets.md`, locate TICKET-1

**Task**: Execute TICKET-1 with self-validation (Tier 1).

**Steps**: 1) Read ticket spec 2) Implement code 3) Write tests 4) Run tests 5) Self-validate 6) Create completion report

**Output**: `docs/brain/EPIC-CCN-112/ticket-1-completion.md` with self-validation results

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 5.1 (Ticket Execution + Self-Validation)
EOFMSG

bob --yolo --chat-mode v12-engineer "$(cat /tmp/phase5_msg_112_t1.txt)" 2>&1 | tee logs/phase5/EPIC-CCN-112-T1.log
echo "DONE_EXIT=$?"

# Made with Bob
