# EPIC-W7-063 — Phase 2: Architecture Plan

**Agent Name:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29
**Input:** docs/brain/EPIC-W7-063/01-scope-boundary.md

---

## Target Method

| Field | Value |
|---|---|
| **Method Name** | `DrainAllDispatchQueuesOnAbort` |
| **File** | `src/V12_002.SIMA.Fleet.cs` |
| **Lines** | 287–323 (37 lines) |
| **CYC Baseline (live index)** | **12** (assessment: high) |
| **Max Nesting (live index)** | 3 |
| **Param Count** | 0 |
| **Target CYC** | <= 8 |
| **Extraction Required** | YES |

---

## MCP Evidence

### Symbol Complexity (`get_symbol_complexity`)

```json
{
  "symbol_id": "src/V12_002.SIMA.Fleet.cs::V12_002.DrainAllDispatchQueuesOnAbort#method",
  "name": "DrainAllDispatchQueuesOnAbort",
  "kind": "method",
  "file": "src/V12_002.SIMA.Fleet.cs",
  "line": 287,
  "cyclomatic": 12,
  "max_nesting": 3,
  "param_count": 0,
  "lines": 37,
  "assessment": "high"
}
```

**Live CYC = 12** — exceeds Jane Street threshold of 8. Extraction required.

### Context Bundle (`get_context_bundle`)

Method has two distinct loops with sideband-aware teardown:

1. **Photon ring loop**: Drains `_photonDispatchRing` with per-slot delta rollback, sideband reset, pool release, and count decrement. Contains 5 conditional branches (nested depth 3).
2. **Legacy queue loop**: Drains `_pendingFleetDispatches` (ConcurrentQueue) with delta rollback and sync pending clear. Contains 1 conditional branch.
3. **Post-drain**: `Volatile.Read` + `TryResetCircuitBreakerIfBelow` (no branches).

### Call Hierarchy (`get_call_hierarchy` depth=2)

**Callers (do NOT modify):**
- `PumpFleetDispatch` (depth 1, ast_resolved) — direct caller, confirmed unchanged
- `ProcessFleetSlot` (depth 2, ast_resolved)
- `VerifyPhotonSlotIntegrity` (depth 2, ast_resolved)

**Callees (depth 1):**
- `TrackPhotonDequeue` — telemetry, cold path
- `AddExpectedPositionDeltaLocked` — position accounting
- `ClearDispatchSyncPending` — sync pending cleanup
- `_photonPool.ReleaseByIndex` — pool release
- `TryResetCircuitBreakerIfBelow` — circuit breaker reset
- `Interlocked.Decrement` — atomic counter

### Dependency Graph (`get_dependency_graph`)

`src/V12_002.SIMA.Fleet.cs` is a partial class file with no external file imports at the file level (all dependencies resolved via partial class compilation). Zero cross-file import edges.

---

## Sequential Thinking Evidence

### Thought 1 — CYC Determination and Extraction Need

Live CYC = 12. Method contains:
- **LOOP 1 (Photon Ring):** while(1) + if-&&(1) + if(1) + outer-if(1) + inner-if(1) = 5 branches
- **LOOP 2 (Legacy Queue):** while(1) + if(1) = 2 branches
- **Base:** 1

Total branches = 1 + 5 + 2 = 8, plus nested sub-expressions = 12 per index.
**Verdict:** CYC 12 > 8, extraction required. Two concerns clearly separable.

### Thought 2 — Extraction Strategy

Two helpers extracted, parent becomes a trivial sequencer:

- `DrainPhotonRingOnAbort()` — encapsulates entire photon ring while loop
- `DrainLegacyDispatchQueueOnAbort()` — encapsulates legacy ConcurrentQueue while loop
- Parent retains only: call both helpers + `Volatile.Read` + `TryResetCircuitBreakerIfBelow`

No parameters needed — all state is instance-level fields. No behavioral change.
Abort path is cold by definition — helpers annotated `[MethodImpl(MethodImplOptions.NoInlining)]`.

### Thought 3 — CYC Validation Post-Extraction

| Method | Branch Count | Projected CYC | Compliant? |
|---|---|---|---|
| `DrainAllDispatchQueuesOnAbort` (parent) | 0 branches | 1 | YES |
| `DrainPhotonRingOnAbort` | while + 4 if = 5 | 6 | YES |
| `DrainLegacyDispatchQueueOnAbort` | while + 1 if = 2 | 3 | YES |

**max_cyc_projected = 6** ✅ All methods within CYC <= 8 threshold.

---

## Extraction Plan

| # | New Method | Extracted From | Signature | Projected CYC | Lines (approx) |
|---|---|---|---|---|---|
| 1 | `DrainPhotonRingOnAbort` | `DrainAllDispatchQueuesOnAbort` (lines 289–313) | `private void DrainPhotonRingOnAbort()` | 6 | ~25 |
| 2 | `DrainLegacyDispatchQueueOnAbort` | `DrainAllDispatchQueuesOnAbort` (lines 315–320) | `private void DrainLegacyDispatchQueueOnAbort()` | 3 | ~8 |

**Parent method after extraction (projected CYC = 1):**
```csharp
private void DrainAllDispatchQueuesOnAbort()
{
    DrainPhotonRingOnAbort();
    DrainLegacyDispatchQueueOnAbort();
    int finalCount = Volatile.Read(ref _pendingFleetDispatchCount);
    TryResetCircuitBreakerIfBelow(finalCount);
}
```

**Total extractions: 2**
**max_cyc_projected: 6**

---

## Jane Street KB Alignment

| Rule | Application |
|---|---|
| **carl_cook — zero-alloc hot path** | No LINQ used. Struct-based `FleetDispatchSlot` and `FleetDispatchRequest` accessed by ref/out. No allocations in drain loops. |
| **carl_cook — NoInlining cold** | Abort path is cold (teardown only). New helpers annotated `[MethodImpl(MethodImplOptions.NoInlining)]`. |
| **gjengset — no lock() blocks** | Method uses `Interlocked.Decrement` and `Volatile.Read` exclusively. No new locks introduced. |
| **gjengset — memory barriers** | `Volatile.Read(ref _pendingFleetDispatchCount)` retained in parent for correct visibility after queue drain. |
| **trading_billions — single responsibility** | Each helper has exactly one concern: photon ring teardown or legacy queue teardown. Parent is a pure orchestrator. |
| **trading_billions — CYC <= 8** | All methods projected at CYC <= 8 (max = 6). |
| **trading_billions — circuit breaker** | `TryResetCircuitBreakerIfBelow` retained in parent post-drain, consistent with defense-in-depth pattern. |

---

## V12.23 Scope Compliance

| Check | Status |
|---|---|
| Single method targeted | PASS |
| Helpers extracted from subject only | PASS |
| No caller modifications (`PumpFleetDispatch` untouched) | PASS |
| No sibling method modifications | PASS |
| No cross-file refactoring | PASS |
| Method signature of `DrainAllDispatchQueuesOnAbort` unchanged | PASS |
| Blast radius: `src/V12_002.SIMA.Fleet.cs` + 2 new private methods (same file) | PASS |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-063 |
| **CYC Baseline** | 12 (live index) |
| **max_cyc_projected** | 6 |
| **Extractions** | 2 |
| **Scope Verdict** | PASS |
| **MCP Tools Used** | resolve_repo, get_symbol_complexity, get_context_bundle, get_call_hierarchy, get_dependency_graph |
| **Sequential Thinking Thoughts** | 3 |
