#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_3vzs4jptuwZ7Z63gqpyn3aNy89ozwWyanh2aNB7TQDa22rfmiRJXWCUivJphxYNLAoT8nJMEYmUxaTgWA5Z8URUd_F6U16mpCReKejNsSHgrd7VxPEHuX8sedjJm4hrV7srcQ'
mkdir -p docs/brain/EPIC-CCN-113
mkdir -p logs/phase6

cat > /tmp/phase6_msg_113.txt << 'EOFMSG'
You are performing EPIC-LEVEL REVIEW (Tier 3) for EPIC-CCN-113.

**Input**: Read all ticket verification reports and completion reports

**Task**: Review entire epic (5 tickets) for integration, consistency, and overall quality.

**Steps**: 1) Verify all tickets passed 2) Check integration 3) Verify architecture 4) Run full test suite 5) Provide final verdict

**Output**: `docs/brain/EPIC-CCN-113/05-completion-report.md` with epic verdict

**MANDATORY REPORTING**: Cost: X.XX | Balance: Y.YY

**Phase**: 6 (Epic-Level Review)
EOFMSG

bob --yolo --chat-mode advanced "$(cat /tmp/phase6_msg_113.txt)" 2>&1 | tee logs/phase6/EPIC-CCN-113.log
echo "DONE_EXIT=$?"

# Made with Bob
