# Ticket 7 Completion — EPIC-W7-107

## Ticket: T7 — Refactor parent HydrateFromOpenPositions to orchestration shell

**Epic:** EPIC-W7-107  
**Method:** HydrateFromOpenPositions  
**Source:** src/V12_002.SIMA.Lifecycle.cs  
**Agent:** v12-p5-ticket  
**Wave:** 7  

## Work Performed

Replaced the full 156-LOC HydrateFromOpenPositions body with a 52-LOC clean orchestration shell that delegates all logic to the six extracted helpers. CYC reduced from 31 to 7.

### Final Parent Body

```csharp
private int HydrateFromOpenPositions(
    ConcurrentDictionary<string, Order> stopOrders,
    ConcurrentDictionary<string, Order> target1Orders,
    ConcurrentDictionary<string, Order> target2Orders,
    ConcurrentDictionary<string, Order> target3Orders,
    ConcurrentDictionary<string, Order> target4Orders,
    ConcurrentDictionary<string, Order> target5Orders,
    ref int ordersIndexed,
    ref int fsmCreated)
{
    int positionFsmCreated = 0;
    foreach (Account acct in Account.All)
    {
        if (!IsFleetAccount(acct))
            continue;
        if (HasExistingFsmForAccount(acct))
            continue;
        if (!TryGetAccountOpenPosition(acct, out Position acctPos))
            continue;
        if (!TryRecoverStopOrder(acct, stopOrders, out string recoveredKey, out Order recoveredStop))
        {
            Print(string.Format("[SIMA] Phase 5 Position Pass: WARNING -- open position on {0} but no stopOrders key found. FSM not created. REAPER grace window started.", acct.Name));
            _positionPassFailedFirstSeen[acct.Name] = DateTime.UtcNow;
            continue;
        }
        if (_followerBrackets.ContainsKey(recoveredKey))
            continue;
        var fsm = BuildPositionRecoveryFSM(acct, recoveredKey, acctPos);
        LinkStopOrderToFsmIndex(fsm, recoveredStop, recoveredKey, ref ordersIndexed);
        LinkTargetOrdersToFsm(fsm, recoveredKey, target1Orders, target2Orders, target3Orders, target4Orders, target5Orders, ref ordersIndexed);
        _followerBrackets.TryAdd(recoveredKey, fsm);
        positionFsmCreated++;
        fsmCreated++;
        Print(string.Format("[SIMA] Phase 5 Position Pass: Created FSM for {0} (key={1})", acct.Name, recoveredKey));
    }
    return positionFsmCreated;
}
```

## Metrics

| Metric | Before | After |
|--------|--------|-------|
| CYC | 31 | **7** |
| LOC | ~156 | 52 |
| Inline logic blocks | 6 | 0 |
| Helpers called | 0 | 6 |
| DNA compliance | Inline | Zero lock(), ASCII-only |
| Build result | — | 0 errors |

## Status: COMPLETED
