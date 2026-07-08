#!/bin/bash
# Test Script for EPIC-CCN-107
# Tests Phase 0 workflow with proper epic data population

set -e

EPIC_ID="EPIC-CCN-107"
API_KEY="b (2).json"
LOG_DIR="/home/malhitticrypto/universal-or-strategy/logs/phase0"

echo "========================================="
echo "Testing Phase 0 for $EPIC_ID"
echo "========================================="
echo "Method: HydrateFromOpenPositions"
echo "File: src/V12_002.SIMA.Lifecycle.cs"
echo "Complexity: 31"
echo "API Key: $API_KEY"
echo ""

# Create log directory
mkdir -p "$LOG_DIR"

# Set API key environment variable
export BOBSHELL_API_KEY=$(cat "/home/malhitticrypto/universal-or-strategy/docs/API/$API_KEY" | jq -r '.api_key')

# Launch Bob Shell in detached screen session
screen -dmS "test-p0-107" bash -c "
    cd /home/malhitticrypto/universal-or-strategy && \
    bob --mode v12-phase0-hotspot --message 'Execute Phase 0 (Hotspot Analysis) for EPIC-CCN-EPIC-CCN-107.

**🚨 CRITICAL FILE I/O PROTOCOL - READ THIS FIRST 🚨**

You are running in SSH/non-interactive mode where Bob'\''s file I/O tools have bugs.

**MANDATORY RULES (Violation = Task Failure)**:
1. ❌ NEVER use write_to_file tool - it has path resolution bugs in SSH mode
2. ❌ NEVER use read_file tool - it fails with "File not found" even when files exist
3. ✅ ALWAYS use run_shell_command with `cat > file << '\''EOF'\''` to create files
4. ✅ ALWAYS use run_shell_command with `ls -lh` and `wc -l` to verify files
5. ✅ ALWAYS follow the EXACT shell command patterns shown below (copy/paste them)

**WHY THIS MATTERS**:
- Shell commands bypass Bob'\''s tool layer and work reliably in SSH mode
- The Phase 0 agent successfully created a 217-line file using this exact approach
- Bob tools will fail silently or with misleading errors - don'\''t waste time debugging them

**YOUR TASK**: Focus on the analysis, not the tools. The shell commands below are proven to work.

## Target Method
- Method: HydrateFromOpenPositions
- File: src/V12_002.SIMA.Lifecycle.cs
- Complexity: 31

## Required Actions

### Step 1: Use jCodemunch to gather data
Use these jCodemunch tools:
1. get_hotspots(repo='\''universal-or-strategy'\'', top_n=50)
2. get_blast_radius(repo='\''universal-or-strategy'\'', symbol='\''HydrateFromOpenPositions'\'')
3. get_call_hierarchy(repo='\''universal-or-strategy'\'', symbol_id='\''HydrateFromOpenPositions'\'')
4. get_symbol_complexity(repo='\''universal-or-strategy'\'', symbol_id='\''HydrateFromOpenPositions'\'')

### Step 2: Write 00-hotspots.md using shell command
Use run_shell_command to create docs/brain/EPIC-CCN-EPIC-CCN-107/00-hotspots.md:

```bash
cat > docs/brain/EPIC-CCN-EPIC-CCN-107/00-hotspots.md << '\''EOF'\''
# Phase 0: Hotspot Analysis - EPIC-CCN-EPIC-CCN-107

## Target Method
- **Method**: HydrateFromOpenPositions
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Cyclomatic Complexity**: 31

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

### Step 3: Write manifest.json using shell command
Use run_shell_command to create docs/brain/EPIC-CCN-EPIC-CCN-107/manifest.json:

```bash
cat > docs/brain/EPIC-CCN-EPIC-CCN-107/manifest.json << '\''EOF'\''
{
  "epic_id": "EPIC-CCN-EPIC-CCN-107",
  "method": "HydrateFromOpenPositions",
  "file": "src/V12_002.SIMA.Lifecycle.cs",
  "complexity": 31,
  "phases": {
    "0": {
      "status": "completed",
      "outputs": ["00-hotspots.md"]
    }
  }
}
EOF
```

### Step 4: VERIFY files exist using shell commands
Use run_shell_command to verify BOTH files were created:

1. Verify 00-hotspots.md:
```bash
ls -lh docs/brain/EPIC-CCN-EPIC-CCN-107/00-hotspots.md && wc -l docs/brain/EPIC-CCN-EPIC-CCN-107/00-hotspots.md
```

2. Verify manifest.json:
```bash
ls -lh docs/brain/EPIC-CCN-EPIC-CCN-107/manifest.json && cat docs/brain/EPIC-CCN-EPIC-CCN-107/manifest.json | head -20
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

## Why Shell Commands?
The read_file and write_to_file tools have path resolution issues in SSH/non-interactive mode.
Shell commands (cat, ls, wc) work reliably and provide immediate verification.' \
    2>&1 | tee $LOG_DIR/$EPIC_ID-test.log
"

echo "✓ Launched test agent in screen session: test-p0-107"
echo ""
echo "Monitor with:"
echo "  screen -r test-p0-107"
echo "  tail -f $LOG_DIR/$EPIC_ID-test.log"
echo ""
echo "Expected outputs:"
echo "  docs/brain/$EPIC_ID/00-hotspots.md"
echo "  docs/brain/$EPIC_ID/manifest.json"
echo ""
echo "Verify with:"
echo "  ls -lh docs/brain/$EPIC_ID/"
echo "  cat docs/brain/$EPIC_ID/00-hotspots.md"
