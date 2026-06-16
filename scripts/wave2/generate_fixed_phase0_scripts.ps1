# Generate Fixed Phase 0 Scripts with execute_command
# Fixes the run_shell_command bug by using execute_command instead

$epics = @(
    @{id=107; method="HydrateFromOpenPositions"; file="src/V12_002.SIMA.Lifecycle.cs"; cyc=31},
    @{id=108; method="SweepBrokerOrders"; file="src/V12_002.SIMA.Lifecycle.cs"; cyc=24},
    @{id=109; method="ProcessOrderUpdate"; file="src/V12_002.SIMA.Lifecycle.cs"; cyc=22},
    @{id=110; method="HandleOrderFill"; file="src/V12_002.SIMA.Lifecycle.cs"; cyc=20},
    @{id=111; method="ValidateOrderStates"; file="src/V12_002.SIMA.Lifecycle.cs"; cyc=19},
    @{id=112; method="SynchronizeBrokerState"; file="src/V12_002.SIMA.Lifecycle.cs"; cyc=18},
    @{id=113; method="HandleSweepErrors"; file="src/V12_002.SIMA.Lifecycle.cs"; cyc=17},
    @{id=114; method="CleanupOrderCollections"; file="src/V12_002.SIMA.Lifecycle.cs"; cyc=16},
    @{id=115; method="ReconcilePositions"; file="src/V12_002.SIMA.Lifecycle.cs"; cyc=16}
)

$apiKey = 'bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu'

foreach ($epic in $epics) {
    $scriptPath = "_p0_$($epic.id)_fixed.sh"
    
    $content = @"
#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='$apiKey'
mkdir -p docs/brain/EPIC-CCN-$($epic.id)
mkdir -p logs/phase0

cat > /tmp/phase0_msg_$($epic.id).txt << 'EOFMSG'
Execute Phase 0 (Hotspot Analysis) for EPIC-CCN-$($epic.id).

**🚨 CRITICAL FILE I/O PROTOCOL - READ THIS FIRST 🚨**

You are running in SSH/non-interactive mode where Bob's file I/O tools have bugs.

**MANDATORY RULES (Violation = Task Failure)**:
1. ❌ NEVER use write_to_file tool - it has path resolution bugs in SSH mode
2. ❌ NEVER use read_file tool - it fails with "File not found" even when files exist
3. ❌ NEVER use run_shell_command tool - it also has persistence bugs in SSH mode
4. ✅ ALWAYS use execute_command tool with ``cat > file << 'EOF'`` to create files
5. ✅ ALWAYS use execute_command tool with ``ls -lh`` and ``wc -l`` to verify files
6. ✅ ALWAYS set cwd parameter to /home/malhitticrypto/universal-or-strategy

**WHY THIS MATTERS**:
- execute_command bypasses Bob's tool layer and works reliably in SSH mode
- run_shell_command, write_to_file, and read_file all fail in SSH/screen sessions
- The working directory must be explicitly set with cwd parameter

**CORRECT TOOL USAGE**:
``````xml
<execute_command>
<command>cat > docs/brain/EPIC-CCN-$($epic.id)/00-hotspots.md << 'EOF'
[content here]
EOF</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
``````

## Target Method
- Method: $($epic.method)
- File: $($epic.file)
- Complexity: $($epic.cyc)

## Required Actions

### Step 1: Use jCodemunch to gather data
Use these jCodemunch tools:
1. get_hotspots(repo='universal-or-strategy', top_n=50)
2. get_blast_radius(repo='universal-or-strategy', symbol='$($epic.method)')
3. get_call_hierarchy(repo='universal-or-strategy', symbol_id='$($epic.method)')
4. get_symbol_complexity(repo='universal-or-strategy', symbol_id='$($epic.method)')

### Step 2: Write 00-hotspots.md using execute_command
Use execute_command (NOT run_shell_command) to create docs/brain/EPIC-CCN-$($epic.id)/00-hotspots.md:

``````xml
<execute_command>
<command>cat > docs/brain/EPIC-CCN-$($epic.id)/00-hotspots.md << 'EOF'
# Phase 0: Hotspot Analysis - EPIC-CCN-$($epic.id)

## Target Method
- **Method**: $($epic.method)
- **File**: $($epic.file)
- **Cyclomatic Complexity**: $($epic.cyc)

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
``````

### Step 3: Write manifest.json using execute_command
Use execute_command (NOT run_shell_command) to create docs/brain/EPIC-CCN-$($epic.id)/manifest.json:

``````xml
<execute_command>
<command>cat > docs/brain/EPIC-CCN-$($epic.id)/manifest.json << 'EOF'
{
  "epic_id": "EPIC-CCN-$($epic.id)",
  "method": "$($epic.method)",
  "file": "$($epic.file)",
  "complexity": $($epic.cyc),
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
``````

### Step 4: VERIFY files exist using execute_command
Use execute_command (NOT run_shell_command) to verify BOTH files were created:

1. Verify 00-hotspots.md:
``````xml
<execute_command>
<command>ls -lh docs/brain/EPIC-CCN-$($epic.id)/00-hotspots.md && wc -l docs/brain/EPIC-CCN-$($epic.id)/00-hotspots.md</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
``````

2. Verify manifest.json:
``````xml
<execute_command>
<command>ls -lh docs/brain/EPIC-CCN-$($epic.id)/manifest.json && cat docs/brain/EPIC-CCN-$($epic.id)/manifest.json | head -20</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
``````

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
EOFMSG

bob --chat-mode v12-phase0-hotspot "`$(cat /tmp/phase0_msg_$($epic.id).txt)" 2>&1 | tee logs/phase0/EPIC-CCN-$($epic.id).log
echo "DONE_EXIT=`$?"
"@
    
    $content | Out-File -FilePath $scriptPath -Encoding UTF8 -NoNewline
    Write-Host "✓ Generated $scriptPath"
}

Write-Host "`n✅ All 9 fixed scripts generated"
Write-Host "Next: Deploy to VM with gcloud compute scp"

# Made with Bob
