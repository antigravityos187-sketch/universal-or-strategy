# Ticket 5 Completion — EPIC-W7-107

## Ticket: T5 — Extract LinkStopOrderToFsmIndex

**Epic:** EPIC-W7-107  
**Method:** HydrateFromOpenPositions  
**Source:** src/V12_002.SIMA.Lifecycle.cs  
**Agent:** v12-p5-ticket  
**Wave:** 7  

## Work Performed

Extracted the stop-order linking block (original lines 702–711) into a focused void helper that handles null-guard, FSM assignment, and `_orderIdToFsmKey` indexing atomically.

### Extracted Method

```csharp
private void LinkStopOrderToFsmIndex(
    FollowerBracketFSM fsm,
    Order recoveredStop,
    string recoveredKey,
    ref int ordersIndexed
)
{
    if (recoveredStop == null)
        return;
    fsm.StopOrder = recoveredStop;
    if (!string.IsNullOrEmpty(recoveredStop.OrderId))
    {
        _orderIdToFsmKey[recoveredStop.OrderId] = recoveredKey;
        ordersIndexed++;
    }
}
```

### Replacement in Parent

```csharp
LinkStopOrderToFsmIndex(fsm, recoveredStop, recoveredKey, ref ordersIndexed);
```

## Metrics

| Metric | Value |
|--------|-------|
| CYC (extracted helper) | 3 |
| LOC (extracted helper) | 12 |
| DNA compliance | Zero lock(), ASCII-only |
| Build result | 0 errors |

## Status: COMPLETED
