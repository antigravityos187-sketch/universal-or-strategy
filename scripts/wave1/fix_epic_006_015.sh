#!/bin/bash
# Fix EPIC-006 through EPIC-015 scripts by customizing from template
# Uses sed to replace 4 key sections in each script

set -e
cd "$(dirname "$0")"

echo "Creating corrected scripts for EPIC-006 through EPIC-015..."

# EPIC-006: V12_002.SIMA.Lifecycle.cs (9 methods)
sed 's/EPIC-003/EPIC-006/g; s/SyncLimitTarget/AdoptFleetWorkingOrders/g; s/SyncStopTarget/ClassifyAndRouteFleetOrder/g; s/V12_002.Orders.Management.StopSync.cs/V12_002.SIMA.Lifecycle.cs/g; s/17, 9/17, 16, 12, 12, 11, 10, 9, 9, 9/g' _p0_003.sh > _p0_006_corrected.sh
chmod +x _p0_006_corrected.sh

# EPIC-007: V12_002.SIMA.Shadow.cs (2 methods)
sed 's/EPIC-003/EPIC-007/g; s/SyncLimitTarget/ShadowPropagateStopMoves/g; s/SyncStopTarget/ShadowProcessFollowerStopUpdate/g; s/V12_002.Orders.Management.StopSync.cs/V12_002.SIMA.Shadow.cs/g; s/17, 9/20, 12/g' _p0_003.sh > _p0_007_corrected.sh
chmod +x _p0_007_corrected.sh

# EPIC-008: V12_002.Symmetry.Replace.cs (5 methods)
sed 's/EPIC-003/EPIC-008/g; s/SyncLimitTarget/SymmetryGuardReplaceExistingFollowerTarget/g; s/SyncStopTarget/SymmetryGuardTryResolveFollowersForDispatch/g; s/V12_002.Orders.Management.StopSync.cs/V12_002.Symmetry.Replace.cs/g; s/17, 9/18, 14, 12, 11, 10/g' _p0_003.sh > _p0_008_corrected.sh
chmod +x _p0_008_corrected.sh

# EPIC-009: V12_002.UI.Compliance.cs (8 methods)
sed 's/EPIC-003/EPIC-009/g; s/SyncLimitTarget/IsOrderAllowed/g; s/SyncStopTarget/HandleFleetTargetFill/g; s/V12_002.Orders.Management.StopSync.cs/V12_002.UI.Compliance.cs/g; s/17, 9/16, 15, 14, 13, 12, 11, 10, 9/g' _p0_003.sh > _p0_009_corrected.sh
chmod +x _p0_009_corrected.sh

# EPIC-010: V12_002.UI.IPC.Commands.Config.cs (3 methods)
sed 's/EPIC-003/EPIC-010/g; s/SyncLimitTarget/TryApplyConfigTarget_Value/g; s/SyncStopTarget/HandleTrimCommand/g; s/V12_002.Orders.Management.StopSync.cs/V12_002.UI.IPC.Commands.Config.cs/g; s/17, 9/17, 11, 11/g' _p0_003.sh > _p0_010_corrected.sh
chmod +x _p0_010_corrected.sh

# EPIC-011: V12_002.UI.IPC.Commands.Fleet.cs (6 methods)
sed 's/EPIC-003/EPIC-011/g; s/SyncLimitTarget/TryHandleFleetCommand/g; s/SyncStopTarget/TryHandleFleet_CancelAll/g; s/V12_002.Orders.Management.StopSync.cs/V12_002.UI.IPC.Commands.Fleet.cs/g; s/17, 9/19, 17, 15, 14, 12, 11/g' _p0_003.sh > _p0_011_corrected.sh
chmod +x _p0_011_corrected.sh

# EPIC-012: V12_002.UI.IPC.cs (5 methods)
sed 's/EPIC-003/EPIC-012/g; s/SyncLimitTarget/IsSymbolMatch/g; s/SyncStopTarget/ProcessIpcCommands/g; s/V12_002.Orders.Management.StopSync.cs/V12_002.UI.IPC.cs/g; s/17, 9/18, 16, 13, 11, 10/g' _p0_003.sh > _p0_012_corrected.sh
chmod +x _p0_012_corrected.sh

# EPIC-013: V12_002.UI.Panel.Construction.cs (3 methods)
sed 's/EPIC-003/EPIC-013/g; s/SyncLimitTarget/DestroyPanel/g; s/SyncStopTarget/PlacePanel/g; s/V12_002.Orders.Management.StopSync.cs/V12_002.UI.Panel.Construction.cs/g; s/17, 9/17, 13, 13/g' _p0_003.sh > _p0_013_corrected.sh
chmod +x _p0_013_corrected.sh

# EPIC-014: V12_002.UI.Panel.Handlers.cs (5 methods)
sed 's/EPIC-003/EPIC-014/g; s/SyncLimitTarget/ShowModeSpecificControls/g; s/SyncStopTarget/UpdateTargetVisibility/g; s/V12_002.Orders.Management.StopSync.cs/V12_002.UI.Panel.Handlers.cs/g; s/17, 9/20, 17, 14, 12, 11/g' _p0_003.sh > _p0_014_corrected.sh
chmod +x _p0_014_corrected.sh

# EPIC-015: V12_002.UI.Panel.StateSync.cs (4 methods)
sed 's/EPIC-003/EPIC-015/g; s/SyncLimitTarget/UpdatePanelState/g; s/SyncStopTarget/SyncPanelConfigFromSnapshot/g; s/V12_002.Orders.Management.StopSync.cs/V12_002.UI.Panel.StateSync.cs/g; s/17, 9/16, 15, 10, 9/g' _p0_003.sh > _p0_015_corrected.sh
chmod +x _p0_015_corrected.sh

echo "✅ Created 10 corrected scripts"
echo ""
echo "Files created:"
ls -lh _p0_00{6..9}_corrected.sh _p0_01{0..5}_corrected.sh 2>/dev/null || echo "Some files may not exist yet"
echo ""
echo "Next steps:"
echo "1. Upload these scripts to VM"
echo "2. Run launch script to execute all 10 epics"

# Made with Bob
