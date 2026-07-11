# Wave 2 Script Cleanup - Bad Tool Instructions

## Problem
Several scripts contain instructions telling agents to use `write_to_file` and `read_file` tools, which **FAIL in SSH/non-interactive mode** on the VM.

## Files with BAD Instructions (Archive These)

### Python Scripts
1. **launch_phase0_fixed.py** - Lines 71, 82-83
   - Tells agents to use `write_to_file` tool
   - Tells agents to use `read_file` tool for verification
   - ❌ DEPRECATED

2. **launch_wave2_phase0_with_verification.py** - Lines 76, 84, 101-103
   - Tells agents to use `write_to_file` tool
   - Tells agents to use `read_file` tool for verification
   - ❌ DEPRECATED

3. **launch_phase0_v3_custom_mode.py** - Lines 41-47
   - Tells agents to use `write_to_file` tool
   - Tells agents to use `read_file` tool for verification
   - Line 47: "CRITICAL: Use write_to_file tool, NOT run_shell_command with cat."
   - ❌ DEPRECATED (this instruction is backwards!)

### Message Templates
4. **phase0_message_template.txt** - Lines 20, 28, 45-47, 52, 57
   - Tells agents to use `write_to_file` tool
   - Tells agents to use `read_file` tool for verification
   - ❌ DEPRECATED

## Files with GOOD Instructions (Use These)

### Message Templates
1. **phase0_message_template_shell.txt** ✅
   - Lines 8-10: Explicitly warns NEVER use write_to_file or read_file
   - Uses shell commands: `cat > file << 'EOF'`, `ls -lh`, `wc -l`
   - **USE THIS TEMPLATE**

### Python Scripts
2. **launch_phase0_v4_shell_commands.py** ✅
   - Loads `phase0_message_template_shell.txt`
   - Generates scripts with shell command instructions
   - **USE THIS SCRIPT**

## Action Required

### 1. Archive Bad Scripts
```powershell
# Create archive directory
mkdir scripts/wave2/_deprecated_tool_bugs

# Move bad scripts
mv scripts/wave2/launch_phase0_fixed.py scripts/wave2/_deprecated_tool_bugs/
mv scripts/wave2/launch_wave2_phase0_with_verification.py scripts/wave2/_deprecated_tool_bugs/
mv scripts/wave2/launch_phase0_v3_custom_mode.py scripts/wave2/_deprecated_tool_bugs/
mv scripts/wave2/phase0_message_template.txt scripts/wave2/_deprecated_tool_bugs/
```

### 2. Use Correct Script
```powershell
# Generate Phase 0 scripts with shell commands
python scripts/wave2/launch_phase0_v4_shell_commands.py
```

## Why Shell Commands?

Bob Shell's `write_to_file` and `read_file` tools have **path resolution bugs in SSH/non-interactive mode**:
- `write_to_file` fails silently or writes to wrong location
- `read_file` returns "File not found" even when files exist

**Shell commands work reliably**:
- `cat > file << 'EOF'` - Create files
- `ls -lh file` - Verify file exists and check size
- `wc -l file` - Count lines
- `cat file` - Read file contents

## Reference
- Tool bug analysis: `scripts/wave2/TOOL_ISSUE_ANALYSIS.md`
- Shell workaround: `scripts/wave2/SOLUTION_SHELL_COMMANDS.md`
- Skill documentation: `.bob/skills/gcp-vm-wave-execution/skill.md`