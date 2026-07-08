#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_2am9d3VjQYnC4mSub1z5SzdSZJeyptWhfMrxGeEBSorZRPj8WmQvBPtTf8qTpjWHWdRuf7toP2WTDtPEfS6aoTYF_7ufADbTYhnLEY42csrSet3f3ssJuNddPhXD65YewpCWX'
mkdir -p docs/brain/EPIC-CCN-129
mkdir -p logs/phase4

bob --yolo /epic-tickets EPIC-CCN-129 2>&1 | tee logs/phase4/EPIC-CCN-129.log
echo "DONE_EXIT=$?"

# Made with Bob
