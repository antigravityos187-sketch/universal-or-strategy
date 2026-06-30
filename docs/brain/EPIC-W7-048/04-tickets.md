# Phase 4: Implementation Tickets — EPIC-W7-048

**Epic:** EPIC-W7-048
**Method:** `UpdateExistingPendingReplacement`
**Source:** [`src/V12_002.Trailing.StopUpdate.cs`](src/V12_002.Trailing.StopUpdate.cs:167)
**Original CYC:** 15 (live jcodemunch index; tool-artefact of 0 in Phase 0 was caused by CYC analyzers not counting lambda decision paths)
**Wave:** 7 | **Phase:** 4 — Ticket Generation

---

## ticket_count: 2

---

## Ticket 1

- **ticket_id:** 1
- **helper_name:** `TryActivateCircuitBreaker`
- **concern:** Isolate circuit breaker activation — threshold comparison, state mutation (`circuitBreakerActive = true`, `circuitBreakerActivatedTime = DateTime.Now`), and diagnostic `Print()`. Removes inline breaker check that duplicates logic with `InitiateStopReplacement`.
- **lines_to_move:** The conditional block immediately following `Interlocked.Increment(ref pendingReplacementCount)` in `UpdateExistingPendingReplacement`: the `if (currentCount >= CIRCUIT_BREAKER_THRESHOLD && !circuitBreakerActive)` body including both field assignments and the `Print(string.Format(...))` call.
- **signature:** `private void TryActivateCircuitBreaker(int currentCount)`
- **call_site_replacement:** Replace inline block with `TryActivateCircuitBreaker(currentCount);`
- **placement:** New private method in `src/V12_002.Trailing.StopUpdate.cs` (same partial class)
- **cyc_reduction:** −2 from parent (removes 1 `if` + 1 `&&` sub-condition)
- **projected_helper_cyc:** 3
- **implementation_sketch:**
```csharp
private void TryActivateCircuitBreaker(int currentCount)
{
    if (currentCount >= CIRCUIT_BREAKER_THRESHOLD && !circuitBreakerActive)
    {
        circuitBreakerActive = true;
        circuitBreakerActivatedTime = DateTime.Now;
        Print(
            string.Format(
                "V8.30: CIRCUIT BREAKER ACTIVATED - {0} pending replacements (threshold: {1})",
                currentCount,
                CIRCUIT_BREAKER_THRESHOLD
            )
        );
    }
}
```

---

## Ticket 2

- **ticket_id:** 2
- **helper_name:** `BuildRefreshedPendingReplacement`
- **concern:** Isolate `AddOrUpdate` update-factory lambda body — conditionally calls `RefreshTargetSnapshot`, derives updated `BracketRestorationNeeded`, and constructs the returned `PendingStopReplacement` struct. Eliminates 4–5 decision paths from the parent method.
- **lines_to_move:** The entire body of the `updateValueFactory` lambda passed to `pendingStopReplacements.AddOrUpdate`: the ternary `!pending.BracketRestorationNeeded ? RefreshTargetSnapshot(entryName) : pending.CapturedTargets`, the compound `&&` expression deriving `restorationNeeded`, and the `new PendingStopReplacement { ... }` struct initializer with all field assignments.
- **signature:** `private PendingStopReplacement BuildRefreshedPendingReplacement(string entryName, PendingStopReplacement pending, double validatedStopPrice)`
- **call_site_replacement:** Replace lambda body with `BuildRefreshedPendingReplacement(entryName, pending, validatedStopPrice)`
- **placement:** New private method in `src/V12_002.Trailing.StopUpdate.cs` (same partial class)
- **cyc_reduction:** −9 from parent (removes 1 ternary + 1 `&&` BracketRestorationNeeded check + 1 `&&` null check + 1 `&&` length check + 1 `||` in restoration assignment + conditional struct field expressions)
- **projected_helper_cyc:** 6
- **implementation_sketch:**
```csharp
private PendingStopReplacement BuildRefreshedPendingReplacement(
    string entryName,
    PendingStopReplacement pending,
    double validatedStopPrice
)
{
    var refreshedTargets = !pending.BracketRestorationNeeded
        ? RefreshTargetSnapshot(entryName)
        : pending.CapturedTargets;
    var restorationNeeded =
        !pending.BracketRestorationNeeded
        && refreshedTargets != null
        && refreshedTargets.Length > 0;
    return new PendingStopReplacement
    {
        EntryName = pending.EntryName,
        Quantity = pending.Quantity,
        StopPrice = validatedStopPrice,
        Direction = pending.Direction,
        OldOrder = pending.OldOrder,
        CreatedTime = pending.CreatedTime,
        CapturedTargets = refreshedTargets ?? pending.CapturedTargets,
        BracketRestorationNeeded = restorationNeeded || pending.BracketRestorationNeeded,
    };
}
```

---

## projected_parent_cyc_after_all: 4

**CYC decomposition (parent after both extractions):**
- 1 base
- +1 `if(TryAdd)` branch
- +2 compound `&&` in `newPending` initializer (`_b955TargetsA != null && _b955TargetsA.Length > 0`)

---

## Jane Street Compliance

| Method | Projected CYC | Threshold | Status |
|---|---|---|---|
| `UpdateExistingPendingReplacement` (parent) | 4 | 8 | **PASS** |
| `TryActivateCircuitBreaker` | 3 | 8 | **PASS** |
| `BuildRefreshedPendingReplacement` | 6 | 8 | **PASS** |
| **max_cyc_projected** | **6** | **8** | **PASS** |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-048 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Bobcoins Used** | 3.0 |
| **Execution Time** | 2026-06-29T01:30:00Z |
| **jcodemunch tools called** | resolve_repo, search_symbols, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 validation thoughts) |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 4 |
| **Original CYC** | 15 (live index; Phase 0 reported 0 as tool artefact) |
