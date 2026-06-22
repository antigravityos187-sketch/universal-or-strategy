#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_2FTTdxZo3mEs7ek4rbpBLVdpkTinfTdgG6Zj2CK9D2A7ct7TwUi1CyQSaHwqEozi9npR6Go4BLkBzAyxQzaWpaii_B1y7Ji37WbeKFZgREwNCqjQEJCdzqfhwpCN9Rfa1BiMN'
mkdir -p docs/brain/EPIC-W7-109
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_109.txt)" 2>&1 | tee logs/phase0/EPIC-W7-109.log
echo "DONE_EXIT=$?"
