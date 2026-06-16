# EPIC-CCN-016 Phase 5 Manual Completion Plan

**Status**: Bob CLI Silent Failure - Manual Completion Required
**Date**: 2026-06-16T06:50:00Z
**Reason**: Bob CLI crashed after TICKET-1, no completion files created

## Current State

### Completed
- ✅ **TICKET-1**: `IsOrderCancellable` helper extracted (lines 224-236)
- ✅ Method reduced from CYC 19 → CYC 8

### Remaining Work
- ❌ **TICKET-2**: Extract `IsProtectedOrderName` (lines 204-212)
- ❌ **TICKET-3**: Extract `CancelAll_ProcessNonSIMAAccount` (lines 197-218)
- ❌ **TICKET-4**: Final verification

## Manual Completion Strategy

### Option A: Complete Manually (Recommended - 15 minutes)
1. Extract `IsProtectedOrderName` helper (5 min)
2. Extract `CancelAll_ProcessNonSIMAAccount` helper (5 min)
3. Create ticket completion files manually (5 min)
4. Proceed to Phase 6

**Advantages**:
- Fast (15 minutes vs 30-45 for Bob retry)
- Reliable (no tool dependency)
- Full control over extraction

**Disadvantages**:
- Manual work (but simple extractions)
- No automated verification

### Option B: Retry Bob CLI (Not Recommended - 30-45 minutes)
1. Debug why Bob crashed
2. Restart Phase 5 with fresh session
3. Wait for completion
4. Risk of another silent failure

## Recommendation

**Proceed with Option A (Manual Completion)**:

1. **User Action Required**: Extract remaining helpers manually
2. **Create Completion Files**: Document each ticket's completion
3. **Verify Build**: Ensure compilation passes
4. **Proceed to Phase 6**: Use phase-6-review MCP tool

## Extraction Details

### TICKET-2: IsProtectedOrderName

**Current Code** (lines 204-212):
```csharp
string oName = order.Name;
if (
    oName.StartsWith("Stop_")
    || oName.StartsWith("S_")
    || oName.StartsWith("T1_")
    || oName.StartsWith("T2_")
    || oName.StartsWith("T3_")
    || oName.StartsWith("T4_")
    || oName.StartsWith("T5_")
)
    continue;
```

**Extract To**:
```csharp
private bool IsProtectedOrderName(string orderName)
{
    return orderName.StartsWith("Stop_")
        || orderName.StartsWith("S_")
        || orderName.StartsWith("T1_")
        || orderName.StartsWith("T2_")
        || orderName.StartsWith("T3_")
        || orderName.StartsWith("T4_")
        || orderName.StartsWith("T5_");
}
```

**Replace With**:
```csharp
if (IsProtectedOrderName(order.Name))
    continue;
```

### TICKET-3: CancelAll_ProcessNonSIMAAccount

**Current Code** (lines 196-219):
```csharp
else
{
    int cancelled = 0;
    foreach (Order order in Account.Orders)
    {
        if (!IsOrderCancellable(order))
            continue;

        string oName = order.Name;
        if (
            oName.StartsWith("Stop_")
            || oName.StartsWith("S_")
            || oName.StartsWith("T1_")
            || oName.StartsWith("T2_")
            || oName.StartsWith("T3_")
            || oName.StartsWith("T4_")
            || oName.StartsWith("T5_")
        )
            continue;

        CancelOrderOnAccount(order, order.Account);
        cancelled++;
    }
    Print($"[V12] CANCEL_ALL -> Cancelled {cancelled} pending entry orders");
}
```

**Extract To**:
```csharp
private int CancelAll_ProcessNonSIMAAccount()
{
    int cancelled = 0;
    foreach (Order order in Account.Orders)
    {
        if (!IsOrderCancellable(order))
            continue;

        if (IsProtectedOrderName(order.Name))
            continue;

        CancelOrderOnAccount(order, order.Account);
        cancelled++;
    }
    Print($"[V12] CANCEL_ALL -> Cancelled {cancelled} pending entry orders");
    return cancelled;
}
```

**Replace With**:
```csharp
else
{
    CancelAll_ProcessNonSIMAAccount();
}
```

## Expected Final State

**Method**: `TryHandleFleet_CancelAll` (lines 177-222)
- **Final CYC**: ≤5 (target met)
- **Final Length**: ~15 lines (from 46 lines)
- **Helpers Created**: 3 (IsOrderCancellable, IsProtectedOrderName, CancelAll_ProcessNonSIMAAccount)

## Next Steps

1. **User**: Manually apply TICKET-2 and TICKET-3 extractions
2. **User**: Create ticket completion files (ticket-2-completion.md, ticket-3-completion.md, ticket-4-completion.md)
3. **Agent**: Execute Phase 6 using phase-6-review MCP tool
4. **Agent**: Commit all files and update roadmap to 80/80

## Time Estimate

- Manual extraction: 10 minutes
- Completion files: 5 minutes
- Phase 6: 15 minutes
- **Total**: 30 minutes to Wave 4 completion (100%)