# EPIC-W7-140 Hotspot Analysis

**Method:** `InitiateStopReplacement`
**CYC (tool-reported):** 0 — ⚠️ Tool could not locate symbol; manual static analysis applied (see note)
**CYC (manual static count):** 10
**File:** `src/V12_002.Trailing.StopUpdate.cs`
**Lines:** 307–369

---

## Symbol Location Note

`mcp__jcodemunch-mcp__search_symbols` and `mcp__jcodemunch-mcp__get_symbol_complexity` returned
CYC = 0, indicating the method was not indexed at the time of analysis (likely because the partial
class span was not resolved). Manual static Cyclomatic Complexity analysis was performed directly
against the source. **All downstream phase work should treat CYC = 10 as the authoritative value.**

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `UpdateStopOrder` (line 128, same file) — invoked when `currentStop.OrderState` is `Working` or `Accepted` |
| **Caller chain** | `BarUpdate` / `Trailing.cs` → `UpdateStopOrder` → `InitiateStopReplacement` |
| **Shared mutable state written** | `pendingStopReplacements` (ConcurrentDictionary, TryAdd), `pendingReplacementCount` (Interlocked), `circuitBreakerActive`, `circuitBreakerActivatedTime`, `pos.CurrentStopPrice`, `pos.CurrentTrailLevel` |
| **Shared state read** | `stopOrders`, `activePositions`, `CIRCUIT_BREAKER_THRESHOLD` |
| **Downstream calls** | `GetTargetOrdersDictionary(int)` ×5 (loop), `CancelOrderForReplace`, `MarkStickyDirty`, `Print` |
| **Files touching `pendingStopReplacements`** | 13 files (Orders.Management, Orders.Callbacks, REAPER, Lifecycle, UI.Compliance, Trailing, V12_002.cs) |
| **Files touching `stopOrders` / `activePositions`** | 43 files — full strategy-wide scope |
| **Threading constraint** | Strategy thread for state writes; `Interlocked` guards counter; `ConcurrentDictionary.TryAdd` is lock-free but `circuitBreakerActive` flag is unguarded bool |
| **Risk on change** | **High** — touches circuit-breaker activation path and the pending-replacement queue that multiple callback files poll; any extraction must preserve `TryAdd`/`Interlocked` atomicity pairing |

**Affected symbol count (blast radius):** 9 directly coupled symbols; 2 shared concurrent state bags;
13 files reference `pendingStopReplacements`; 43 files reference `stopOrders`/`activePositions`.

---

## Top 3 Complexity Drivers

### 1. Inline target-snapshot `for` loop with compound multi-null guard (lines 317–336) — ~4 CYC

The method opens with a `for (_tB = 1; _tB <= 5; _tB++)` that calls `GetTargetOrdersDictionary`
on each iteration, then evaluates a four-clause compound `if`: `_tDB != null`, `TryGetValue` truthy,
`_tOB != null`, and a two-branch `||` over `OrderState.Working || OrderState.Accepted`. Each branch
predicate is a distinct CYC contributor (+1 for `for`, +1 for `if`, +1 for `||`). This entire block
is a verbatim copy of the same pattern in `CaptureTargetSnapshot` and `RefreshTargetSnapshot` —
three separate in-file duplicates performing identical work, creating a strong cohesion smell and
making any future `OrderState` extension a triple-site edit.

### 2. Thread-safe `TryAdd` / circuit-breaker nested `if` block (lines 351–360) — ~3 CYC

The `if (pendingStopReplacements.TryAdd(...))` block contains a nested
`if (currentCount >= CIRCUIT_BREAKER_THRESHOLD && !circuitBreakerActive)` which modifies two
unguarded boolean/DateTime fields (`circuitBreakerActive`, `circuitBreakerActivatedTime`). The
`&&` short-circuit adds +1 CYC; the outer `if` adds +1; the fact that the else branch (TryAdd
fails = duplicate key) is silently swallowed is a latent correctness issue — `UpdateExistingPendingReplacement`
uses `AddOrUpdate` for the duplicate path, but `InitiateStopReplacement` does not. The duplicated
circuit-breaker block also appears verbatim inside `UpdateExistingPendingReplacement`, spreading
the same logic across two methods.

### 3. Ternary-chain level-name formatter (line 367) — ~2 CYC + inline string logic debt

```csharp
string levelName = newTrailLevel <= 0 ? "Initial" : (newTrailLevel == 1 ? "BE" : "T" + (newTrailLevel - 1));
```

This nested ternary adds 2 CYC points (two `?` operators). It is a pure string-formatting concern
embedded directly in an order-management method, violating SRP. The same level-name pattern appears
in `CreateDirectStopOrder` (line 454) as a nearly identical expression (`newTrailLevel == 1 ? "BE" : "T"…`),
making this another dual-site duplicate. Extracting a `FormatTrailLevelName(int)` helper would
eliminate both occurrences and the CYC contribution.

---

## Recommended Extraction Count

**3 extractions recommended:**

| # | Proposed Helper | CYC Removed | Justification |
|---|---|---|---|
| 1 | `SnapshotActiveTargets(string entryName) → TargetSnapshot[]` | ~4 | Deduplicate the for-loop/compound-guard pattern shared with `CaptureTargetSnapshot` and `RefreshTargetSnapshot`; consolidate into a single method |
| 2 | `TryActivateCircuitBreaker()` | ~2 | Deduplicate the `TryAdd` + `Interlocked.Increment` + circuit-breaker guarded block shared with `UpdateExistingPendingReplacement`; also surfaces the unguarded `circuitBreakerActive` write as an explicit concern |
| 3 | `FormatTrailLevelName(int level) → string` | ~2 | Pure helper, eliminates dual-site ternary duplication with `CreateDirectStopOrder`; zero side effects |

**Estimated post-extraction CYC of `InitiateStopReplacement`:** ≤ 3
(base + TryAdd branch + null guard on circuit-breaker helper result)

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~120s |
| **Analysis Method** | Manual static CYC count (tool returned 0 — symbol not indexed) |
| **Timestamp** | 2026-06-26T01:53:51Z |
| **Requires Manual Review** | YES — CYC=0 from tooling; manual count = 10; verify with next indexing pass |
