# Ticket 4 Completion — EPIC-W7-107

## Ticket: T4 — Extract BuildPositionRecoveryFSM

**Epic:** EPIC-W7-107  
**Method:** HydrateFromOpenPositions  
**Source:** src/V12_002.SIMA.Lifecycle.cs  
**Agent:** v12-p5-ticket  
**Wave:** 7  

## Work Performed

Extracted the `FollowerBracketFSM` object initializer (original lines 692–700) into a pure factory method. Zero side-effects.

### Extracted Method

```csharp
private FollowerBracketFSM BuildPositionRecoveryFSM(Account acct, string recoveredKey, Position acctPos)
{
    return new FollowerBracketFSM
    {
        AccountName = acct.Name,
        EntryName = recoveredKey,
        State = FollowerBracketState.Active,
        RemainingContracts = Math.Abs(acctPos.Quantity),
        LastUpdateUtc = DateTime.UtcNow,
        EntryOrder = null,
    };
}
```

### Replacement in Parent

```csharp
var fsm = BuildPositionRecoveryFSM(acct, recoveredKey, acctPos);
```

## Metrics

| Metric | Value |
|--------|-------|
| CYC (extracted helper) | 1 |
| LOC (extracted helper) | 9 |
| DNA compliance | Zero lock(), ASCII-only, pure factory |
| Build result | 0 errors |

## Status: COMPLETED
