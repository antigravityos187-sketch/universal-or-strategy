#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_65hPWuoJAPhLQKgnKSePPDiqS5YRKW1XDF1LM8kRporvu9XTpgAaY4WYvJgAe72VzRDARKEQzqzMei9UqCj28buk_2Astcnxpem897Pn91xpJXnKY6N7dMhDXAriwNtncfzsB'
mkdir -p docs/brain/EPIC-CCN-133
mkdir -p logs/phase4

bob --yolo /epic-tickets EPIC-CCN-133 2>&1 | tee logs/phase4/EPIC-CCN-133.log
echo "DONE_EXIT=$?"

# Made with Bob
