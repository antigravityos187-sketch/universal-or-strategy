#!/bin/bash
# Restore backups and re-apply Python fix to all scripts
set -e
cd /home/malhitticrypto/universal-or-strategy

echo "Restoring backups for first 20 scripts..."
for i in 001 002 003 004 005 006 007 008 009 010 011 012 013 014 015 017 018 019 020 021; do
    if [ -f "scripts/wave4/_p6_${i}.sh.bak" ]; then
        cp "scripts/wave4/_p6_${i}.sh.bak" "scripts/wave4/_p6_${i}.sh"
        echo "Restored: _p6_${i}.sh"
    fi
done

echo ""
echo "Re-applying Python fix to all scripts..."
python3 scripts/wave4/fix_phase6_scripts.py

echo ""
echo "✅ All scripts fixed and ready for recovery launch"

# Made with Bob
