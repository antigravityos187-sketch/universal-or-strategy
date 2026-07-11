# Customize Phase 0 scripts for EPIC-006 through EPIC-015
# Building Blocks method: Find and replace in template copies

$epics = @(
    @{
        id = "006"
        file = "V12_002.SIMA.Lifecycle.cs"
        methods = @("AdoptFleetWorkingOrders", "ClassifyAndRouteFleetOrder", "SweepTrackedOrders", "SweepBrokerOrders", "DrainPhotonQueuesOnShutdown", "ShouldProtectBracketOrder", "AdoptMasterWorkingOrders", "HydrateFSM_MapOrderStateToFsmState", "HydrateFSMsFromWorkingOrders")
        complexity = @(17, 16, 12, 12, 11, 10, 9, 9, 9)
        api_key = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    @{
        id = "007"
        file = "V12_002.SIMA.Shadow.cs"
        methods = @("ShadowPropagateStopMoves", "ShadowProcessFollowerStopUpdate")
        complexity = @(20, 12)
        api_key = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    @{
        id = "008"
        file = "V12_002.Symmetry.Replace.cs"
        methods = @("SymmetryGuardReplaceExistingFollowerTarget", "SymmetryGuardTryResolveFollowersForDispatch", "SymmetryGuardCascadeFollowerCleanup", "SymmetryGuardPruneDispatches", "SymmetryNormalizeTradeType")
        complexity = @(18, 18, 10, 10, 10)
        api_key = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    @{
        id = "009"
        file = "V12_002.UI.Compliance.cs"
        methods = @("IsOrderAllowed", "HandleFleetTargetFill", "CancelOrphanedTargets", "ProcessQueuedExecution_HandleFleetOCO", "ProcessQueuedExecution_SyncFlatPosition", "LogApexPerformance", "ProcessQueuedExecution_HandleFleetBrackets", "TrackTradeEntry")
        complexity = @(16, 16, 14, 13, 13, 13, 10, 9)
        api_key = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    @{
        id = "010"
        file = "V12_002.UI.IPC.Commands.Config.cs"
        methods = @("TryApplyConfigTarget_Value", "HandleTrimCommand", "TryApplyConfigTarget_Type")
        complexity = @(17, 11, 11)
        api_key = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    @{
        id = "011"
        file = "V12_002.UI.IPC.Commands.Fleet.cs"
        methods = @("TryHandleFleetCommand", "TryHandleFleet_CancelAll", "CancelAll_ProcessSingleFleetAccount", "TryHandleFleet_MoveTarget", "CancelAll_ProcessMasterAccount", "TryHandleFleet_LongShort")
        complexity = @(19, 19, 18, 15, 14, 11)
        api_key = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    @{
        id = "012"
        file = "V12_002.UI.IPC.cs"
        methods = @("IsSymbolMatch", "ProcessIpcCommands", "TryParseTargetMode", "ProcessIpcCommandCore", "IsAllowedIpcAction")
        complexity = @(18, 14, 13, 13, 10)
        api_key = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    @{
        id = "013"
        file = "V12_002.UI.Panel.Construction.cs"
        methods = @("DestroyPanel", "PlacePanel", "CreateSection0_Identity")
        complexity = @(17, 13, 13)
        api_key = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    @{
        id = "014"
        file = "V12_002.UI.Panel.Handlers.cs"
        methods = @("ShowModeSpecificControls", "UpdateTargetVisibility", "AttachExecutionPanelHandlers", "OnSubmitClick", "CollapseAllExecutionControls")
        complexity = @(20, 19, 12, 12, 11)
        api_key = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    },
    @{
        id = "015"
        file = "V12_002.UI.Panel.StateSync.cs"
        methods = @("UpdatePanelState", "SyncPanelConfigFromSnapshot", "SyncLiveTargetRows", "SyncModeChipVisuals")
        complexity = @(16, 15, 10, 9)
        api_key = "bob_prod_bob-admin_V7HJU1JXC5q7bLKAr7o8nYQMwWb3uLVj6U8b3FYjkbDzzYaccrZX5E7U9pxZxTBoiz2xTv7FGBtSW5QaTZppUzr_FFZsSht5Ab1MM5H97Z4jcfTweD36Ym7i11JATwHMbAvu"
    }
)

foreach ($epic in $epics) {
    $scriptFile = "_p0_$($epic.id).sh"
    Write-Host "Customizing $scriptFile..."
    
    # Read template (currently has EPIC-003 data)
    $content = Get-Content $scriptFile -Raw
    
    # Replace epic ID (003 -> XXX)
    $content = $content -replace 'EPIC-003', "EPIC-$($epic.id)"
    $content = $content -replace 'epic_003', "epic_$($epic.id)"
    $content = $content -replace 'phase0_msg_003', "phase0_msg_$($epic.id)"
    $content = $content -replace '/003\.', "/$($epic.id)."
    
    # Replace API key
    $content = $content -replace 'bob_prod_bob-admin_[A-Za-z0-9_-]+', $epic.api_key
    
    # Replace file name
    $content = $content -replace 'V12_002\.Orders\.Management\.StopSync\.cs', $epic.file
    
    # Replace method count
    $content = $content -replace '\(2 methods in same file\)', "($($epic.methods.Count) methods in same file)"
    
    # Replace method list
    $oldMethods = "- Method 1: SyncLimitTarget" + [Environment]::NewLine + "- Method 2: SyncStopTarget"
    $newMethods = ($epic.methods | ForEach-Object { $i = [array]::IndexOf($epic.methods, $_) + 1; "- Method ${i}: $_" }) -join [Environment]::NewLine
    $content = $content -replace [regex]::Escape($oldMethods), $newMethods
    
    # Replace complexity list
    $content = $content -replace '\[17, 9\]', "[$($epic.complexity -join ', ')]"
    $content = $content -replace 'Complexity: 17, 9', "Complexity: $($epic.complexity -join ', ')"
    
    # Replace first method name (for jCodemunch queries)
    $content = $content -replace "symbol='SyncLimitTarget'", "symbol='$($epic.methods[0])'"
    $content = $content -replace 'symbol_id=''SyncLimitTarget''', "symbol_id='$($epic.methods[0])'"
    
    # Replace method details in hotspot template
    $oldDetails = "- **Method 1**: SyncLimitTarget (CYC=17)" + [Environment]::NewLine + "- **Method 2**: SyncStopTarget (CYC=9)"
    $newDetails = ($epic.methods | ForEach-Object {
        $i = [array]::IndexOf($epic.methods, $_)
        "- **Method $($i+1)**: $_ (CYC=$($epic.complexity[$i]))"
    }) -join [Environment]::NewLine
    $content = $content -replace [regex]::Escape($oldDetails), $newDetails
    
    # Replace JSON arrays
    $methodsJson = ($epic.methods | ForEach-Object { '"' + $_ + '"' }) -join ", "
    $complexityJson = $epic.complexity -join ", "
    $content = $content -replace '\["SyncLimitTarget", "SyncStopTarget"\]', "[$methodsJson]"
    $content = $content -replace '\[17, 9\]', "[$complexityJson]"
    
    # Write customized script
    $content | Set-Content $scriptFile -NoNewline
    
    Write-Host "  ✓ Customized $scriptFile"
}

Write-Host ""
Write-Host "All 10 scripts customized successfully!"
Write-Host "Next: Upload to VM and launch with tmux"

# Made with Bob
