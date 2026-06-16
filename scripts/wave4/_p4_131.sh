#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_3abxQUhB6oz3484pgXxkjkeZEXxTEJfFGwg4D5cY6GWrCXFjT6uUQhvtLz5n8dB5g9Pue31DVuLwR9wa34zrBNmT_DdGCwiky7h1JVUEzJZVTrDxZNUigAnSRPPdUEJNzeLZT'
mkdir -p docs/brain/EPIC-CCN-131
mkdir -p logs/phase4

bob --yolo /epic-tickets EPIC-CCN-131 2>&1 | tee logs/phase4/EPIC-CCN-131.log
echo "DONE_EXIT=$?"

# Made with Bob
