#!/bin/bash
# Add --yolo flag to all Phase 0 scripts

for i in 107 108 109 110 111 112 113 114 115; do
    script="_p0_$i.sh"
    if [ -f "$script" ]; then
        sed -i 's/bob --chat-mode/bob --yolo --chat-mode/' "$script"
        echo "✅ Fixed $script"
    fi
done

echo ""
echo "All 9 scripts updated with --yolo flag"
echo "Files will now persist on disk!"

# Made with Bob
