#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_t9tV9fuaYCkKYJNm5xCaHWAAR5yJT59mUXoLRHLyb3G4uVHazEQaFacXSz2Nd9Pij2WYNHkvn7THr5amYPqQeDa_ASoyvBNoW8FE2m47D2fhv67cbYGy7TXVeWYswv5N1MNF'
mkdir -p docs/brain/EPIC-CCN-109
mkdir -p logs/phase5v

cat > /tmp/phase5v_msg_109_t3.txt << 'EOFMSG'
You are performing INDEPENDENT VALIDATION (Tier 2) for TICKET-3 of EPIC-CCN-109.

**Input**: Read `docs/brain/EPIC-CCN-109/ticket-3-completion.md` and original ticket spec

**Task**: Independent adversarial review of TICKET-3 implementation.

**Steps**: 1) Read completion report 2) Verify against spec 3) Run tests independently 4) Check quality 5) Provide PASS/FAIL verdict

**Output**: `docs/brain/EPIC-CCN-109/ticket-3-verification.md` with verdict and detailed findings

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 5.3.V (Independent Ticket Validation)
EOFMSG

bob --yolo --chat-mode advanced "$(cat /tmp/phase5v_msg_109_t3.txt)" 2>&1 | tee logs/phase5v/EPIC-CCN-109-T3-VALIDATION.log
echo "DONE_EXIT=$?"

# Made with Bob
