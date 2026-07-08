#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_3yjmERkNNh2XmujxzhYwLuTDZYpVFnfsGkprcHKjsQorxhwDPxnrVETB3RtXwBLc565zrsDcVKrCxsKB5uqWANpY_EJiS1xkNmY2hW2SowTmNRy5nd6HUiPiFkSRsCxpgLKuh'
mkdir -p docs/brain/EPIC-W7-108
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_108.txt)" 2>&1 | tee logs/phase0/EPIC-W7-108.log
echo "DONE_EXIT=$?"
