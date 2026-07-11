#!/usr/bin/env python3
"""
Generate Phase 0 scripts for EPIC-006 through EPIC-015
Building Blocks method: Copy template, change 4 lines
"""

import json

# Epic data from EPIC_ROADMAP_FINAL_V1.md
epics = [
    {
        "id": "006",
        "file": "V12_002.SIMA.Lifecycle.cs",
        "methods": [
            "AdoptFleetWorkingOrders", "ClassifyAndRouteFleetOrder",
            "SweepTrackedOrders", "SweepBrokerOrders", "DrainPhotonQueuesOnShutdown",
            "ShouldProtectBracketOrder", "AdoptMasterWorkingOrders",
            "HydrateFSM_MapOrderStateToFsmState", "HydrateFSMsFromWorkingOrders"
        ],
        "complexity": [17, 16, 12, 12, 11, 10, 9, 9, 9],
        "api_key": "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    {
        "id": "007",
        "file": "V12_002.SIMA.Shadow.cs",
        "methods": ["ShadowPropagateStopMoves", "ShadowProcessFollowerStopUpdate"],
        "complexity": [20, 12],
        "api_key": "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    {
        "id": "008",
        "file": "V12_002.Symmetry.Replace.cs",
        "methods": [
            "SymmetryGuardReplaceExistingFollowerTarget",
            "SymmetryGuardTryResolveFollowersForDispatch",
            "SymmetryGuardCascadeFollowerCleanup",
            "SymmetryGuardPruneDispatches",
            "SymmetryNormalizeTradeType"
        ],
        "complexity": [18, 18, 10, 10, 10],
        "api_key": "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    {
        "id": "009",
        "file": "V12_002.UI.Compliance.cs",
        "methods": [
            "IsOrderAllowed", "HandleFleetTargetFill", "CancelOrphanedTargets",
            "ProcessQueuedExecution_HandleFleetOCO", "ProcessQueuedExecution_SyncFlatPosition",
            "LogApexPerformance", "ProcessQueuedExecution_HandleFleetBrackets", "TrackTradeEntry"
        ],
        "complexity": [16, 16, 14, 13, 13, 13, 10, 9],
        "api_key": "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    {
        "id": "010",
        "file": "V12_002.UI.IPC.Commands.Config.cs",
        "methods": ["TryApplyConfigTarget_Value", "HandleTrimCommand", "TryApplyConfigTarget_Type"],
        "complexity": [17, 11, 11],
        "api_key": "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    {
        "id": "011",
        "file": "V12_002.UI.IPC.Commands.Fleet.cs",
        "methods": [
            "TryHandleFleetCommand", "TryHandleFleet_CancelAll",
            "CancelAll_ProcessSingleFleetAccount", "TryHandleFleet_MoveTarget",
            "CancelAll_ProcessMasterAccount", "TryHandleFleet_LongShort"
        ],
        "complexity": [19, 19, 18, 15, 14, 11],
        "api_key": "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    {
        "id": "012",
        "file": "V12_002.UI.IPC.cs",
        "methods": [
            "IsSymbolMatch", "ProcessIpcCommands", "TryParseTargetMode",
            "ProcessIpcCommandCore", "IsAllowedIpcAction"
        ],
        "complexity": [18, 14, 13, 13, 10],
        "api_key": "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    {
        "id": "013",
        "file": "V12_002.UI.Panel.Construction.cs",
        "methods": ["DestroyPanel", "PlacePanel", "CreateSection0_Identity"],
        "complexity": [17, 13, 13],
        "api_key": "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    {
        "id": "014",
        "file": "V12_002.UI.Panel.Handlers.cs",
        "methods": [
            "ShowModeSpecificControls", "UpdateTargetVisibility",
            "AttachExecutionPanelHandlers", "OnSubmitClick", "CollapseAllExecutionControls"
        ],
        "complexity": [20, 19, 12, 12, 11],
        "api_key": "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    {
        "id": "015",
        "file": "V12_002.UI.Panel.StateSync.cs",
        "methods": [
            "UpdatePanelState", "SyncPanelConfigFromSnapshot",
            "SyncLiveTargetRows", "SyncModeChipVisuals"
        ],
        "complexity": [16, 15, 10, 9],
        "api_key": "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    }
]

# Template (from working _p0_003.sh)
template = '''#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='{api_key}'
mkdir -p docs/brain/EPIC-{epic_id}
mkdir -p logs/phase0

cat > /tmp/phase0_msg_{epic_id}.txt << 'EOFMSG'
Execute Phase 0 (Hotspot Analysis) for EPIC-{epic_id}.

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

## Target Methods ({method_count} methods in same file)
{method_list}
- File: src/{file}
- Complexity: {complexity_list} (target: all ≤8)

## Required Actions

### Step 1: Use jCodemunch to gather data
Use these jCodemunch tools:
1. get_hotspots(repo='universal-or-strategy', top_n=50)
2. get_blast_radius(repo='universal-or-strategy', symbol='{first_method}')
3. get_call_hierarchy(repo='universal-or-strategy', symbol_id='{first_method}')
4. get_symbol_complexity(repo='universal-or-strategy', symbol_id='{first_method}')

### Step 2: Write 00-hotspots.md using execute_command
Use execute_command (NOT run_shell_command) to create docs/brain/EPIC-{epic_id}/00-hotspots.md:

```xml
<execute_command>
<command>
cat > docs/brain/EPIC-{epic_id}/00-hotspots.md << 'EOF'
# Phase 0: Hotspot Analysis - EPIC-{epic_id}

## Target Methods
{method_details}
- **File**: src/{file}

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
Use execute_command (NOT run_shell_command) to create docs/brain/EPIC-{epic_id}/manifest.json:

```xml
<execute_command>
<command>
cat > docs/brain/EPIC-{epic_id}/manifest.json << 'EOF'
{{
  "epic_id": "EPIC-{epic_id}",
  "methods": {methods_json},
  "file": "src/{file}",
  "complexity": {complexity_json},
  "phases": {{
    "0": {{
      "status": "completed",
      "outputs": ["00-hotspots.md"]
    }}
  }}
}}
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
ls -lh docs/brain/EPIC-{epic_id}/00-hotspots.md && wc -l docs/brain/EPIC-{epic_id}/00-hotspots.md</command>
<cwd>/home/malhitticrypto/universal-or-strategy</cwd>
</execute_command>
```

2. Verify manifest.json:
```xml
<execute_command>
<command>
ls -lh docs/brain/EPIC-{epic_id}/manifest.json && cat docs/brain/EPIC-{epic_id}/manifest.json | head -20</command>
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
EOFMSG

bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_{epic_id}.txt)" 2>&1 | tee logs/phase0/EPIC-{epic_id}.log
echo "DONE_EXIT=$?"

# Made with Bob
'''

# Generate scripts
for epic in epics:
    # Format method list
    method_list = "\n".join([f"- Method {i+1}: {m}" for i, m in enumerate(epic["methods"])])
    
    # Format method details for hotspot file
    method_details = "\n".join([
        f"- **Method {i+1}**: {m} (CYC={epic['complexity'][i]})"
        for i, m in enumerate(epic["methods"])
    ])
    
    # Format complexity list
    complexity_list = ", ".join(map(str, epic["complexity"]))
    
    # JSON arrays
    methods_json = json.dumps(epic["methods"])
    complexity_json = json.dumps(epic["complexity"])
    
    # Generate script
    script = template.format(
        api_key=epic["api_key"],
        epic_id=epic["id"],
        method_count=len(epic["methods"]),
        method_list=method_list,
        file=epic["file"],
        complexity_list=complexity_list,
        first_method=epic["methods"][0],
        method_details=method_details,
        methods_json=methods_json,
        complexity_json=complexity_json
    )
    
    # Write script
    filename = f"_p0_{epic['id']}.sh"
    with open(filename, 'w', newline='\n') as f:
        f.write(script)
    
    print(f"✅ Created {filename}")

print(f"\n✅ Generated {len(epics)} Phase 0 scripts")
print("Next: Upload to VM and launch")