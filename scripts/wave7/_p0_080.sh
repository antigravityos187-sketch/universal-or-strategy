#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_58sbZY3cBGWbej6dmAvwYeRTKyuceZJppgm4vYoS7bb2yzKqFxwAmzsR46D6G86LVJWNBmsUaZLBpMgRpiZyPQDf_GpwXFSKRi7nWHCJP2m1S6guZ1Y4kzUKBR9C1mrkKQm3s'
mkdir -p docs/brain/EPIC-W7-080
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_080.txt)" 2>&1 | tee logs/phase0/EPIC-W7-080.log
echo "DONE_EXIT=$?"
