#!/bin/bash
# Recovery launch for failed Phase 5 epics
# Fixed: Use bash -l (login shell) to source .bashrc and set PATH

cd /home/malhitticrypto/universal-or-strategy

echo "[$(date)] Starting Phase 5 recovery (2 epics)"

# Failed epic IDs
FAILED_EPICS=("016" "045")

for epic in "${FAILED_EPICS[@]}"; do
    echo "[$(date)] Launching recovery for EPIC-CCN-$epic"
    
    # Launch in screen session with login shell (-l flag)
    screen -dmS "p5-recovery-$epic" bash -l -c \
        "./scripts/wave4/_p5_$epic.sh 2>&1 | tee logs/phase5/EPIC-CCN-$epic-recovery.log"
    
    sleep 12
done

echo "[$(date)] Recovery launch complete (2 epics)"
echo "Monitor with: screen -ls"
echo "Check files: ls docs/brain/EPIC-CCN-{016,045}/*completion*.md"

# Made with Bob
