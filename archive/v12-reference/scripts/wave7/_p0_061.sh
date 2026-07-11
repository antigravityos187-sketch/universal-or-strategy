#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_c8SKNdvWX47LjEA1771m3PtSTg5Rd95DFurnpmpuoEEBD4Q1DAwe9UibFmH1wSeyL5u2MwZFGWDZPbbS5iPh8jC_ESknTx4s3SD4zbfW5Gu6sHTNPA5AYwSnsWy9uS5rkpKu'
mkdir -p docs/brain/EPIC-W7-061
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_061.txt)" 2>&1 | tee logs/phase0/EPIC-W7-061.log
echo "DONE_EXIT=$?"
