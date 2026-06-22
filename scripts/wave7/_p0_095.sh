#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_2RC6JDaVuiwh9Ag5xuFucgJo81gJW3KZQp3yumcVfpCkY9hCvZhhvaGzx6KiuWtXqNJamkoDzdNLxUEAN3MjbCXp_9zESTyeEwLZJ1y7apWYhu24fmp1gc84qcCEsGn4iJo6S'
mkdir -p docs/brain/EPIC-W7-095
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_095.txt)" 2>&1 | tee logs/phase0/EPIC-W7-095.log
echo "DONE_EXIT=$?"
