#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_yN7cbWSG9B926LkYPex4pXBGgTbZdN7Xg1ihASxzGdFGz7N8Z5WWDiqeWGUvsXiTWMzag9Hur9EA53BtXQRr2E4_4Z2YTW686zBchNH8KMgN69E3YGDzeRYcWMYxtKkxooeR'
mkdir -p docs/brain/EPIC-W7-030
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_030.txt)" 2>&1 | tee logs/phase0/EPIC-W7-030.log
echo "DONE_EXIT=$?"
