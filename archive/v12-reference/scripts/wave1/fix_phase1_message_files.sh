#!/bin/bash
# Fix message file numbers in Phase 1 scripts
# Each script should use its own message file number

set -e
cd "$(dirname "$0")"

echo "Fixing message file numbers in Phase 1 scripts..."
echo ""

# Fix each script to use correct message file number
for i in $(seq -w 1 15); do
    script="_p1_${i}.sh"
    
    if [ -f "$script" ]; then
        # Replace phase1_msg_003.txt with phase1_msg_XXX.txt where XXX matches the epic number
        sed -i "s/phase1_msg_003\.txt/phase1_msg_${i}.txt/g" "$script"
        echo "  ✅ Fixed $script (now uses phase1_msg_${i}.txt)"
    else
        echo "  ❌ Script not found: $script"
    fi
done

echo ""
echo "✅ All message file numbers fixed"
echo ""
echo "Verification (check line 8 of first script):"
head -10 _p1_01.sh | grep "cat >"

# Made with Bob