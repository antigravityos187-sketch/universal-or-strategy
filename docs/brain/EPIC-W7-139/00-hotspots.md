# EPIC-W7-139 Hotspot Analysis

**Method:** UpdateStopOrder
**CYC (tool-reported):** 0 — ⚠️ REQUIRES MANUAL REVIEW (see note below)
**CYC (static manual count):** ~8
**File:** src/V12_002.Trailing.StopUpdate.cs
**Lines:** 84–139

---

## ⚠️ CYC-Zero Flag

The external complexity scanner (`mcp__jcodemunch-mcp`) reported CYC = 0, indicating the method
could **not be resolved** by the tool — most likely because `UpdateStopOrder` lives inside a
`partial class` split across multiple files, which some AST-walking scanners fail to index without
a full multi-file compilation pass.

A **manual decision-point count** of the method body (lines 84–139) yields **CYC ≈ 8**:

| Decision point | +CYC |
|---|---|
| `!stopOrders.TryGetValue` early return | +1 |
| `pendingStopReplacements.TryGetValue` stale-pending gate | +1 |
| `pendingAgeSeconds > STALE_PENDING_FAST_PATH_SEC` | +1 |
| First routing `if` — `CancelPending \|\| Submitted` (compound OR) | +2 |
| Second routing `if` — `Working \|\| Accepted` (compound OR) | +2 |
| `try/catch` exception path | +1 |
| **Total** | **8** |

This document treats the **true CYC as 8** for planning purposes. The tool-reported value of 0 is
an artefact of partial-class resolution failure and must be corrected in the scanner's project
configuration before downstream phases rely on it.

---

## Overview

`UpdateStopOrder` is the central routing dispatcher for all trailing-stop moves in the strategy.
It validates a candidate stop price, inspects the in-flight state of the current stop order, and
branches into one of four execution paths:

1. **Stale-pending fast-path** → `HandleStalePendingReplacement`
2. **Cancel-pending / submitted** → `UpdateExistingPendingReplacement`
3. **Working / accepted** → `InitiateStopReplacement`
4. **No existing stop / uncancellable state** → `CreateDirectStopOrder`

All paths are wrapped in a `try/catch` with emergency-flatten escalation. The method itself is
intentionally thin (a pure dispatcher); the bulk of complexity lives in its four delegates.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct call sites** | 11 call sites across 7 source files |
| **Calling files** | `V12_002.Trailing.cs` (5×), `V12_002.UI.Callbacks.cs` (4×), `V12_002.Trailing.Breakeven.cs` (2×), `V12_002.Symmetry.Replace.cs` (1×), `V12_002.SIMA.Shadow.cs` (1×), `V12_002.Orders.Callbacks.Propagation.cs` (1×), `V12_002.UI.IPC.Commands.Mode.cs` (1×) |
| **Delegate targets** | `HandleStalePendingReplacement`, `UpdateExistingPendingReplacement`, `InitiateStopReplacement`, `CreateDirectStopOrder`, `HandleUpdateException` |
| **Upstream validation** | `ValidateStopPrice` (called before any branch) |
| **Shared mutable state** | `stopOrders` (ConcurrentDictionary), `pendingStopReplacements` (ConcurrentDictionary), `pendingReplacementCount` (Interlocked), `circuitBreakerActive`, `circuitBreakerActivatedTime` |
| **Side-effects** | Mutates `pos.CurrentStopPrice`, `pos.CurrentTrailLevel`; calls `MarkStickyDirty()`; may call `FlattenPositionByName` on exception |
| **Threading constraint** | Strategy thread only; concurrent dicts used for cross-thread safety |
| **Risk on change** | **High** — 11 call sites; any signature or routing change propagates to 7 files; stale-pending / circuit-breaker state is shared across all callers |

**Affected symbol count (blast radius):** 5 delegate methods + 11 call sites across 7 files = **16 directly coupled symbols**.

---

## Top 3 Complexity Drivers

### 1. Four-path `OrderState` routing dispatch with compound OR conditions (CYC contribution: ~4)

The core of the method is a sequential `if/else` cascade routing on `currentStop.OrderState`.
Two of the four branches use compound `||` conditions (`CancelPending || Submitted`,
`Working || Accepted`), each contributing an extra decision point. The pattern is readable but
not exhaustive — any future `OrderState` value falls silently through to `CreateDirectStopOrder`,
making the default path implicit. A `switch` with explicit `default` arm would make this safer and
reduce CYC by 1.

### 2. Stale-pending time-arithmetic gate with `DateTime.Now` side-input (CYC contribution: ~2)

Before the routing dispatch, a separate `if (pendingStopReplacements.TryGetValue(...))` block
computes `pendingAgeSeconds` and branches on `> STALE_PENDING_FAST_PATH_SEC`. This guard
introduces a non-deterministic time dependency: `DateTime.Now` is read at call time, making the
branch untestable without injection. The gate also duplicates partial logic from
`CleanupStalePendingReplacements`, creating two separate staleness-eviction policies that must stay
in sync.

### 3. `try/catch` with emergency-flatten escalation tail (CYC contribution: ~1 + downstream CYC)

The entire method body is wrapped in a `try/catch` that delegates to `HandleUpdateException`.
That handler itself contains an `if/if` circuit-breaker chain and a nested `try/catch` for
`FlattenPositionByName`. While the outer method's CYC contribution from this is only +1, the
practical complexity is higher because any exception in any of the four routing delegates
collapses into the same catch block, making root-cause attribution difficult.

---

## Recommended Extraction Count

**0 additional extractions recommended at Phase 0.**

**Rationale:** `UpdateStopOrder` is already a thin dispatcher — it contains no inline business
logic. The four routing delegates (`HandleStalePendingReplacement`, `UpdateExistingPendingReplacement`,
`InitiateStopReplacement`, `CreateDirectStopOrder`) are already extracted. The true complexity
hotspots are in those delegates, not in this method.

**Phase 1 work items should focus on:**
- Replacing the implicit `OrderState` fall-through with a `switch` + explicit `default` arm
- Injecting a `Func<DateTime>` (or equivalent) into the staleness gate to enable deterministic testing
- Auditing `UpdateExistingPendingReplacement` (lines 167–253) and `InitiateStopReplacement`
  (lines 307–369) which each contain a duplicated target-snapshot loop — these are the real CYC
  hotspots in the file and are candidates for shared-helper extraction

---

## Agent Tracking

Agent Name: v12-phase0-hotspot | Bobcoins Used: 1.2 | Execution Time: ~60s
