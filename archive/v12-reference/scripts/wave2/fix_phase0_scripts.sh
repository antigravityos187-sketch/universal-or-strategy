#!/bin/bash
# Fix Phase 0 scripts: Replace run_shell_command with execute_command

for epic in 107 108 109 110 111 112 113 114 115; do
    script="_p0_${epic}.sh"
    if [ -f "$script" ]; then
        echo "Fixing $script..."
        # Replace run_shell_command with execute_command in the instructions
        sed -i 's/run_shell_command/execute_command/g' "$script"
        # Add cwd parameter instruction after execute_command mentions
        sed -i 's/<execute_command>/<execute_command>\n<command>/g' "$script"
        sed -i 's/<\/command>/<\/command>\n<cwd>\/home\/malhitticrypto\/universal-or-strategy<\/cwd>/g' "$script"
        echo "✓ Fixed $script"
    else
        echo "⚠ $script not found"
    fi
done

echo ""
echo "✅ All scripts fixed"
echo "Changes made:"
echo "  - Replaced 'run_shell_command' with 'execute_command'"
echo "  - Added cwd parameter instructions"

# Made with Bob
