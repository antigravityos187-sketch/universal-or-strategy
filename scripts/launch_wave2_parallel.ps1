# Wave 2 Parallel Execution Launcher
# Spawns separate Bob CLI sessions for each epic/phase combination
# Each session has isolated context window and uses BobCoins (Fable 5)

param(
    [int]$Phase = 4,  # Start with Phase 4 (Ticket Generation)
    [switch]$DryRun
)

# Wave 2 Epics (9 remaining)
$epics = @(
    @{id="EPIC-CCN-164"; method="IsCommandForThisInstrument"; file="src/V12_002.UI.IPC.cs"; cyc=36},
    @{id="EPIC-CCN-107"; method="HydrateFromOpenPositions"; file="src/V12_002.SIMA.Lifecycle.cs"; cyc=31},
    @{id="EPIC-CCN-108"; method="SweepBrokerOrders"; file="src/V12_002.SIMA.Lifecycle.cs"; cyc=24},
    @{id="EPIC-CCN-109"; method="HydrateWorkingOrdersFromBroker"; file="src/V12_002.SIMA.Lifecycle.cs"; cyc=19},
    @{id="EPIC-CCN-110"; method="AdoptMasterOrders"; file="src/V12_002.SIMA.Lifecycle.cs"; cyc=19},
    @{id="EPIC-CCN-155"; method="TryHandleFleetCommand"; file="src/V12_002.UI.IPC.Commands.Fleet.cs"; cyc=19},
    @{id="EPIC-CCN-98"; method="ProcessFlattenWorkItem_CancelOrders"; file="src/V12_002.SIMA.Flatten.cs"; cyc=18},
    @{id="EPIC-CCN-128"; method="SymmetryGuardReplaceExistingFollowerTarget"; file="src/V12_002.Symmetry.Replace.cs"; cyc=18},
    @{id="EPIC-CCN-129"; method="SymmetryGuardTryResolveFollowersForDispatch"; file="src/V12_002.Symmetry.Replace.cs"; cyc=18}
)

# Phase configurations
$phaseConfigs = @{
    0 = @{name="Hotspot Analysis"; mcp="phase-0-hotspot"; tool="execute_phase_0"}
    1 = @{name="Scope Definition"; mcp="phase-1-scope"; tool="execute_phase_1"}
    1.5 = @{name="Scope Boundary"; mcp="phase-1-5-boundary"; tool="execute_phase_1_5"}
    2 = @{name="Architecture Planning"; mcp="phase-2-architecture"; tool="execute_phase_2"}
    3 = @{name="DNA & PR Audit"; mcp="phase-3-audit"; tool="execute_phase_3"}
    4 = @{name="Ticket Generation"; mcp="phase-4-tickets"; tool="execute_phase_4"}
    5 = @{name="Ticket Execution"; mcp="phase-5-execute"; tool="execute_phase_5"}
    5.5 = @{name="Verification"; mcp="phase-5-verify"; tool="execute_phase_5_verify"}
    6 = @{name="Final Review"; mcp="phase-6-review"; tool="execute_phase_6"}
}

$config = $phaseConfigs[$Phase]
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Wave 2 Phase ${Phase}: $($config.name)" -ForegroundColor Cyan
Write-Host "Launching $($epics.Count) parallel Bob sessions" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$jobs = @()

foreach ($epic in $epics) {
    $epicId = $epic.id
    $prompt = "Execute Phase $Phase for $epicId using MCP tool $($config.mcp).$($config.tool). "
    
    if ($Phase -eq 0) {
        $prompt += "Args: epic_id='$epicId', method='$($epic.method)', file='$($epic.file)', cyc=$($epic.cyc), jcodemunch_data={}"
    } else {
        $prompt += "Args: epic_id='$epicId'"
    }
    
    Write-Host "  Launching: $epicId" -ForegroundColor Yellow
    
    if ($DryRun) {
        Write-Host "    [DRY RUN] Would execute: bob chat '$prompt'" -ForegroundColor Gray
    } else {
        # Launch Bob CLI with proper syntax
        # Use --chat-mode advanced (has MCP access) and --yolo (auto-approve)
        $job = Start-Job -ScriptBlock {
            param($prompt)
            & bob $prompt --chat-mode advanced --yolo
        } -ArgumentList $prompt
        
        $jobs += @{
            Job = $job
            EpicId = $epicId
        }
    }
}

if (-not $DryRun) {
    Write-Host "`n  Waiting for $($jobs.Count) jobs to complete..." -ForegroundColor Yellow
    
    # Wait for all jobs
    $jobs | ForEach-Object {
        $result = Wait-Job $_.Job | Receive-Job
        $status = if ($_.Job.State -eq "Completed") { "✅" } else { "❌" }
        Write-Host "  $status $($_.EpicId): $($_.Job.State)" -ForegroundColor $(if ($_.Job.State -eq "Completed") { "Green" } else { "Red" })
        Remove-Job $_.Job
    }
    
    Write-Host "`n✅ Phase $Phase complete for all epics!" -ForegroundColor Green
} else {
    Write-Host "`n[DRY RUN] No jobs launched" -ForegroundColor Gray
}

Write-Host "`nNext: Run with -Phase $($Phase + 1) to continue" -ForegroundColor Cyan

# Made with Bob