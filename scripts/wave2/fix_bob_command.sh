#!/bin/bash
# Fix Bob CLI command syntax in all Phase 0 scripts

cd /home/malhitticrypto/universal-or-strategy

for i in 107 108 109 110 111 112 113 114 115; do
  echo "Fixing _p0_$i.sh..."
  
  # Replace incorrect Bob command with correct syntax
  sed -i 's/bob --mode advanced --message "$(cat \/tmp\/phase0_msg_'$i'.txt)"/bob --chat-mode advanced "$(cat \/tmp\/phase0_msg_'$i'.txt)"/' _p0_$i.sh
  
done

echo "All scripts fixed!"
echo "Correct syntax: bob --chat-mode advanced \"prompt text\""

# Made with Bob
