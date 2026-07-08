#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy

echo "Launching Wave 4 Phase 1 (Scope + Boundary) for 10 epics..."
echo "Start time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"


# Launch EPIC-CCN-126
chmod +x _p1_126.sh
screen -dmS "p1-126" bash -l "_p1_126.sh"
echo "Launched EPIC-CCN-126 in screen session p1-126"

# Launch EPIC-CCN-127
chmod +x _p1_127.sh
screen -dmS "p1-127" bash -l "_p1_127.sh"
echo "Launched EPIC-CCN-127 in screen session p1-127"

# Launch EPIC-CCN-128
chmod +x _p1_128.sh
screen -dmS "p1-128" bash -l "_p1_128.sh"
echo "Launched EPIC-CCN-128 in screen session p1-128"

# Launch EPIC-CCN-129
chmod +x _p1_129.sh
screen -dmS "p1-129" bash -l "_p1_129.sh"
echo "Launched EPIC-CCN-129 in screen session p1-129"

# Launch EPIC-CCN-130
chmod +x _p1_130.sh
screen -dmS "p1-130" bash -l "_p1_130.sh"
echo "Launched EPIC-CCN-130 in screen session p1-130"

# Launch EPIC-CCN-131
chmod +x _p1_131.sh
screen -dmS "p1-131" bash -l "_p1_131.sh"
echo "Launched EPIC-CCN-131 in screen session p1-131"

# Launch EPIC-CCN-132
chmod +x _p1_132.sh
screen -dmS "p1-132" bash -l "_p1_132.sh"
echo "Launched EPIC-CCN-132 in screen session p1-132"

# Launch EPIC-CCN-133
chmod +x _p1_133.sh
screen -dmS "p1-133" bash -l "_p1_133.sh"
echo "Launched EPIC-CCN-133 in screen session p1-133"

# Launch EPIC-CCN-134
chmod +x _p1_134.sh
screen -dmS "p1-134" bash -l "_p1_134.sh"
echo "Launched EPIC-CCN-134 in screen session p1-134"

# Launch EPIC-CCN-135
chmod +x _p1_135.sh
screen -dmS "p1-135" bash -l "_p1_135.sh"
echo "Launched EPIC-CCN-135 in screen session p1-135"

echo ""
echo "All 10 Phase 1 sessions launched!"
echo "Monitor with: screen -ls"
echo "Attach to session: screen -r p1-126"
echo "Check logs: tail -f logs/phase1/EPIC-CCN-126.log"
