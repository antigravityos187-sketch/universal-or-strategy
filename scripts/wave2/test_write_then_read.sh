#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu'
mkdir -p docs/brain/EPIC-CCN-107

cat > /tmp/test_write_read_msg.txt << 'EOFMSG'
Test write_to_file followed by read_file.

TASK:
1. Use write_to_file to create docs/brain/EPIC-CCN-107/test.md with content:
   "# Test File\nThis is a test.\nLine 3."
2. Immediately use read_file to read docs/brain/EPIC-CCN-107/test.md
3. Report if read_file succeeded or failed

CRITICAL: Use write_to_file tool (not run_shell_command).
EOFMSG

bob --chat-mode v12-phase0-hotspot "$(cat /tmp/test_write_read_msg.txt)"
echo "TEST_EXIT=$?"

# Made with Bob
