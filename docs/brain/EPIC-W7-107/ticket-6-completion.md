# Ticket 6 Completion — EPIC-W7-107

## Ticket: T6 — Extract LinkTargetOrdersToFsm + LinkSingleTargetOrder

**Epic:** EPIC-W7-107  
**Method:** HydrateFromOpenPositions  
**Source:** src/V12_002.SIMA.Lifecycle.cs  
**Agent:** v12-p5-ticket  
**Wave:** 7  

## Work Performed

Extracted the 5x repeated target-order linking blocks (original lines 713–759) into two helpers: a dispatcher `LinkTargetOrdersToFsm` that calls a single-slot helper `LinkSingleTargetOrder`, eliminating the repeated pattern entirely.

### Extracted Methods

```csharp
private void LinkTargetOrdersToFsm(
    FollowerBracketFSM fsm,
    string recoveredKey,
    ConcurrentDictionary<string, Order> target1Orders,
    ConcurrentDictionary<string, Order> target2Orders,
    ConcurrentDictionary<string, Order> target3Orders,
    ConcurrentDictionary<string, Order> target4Orders,
    ConcurrentDictionary<string, Order> target5Orders,
    ref int ordersIndexed
)
{
    LinkSingleTargetOrder(fsm, 0, recoveredKey, target1Orders, ref ordersIndexed);
    LinkSingleTargetOrder(fsm, 1, recoveredKey, target2Orders, ref ordersIndexed);
    LinkSingleTargetOrder(fsm, 2, recoveredKey, target3Orders, ref ordersIndexed);
    LinkSingleTargetOrder(fsm, 3, recoveredKey, target4Orders, ref ordersIndexed);
    LinkSingleTargetOrder(fsm, 4, recoveredKey, target5Orders, ref ordersIndexed);
}

private void LinkSingleTargetOrder(
    FollowerBracketFSM fsm,
    int targetIndex,
    string key,
    ConcurrentDictionary<string, Order> targetDict,
    ref int ordersIndexed
)
{
    if (targetDict.TryGetValue(key, out Order targetOrd) && targetOrd != null)
    {
        fsm.Targets[targetIndex] = targetOrd;
        if (!string.IsNullOrEmpty(targetOrd.OrderId))
        {
            _orderIdToFsmKey[targetOrd.OrderId] = key;
            ordersIndexed++;
        }
    }
}
```

### Replacement in Parent

```csharp
LinkTargetOrdersToFsm(fsm, recoveredKey, target1Orders, target2Orders, target3Orders, target4Orders, target5Orders, ref ordersIndexed);
```

## Metrics

| Metric | Value |
|--------|-------|
| CYC (LinkTargetOrdersToFsm) | 1 |
| CYC (LinkSingleTargetOrder) | 4 |
| LOC (LinkTargetOrdersToFsm) | 15 |
| LOC (LinkSingleTargetOrder) | 12 |
| Lines removed from parent | 47 |
| DNA compliance | Zero lock(), ASCII-only |
| Build result | 0 errors |

## Status: COMPLETED
