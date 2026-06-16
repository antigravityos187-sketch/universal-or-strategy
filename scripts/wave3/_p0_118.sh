#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_t9tV9fuaYCkKYJNm5xCaHWAAR5yJT59mUXoLRHLyb3G4uVHazEQaFacXSz2Nd9Pij2WYNHkvn7THr5amYPqQeDa_ASoyvBNoW8FE2m47D2fhv67cbYGy7TXVeWYswv5N1MNF'
mkdir -p docs/brain/EPIC-CCN-118
mkdir -p logs/phase0

cat > /tmp/phase0_msg_118.txt << 'EOFMSG'
Execute Phase 0 (Hotspot Analysis) for EPIC-CCN-118.

**🚨 CRITICAL FILE I/O PROTOCOL - READ THIS FIRST 🚨**

You are running in SSH/non-interactive mode where Bob's file I/O tools have bugs.

**MANDATORY RULES (Violation = Task Failure)**:
1. ❌ NEVER use write_to_file tool - it has path resolution bugs in SSH mode
2. ❌ NEVER use read_file tool - it fails with "File not found" even when files exist
3. ❌ NEVER use run_shell_command tool - it also has persistence bugs in SSH mode
4. ✅ ALWAYS use execute_command tool with `cat > file << 'EOF'` to create files
5. ✅ ALWAYS use execute_command tool with `ls -lh` and `wc -l` to verify files
6. ✅ ALWAYS set cwd parameter to /home/malhitticrypto/universal-or-strategy
7. ✅ ALWAYS follow the EXACT tool usage patterns shown below (copy/paste them)

**WHY THIS MATTERS**:
- execute_command bypasses Bob's tool layer and works reliably in SSH mode
- run_shell_command, write_to_file, and read_file all fail in SSH/screen sessions
- The working directory must be explicitly set with cwd parameter

**YOUR TASK**: Focus on the analysis, not the tools. The shell commands below are proven to work.

## Target Method
- Method: ProcessSingleFleetRMAAccount
- File: src/V12_002.SIMA.Execution.cs
- Complexity: 16

## Required Actions

### Step 1: Use jCodemunch to gather data
Use these jCodemunch tools:
1. get_hotspots(repo='universal-or-strategy', top_n=50)
2. get_blast_radius(repo='universal-or-strategy', symbol='ProcessSingleFleetRMAAccount')
3. get_call_hierarchy(repo='universal-or-strategy', symbol_id='ProcessSingleFleetRMAAccount')
4. get_symbol_complexity(repo='universal-or-strategy', symbol_id='ProcessSingleFleetRMAAccount')

### Step 2: Write 00-hotspots.md using execute_command
Use execute_command (NOT run_shell_command) to create docs/brain/EPIC-CCN-118/00-hotspots.md:

```xml
<execute_command>
<command>
cat > docs/brain/EPIC-CCN-118/00-hotspots.md << 'EOF'
# Phase 0: Hotspot Analysis - EPIC-CCN-118

## Target Method
- **Method**: ProcessSingleFleetRMAAccount
- **File**: src/V12_002.SIMA.Execution.cs
- **Cyclomatic Complexity**: 16

## Complexity Metrics
[Include data from get_symbol_complexity]

## Blast Radius
[Include data from get_blast_radius]

## Call Hierarchy
[Include data from get_call_hierarchy]

## Risk Assessment
[LOW/MEDIUM/HIGH based on metrics]
EOF</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

### Step 3: Write manifest.json using execute_command
Use execute_command (NOT run_shell_command) to create docs/brain/EPIC-CCN-118/manifest.json:

```xml
<execute_command>
<command>
cat > docs/brain/EPIC-CCN-118/manifest.json << 'EOF'
{
  "epic_id": "EPIC-CCN-118",
  "method": "ProcessSingleFleetRMAAccount",
  "file": "src/V12_002.SIMA.Execution.cs",
  "complexity": 16,
  "phases": {
    "0": {
      "status": "completed",
      "outputs": ["00-hotspots.md"]
    }
  }
}
EOF</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

### Step 4: VERIFY files exist using execute_command
Use execute_command (NOT run_shell_command) to verify BOTH files were created:

1. Verify 00-hotspots.md:
```xml
<execute_command>
<command>
ls -lh docs/brain/EPIC-CCN-118/00-hotspots.md && wc -l docs/brain/EPIC-CCN-118/00-hotspots.md</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

2. Verify manifest.json:
```xml
<execute_command>
<command>
ls -lh docs/brain/EPIC-CCN-118/manifest.json && cat docs/brain/EPIC-CCN-118/manifest.json | head -20</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

If either file is missing, CREATE IT AGAIN using execute_command (NOT run_shell_command).

### Step 5: Confirm completion
Only use attempt_completion when:
- BOTH files exist (verified with ls command via execute_command)
- File sizes are reasonable (00-hotspots.md should be >100 lines)
- You can see the content with cat/head commands via execute_command

## Success Criteria
- 00-hotspots.md exists and contains hotspot analysis (verify with wc -l)
- manifest.json exists and shows phase 0 completed (verify with cat)
- Both files verified with execute_command shell commands (ls + cat/head)
- No file creation errors

## Critical Reminder
ALWAYS use execute_command with cwd parameter. NEVER use run_shell_command, write_to_file, or read_file in SSH mode.

**MANDATORY REPORTING**:
After completing all tasks, you MUST report:
1. Bobcoins used this session: [X.XX]
2. Remaining balance in API key: [Y.YY]

Format: "Cost: X.XX | Balance: Y.YY"
EOFMSG

bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_118.txt)" 2>&1 | tee logs/phase0/EPIC-CCN-118.log
echo "DONE_EXIT=$?"

# Made with Bob
