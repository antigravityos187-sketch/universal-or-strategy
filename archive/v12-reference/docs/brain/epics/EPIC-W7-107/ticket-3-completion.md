# Ticket 3 Completion — EPIC-W7-107

## Ticket: T3 — Extract TryRecoverStopOrder

**Epic:** EPIC-W7-107  
**Method:** HydrateFromOpenPositions  
**Source:** src/V12_002.SIMA.Lifecycle.cs  
**Agent:** v12-p5-ticket  
**Wave:** 7  

## Work Performed

Extracted the stopOrders scan + null-guard block (original lines 653–686) into a single Try-pattern helper. The REAPER grace window print and `_positionPassFailedFirstSeen` assignment remain in the parent call site.

### Extracted Method

```csharp
private bool TryRecoverStopOrder(
    Account acct,
    ConcurrentDictionary<string, Order> stopOrders,
    out string recoveredKey,
    out Order recoveredStop
)
{
    recoveredKey = null;
    recoveredStop = null;
    foreach (var stopKvp in stopOrders.ToArray())
    {
        Order stopCand = stopKvp.Value;
        if (stopCand == null)
            continue;
        if (stopCand.Account == null)
            continue;
        if (string.Equals(stopCand.Account.Name, acct.Name, StringComparison.OrdinalIgnoreCase))
        {
            recoveredKey = stopKvp.Key;
            recoveredStop = stopCand;
            return true;
        }
    }
    return false;
}
```

## Metrics

| Metric | Value |
|--------|-------|
| CYC (extracted helper) | 5 |
| LOC (extracted helper) | 19 |
| DNA compliance | Zero lock(), ASCII-only |
| Build result | 0 errors |

## Status: COMPLETED
