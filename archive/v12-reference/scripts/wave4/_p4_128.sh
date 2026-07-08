#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_t9tV9fuaYCkKYJNm5xCaHWAAR5yJT59mUXoLRHLyb3G4uVHazEQaFacXSz2Nd9Pij2WYNHkvn7THr5amYPqQeDa_ASoyvBNoW8FE2m47D2fhv67cbYGy7TXVeWYswv5N1MNF'
mkdir -p docs/brain/EPIC-CCN-128
mkdir -p logs/phase4

bob --yolo /epic-tickets EPIC-CCN-128 2>&1 | tee logs/phase4/EPIC-CCN-128.log
echo "DONE_EXIT=$?"

# Made with Bob
