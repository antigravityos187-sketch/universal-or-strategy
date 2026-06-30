# Phase 4 Tickets — EPIC-W7-101
## Method: VerifyPhotonSlotIntegrity
## Source: src/V12_002.SIMA.Fleet.cs
## Agent: v12-phase4-tickets

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-101 |
| **Baseline CYC** | 16 |
| **Target CYC** | <= 8 |
| **max_cyc_projected** | 8 |
| **ticket_count** | 2 |
| **DNA Verdict (Phase 3)** | PASS |
| **Helpers** | RollbackPhotonStateOnIntegrityFailure, PumpFleetDispatchIfPending |

Two extraction tickets are required to bring `VerifyPhotonSlotIntegrity` from cyc 16 down to cyc 2 in the residual parent. Each ticket targets a single, independent logical cluster.

---

## Ticket T1 — Extract RollbackPhotonStateOnIntegrityFailure

**ID:** EPIC-W7-101-T1
**Type:** extraction
**Priority:** P1

### Description

Extract the failure-path state-rollback block from `VerifyPhotonSlotIntegrity` into a new private helper `RollbackPhotonStateOnIntegrityFailure`. This helper owns all resource cleanup that executes when an integrity failure is detected: ExpectedKey guard (hoisted), ReservedDelta inner guard, FleetEntryName target-dict removal loop, pool release, and sideband release. The cyc contribution of this block is reduced from 9 to 8 by hoisting the repeated `_sb.ExpectedKey != null` check — this is semantically identical and removes one redundant branch.

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void RollbackPhotonStateOnIntegrityFailure(
    ref FleetDispatchSlot _ringSlot,
    FleetDispatchSideband _sb,
    int _sbIdx)
```

### Acceptance Criteria

- [ ] Method `RollbackPhotonStateOnIntegrityFailure` exists in `src/V12_002.SIMA.Fleet.cs` with attribute `[MethodImpl(MethodImplOptions.NoInlining)]`
- [ ] Projected cyc = 8 (verified by complexity audit): 1 base + 1 ExpectedKey + 1 ReservedDelta + 1 FleetEntryName + 1 for-loop + 1 td-null + 1 sbIdx>=0 + 1 sbIdx<Length
- [ ] ExpectedKey null check is hoisted — the combined `ReservedDelta != 0 && ExpectedKey != null` guard is replaced with a single outer `ExpectedKey != null` wrapping both `AddExpectedPositionDeltaLocked` (guarded by `ReservedDelta != 0`) and `ClearDispatchSyncPending`
- [ ] Pool release and sideband release logic preserved exactly (no semantic change)
- [ ] `_sbIdx >= 0` and `_sbIdx < _photonSideband.Length` guards preserved in order
- [ ] No `lock()` block introduced
- [ ] No new allocations in the helper signature (all params are value types or pre-existing class references)
- [ ] ASCII-only identifiers
- [ ] Residual `VerifyPhotonSlotIntegrity` calls `RollbackPhotonStateOnIntegrityFailure(ref _ringSlot, _sb, _sbIdx)` on the failure branch
- [ ] Build passes (`dotnet build`) with zero new errors or warnings
- [ ] `python scripts/complexity_audit.py` reports cyc <= 8 for this method

### Implementation Steps

1. Read the current body of `VerifyPhotonSlotIntegrity` at `src/V12_002.SIMA.Fleet.cs` (line ~329)
2. Identify the rollback block: from the `if (_ringSlot.ReservedDelta != 0 && _sb.ExpectedKey != null)` guard through the sideband-index-bounds `_photonSideband[_sbIdx]` null-assignment
3. Apply the ExpectedKey hoist transformation:
   - Replace `if (_ringSlot.ReservedDelta != 0 && _sb.ExpectedKey != null) { AddExpectedPositionDeltaLocked(...); }` followed by `if (_sb.ExpectedKey != null) { ClearDispatchSyncPending(...); }` with a single outer `if (_sb.ExpectedKey != null) { if (_ringSlot.ReservedDelta != 0) { AddExpectedPositionDeltaLocked(...); } ClearDispatchSyncPending(...); }`
4. Move the transformed block (all 7 branches) into `RollbackPhotonStateOnIntegrityFailure` with `[MethodImpl(MethodImplOptions.NoInlining)]`
5. In the residual parent, replace the extracted block with a single call: `RollbackPhotonStateOnIntegrityFailure(ref _ringSlot, _sb, _sbIdx);`
6. Run `dotnet build` — verify zero errors
7. Run `python scripts/complexity_audit.py` — verify cyc = 8 for new helper

---

## Ticket T2 — Extract PumpFleetDispatchIfPending

**ID:** EPIC-W7-101-T2
**Type:** extraction
**Priority:** P1

### Description

Extract the pump-prime block from `VerifyPhotonSlotIntegrity` into a new private helper `PumpFleetDispatchIfPending`. This helper owns counter management, circuit-breaker reset, and conditional dispatch pump-prime: `Interlocked.Decrement(ref _pendingFleetDispatchCount)`, `Volatile.Read(ref _pendingFleetDispatchCount)`, `TryResetCircuitBreakerIfBelow(...)`, and the conditional pump-prime (the `if (!_photonDispatchRing.IsEmpty || !_pendingFleetDispatches.IsEmpty)` guard with `try/catch` around the lambda dispatch). The cyc contribution of this block is 5 (two `||` operands count as +2, plus try/catch +1, plus diagnostics guard +1, plus the base branch +1).

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void PumpFleetDispatchIfPending()
```

