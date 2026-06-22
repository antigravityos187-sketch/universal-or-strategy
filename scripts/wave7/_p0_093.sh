#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_22HihoispYfg9TBX2smEUD6b18c9zwRRFnssCBDwTquLYiH4bvHLkntgiNVgt5DZtcfSUqE7LDbMBJxrb9W6cCQc_AKDtJW7uTVi1ZpoDxktCmz2WNcvi2REwiTe88STW7h4J'
mkdir -p docs/brain/EPIC-W7-093
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_093.txt)" 2>&1 | tee logs/phase0/EPIC-W7-093.log
echo "DONE_EXIT=$?"
