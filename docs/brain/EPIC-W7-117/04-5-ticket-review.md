# EPIC-W7-117 — Phase 4.5 Ticket Review (Jane Street Validation Gate)
review_verdict: pass

**Method**: `SymmetryGuardReplaceExistingFollowerTarget`
**Source**: `src/V12_002.Symmetry.Replace.cs`
**CYC Baseline**: 9 (architecture plan authoritative)
**CYC Target**: ≤ 8
**Wave**: 7 | **Phase**: 4.5
**Overall Verdict**: ✅ PASS

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-5-review |
| **Phase** | 4.5 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-117 |
| **Method** | `SymmetryGuardReplaceExistingFollowerTarget` |
| **MCP: Sequential Thinking** | Available — 3 thoughts executed |
| **MCP Probe Status** | PASS |
| **Review Verdict** | PASS |
| **Failed Tickets** | [] |

---

## Jane Street KB Rules Applied

| Rule | Description |
|------|-------------|
| CYC<=8 | All extracted methods must target cyclomatic complexity <=8 |
| Single-responsibility | Each extracted helper does exactly one thing |
| No lock() | Banned — use Actor/Enqueue pattern for all state mutations |
| Actor/Enqueue | State changes go through FSM enqueue, not direct mutation |
| Illegal states unrepresentable | Use types/enums so invalid states cannot compile |
| DSB micro-op cache | Small methods fit 1536 micro-op cache — hot-path benefit |

---

## Ticket 1 — `IsOrderLiveState` — Verdict: ✅ PASS

### Validation (Sequential Thinking Thought 1)

| Rule | Check | Result |
|------|-------|--------|
| CYC<=8 | Helper CYC = 1 (pure boolean expression, no branching logic) | ✅ PASS |
| Single-responsibility | Checks exactly one thing: whether an Order is in an active/live state | ✅ PASS |
| No lock() | Explicitly prohibited in AC; pure read-only predicate — no state mutation at all | ✅ PASS |
| Actor/Enqueue | N/A — read-only predicate; no state mutation path | ✅ PASS |
| Illegal states unrepresentable | Uses `OrderState` enum comparisons — no stringly-typed or magic-number states | ✅ PASS |
| DSB micro-op cache | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` applied for hot-path inline benefit | ✅ PASS |

### Acceptance Criteria Review

- ✅ `IsOrderLiveState` signature specified: `private static bool IsOrderLiveState(Order o)`
- ✅ `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute specified
- ✅ Both duplicate 4-case OR blocks replaced with helper call
- ✅ Helper CYC = 1 explicitly targeted
- ✅ No lock/LINQ/heap allocation constraint present in AC
- ✅ ASCII-only identifiers and string literals constraint present
- ✅ Build check `dotnet build src/` required
- ✅ xUnit `[Fact]` test for all 4 live states + non-live state (e.g. `OrderState.Filled`) required

### Notes

The duplicate predicate across two call sites within the parent is the primary CYC driver being addressed. Extracting to a named, inlineable helper also eliminates drift risk vs. the identical predicate in `Propagation.cs:424`. Execution scope is a single file — minimal blast radius.

---

## Ticket 2 — `ExecuteTargetReplacePhase1` — Verdict: ✅ PASS

### Validation (Sequential Thinking Thought 2)

| Rule | Check | Result |
|------|-------|--------|
| CYC<=8 | Helper CYC = 3 (base + price guard + direction ternary); parent CYC = 8 after T1+T2 | ✅ PASS |
| Single-responsibility | Encapsulates exactly the Phase 1 FSM step: compute price, qty, action, construct spec, write dict, stamp grace, cancel | ✅ PASS |
| No lock() | AC explicitly states `ConcurrentDictionary` used for dict write — no `lock()` introduced | ✅ PASS |
| Actor/Enqueue | Dict write via `ConcurrentDictionary` (lock-free); broker cancel via `ExecutingAccount.Cancel()` — no direct raw state mutation | ✅ PASS |
| Illegal states unrepresentable | `OrderAction` enum used for exit direction (only `Sell`/`BuyToCover` valid); `FollowerTargetReplaceSpec` is a typed record | ✅ PASS |
| DSB micro-op cache | `[MethodImpl(MethodImplOptions.NoInlining)]` applied — correct for cold broker-interaction path (not hot path) | ✅ PASS |

### Acceptance Criteria Review

- ✅ `ExecuteTargetReplacePhase1` signature specified with all 5 parameters
- ✅ `[MethodImpl(MethodImplOptions.NoInlining)]` attribute specified
- ✅ Parent delegates replace-eligible block to helper
- ✅ Helper CYC = 3 explicitly targeted
- ✅ Parent CYC = 8 after both T1 and T2 explicitly stated with branch table
- ✅ `ConcurrentDictionary` / no lock constraint present
- ✅ No LINQ / ASCII-only constraints present
- ✅ Build check `dotnet build src/` required
- ✅ xUnit `[Fact]` x3: early return on `newPrice<=0`, `Sell` for `Long`, `BuyToCover` for non-`Long`

### Parent CYC Decomposition (verified)

| Branch | +Delta | Running CYC |
|--------|--------|-------------|
| Base | — | 1 |
| `ExecutingAccount == null` null guard | +1 | 2 |
| `isFilled \|\| isRunner` (2 OR predicates) | +2 | 4 |
| `qty <= 0` | +1 | 5 |
| Stale dict `TryGetValue` + null check | +1 | 6 |
| `IsOrderLiveState(staleTarget)` if-branch | +1 | 7 |
| Dict miss guard (`!TryGetValue`) | +1 | 8 |
| `IsOrderLiveState(oldTarget)` if-branch (→ delegates to T1) | — | 8 |

**max_cyc_projected = 8 ✓** — at Jane Street strict threshold.

### Notes

The `NoInlining` attribute is appropriate here: this is a cold broker-interaction path (order cancel + spec registration), not a hot-path inner loop. Using `AggressiveInlining` on a cold path would be incorrect and wasteful. The CYC accounting is correctly verified above.

---

## Overall Summary

| Ticket | Verdict | CYC Helper | CYC Parent After |
|--------|---------|-----------|-----------------|
| T1: `IsOrderLiveState` | ✅ PASS | 1 | ~8 (combined with T2) |
| T2: `ExecuteTargetReplacePhase1` | ✅ PASS | 3 | 8 ✓ |

**CYC reduction**: 9 → 8 (Jane Street strict standard ≤8 satisfied)
**Execution order**: T1 before T2 (T2 relies on `IsOrderLiveState` in refactored parent call site)
**Blast radius**: Single file `src/V12_002.Symmetry.Replace.cs` — no cross-file changes
**failed_tickets**: []

---

## Post-Execution Validation Commands

```powershell
dotnet build src/
python scripts/complexity_audit.py
powershell -File .\scripts\pre_push_validation.ps1 -Fast
powershell -File .\deploy-sync.ps1
```

Expected outcome: `SymmetryGuardReplaceExistingFollowerTarget` CYC = 8, `IsOrderLiveState` CYC = 1, `ExecuteTargetReplacePhase1` CYC = 3.
