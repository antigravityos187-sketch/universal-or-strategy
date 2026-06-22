#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_4tdFq99zrsvGGgqpLmsaDid9QqycnQT74EtvTFttZpWcJdWW5L3VEQuCTsQxM1GTWDCd8HWkPW9jcWPFqYp5hW9v_8TSHVEQRkt3DbE6zuqMQHoajMzLtuUUYdUxTxSrofQMg'
mkdir -p docs/brain/EPIC-W7-064
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_064.txt)" 2>&1 | tee logs/phase0/EPIC-W7-064.log
echo "DONE_EXIT=$?"
