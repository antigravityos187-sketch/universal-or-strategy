#!/bin/bash
# Apply execute_command fix to all Phase 0 scripts
# This script replicates the manual fix from _p0_107.sh to all other scripts

cd /c/WSGTA/universal-or-strategy

for epic in 108 109 110 111 112 113 114 115; do
    script="_p0_${epic}.sh"
    echo "Fixing $script..."
    
    # Use sed to replace run_shell_command with execute_command
    sed -i 's/run_shell_command/execute_command/g' "$script"
    
    # Add the missing rule about run_shell_command being banned
    sed -i 's/2\. ❌ NEVER use read_file tool - it fails with "File not found" even when files exist/2. ❌ NEVER use read_file tool - it fails with "File not found" even when files exist\n3. ❌ NEVER use run_shell_command tool - it also has persistence bugs in SSH mode/g' "$script"
    
    # Update the numbering of subsequent rules
    sed -i 's/^3\. ✅ ALWAYS use execute_command/4. ✅ ALWAYS use execute_command/g' "$script"
    sed -i 's/^4\. ✅ ALWAYS use execute_command with `ls/5. ✅ ALWAYS use execute_command with `ls/g' "$script"
    sed -i 's/^5\. ✅ ALWAYS follow/6. ✅ ALWAYS set cwd parameter to \/home\/malhitticrypto\/universal-or-strategy\n7. ✅ ALWAYS follow/g' "$script"
    
    # Update WHY THIS MATTERS section
    sed -i 's/- Shell commands bypass/- execute_command bypasses/g' "$script"
    sed -i 's/- The Phase 0 agent successfully created a 217-line file using this exact approach/- run_shell_command, write_to_file, and read_file all fail in SSH\/screen sessions/g' "$script"
    sed -i 's/- Bob tools will fail silently or with misleading errors - don'\''t waste time debugging them/- The working directory must be explicitly set with cwd parameter/g' "$script"
    
    # Update Step 2 header and add XML tags
    sed -i 's/### Step 2: Write 00-hotspots.md using shell command/### Step 2: Write 00-hotspots.md using execute_command/g' "$script"
    sed -i "s/Use execute_command to create docs\/brain\/EPIC-CCN-${epic}\/00-hotspots.md:/Use execute_command (NOT run_shell_command) to create docs\/brain\/EPIC-CCN-${epic}\/00-hotspots.md:/g" "$script"
    
    # Similar updates for Step 3, 4, 5
    sed -i 's/### Step 3: Write manifest.json using shell command/### Step 3: Write manifest.json using execute_command/g' "$script"
    sed -i 's/### Step 4: VERIFY files exist using shell commands/### Step 4: VERIFY files exist using execute_command/g' "$script"
    
    # Update final reminder
    sed -i 's/## Why Shell Commands?/## Critical Reminder/g' "$script"
    sed -i 's/The read_file and write_to_file tools have path resolution issues in SSH\/non-interactive mode\./ALWAYS use execute_command with cwd parameter. NEVER use run_shell_command, write_to_file, or read_file in SSH mode./g' "$script"
    sed -i 's/Shell commands (cat, ls, wc) work reliably and provide immediate verification\.//g' "$script"
    
    echo "✓ Fixed $script"
done

echo ""
echo "✅ All 8 remaining scripts fixed"
echo "Note: _p0_107.sh was already fixed manually"

# Made with Bob
