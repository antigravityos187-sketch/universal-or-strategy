# Wave 2 Phase 0 Tool Issue - Final Solution Summary

## Problem Identified

Bob Shell's `read_file` tool fails in SSH/non-interactive mode with "File not found" even when files demonstrably exist on the filesystem.

## Evidence from EPIC-CCN-107 Test

```bash
# Shell command shows file exists with 217 lines:
$ ls -lah /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-107/
-rw-r--r-- 1 malhitticrypto 9.1K Jun 13 01:08 00-hotspots.md

$ wc -l 00-hotspots.md
217 00-hotspots.md

# But read_file tool fails:
Error executing tool read_file: File not found
```

## Agent Behavior (Correct)

The Phase 0 agent correctly:
1. ✅ Created `00-hotspots.md` using shell commands (217 lines)
2. ❌ Tried `read_file` for verification - got "File not found"
3. ✅ Recognized the tool bug: "The read_file tool has a bug or path issue"
4. ✅ Verified file exists using shell commands (`ls`, `wc -l`)
5. ✅ Proceeded to next step despite `read_file` failure
6. ✅ Completed successfully

**This is the correct behavior** - the agent is smart enough to work around the tool bug.

## Solution: Shell Commands for File I/O

### File Creation Pattern

```bash
cat > docs/brain/EPIC-CCN-{ID}/00-hotspots.md << 'EOFMARKER'
# Phase 0: Hotspot Analysis

[Content - can be hundreds of lines]

END OF HOTSPOT ANALYSIS
EOFMARKER
```

### Verification Pattern

```bash
# Verify file exists and check line count
ls -lah docs/brain/EPIC-CCN-{ID}/00-hotspots.md && \
wc -l docs/brain/EPIC-CCN-{ID}/00-hotspots.md

# Preview content
cat docs/brain/EPIC-CCN-{ID}/00-hotspots.md | head -20
```

## Configuration Status

### .bob/custom_modes.yaml ✅ CORRECT

Current configuration is correct and requires NO changes:

```yaml
v12-phase0-hotspot:
  name: "V12 Phase 0 Hotspot Analyzer"
  groups:
    - read      # Provides read_file (even though it's buggy)
    - edit      # Provides write_to_file (even though we use shell)
    - command   # Provides run_shell_command (THIS IS WHAT WORKS)
    - mcp       # Provides jCodemunch tools
  tools:
    - read_file
    - write_to_file
    - apply_diff
    - insert_content
    - execute_command
    - use_mcp_tool
  toolRestrictions:
    write_to_file:
      fileRegex: \.(md|json|yaml|yml|txt)$
```

**Why this works**:
- `groups: [command]` gives access to `run_shell_command`
- Shell commands bypass the buggy `read_file` tool
- Agent can still try `read_file` (will fail) then fall back to shell
- No configuration changes needed

## Message Template

### Use Shell-Based Template

**File**: `scripts/wave2/phase0_message_template_shell.txt`

This template instructs agents to:
- Use `cat >` with heredoc for file creation
- Use `ls -lh && wc -l` for verification
- Use `cat | head` for content preview
- Ignore `read_file` failures (expected)

## Launch Script Integration

Update your launch script to use the shell template:

```python
# In launch_wave_v4_safe_budget.py or create v5
template_path = 'scripts/wave2/phase0_message_template_shell.txt'
with open(template_path, 'r') as f:
    template = f.read()

message = (template
    .replace('{EPIC_ID}', epic_id)
    .replace('{METHOD}', method)
    .replace('{FILE}', file)
    .replace('{CYC}', str(cyc)))
```

## Testing

### Single Epic Test

```bash
cd /home/malhitticrypto/universal-or-strategy
bash _p0_107.sh

# Verify output
ls -lh docs/brain/EPIC-CCN-107/
wc -l docs/brain/EPIC-CCN-107/00-hotspots.md
cat docs/brain/EPIC-CCN-107/manifest.json
```

### Expected Results

```
✅ 00-hotspots.md: 200+ lines
✅ manifest.json: Valid JSON
✅ Agent completes successfully
⚠️ read_file errors in log (EXPECTED - IGNORE)
```

## What You Need to Do

### 1. No Configuration Changes Needed ✅

The `.bob/custom_modes.yaml` is already correct. Do NOT modify it.

### 2. Update Launch Script

Edit your Wave 2 launch script to use `phase0_message_template_shell.txt` instead of `phase0_message_template.txt`.

### 3. Test on VM

```bash
# SSH to VM
ssh malhitticrypto@v12-test-golden-v2

# Test single epic
cd /home/malhitticrypto/universal-or-strategy
bash _p0_107.sh

# Verify files created
ls -lh docs/brain/EPIC-CCN-107/
wc -l docs/brain/EPIC-CCN-107/00-hotspots.md
```

### 4. Launch Wave 2

Once single epic test passes, launch all 9 epics:

```bash
python scripts/wave2/launch_wave_v5_shell.py
```

## Success Criteria

- ✅ All 9 epics create `00-hotspots.md` files (200+ lines each)
- ✅ All 9 epics create `manifest.json` files (valid JSON)
- ✅ All agents complete with `attempt_completion`
- ⚠️ `read_file` errors appear in logs (EXPECTED - IGNORE)
- ✅ Shell verification confirms all files exist

## Key Insight

**The agent is working correctly**. It:
1. Tries the tool (as instructed)
2. Recognizes the tool bug
3. Works around it using shell commands
4. Completes successfully

This is exactly the behavior we want. The `read_file` failures are expected and can be ignored.

## Documentation

- **Complete Protocol**: `plugins/multi-agent-orchestrator/WAVE2_SHELL_WORKAROUND.md`
- **Tool Analysis**: `scripts/wave2/TOOL_ISSUE_ANALYSIS.md`
- **Shell Template**: `scripts/wave2/phase0_message_template_shell.txt`
- **Skill Updated**: `plugins/multi-agent-orchestrator/SKILL.md` (section added)

## Next Steps

1. ✅ Configuration verified (no changes needed)
2. ✅ Shell template created
3. ✅ Documentation complete
4. ⏳ **YOU**: Update launch script to use shell template
5. ⏳ **YOU**: Test single epic on VM
6. ⏳ **YOU**: Launch Wave 2 with all 9 epics

## Questions?

If you see `read_file` errors in the logs:
- ✅ **This is expected** - the tool is buggy
- ✅ **Check shell verification** - files should exist via `ls` and `wc -l`
- ✅ **Agent should proceed** - it recognizes the bug and works around it
- ✅ **Epic should complete** - shell commands work reliably

The workflow is ready to go! 🚀