#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_34gay3JrSM5CqZF7cg5BjDGDwEk7ZLQdBXUjWdQH9vnaSM6YKaigEytQDQXSygmGqEEXHm7qiLKLupwdhK5DAQp4_61R5yxHVTtKmgDRRR9mcSxJ1HBAPdYnzLcY9utoNmrfo'
mkdir -p docs/brain/EPIC-W7-087
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_087.txt)" 2>&1 | tee logs/phase0/EPIC-W7-087.log
echo "DONE_EXIT=$?"
