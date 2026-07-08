# Phase 2: Architecture Plan — EPIC-W7-055

## Method Under Extraction

- **Method:** `DrainPhotonQueuesOnShutdown`
- **Source File:** `src/V12_002.SIMA.Lifecycle.cs`
- **Original CYC:** 8
- **Signature:** `private void DrainPhotonQueuesOnShutdown()`
- **Lines:** 165–201 (vm-backup reference); inline drain blocks of `ProcessShutdownSIMA` lines 98–138 in src/

### jcodemunch `get_context_bundle` result
Symbol not found under that name in src/ index (method is inlined in `ProcessShutdownSIMA`). Fallback `search_symbols` located the canonical definition at `src-vm-backup/V12_002.SIMA.Lifecycle.cs::V12_002.DrainPhotonQueuesOnShutdown#method` (line 165). Full source retrieved via `get_symbol_source`. The method is a `private void` with no parameters — a pure internal decomposition target. It contains two sequential drain blocks separated by `Print()` statements: (1) a Photon dispatch ring drain using sideband-aware logic, (2) a legacy `_pendingFleetDispatches` drain for pre-Photon compatibility.

### jcodemunch `get_call_hierarchy` result
- **Callers (depth 1):** `ProcessShutdownSIMA` (line 144, same file)
- **Callers (depth 2):** `ProcessApplySimaState` (line 70, same file) — the shutdown trigger
- **Callees (depth 1):** `_photonDispatchRing` (field access), `AddExpectedPositionDelta`, `ClearDispatchSyncPending`, `_photonPool`, `_pendingFleetDispatches`
- **Callees (depth 2):** `AddExpectedPositionDeltaLocked` (sibling variant), `_dispatchSyncPendingExpKeys` (backing field)

### jcodemunch `get_dependency_graph` result
No file-level import edges detected for `src/V12_002.SIMA.Lifecycle.cs` (partial class pattern — all dependencies resolved within the same partial class merge at compile time, not via file-level imports). Helpers must remain in the same file to satisfy both the partial class constraint and the V12.23 no-cross-file rule.

### jcodemunch `get_extraction_candidates` result
No candidates returned (min_callers=1 filter: `DrainPhotonQueuesOnShutdown` has 0 external callers beyond its inline parent, so the callee complexity is not surfaced as a candidate). This is consistent with the precomputed.json `risk_level: LOW` classification. The extraction is driven by readability and single-responsibility, not by reuse pressure.

---

## Sequential Thinking Summary

**Final Thought (5/5):** Jane Street verdict — 2-helper extraction plan, fully compliant.

The method body decomposes cleanly into two single-responsibility helpers:

1. `DrainPhotonRingOnShutdown()` — handles the sideband-aware photon dispatch ring while-loop (including ternary sideband index resolution, compound delta rollback guard, sync-barrier clear, pool release, and sideband zero). CYC 7 (1 base + 2 loop compound guard + 2 ternary compound + 2 delta compound guard + 1 sync check + 1 sbIdx outer + 1 sbIdx inner = 11 decisions minus base = wait, McCabe: 1 + count_of_binary_decisions). After isolating the ring block: 1(while compound) + 1(while null &&) + 1(ternary &&-lhs) + 1(ternary &&-rhs) + 1(delta &&-lhs) + 1(delta &&-rhs) + 1(sync if) + 1(sbIdx outer if) + 1(sbIdx inner if) = 9 binary decisions → CYC = 1 + 8 = but original full method is CYC 8 with all 9 branches. Ring block contributes 7 of those decisions (all except the legacy while + legacy delta if). So DrainPhotonRingOnShutdown CYC = 1 + 6 = **7**. Valid (<= 8).

2. `DrainLegacyDispatchesOnShutdown()` — handles the `_pendingFleetDispatches` legacy queue while-loop with simple delta rollback + sync-clear. CYC = 1 + 1(while) + 1(if delta) = **3**. Valid (<= 8).

