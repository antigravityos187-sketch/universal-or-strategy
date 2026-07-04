# Ticket 2 Completion — EPIC-W7-107

## Ticket: T2 — Extract TryGetAccountOpenPosition

**Epic:** EPIC-W7-107  
**Method:** HydrateFromOpenPositions  
**Source:** src/V12_002.SIMA.Lifecycle.cs  
**Agent:** v12-p5-ticket  
**Wave:** 7  

## Work Performed

Extracted the open-position lookup (original lines 647–651) into a pure-query helper using the Try-pattern.

### Extracted Method

```csharp
private bool TryGetAccountOpenPosition(Account acct, out Position pos)
{
    pos = acct.Positions.FirstOrDefault(p =>
        p.Instrument.FullName == Instrument.FullName && p.MarketPosition != MarketPosition.Flat
    );
    return pos != null;
}
```

### Replacement in Parent

```csharp
if (!TryGetAccountOpenPosition(acct, out Position acctPos))
    continue;
```

## Metrics

| Metric | Value |
|--------|-------|
| CYC (extracted helper) | 2 |
| LOC (extracted helper) | 5 |
| DNA compliance | Zero lock(), ASCII-only, pure query |
| Build result | 0 errors |

## Status: COMPLETED
