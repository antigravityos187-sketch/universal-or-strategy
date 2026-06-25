#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_5PbEKjG9uqQAMCizgY3pPmDxRFiTW6XJvBvp3RDcYGbT6vk5tE8FJmLUCWsT1WfH6nn7WYoxMHrXe3GfhFo8LPgi_5dYLFXNSc2SBcHL9ZrQXwxj1XrvbotdUsWJXVTv13u2a'
mkdir -p docs/brain/EPIC-W7-117
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_117.txt)" 2>&1 | tee logs/phase0/EPIC-W7-117.log
echo "DONE_EXIT=$?"
