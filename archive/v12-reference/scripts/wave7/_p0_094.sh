#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_aRSjzM4xwaEhbcjDdViPqh3giwmvtQksbGerdHvRxq8MPyN2X7KHUU9q6H9DYDBj2YaJwhkgDci2HcT1gRbS9d6_9MHxQ1wMuJVJYeJG2gbRe4NCDCAdf2GBd4wKLhQMg1hS'
mkdir -p docs/brain/EPIC-W7-094
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_094.txt)" 2>&1 | tee logs/phase0/EPIC-W7-094.log
echo "DONE_EXIT=$?"
