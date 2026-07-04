# Ticket 1 Completion — EPIC-W7-107

## Ticket: T1 — Extract HasExistingFsmForAccount

**Epic:** EPIC-W7-107  
**Method:** HydrateFromOpenPositions  
**Source:** src/V12_002.SIMA.Lifecycle.cs  
**Agent:** v12-p5-ticket  
**Wave:** 7  

## Work Performed

Extracted the inline FSM-account check (original lines 639–644) into a dedicated pure-query helper.

### Extracted Method

```csharp
private bool HasExistingFsmForAccount(Account acct)
{
    return _followerBrackets.Values.Any(f =>
        string.Equals(f.AccountName, acct.Name, StringComparison.OrdinalIgnoreCase)
    );
}
```

### Replacement in Parent

```csharp
if (HasExistingFsmForAccount(acct))
    continue;
```

## Metrics

| Metric | Value |
|--------|-------|
| CYC (extracted helper) | 1 |
| LOC (extracted helper) | 4 |
| DNA compliance | Zero lock(), ASCII-only, no allocation |
| Build result | 0 errors |

## Status: COMPLETED
