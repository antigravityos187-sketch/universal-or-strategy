#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu'
mkdir -p docs/brain/EPIC-013
mkdir -p logs/phase1

cat > /tmp/phase1_msg_003.txt << 'EOFMSG'
Execute Phase 1 (Scope Definition) for EPIC-013.

**???? CRITICAL FILE I/O PROTOCOL - READ THIS FIRST ????**

You are running in SSH/non-interactive mode where Bob's file I/O tools have bugs.

**MANDATORY RULES (Violation = Task Failure)**:
1. ??? NEVER use write_to_file tool - it has path resolution bugs in SSH mode
2. ??? NEVER use read_file tool - it fails with "File not found" even when files exist
3. ??? NEVER use run_shell_command tool - it also has persistence bugs in SSH mode
4. ??? ALWAYS use execute_command tool with `cat > file << 'EOF'` to create files
5. ??? ALWAYS use execute_command tool with `ls -lh` and `wc -l` to verify files
6. ??? ALWAYS set cwd parameter to /home/malhitticrypto/universal-or-strategy
7. ??? ALWAYS follow the EXACT tool usage patterns shown below (copy/paste them)

**WHY THIS MATTERS**:
- execute_command bypasses Bob's tool layer and works reliably in SSH mode
- run_shell_command, write_to_file, and read_file all fail in SSH/screen sessions
- The working directory must be explicitly set with cwd parameter

**YOUR TASK**: Focus on the analysis, not the tools. The shell commands below are proven to work.

## Target Methods (2 methods in same file)
- Method 1: SyncLimitTarget
- Method 2: SyncStopTarget
- File: src/V12_002.Orders.Management.StopSync.cs
- Complexity: 17, 9 (target: all ???8)

## Required Actions

### Step 1: Use jCodemunch to gather data
Use these jCodemunch tools:
1. get_hotspots(repo='universal-or-strategy', top_n=50)
2. get_blast_radius(repo='universal-or-strategy', symbol='SyncLimitTarget')
3. get_call_hierarchy(repo='universal-or-strategy', symbol_id='SyncLimitTarget')
4. get_symbol_complexity(repo='universal-or-strategy', symbol_id='SyncLimitTarget')

### Step 2: Write 00-scope.md using execute_command
Use execute_command (NOT run_shell_command) to create docs/brain/EPIC-013/00-scope.md:

```xml
<execute_command>
<command>
cat > docs/brain/EPIC-013/00-scope.md << 'EOF'
# Phase 1: Scope Definition - EPIC-013

## Target Methods
- **Method 1**: SyncLimitTarget
- **Method 2**: SyncStopTarget
- **File**: src/V12_002.Orders.Management.StopSync.cs
- **Cyclomatic Complexity**: 17, 9

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
Use execute_command (NOT run_shell_command) to create docs/brain/EPIC-013/manifest.json:

```xml
<execute_command>
<command>
cat > docs/brain/EPIC-013/manifest.json << 'EOF'
{
  "epic_id": "EPIC-013",
  "methods": ["SyncLimitTarget", "SyncStopTarget"],
  "file": "src/V12_002.Orders.Management.StopSync.cs",
  "complexity": [17, 9],
  "phases": {
    "1": {
      "status": "completed",
      "outputs": ["00-scope.md"]
    }
  }
}
EOF</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

### Step 4: VERIFY files exist using execute_command
Use execute_command (NOT run_shell_command) to verify BOTH files were created:

1. Verify 00-scope.md:
```xml
<execute_command>
<command>
ls -lh docs/brain/EPIC-013/00-scope.md && wc -l docs/brain/EPIC-013/00-scope.md</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

2. Verify manifest.json:
```xml
<execute_command>
<command>
ls -lh docs/brain/EPIC-013/manifest.json && cat docs/brain/EPIC-013/manifest.json | head -20</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

If either file is missing, CREATE IT AGAIN using execute_command (NOT run_shell_command).

### Step 5: Confirm completion
Only use attempt_completion when:
- BOTH files exist (verified with ls command via execute_command)
- File sizes are reasonable (00-scope.md should be >100 lines)
- You can see the content with cat/head commands via execute_command

## Success Criteria
- 00-scope.md exists and contains Scope Definition (verify with wc -l)
- manifest.json exists and shows Phase 1 completed (verify with cat)
- Both files verified with execute_command shell commands (ls + cat/head)
- No file creation errors

## Critical Reminder
ALWAYS use execute_command with cwd parameter. NEVER use run_shell_command, write_to_file, or read_file in SSH mode.
EOFMSG

bob --yolo --chat-mode plan "$(cat /tmp/phase1_msg_003.txt)" 2>&1 | tee logs/phase1/EPIC-013.log
echo "DONE_EXIT=$?"

# Made with Bob
