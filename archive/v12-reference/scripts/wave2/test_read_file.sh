#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu'

cat > /tmp/test_read_msg.txt << 'EOFMSG'
Test read_file tool with relative path.

TASK: Use read_file tool to read the file at:
docs/brain/EPIC-CCN-107/00-hotspots.md

Show the first 20 lines of the file content.
EOFMSG

bob --chat-mode v12-phase0-hotspot "$(cat /tmp/test_read_msg.txt)"
echo "TEST_EXIT=$?"

# Made with Bob
