# Phase 2 Architecture Plan — EPIC-W7-099
## Method: PurgePositionIfEligible
## Source: src/V12_002.Orders.Management.Cleanup.cs
## Agent: v12-phase2-architecture

## Complexity Analysis

Current CYC=11. Complexity drivers enumerated from full method source (lines 207-243):

| # | Branch / Condition | CYC contribution |
|---|---|---|
| 1 | `followerExpected == 0` (Block A outer guard, left operand) | +1 |
| 2 | `!HasActiveOrPendingOrderForEntry(entryName)` (Block A outer guard, right operand) | +1 |
| 3 | `if (removed)` (post-TryRemove null-guard) | +1 |
| 4 | `followerExpected == 0` (Block B outer guard, first condition) | +1 |
| 5 | `activePositions.TryGetValue(entryName, out var followerCheck)` (Block B guard) | +1 |
| 6 | `followerCheck.IsFollower` (Block B guard chain) | +1 |
| 7 | `followerCheck.ExecutingAccount != null` (Block B guard chain) | +1 |
| 8 | LINQ predicate `p => p.Instrument == Instrument` inside FirstOrDefault | +1 |
| 9 | `brokerPos != null` (null check post-LINQ) | +1 |
| 10 | `brokerPos.MarketPosition == MarketPosition.Flat` (flat check) | +1 |
| 11 | `if (removedFZP)` (post-TryRemove logging guard) | +1 |

**Base CYC = 1 + 10 branch points = 11. Confirmed.**

Two distinct logical concerns exist:
- **Block A** (lines ~210-219): Standard META-GUARD purge — followerExpected==0 and no active/pending orders
- **Block B** (lines ~221-242): FIX-ZP-02 secondary safety net — broker-confirmed flat SIMA follower force-purge using LINQ position lookup

## Extraction Plan

| Helper Method | Signature | CYC | Attribute | Rationale |
|---|---|---|---|---|
| `TryPurgeStandardPosition` | `private void TryPurgeStandardPosition(string entryName)` | 3 | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` | Hot path — extracts Block A: no-active-orders guard + TryRemove + SymmetryGuardForgetEntry. Zero alloc. 2 branches. |
| `TryPurgeFlatFollowerByBroker` | `private void TryPurgeFlatFollowerByBroker(string entryName)` | 8 | `[MethodImpl(MethodImplOptions.NoInlining)]` | Cold path — extracts Block B (FIX-ZP-02): full broker-confirmed flat follower guard chain + LINQ lookup + TryRemove + Print. |
| `PurgePositionIfEligible` (residual) | `private void PurgePositionIfEligible(string entryName, int followerExpected)` | 3 | _(none — orchestrator)_ | Residual: two `if (followerExpected == 0)` dispatch guards, delegates to helpers. |

### Residual parent after extraction (pseudocode):
```csharp
private void PurgePositionIfEligible(string entryName, int followerExpected)
{
    if (followerExpected == 0)
        TryPurgeStandardPosition(entryName);

    if (followerExpected == 0)
        TryPurgeFlatFollowerByBroker(entryName);
}
```
Parent CYC = base(1) + 2 branch points = **3**.

### TryPurgeStandardPosition CYC breakdown:
base(1) + `if (!HasActiveOrPendingOrderForEntry)` (1) + `if (removed)` (1) = **3**

### TryPurgeFlatFollowerByBroker CYC breakdown:
base(1) + TryGetValue(1) + IsFollower(1) + ExecutingAccount!=null(1) + LINQ predicate(1) + brokerPos!=null(1) + MarketPosition.Flat(1) + removedFZP(1) = **8**

## max_cyc_projected: 8 (must be <= 8)

All units pass Jane Street threshold of 8. ✓

## Jane Street Alignment

- **carl_cook**: `TryPurgeStandardPosition` is `[AggressiveInlining]` — hot path, zero alloc, no LINQ, tight 2-branch structure. `TryPurgeFlatFollowerByBroker` is `[NoInlining]` — cold diagnostic path containing LINQ and Print; extracted out-of-line to protect the hot path. Structs/ConcurrentDictionary used throughout — no heap alloc in hot path.
- **gjengset**: No new `lock()` blocks introduced. `activePositions` is a `ConcurrentDictionary` (lock-free). No volatile or `Thread.MemoryBarrier` changes required — existing lock-free semantics preserved through extraction. No shared mutable state added.
- **trading_billions**: Single responsibility enforced — Block A (standard purge) and Block B (FIX-ZP-02 broker confirmation) are now separate named helpers with clear intent. Each helper CYC <= 8. Defense in depth maintained: both purge paths remain independent (Block B is a secondary safety net that does not depend on Block A success). No rate-limit or circuit-breaker needed here (no external I/O in hot path).

## MCP Evidence

- **resolve_repo**: `antigravityos187-sketch/universal-or-strategy` — indexed, 5147 symbols, 2000 files, loadable. Status: OK.
- **get_context_bundle**: Full source retrieved for `PurgePositionIfEligible` (lines 207-243). Two logical blocks confirmed: Block A (standard purge) and Block B (FIX-ZP-02 SIMA flat follower purge). Method uses `activePositions.TryRemove`, `SymmetryGuardForgetEntry`, `HasActiveOrPendingOrderForEntry`, LINQ `FirstOrDefault`, `Print`. Docstring confirms FIX-ZP-02 intent.
- **get_call_hierarchy**: 1 direct caller (`CleanupPosition` in same file, line 37). 28 callee references (depth 2, includes `HasActiveOrPendingOrderForEntry`, `SymmetryGuardForgetEntry`, `activePositions`, `LogBuffer.Format`). Caller signature unchanged — safe extraction confirmed.
- **get_dependency_graph**: 0 import edges, 0 importer edges for `V12_002.Orders.Management.Cleanup.cs` at depth 1 (partial class pattern — all deps resolved at compile-time via partial class merge). No cross-file import rewrites needed.

## Sequential Thinking Evidence

- **Thought 1**: Mapped all 11 CYC branch points across two logical blocks (Block A: 3 points, Block B: 8 points). Confirmed two distinct responsibilities — standard META-GUARD purge and FIX-ZP-02 broker-confirmed flat follower purge.
- **Thought 2**: Designed 2-helper extraction: `TryPurgeStandardPosition` (Block A, hot path, [AggressiveInlining], CYC=3) and `TryPurgeFlatFollowerByBroker` (Block B, cold path, [NoInlining], CYC=8). Residual parent reduced to 2-dispatch guard, CYC=3.
- **Thought 3**: Validated all units CYC <= 8. max_cyc_projected=8. Confirmed LINQ retention in cold helper is acceptable (No Scope Creep Protocol prohibits removing existing logic; cold path justified for [NoInlining]). All Jane Street rules applied correctly.

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Epic** | EPIC-W7-099 |
| **Method** | PurgePositionIfEligible |
| **Source File** | src/V12_002.Orders.Management.Cleanup.cs |
| **CYC Baseline** | 11 |
| **max_cyc_projected** | 8 |
| **Helpers Extracted** | 2 |
| **Phase** | 2 |
