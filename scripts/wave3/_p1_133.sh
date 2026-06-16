#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu'
mkdir -p docs/brain/EPIC-CCN-133
mkdir -p logs/phase1

bob --yolo /epic-intake EPIC-CCN-133 2>&1 | tee logs/phase1/EPIC-CCN-133.log
echo "DONE_EXIT=$?"

# Made with Bob
