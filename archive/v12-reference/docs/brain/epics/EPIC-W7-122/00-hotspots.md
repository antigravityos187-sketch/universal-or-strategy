# EPIC-W7-122 Hotspot Analysis

**Method:** RemoveFsmOrderIdMappings
**CYC:** 10
**File:** src/V12_002.Symmetry.BracketFSM.cs

---

## Overview

`RemoveFsmOrderIdMappings` is a cleanup utility called exclusively by
`TryTerminateFollowerBracket` during FSM teardown. Its responsibility is to
de-index every `OrderId` that was registered in `_orderIdToFsmKey` for the
given `FollowerBracketFSM` — covering the entry order, the in-flight replace
cancel order, the stop order, and each of up to 5 target orders. Despite the
method's small surface area (22 lines), its CYC of 10 is driven by the
parallel null-guard + string-empty guard pattern repeated for each order
slot, compounded by a `foreach` loop containing its own compound condition,
and an early-return sentinel guard on the targets array itself.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `TryTerminateFollowerBracket` (line 135, `src/V12_002.Symmetry.BracketFSM.cs`) |
| **Caller chain** | Various termination paths → `TryTerminateFollowerBracket` → `RemoveFsmOrderIdMappings` |
| **Shared mutable state** | `_orderIdToFsmKey` (`ConcurrentDictionary<string, string>`) — declared in `src/V12_002.cs` line 836 |
| **Writers to `_orderIdToFsmKey`** | 14 write sites across: `BracketFSM.cs`, `SIMA.Lifecycle.cs`, `SIMA.Fleet.cs`, `SIMA.Execution.cs`, `Orders.Callbacks.Propagation.cs` |
| **Readers of `_orderIdToFsmKey`** | `ResolveFsm_ByOrderId` (line 169), back-fill paths in `ResolveFsm_BySignalName` (line 196), `ResolveFsm_ByScan` (lines 221, 230, 240) |
| **FSM fields touched** | `EntryOrder.OrderId`, `ReplacingCancelOrderId`, `StopOrder.OrderId`, `Targets[0..4].OrderId` |
| **Side-effects** | Mutates shared concurrent dictionary; no logging, no state transitions |
| **Threading constraint** | Strategy thread only (called from teardown path; `ConcurrentDictionary` handles thread-safety at the map level) |
| **Risk on change** | Medium-high — any extraction must preserve all 7 removal branches exactly; missing a branch leaves stale OrderId entries that corrupt future O(1) FSM resolution via `ResolveFsm_ByOrderId` |

**Affected symbol count (blast radius):** 1 direct caller; 14 write-path peers; 3 read-path consumers; 1 shared concurrent state bag.

---

## Top 3 Complexity Drivers

1. **Parallel null + empty-string compound guards for each order slot (×3)**
   Lines 108–115 repeat the same two-part compound condition
   (`order != null && !string.IsNullOrEmpty(order.OrderId)`) three times —
   once for `EntryOrder`, once for `ReplacingCancelOrderId` (single-part only), and
   once for `StopOrder`. Each compound `&&` adds +1 CYC above the base `if`. With three
   such `if` blocks this alone contributes 6 of the 10 CYC points (3 branch decisions
   + 3 compound short-circuit penalties). The repeated structural idiom signals an
   extractable `TryRemoveOrderId(Order)` helper that would collapse all three into a
   single-purpose method.

2. **Sentinel early-return on `Targets` array + loop compound condition**
   Line 117 (`if (fsm.Targets == null) return;`) adds a CYC point as an independent
   guard branch, then the `foreach` at line 120 adds another (+1 for the loop), and the
   inner `if (target != null && !string.IsNullOrEmpty(target.OrderId))` at line 122
   contributes 2 more (branch + compound). This 4-point cluster — null-sentinel →
   loop → inner compound — is the densest single structural unit in the method and is
   the primary candidate for extraction into a dedicated
   `RemoveFsmTargetOrderIdMappings(FollowerBracketFSM)` helper.

3. **Null-guard on FSM itself as a top-level early-return (CYC=1, structural friction)**
   Line 105 (`if (fsm == null) return;`) is a necessary defensive guard but it
   contributes 1 CYC and reflects a design smell: a private method whose sole caller
   (`TryTerminateFollowerBracket`) already guards for null via `TryRemove` semantics
   (a removed value will never be null in practice). This guard exists purely as a
   defensive coding pattern and could be removed post-extraction, reducing the residual
   dispatcher CYC to 1. Its presence currently prevents the compiler from pruning the
   remainder of the method as unconditional, adding marginal pressure to the overall score.

---

## Recommended Extraction Count

**2 helpers recommended.**

| Helper | Responsibility | Estimated CYC |
|---|---|---|
| `TryRemoveOrderId(Order order)` | Null + empty-string guard → single `TryRemove` call | 2 |
| `RemoveFsmTargetOrderIdMappings(FollowerBracketFSM fsm)` | Null-sentinel on `Targets` + `foreach` + inner guard | 4 |

**Post-extraction residual CYC of `RemoveFsmOrderIdMappings`:**
3 delegating calls to `TryRemoveOrderId` + 1 call to `RemoveFsmTargetOrderIdMappings`
+ 1 null-guard on `fsm` = **CYC 3** (target: ≤5).

**Rationale:** The two extractions are semantically clean, have no shared mutable state
beyond what they already receive as parameters, and each maps to a single named
responsibility. No further decomposition is warranted at Phase 0. The inline-null guard
on `fsm` (line 105) may be removed in Phase 2 if the call site is hardened — do not
remove it prematurely.

---

## Agent Tracking

Agent Name: v12-phase0-hotspot | Bobcoins Used: 1.0 | Execution Time: ~52s
