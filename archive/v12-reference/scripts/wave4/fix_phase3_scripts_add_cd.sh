#!/bin/bash
# Fix Phase 3 scripts: Add missing 'cd' command
# Root Cause: Bob Shell can't find .bob/mcp.linux.json without cd to project dir

set -e

echo "Fixing Phase 3 scripts: Adding 'cd' command..."

for script in scripts/wave4/_p3_*.sh; do
    if [ -f "$script" ]; then
        # Check if script already has 'cd' command
        if ! grep -q "^cd /home/malhitticrypto/universal-or-strategy" "$script"; then
            echo "Fixing: $script"
            
            # Create temp file with cd command inserted after shebang and set -e
            awk '
                /^set -e$/ {
                    print
                    print "cd /home/malhitticrypto/universal-or-strategy"
                    next
                }
                {print}
            ' "$script" > "$script.tmp"
            
            # Replace original
            mv "$script.tmp" "$script"
            chmod +x "$script"
        else
            echo "Already fixed: $script"
        fi
    fi
done

echo "✅ All Phase 3 scripts fixed!"
echo ""
echo "Next steps:"
echo "1. Upload fixed scripts to VM"
echo "2. Re-run pilot test with EPIC-CCN-001, EPIC-CCN-002"
echo "3. If pilot succeeds, re-run 62 failed epics"

# Made with Bob
