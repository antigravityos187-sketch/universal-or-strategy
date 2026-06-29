# EPIC-W7-124 Hotspot Analysis

**Method:** `SymmetryFindDispatchForMasterFill`
**CYC (reported by task):** 0 — ⚠️ tool could not locate; manually audited as **CYC = 9**
**File:** `src/V12_002.Symmetry.cs`
**Lines:** 326–352

---

## Locate Status

The MCP `search_symbols` / `get_symbol_complexity` tools returned CYC=0 and could not locate the
symbol (no jcodemunch-mcp server available in this environment). The method **is present** at line
326 of [`src/V12_002.Symmetry.cs`](../../src/V12_002.Symmetry.cs). A manual cyclomatic complexity
audit was performed from the raw source and is documented below. This artifact is therefore marked
**REQUIRES MANUAL REVIEW** for toolchain CYC confirmation.

---

## Manual CYC Audit

| Branch / Decision Point | +CYC |
|---|---|
| Base (method entry) | 1 |
| `foreach` loop over `symmetryDispatchById.ToArray()` | +1 |
| `if (ctx == null \|\| ctx.Anchor.IsResolved)` — short-circuit OR | +2 |
| `if (ctx.Direction != direction)` | +1 |
| `if (!string.Equals(ctx.TradeType, norm, ...))` | +1 |
| `if (fillTimeUtc - ctx.CreatedUtc > SymmetryDispatchTtl)` | +1 |
| `if (best == null \|\| ctx.CreatedUtc < best.CreatedUtc)` — short-circuit OR | +2 |
| **Total** | **9** |

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct callers** | `SymmetryGuardOnMasterFill` (line 283, `src/V12_002.Symmetry.cs`) — fallback path when direct `symmetryMasterEntryToDispatch` lookup misses |
| **Transitive caller** | `ValidateAndPrepareEntryFill` → `SymmetryGuardOnMasterFill` → `SymmetryFindDispatchForMasterFill` (`src/V12_002.Orders.Callbacks.cs:368`) |
| **Methods invoked** | `SymmetryNormalizeTradeType` (`src/V12_002.Symmetry.Replace.cs:322`) |
| **Shared state read** | `symmetryDispatchById` (`ConcurrentDictionary<string, SymmetryDispatchContext>`) — enumerated via `.ToArray()` snapshot |
| **Return coupling** | Returns `SymmetryDispatchContext`; result immediately feeds the CAS-loop anchor-publishing block in `SymmetryGuardOnMasterFill` (lines 291–323) |
| **Threading constraint** | Called from `SymmetryGuardOnMasterFill` which is triggered by `ValidateAndPrepareEntryFill` on the NT execution callback thread; `ConcurrentDictionary.ToArray()` provides snapshot-safe enumeration |
| **Risk on change** | **Medium** — "earliest unresolved dispatch" selection logic is the fallback disambiguation strategy; any change to ordering or filter criteria can silently misroute master fills to wrong dispatch contexts |

**Affected symbol count (blast radius):** 3 symbols directly coupled; 1 shared concurrent state bag.

---

## Top 3 Complexity Drivers

### 1. Multi-filter guard chain with four sequential `continue` branches (CYC +5)

The inner loop body contains four independent guard checks, each issuing a `continue` to discard
non-matching dispatch contexts:

```
if (ctx == null || ctx.Anchor.IsResolved) continue;   // null + resolved-anchor guard
if (ctx.Direction != direction)           continue;   // direction mismatch
if (!string.Equals(ctx.TradeType, norm))  continue;   // trade-type mismatch
if (fillTimeUtc - ctx.CreatedUtc > TTL)   continue;   // TTL expiry
```

Each `continue` is an independent branch path — the short-circuit `||` in the first guard adds one
additional path. These four guards are logically correct and idiomatic, but their flat sequential
layout prevents easy extraction without wrapping them in a named predicate (e.g.
`IsDispatchCandidateForFill`). Combined, they account for **5 of the 9 CYC points**.

### 2. Short-circuit OR conditions in null-safety and best-candidate selection (CYC +2 each = +4)

Two `||` short-circuit compound conditions contribute extra paths:
- Line 338: `ctx == null || ctx.Anchor.IsResolved` — dual exit (null dereference guard + resolved
  anchor skip) folded into one `if`. This is safe and minimal, but the `||` adds a distinct path.
- Line 347: `best == null || ctx.CreatedUtc < best.CreatedUtc` — first-iteration initialisation
  fused with the "earlier wins" comparator in a single branch. A dedicated `IsBetterCandidate`
  helper would remove both `||` exits from this method's decision tree.

Together these two compound conditions add **4 CYC points** — nearly half the method's score.

### 3. `foreach` over a `.ToArray()` snapshot of a `ConcurrentDictionary` (CYC +1, latent risk)

The loop `foreach (var kvp in symmetryDispatchById.ToArray())` allocates a full key-value snapshot
every call. Under normal operational cadence this is low-frequency and safe, but the iteration
itself introduces one CYC point and the snapshot creates a subtle temporal race window: a dispatch
added by `SymmetryGuardBeginDispatch` *after* the snapshot is invisible to this call, making
"fallback" dispatch matching non-deterministic relative to concurrent activity. This is the primary
reason the caller prefers the direct `symmetryMasterEntryToDispatch` lookup path.

---

## Recommended Extraction Count

**2 helper extractions recommended.**

| # | Helper Name | Removes | Resulting CYC |
|---|---|---|---|
| 1 | `IsDispatchCandidateForFill(ctx, norm, direction, fillTimeUtc)` | 4 guard branches (CYC −5) | Caller drops to ~4 |
| 2 | `IsBetterCandidate(ctx, best)` | compound `\|\|` comparator (CYC −2) | Caller drops to ~2 |

**Rationale:** The method body is 26 lines and already correctly structured. Extraction is
recommended not because the method is unsafe at CYC=9, but because:
- `IsDispatchCandidateForFill` would be independently unit-testable with mock `SymmetryDispatchContext` instances.
- `IsBetterCandidate` encapsulates the "earliest unresolved dispatch wins" policy as a named invariant, making future
  priority changes (e.g. switching to closest-requested-price instead of oldest) surgical rather than in-situ.

The loop and snapshot infrastructure (lines 335–349) should remain in the parent method; only the
predicate logic and comparator belong in helpers.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.2 |
| **Execution Time** | ~90s |
| **CYC Source** | Manual McCabe branch count from source (CYC=0 in index is a measurement gap; see analysis) |
| **Review Flag** | ⚠️ REQUIRES MANUAL CYC CONFIRMATION via jcodemunch-mcp when toolchain is available |