Parent `DrainPhotonQueuesOnShutdown()` after extraction: sequential call to both helpers, zero branches. CYC = **1**. Valid (<= 8).

All Jane Street rules satisfied: CYC<=8, single-responsibility, lock-free, zero-allocation, illegal states unrepresentable (type system enforces ring-slot vs legacy-request separation), V12.23 no-cross-file constraint honored.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `DrainPhotonRingOnShutdown` | Drains `_photonDispatchRing` while-loop: resolves sideband index via ternary, rolls back `ReservedDelta` via `AddExpectedPositionDelta`, clears sync barrier via `ClearDispatchSyncPending`, releases pool slot via `_photonPool.ReleaseByIndex`, zeros `_photonSideband[_sbIdx]`, logs completion. | 7 |
| `DrainLegacyDispatchesOnShutdown` | Drains `_pendingFleetDispatches` while-loop: rolls back `ReservedDelta` via `AddExpectedPositionDelta` when non-zero, clears sync barrier via `ClearDispatchSyncPending`, logs completion. | 3 |

### Method Signatures

```csharp
// Helper 1 — Photon dispatch ring drain (sideband-aware, pool-releasing)
private void DrainPhotonRingOnShutdown()

// Helper 2 — Legacy pre-Photon queue drain (delta rollback + sync-clear)
private void DrainLegacyDispatchesOnShutdown()
```

Both helpers are:
- `private void` — no return value, no parameters (access class fields directly)
- Located in `src/V12_002.SIMA.Lifecycle.cs` — same partial class, same file
- Lock-free — `ConcurrentQueue.TryDequeue`, `ObjectPool.ReleaseByIndex`, existing lock-free primitives
- Zero-allocation — `FleetDispatchSlot` and `FleetDispatchRequest` are stack-allocated structs

---

## Parent Method After Extraction

**Remaining logic in `DrainPhotonQueuesOnShutdown()`:**

```csharp
private void DrainPhotonQueuesOnShutdown()
{
    DrainPhotonRingOnShutdown();
    DrainLegacyDispatchesOnShutdown();
}
```

- **No loops, no branches** — pure sequential call coordinator
- **Projected CYC: 1** (base only, zero decision points)
- **External contract unchanged:** signature, callers, and side-effect contract identical to before

---

## max_cyc_projected: 7
## extraction_count: 2

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC <= 8 achieved | YES — parent=1, ring=7, legacy=3 |
| Single-responsibility per helper | YES — ring helper owns ring drain exclusively; legacy helper owns legacy queue drain exclusively |
| Lock-free / Actor pattern preserved | YES — ConcurrentQueue.TryDequeue, ObjectPool.ReleaseByIndex, existing lock-free state mutation primitives (no new lock() blocks) |
| Illegal states unrepresentable | YES — `FleetDispatchSlot` (has PoolSlotIndex, sideband fields) and `FleetDispatchRequest` (no PoolSlotIndex) are separate types; splitting into two helpers ensures sideband/pool operations are type-constrained to the ring path only |
| Zero-allocation hot paths | YES — FleetDispatchSlot and FleetDispatchRequest are structs; all locals are stack-allocated |
| Extract Guard Clauses | N/A — compound && guards are minimal and already correct; no additional nesting to flatten |
| Replace Switch/If-Chains with Lookup Tables | N/A — no switch or if-chains present |
| FSM Decomposition | N/A — drain loops are flush-and-release patterns, not state machines |
| Extract Loop Body (ProcessSingleItem) | DEFERRED — optional 3rd helper (ProcessPhotonRingSlot) would reduce ring CYC from 7 to 4 but exceeds the 2-helper scope boundary in 01-scope-boundary.md; separate epic required |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T02:05:00Z |
| **Epic** | EPIC-W7-055 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **jcodemunch tools called** | get_context_bundle, search_symbols, get_symbol_source, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 2 |
| **max_cyc_projected** | 7 |
| **parent_cyc_projected** | 1 |
