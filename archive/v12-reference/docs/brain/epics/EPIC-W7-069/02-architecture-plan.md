# EPIC-W7-069 Architecture Plan: GetFsmExpectedPosition
Agent Name: v12-phase2-architecture

## Method Under Analysis
- Method: `GetFsmExpectedPosition`
- File: [`src/V12_002.Symmetry.BracketFSM.cs`](src/V12_002.Symmetry.BracketFSM.cs:422)
- Reported CYC: 0 (placeholder)
- Live CYC: 9-14 (modified cyclomatic, counting each boolean sub-expression; McCabe ~7)
- Line: 422-460 (39 lines)
- Signature: `private int GetFsmExpectedPosition(string accountName)`

## MCP Evidence

### search_symbols (Step 4)
- Symbol confirmed at `src/V12_002.Symmetry.BracketFSM.cs` line 422
- Symbol ID: `src/V12_002.Symmetry.BracketFSM.cs::V12_002.GetFsmExpectedPosition#method`
- Signature: `private int GetFsmExpectedPosition(string accountName)`
- Complexity drivers identified from full source:
  - `foreach` over `_followerBrackets` dictionary
  - Null/account filter: `f == null || f.AccountName != accountName` (2 boolean conditions)
  - 6-way OR state filter: `Active || Accepted || Submitted || PendingSubmit || Replacing || Modifying`
  - `EntryOrder != null` null guard
  - `OrderAction == Buy || OrderAction == BuyToCover` sign determination (2 conditions)
  - `else if (f.State == Active)` hydrated-FSM fallback branch

### get_context_bundle (Step 4)
- Full source retrieved (lines 422-460)
- Docstring: "Computes the net expected position for a given account by summing all non-terminal FollowerBracketFSMs. SOLE authority for follower expected position (Build 1105)."
- Imports: System, Collections.Concurrent, Collections.Generic, Linq, NinjaTrader.Cbi, NinjaScript
- No LINQ used in method body (correct foreach pattern)

### get_call_hierarchy (Step 5)
- Callers: 0 detected by index (index depth limited; scope boundary Phase 1.5 confirmed 1 caller)
- Callees: 0 detected by index (field access and property reads, not indexed as calls)
- Direction: both, depth=2
- Note: The method accesses `_followerBrackets` (ConcurrentDictionary or similar) and `FollowerBracketFSM` properties — no method calls that the index resolves

### get_dependency_graph (Step 6)
- File: `src/V12_002.Symmetry.BracketFSM.cs`
- Imports: 0 edges detected (partial class — dependencies resolved via compilation unit)
- Importers: 0 explicit import edges (accessed as partial class of `V12_002`)
- Blast radius: confined to partial class definition in single file

---

## Sequential Thinking Evidence

### Thought 1 — Real CYC Analysis
Modified cyclomatic (Lizard standard — counts each boolean sub-expression):
- Base: 1
- foreach: +1
- `f == null || f.AccountName != accountName`: +2 (two conditions)
- 6-way OR state check: +6 (six conditions)
- `f.EntryOrder != null`: +1
- `Buy || BuyToCover` ternary: +2 (two conditions)
- `else if Active`: +1
- **Total modified CYC = 14**

McCabe (counts each if/else-if/foreach as one decision):
- Base: 1 + 1(foreach) + 1(null/name if) + 1(state if) + 1(entryOrder if) + 1(sign if) + 1(else-if) = 7

**Live CYC range: 7-14 (over threshold for modified metric)**. Extraction required.

### Thought 2 — Extraction Design
Two helpers identified, aligning with Phase 1.5 scope estimate of 2 helpers:

1. **`IsActiveFollowerState`** — encapsulates the 6-way OR state classification using C# switch expression with `or` pattern syntax (zero branch overhead, CYC=2)
2. **`ComputeEntrySignedQuantity`** — encapsulates the null-guarded sign+quantity computation (CYC=3)

After extraction, main method CYC:
- 1 + 1(foreach) + 2(null||name) + 1(IsActiveFollowerState call) + 1(entryOrder null) + 1(else-if) = **7**

### Thought 3 — Validation
- All helpers: max CYC = 3 <= 8 PASS
- Main method post-extraction: CYC = 7 <= 8 PASS
- No new lock() blocks needed
- No allocation introduced
- Single responsibility per helper confirmed
- max_cyc_projected = 7

---

## Extraction Plan

| Helper Method | Responsibility | Projected CYC | Jane Street Rule |
|---|---|---|---|
| `IsActiveFollowerState(FollowerBracketState state)` | Returns `true` if the state is one of the 6 non-terminal contributing states; uses C# switch `or`-pattern expression — eliminates the 6-way OR chain | 2 | trading_billions: single responsibility; carl_cook: zero-alloc, AggressiveInlining hot |
| `ComputeEntrySignedQuantity(Order entryOrder)` | Returns `entryOrder.Quantity * sign` where sign is +1 for Buy/BuyToCover, -1 otherwise; null-safe contract (caller ensures non-null) | 3 | trading_billions: single responsibility; carl_cook: zero-alloc, no LINQ; gjengset: no mutation |

### Post-extraction `GetFsmExpectedPosition` skeleton
```csharp
private int GetFsmExpectedPosition(string accountName)
{
    int sum = 0;
    foreach (var kvp in _followerBrackets)
    {
        FollowerBracketFSM f = kvp.Value;
        if (f == null || f.AccountName != accountName)
            continue;
        if (!IsActiveFollowerState(f.State))
            continue;
        if (f.EntryOrder != null)
            sum += ComputeEntrySignedQuantity(f.EntryOrder);
        else if (f.State == FollowerBracketState.Active)
        {
            // Hydrated Active FSM — caller handles fallback to broker position
        }
    }
    return sum;
}
```
CYC: 1 + 1(foreach) + 2(null||name) + 1(IsActiveFollowerState) + 1(entryOrder null) + 1(else-if) = **7**

---

## Jane Street Alignment

- **carl_cook**: `IsActiveFollowerState` decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — zero-alloc, pure computation on value type enum. `ComputeEntrySignedQuantity` also AggressiveInlining candidate. No LINQ in either helper.
- **gjengset**: No new `lock()` blocks introduced. Both helpers are read-only (no state mutation). `_followerBrackets` is accessed via existing concurrency model — no changes to lock pattern.
- **trading_billions**: Each helper has exactly one responsibility. `IsActiveFollowerState` = state classification only. `ComputeEntrySignedQuantity` = sign+quantity only. `GetFsmExpectedPosition` = aggregation only. Defense in depth: `IsActiveFollowerState` is the canonical authority for state classification (reusable across callers if needed).

---

## V12.23 Scope Compliance

- ONE CONCERN per helper: PASS
- No pre-existing issues modified: PASS — only `GetFsmExpectedPosition` body changes
- No scope creep: PASS — 2 new private helpers added to same partial class, no other methods touched
- Caller signature unchanged: PASS — `private int GetFsmExpectedPosition(string accountName)` identical
- Cross-file impact: NONE — same partial class, same file

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Epic** | EPIC-W7-069 |
| **Phase** | 2 |
| **Live CYC** | 7-14 (modified: 14, McCabe: 7) |
| **Extractions** | 2 |
| **max_cyc_projected** | 7 |
| **Bobcoins Used** | 1.0 |

## max_cyc_projected: 7
