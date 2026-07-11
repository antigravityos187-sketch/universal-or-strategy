# EPIC-W7-069 — Phase 0: Hotspot Analysis
**Wave:** 7 | **Phase:** 0 | **Method:** `GetFsmExpectedPosition` | **CYC:** 0

---

## 1. Symbol Identity

| Field            | Value                                                            |
|------------------|------------------------------------------------------------------|
| Method           | `GetFsmExpectedPosition(string accountName)`                     |
| Declared in      | `src/V12_002.Symmetry.BracketFSM.cs` · line 422                 |
| Visibility       | `private`                                                        |
| Return type      | `int` (signed net expected contracts)                            |
| Class            | `V12_002 : Strategy` (partial)                                  |
| Build introduced | Build 1105 — sole FSM authority                                  |
| Cyclomatic CYC   | **0** (linear enumeration + single `if`/`else if` branch chain) |

---

## 2. What the Method Does

`GetFsmExpectedPosition` iterates every entry in `_followerBrackets`
(`ConcurrentDictionary<string, FollowerBracketFSM>`) and accumulates a **signed
net contract count** for one named account.

**Inclusion rule — non-terminal states only:**

```
Active | Accepted | Submitted | PendingSubmit | Replacing | Modifying
```

Terminal states `Filled`, `Cancelled`, `Rejected`, and `Disconnected` are
explicitly excluded — contributing `0`.

**Sign resolution:**

- `EntryOrder.OrderAction == Buy || BuyToCover` → `+1`
- All other actions → `−1`
- Multiplied by `EntryOrder.Quantity`

**Hydrated-Active edge case (restart):** When `f.State == Active` but
`f.EntryOrder == null` (broker order reference was terminal before restart), the
method contributes `0` and defers to the REAPER caller to resolve via actual
broker position.

---

## 3. Cyclomatic Complexity Rationale (CYC 0)

The reported CYC is **0** because the method contains no decision points that
branch to independent termination paths:

- The `foreach` loop is a single-exit linear scan.
- The null/account filter `continue` statements are guard clauses, not
  independent path forks.
- The inner `if/else if` for `EntryOrder != null` is a data-selection branch,
  not a logic fork with independent outcomes.
- No early `return`, no `try/catch`, no nested loops with conditional exits.

This is a pure **aggregation kernel** — its complexity lives in the state space
it queries (`FollowerBracketState` × N FSMs), not in its own control flow.

---

## 4. Blast Radius

| Consumer                                                       | File                                                | Role                                              |
|----------------------------------------------------------------|-----------------------------------------------------|---------------------------------------------------|
| `V12_002.REAPER.Audit.cs` line 404                             | `src/V12_002.REAPER.Audit.cs`                       | **Primary caller** — sole direct call site        |
| `expectedPositions` dict (master)                              | `src/V12_002.cs` line 664                           | Parallel authority path (master accounts, legacy) |
| `_followerBrackets` dictionary (40+ references)                | 14 source files                                     | Data source mutated by all bracket lifecycle code |
| `DrainAccountMailbox` → `ProcessBracketEvent`                  | `src/V12_002.Symmetry.BracketFSM.cs` lines 88–414  | State mutations that change what this method sees |
| `TryTerminateFollowerBracket` (REAPER post-call)               | `src/V12_002.Symmetry.BracketFSM.cs` line 127       | Acts on the value returned by this method         |

**Downstream risk:** Any mutation of `_followerBrackets` state without going
through `ProcessBracketEvent` (e.g., direct writes in `SIMA.Dispatch`,
`SIMA.Lifecycle`, `Orders.Callbacks`) bypasses the FSM guard and can silently
corrupt the value `GetFsmExpectedPosition` returns.

---

## 5. Hotspot Classification

| Attribute             | Finding                                                           |
|-----------------------|-------------------------------------------------------------------|
| Structural risk       | **Low** — method is a read-only aggregator, no mutations          |
| State-space risk      | **Medium** — correctness depends on all 40+ `_followerBrackets`  |
|                       | mutation sites being state-consistent before this is called       |
| Concurrency risk      | **Medium** — `ConcurrentDictionary` iteration is snapshot-safe   |
|                       | but `f.EntryOrder` / `f.State` field reads are not atomic;        |
|                       | a partial-update race during `Replacing` → `Active` transition   |
|                       | could yield a stale `EntryOrder` read                             |
| Restart edge case     | **High** — hydrated-Active + null EntryOrder falls through to `0` |
|                       | silently; REAPER caller must compensate (line 409–429)            |
| Observability gap     | No `Print` / telemetry emitted when `0` is returned for a        |
|                       | non-terminal FSM (silent under-count)                             |

---

## 6. Key Invariants to Preserve

1. `GetFsmExpectedPosition` **must remain read-only** — no FSM state mutations.
2. The six non-terminal states listed are **load-bearing** — removing any causes
   REAPER to under-count inflight contracts.
3. The `Disconnected` state is intentionally excluded — positions are frozen and
   unverifiable; REAPER must handle separately.
4. Master accounts must **never** be routed here — they use `expectedPositions`.

---

## 7. References

- `src/V12_002.Symmetry.BracketFSM.cs` lines 422–460 — method body
- `src/V12_002.REAPER.Audit.cs` lines 402–430 — sole call site + hydration fix
- `src/V12_002.cs` line 661 — deprecation comment for `expectedPositions`
- EPIC build notes: Build 982 (BracketFSM Phase 2), Build 1105 (FSM sole authority)
