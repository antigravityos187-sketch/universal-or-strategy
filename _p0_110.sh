#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='bob_prod_bob-admin_34gay3JrSM5CqZF7cg5BjDGDwEk7ZLQdBXUjWdQH9vnaSM6YKaigEytQDQXSygmGqEEXHm7qiLKLupwdhK5DAQp4_61R5yxHVTtKmgDRRR9mcSxJ1HBAPdYnzLcY9utoNmrfo'
mkdir -p docs/brain/EPIC-W7-110
mkdir -p logs/phase0

cat > /tmp/phase0_msg_110.txt << 'EOFMSG'
Execute Phase 0 (Hotspot Analysis) for EPIC-W7-110.

**🚨 CRITICAL FILE I/O PROTOCOL - READ THIS FIRST 🚨**

You are running in SSH/non-interactive mode where Bob's file I/O tools have bugs.

**MANDATORY RULES (Violation = Task Failure)**:
1. ❌ NEVER use write_to_file tool - it has path resolution bugs in SSH mode
2. ❌ NEVER use read_file tool - it fails with "File not found" even when files exist
3. ❌ NEVER use run_shell_command tool - it also has persistence bugs in SSH mode
4. ✅ ALWAYS use execute_command with `cat > file << 'EOF'` to create files
5. ✅ ALWAYS use execute_command with `ls -lh` and `wc -l` to verify files
6. ✅ ALWAYS set cwd parameter to /home/malhitticrypto/universal-or-strategy
7. ✅ ALWAYS follow the EXACT shell command patterns shown below (copy/paste them)

**WHY THIS MATTERS**:
- execute_command bypasses Bob's tool layer and work reliably in SSH mode
- run_shell_command, write_to_file, and read_file all fail in SSH/screen sessions
- The working directory must be explicitly set with cwd parameter

**YOUR TASK**: Focus on the analysis, not the tools. The shell commands below are proven to work.

## Target Method
- Method: AdoptMasterOrders
- File: src/V12_002.SIMA.Lifecycle.cs
- Complexity: 19

## Required Actions

### Step 1: Use jCodemunch to gather data
Use these jCodemunch tools:
1. get_hotspots(repo='universal-or-strategy', top_n=50)
2. get_blast_radius(repo='universal-or-strategy', symbol='AdoptMasterOrders')
3. get_call_hierarchy(repo='universal-or-strategy', symbol_id='AdoptMasterOrders')
4. get_symbol_complexity(repo='universal-or-strategy', symbol_id='AdoptMasterOrders')

### Step 2: Write 00-hotspots.md using execute_command
Use execute_command (NOT run_shell_command) to create docs/brain/EPIC-W7-110/00-hotspots.md:

```bash
cat > docs/brain/EPIC-W7-110/00-hotspots.md << 'EOF'
# Phase 0: Hotspot Analysis - EPIC-W7-110

## Target Method
- **Method**: AdoptMasterOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Cyclomatic Complexity**: 19

## Complexity Metrics
[Include data from get_symbol_complexity]

## Blast Radius
[Include data from get_blast_radius]

## Call Hierarchy
[Include data from get_call_hierarchy]

## Risk Assessment
[LOW/MEDIUM/HIGH based on metrics]
EOF
```

### Step 3: Write manifest.json using execute_command
Use execute_command to create docs/brain/EPIC-W7-110/manifest.json:

```bash
cat > docs/brain/EPIC-W7-110/manifest.json << 'EOF'
{
  "epic_id": "EPIC-W7-110",
  "method": "AdoptMasterOrders",
  "file": "src/V12_002.SIMA.Lifecycle.cs",
  "complexity": 19,
  "phases": {
    "0": {
      "status": "completed",
      "outputs": ["00-hotspots.md"]
    }
  }
}
EOF
```

### Step 4: VERIFY files exist using execute_command
Use execute_command to verify BOTH files were created:

1. Verify 00-hotspots.md:
```bash
ls -lh docs/brain/EPIC-W7-110/00-hotspots.md && wc -l docs/brain/EPIC-W7-110/00-hotspots.md
```

2. Verify manifest.json:
```bash
ls -lh docs/brain/EPIC-W7-110/manifest.json && cat docs/brain/EPIC-W7-110/manifest.json | head -20
```

If either file is missing, CREATE IT AGAIN using the shell commands above.

### Step 5: Confirm completion
Only use attempt_completion when:
- BOTH files exist (verified with ls command)
- File sizes are reasonable (00-hotspots.md should be >100 lines)
- You can see the content with cat/head commands

## Success Criteria
- 00-hotspots.md exists and contains hotspot analysis (verify with wc -l)
- manifest.json exists and shows phase 0 completed (verify with cat)
- Both files verified with shell commands (ls + cat/head)
- No file creation errors

## Critical Reminder
ALWAYS use execute_command with cwd parameter. NEVER use run_shell_command, write_to_file, or read_file in SSH mode.

EOFMSG

bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_110.txt)" 2>&1 | tee logs/phase0/EPIC-W7-110.log
echo "DONE_EXIT=$?"
