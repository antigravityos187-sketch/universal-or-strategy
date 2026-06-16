#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_t9tV9fuaYCkKYJNm5xCaHWAAR5yJT59mUXoLRHLyb3G4uVHazEQaFacXSz2Nd9Pij2WYNHkvn7THr5amYPqQeDa_ASoyvBNoW8FE2m47D2fhv67cbYGy7TXVeWYswv5N1MNF'
mkdir -p docs/brain/EPIC-CCN-109
mkdir -p logs/phase6

cat > /tmp/phase6_msg_109.txt << 'EOFMSG'
You are performing EPIC-LEVEL REVIEW (Tier 3) for EPIC-CCN-109.

**Input**: Read all ticket verification reports and completion reports

**Task**: Review entire epic (4 tickets) for integration, consistency, and overall quality.

**Steps**: 1) Verify all tickets passed 2) Check integration 3) Verify architecture 4) Run full test suite 5) Provide final verdict

**Output**: `docs/brain/EPIC-CCN-109/05-completion-report.md` with epic verdict

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 6 (Epic-Level Review)
EOFMSG

bob --yolo --chat-mode advanced "$(cat /tmp/phase6_msg_109.txt)" 2>&1 | tee logs/phase6/EPIC-CCN-109.log
echo "DONE_EXIT=$?"

# Made with Bob
