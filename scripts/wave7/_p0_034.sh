#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_44TtZXuuACpNu133KVpJ7nSGsRr8hhdVUJj3h3jYe5MUk44L1xm6bUAbv5WDab98VadJx53pvp1Kdxmch4E4Qh1H_7J5ULr6U54NC12M2tpGVD6FWjmjk5rgZWcDie42W6mRh'
mkdir -p docs/brain/EPIC-W7-034
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_034.txt)" 2>&1 | tee logs/phase0/EPIC-W7-034.log
echo "DONE_EXIT=$?"