### Acceptance Criteria

- [ ] Method `PumpFleetDispatchIfPending` exists in `src/V12_002.SIMA.Fleet.cs` with attribute `[MethodImpl(MethodImplOptions.NoInlining)]`
- [ ] Projected cyc = 5 (verified by complexity audit): 1 base + 2 (||) + 1 try/catch + 1 diagnostics guard
- [ ] `Interlocked.Decrement(ref _pendingFleetDispatchCount)` call preserved exactly — same field reference, no wrapping
- [ ] `Volatile.Read(ref _pendingFleetDispatchCount)` call preserved exactly — memory barrier semantics intact
- [ ] `TryResetCircuitBreakerIfBelow(...)` call preserved exactly with the same arguments
- [ ] `try/catch` block structure preserved — catch still calls `Print` only when `_diagFleet` is true
- [ ] Lambda `o => PumpFleetDispatch()` preserved exactly inside the try block
- [ ] No `lock()` block introduced
- [ ] ASCII-only identifiers
- [ ] Residual `VerifyPhotonSlotIntegrity` calls `PumpFleetDispatchIfPending();` (no parameters — all state accessed via `this`)
- [ ] Build passes (`dotnet build`) with zero new errors or warnings
- [ ] `python scripts/complexity_audit.py` reports cyc <= 8 for this method

### Implementation Steps

1. Identify the pump-prime block in `VerifyPhotonSlotIntegrity`: from `Interlocked.Decrement(ref _pendingFleetDispatchCount)` through the closing brace of the `if (!_photonDispatchRing.IsEmpty || !_pendingFleetDispatches.IsEmpty)` block
2. Create `PumpFleetDispatchIfPending()` with `[MethodImpl(MethodImplOptions.NoInlining)]` — no parameters needed; all references are instance fields (`this`)
3. Move the block verbatim into the new helper — preserve `Volatile.Read`, `Interlocked.Decrement`, `TryResetCircuitBreakerIfBelow`, `try/catch`, `_diagFleet`, and lambda call order exactly
4. In the residual parent, replace the extracted block with: `PumpFleetDispatchIfPending();`
5. Verify residual `VerifyPhotonSlotIntegrity` has `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — add if not already present
6. Run `dotnet build` — verify zero errors
7. Run `python scripts/complexity_audit.py` — verify cyc = 5 for new helper, cyc = 2 for residual parent

---

## Post-Extraction Verification Checklist

| Check | Tool | Expected |
|---|---|---|
| Build passes | `dotnet build` | 0 errors, 0 new warnings |
| CYC: RollbackPhotonStateOnIntegrityFailure | complexity_audit.py | 8 |
| CYC: PumpFleetDispatchIfPending | complexity_audit.py | 5 |
| CYC: VerifyPhotonSlotIntegrity (residual) | complexity_audit.py | 2 |
| Zero lock() introduced | `grep -r "lock(" src/V12_002.SIMA.Fleet.cs` | 0 matches |
| No NUnit/MSTest | repo search | 0 matches |
| deploy-sync | `powershell -File ./deploy-sync.ps1` | PASS |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Epic** | EPIC-W7-101 |
| **Method** | VerifyPhotonSlotIntegrity |
| **Source** | src/V12_002.SIMA.Fleet.cs |
| **CYC Baseline** | 16 |
| **max_cyc_projected** | 8 |
| **ticket_count** | 2 |
| **Phase** | 4 -- Ticket Generation |
| **Generated** | 2026-06-29T22:40:00Z |
