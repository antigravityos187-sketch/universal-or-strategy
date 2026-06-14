#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_44TtZXuuACpNu133KVpJ7nSGsRr8hhdVUJj3h3jYe5MUk44L1xm6bUAbv5WDab98VadJx53pvp1Kdxmch4E4Qh1H_7J5ULr6U54NC12M2tpGVD6FWjmjk5rgZWcDie42W6mRh'
mkdir -p docs/brain/EPIC-CCN-135
mkdir -p logs/phase4

bob --yolo /epic-tickets EPIC-CCN-135 2>&1 | tee logs/phase4/EPIC-CCN-135.log
echo "DONE_EXIT=$?"

# Made with Bob
