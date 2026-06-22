#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_5AhL7B8mdcv3xYTjzNDqWnK3WJvnHgAjfNh29jy7FsJ7VTpNM3j6AqoTouBQxoguDXHYzS5d6MPfBm7Qei19WA2y_2sR99qCwwuCZBB9rcWNUh9wLyg9frjYS6gW64BNavSoD'
mkdir -p docs/brain/EPIC-W7-092
mkdir -p logs/phase0

# Message file is created by Python generator, not bash heredoc
~/.npm-global/bin/bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_092.txt)" 2>&1 | tee logs/phase0/EPIC-W7-092.log
echo "DONE_EXIT=$?"
