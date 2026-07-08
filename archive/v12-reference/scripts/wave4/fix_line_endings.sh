#!/bin/bash
# Fix CRLF line endings in regenerated Phase 6 scripts
set -e
cd /home/malhitticrypto/universal-or-strategy/scripts/wave4

echo "Fixing line endings for 4 regenerated scripts..."

for f in _p6_003.sh _p6_015.sh _p6_030.sh _p6_045.sh; do
    if [ -f "$f" ]; then
        sed -i 's/\r$//' "$f"
        echo "Fixed: $f"
    else
        echo "ERROR: $f not found"
    fi
done

echo ""
echo "Verification:"
file _p6_003.sh _p6_015.sh _p6_030.sh _p6_045.sh

echo ""
echo "Setting executable permissions..."
chmod +x _p6_003.sh _p6_015.sh _p6_030.sh _p6_045.sh

echo ""
echo "Done! Scripts ready for execution."

# Made with Bob
