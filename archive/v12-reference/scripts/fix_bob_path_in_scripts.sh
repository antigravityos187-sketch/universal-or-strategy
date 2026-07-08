#!/bin/bash
# Fix bob path in all Phase 5 execution and validation scripts

set -e
cd /home/malhitticrypto/universal-or-strategy

BOB_FULL_PATH="/home/malhitticrypto/.npm-global/bin/bob"

echo "=== Fixing bob path in Phase 5 scripts ==="
echo "Started: $(date)"
echo ""

# Find all _p5_*.sh and _p5v_*.sh scripts
for script in _p5_*.sh _p5v_*.sh _p6_*.sh; do
    if [ -f "$script" ]; then
        echo "Processing: $script"
        # Replace 'bob ' with full path (preserving arguments)
        sed -i "s|^bob |$BOB_FULL_PATH |g" "$script"
        sed -i "s| bob | $BOB_FULL_PATH |g" "$script"
        echo "  ✅ Fixed"
    fi
done

echo ""
echo "=== Verification ==="
echo "Checking a sample script:"
grep "bob " _p5_108_t2.sh || echo "No 'bob ' found (good - should be full path now)"
grep "$BOB_FULL_PATH" _p5_108_t2.sh | head -1

echo ""
echo "=== Complete ==="
echo "Finished: $(date)"

# Made with Bob
