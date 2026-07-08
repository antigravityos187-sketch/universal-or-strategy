#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_V8sa2xf9tLezoczf9f7WZADcMhiUphzZPhDfRiMwx82Wxo1VtH3KMprtBvQFAmRYgECy254WHMSeWFxAuzBGzLj_2SQz2BrZKRs3WsotGTN56eL2Gthg4voAhcMZeefDi7wp'
mkdir -p docs/brain/EPIC-CCN-108
mkdir -p logs/phase5

cat > /tmp/phase5_msg_108_t4.txt << 'EOFMSG'
You are executing TICKET-4 for EPIC-CCN-108.

**Input**: Read `docs/brain/EPIC-CCN-108/04-tickets.md`, locate TICKET-4

**Task**: Execute TICKET-4 with self-validation (Tier 1).

**Steps**: 1) Read ticket spec 2) Implement code 3) Write tests 4) Run tests 5) Self-validate 6) Create completion report

**Output**: `docs/brain/EPIC-CCN-108/ticket-4-completion.md` with self-validation results

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 5.4 (Ticket Execution + Self-Validation)
EOFMSG

bob --yolo --chat-mode v12-engineer "$(cat /tmp/phase5_msg_108_t4.txt)" 2>&1 | tee logs/phase5/EPIC-CCN-108-T4.log
echo "DONE_EXIT=$?"

# Made with Bob
