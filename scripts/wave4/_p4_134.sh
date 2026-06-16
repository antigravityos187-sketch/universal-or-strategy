#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_5A6hXsy7FL4vf9T2jqr11gdYTmAZcFgxVm1dGD9qGPmpD5fV6emRy6XYzZPsqw56mjCtoiEbJmLU8B2VL4ZtgXeS_ALp1DF9sj3R3cU3dzddRRAVu44Y52VHhkt1BNkSdC2Nq'
mkdir -p docs/brain/EPIC-CCN-134
mkdir -p logs/phase4

bob --yolo /epic-tickets EPIC-CCN-134 2>&1 | tee logs/phase4/EPIC-CCN-134.log
echo "DONE_EXIT=$?"

# Made with Bob
