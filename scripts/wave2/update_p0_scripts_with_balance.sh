#!/bin/bash
# Update all Phase 0 scripts to request bobcoin usage AND remaining balance

cd /home/malhitticrypto/universal-or-strategy

for epic_id in 107 108 109 110 111 112 113 114 115; do
  script="_p0_${epic_id}.sh"
  
  # Add bobcoin reporting before DONE_EXIT
  sed -i '/echo "DONE_EXIT=\$?"/i \
\
# Report bobcoin usage and remaining balance\
echo ""\
echo "=== BOBCOIN REPORT ==="\
echo "Please report:"\
echo "1. Bobcoins used this session"\
echo "2. Remaining balance in your API key"\
echo "Format: Used: X.XX bobcoins | Remaining: Y.YY bobcoins"\
echo "======================"' "$script"
  
  echo "[OK] Updated $script"
done

echo ""
echo "All scripts updated with bobcoin reporting"

# Made with Bob
