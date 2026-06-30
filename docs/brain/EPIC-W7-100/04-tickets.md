# Phase 4 Tickets — EPIC-W7-100
## Method: ClosePositionsOnlyApexAccounts
## Source: src/V12_002.SIMA.Flatten.cs (lines 516-589)
## Agent: v12-phase4-tickets
## Wave: 7

---

## Summary

| Field | Value |
|---|---|
| ticket_count | 3 |
| Manual CYC (before) | 10 |
| max_cyc_projected | 5 |
| Residual parent CYC | 2 |
| Jane Street threshold | 8 |
| dna_verdict (Phase 3) | PASS |
| Extraction required | YES |

---

## Ticket T1 — extraction of EnqueueFleetAccountFlattenOps

**ID:** EPIC-W7-100-T1
**Helper:** `EnqueueFleetAccountFlattenOps`
**cyc target:** 3
**Attribute:** `[MethodImpl(MethodImplOptions.NoInlining)]`

### Description

Extract the fleet enumeration loop from `ClosePositionsOnlyApexAccounts` into a dedicated private helper.
The extracted block contains:
- `foreach (Account acct in snapshot)` — loop branch (+1)
- `if (!IsFleetAccount(acct)) continue` — guard branch (+1)
- Baseline (+1)

Resulting cyc = 3 for this extraction. The helper receives `Account[] snapshot` and `ref int enqueued`
so the caller's enqueued counter is correctly incremented in-place.
All identifier names are ASCII-only. No `lock()` block introduced.

### Signature

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void EnqueueFleetAccountFlattenOps(Account[] snapshot, ref int enqueued)
```

### Implementation Steps

1. Read source lines 516-589 of `src/V12_002.SIMA.Flatten.cs` to identify the exact foreach block.
2. Create `EnqueueFleetAccountFlattenOps` immediately below `ClosePositionsOnlyApexAccounts`
   with the `[MethodImpl(MethodImplOptions.NoInlining)]` attribute.
3. Move the foreach loop body (guard + Enqueue + enqueued++ increment) into the new helper.
4. In `ClosePositionsOnlyApexAccounts`, replace the foreach block with:
   `EnqueueFleetAccountFlattenOps(snapshot, ref enqueued);`
5. Run `dotnet build src/` — zero errors required before proceeding.
6. Add xUnit `[Fact]` test verifying `enqueued` is incremented only for fleet accounts.

### Acceptance Criteria

- [ ] `EnqueueFleetAccountFlattenOps` compiles with zero errors.
- [ ] `[MethodImpl(MethodImplOptions.NoInlining)]` attribute is present.
- [ ] cyc of `EnqueueFleetAccountFlattenOps` = 3 (verified by manual count or `complexity_audit.py`).
- [ ] Parent `ClosePositionsOnlyApexAccounts` no longer contains the foreach block.
- [ ] `ref int enqueued` correctly reflects all fleet-account enqueue operations.
- [ ] No `lock()` blocks introduced.
- [ ] ASCII-only identifiers — no Unicode characters in new code.
- [ ] xUnit `[Fact]` test passes (`Assert.Equal` only, no NUnit/MSTest).
- [ ] `dotnet build src/` passes with zero errors.

---

## Ticket T2 — extraction of EnqueueMasterAccountFallbackFlatten

**ID:** EPIC-W7-100-T2
**Helper:** `EnqueueMasterAccountFallbackFlatten`
**cyc target:** 3
**Attribute:** `[MethodImpl(MethodImplOptions.NoInlining)]`

### Description

Extract the master-account fallback guard from `ClosePositionsOnlyApexAccounts` into a dedicated
private helper. The extracted block contains:
- `if (!masterCovered && Positions.Count > 0)` — if branch (+1) and logical-AND (+1)
- Baseline (+1)

Resulting cyc = 3 for this extraction. The helper receives `ref int enqueued` so the caller's
counter is correctly updated. The guard ensures the master account is covered when no fleet account
satisfied the masterCovered flag. This single-responsibility extraction preserves the defense-in-depth
fallback semantic documented in the architecture plan.
No `lock()` block introduced. ASCII-only identifiers.

### Signature

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void EnqueueMasterAccountFallbackFlatten(ref int enqueued)
```

### Implementation Steps

1. Locate the `if (!masterCovered && Positions.Count > 0)` block in
   `src/V12_002.SIMA.Flatten.cs` (after the foreach loop, before the trigger block).
2. Create `EnqueueMasterAccountFallbackFlatten` with `[MethodImpl(MethodImplOptions.NoInlining)]`.
3. Move the guard body (build master FlattenWorkItem, Enqueue, enqueued++) into the helper.
4. In `ClosePositionsOnlyApexAccounts`, replace the block with:
   `EnqueueMasterAccountFallbackFlatten(ref enqueued);`
5. Run `dotnet build src/` — zero errors required before proceeding.
6. Add xUnit `[Fact]` test verifying enqueue fires only when `!masterCovered && Positions.Count > 0`.

### Acceptance Criteria

- [ ] `EnqueueMasterAccountFallbackFlatten` compiles with zero errors.
- [ ] `[MethodImpl(MethodImplOptions.NoInlining)]` attribute is present.
- [ ] cyc of `EnqueueMasterAccountFallbackFlatten` = 3.
- [ ] Parent no longer contains the `!masterCovered && ...` conditional block.
- [ ] `ref int enqueued` correctly reflects the master-account enqueue when triggered.
- [ ] No `lock()` blocks introduced.
- [ ] ASCII-only identifiers.
- [ ] xUnit `[Fact]` test passes (`Assert.Equal` only, no NUnit/MSTest).
- [ ] `dotnet build src/` passes with zero errors.

---

## Ticket T3 — extraction of TriggerOrFallbackFlattenExecution

**ID:** EPIC-W7-100-T3
**Helper:** `TriggerOrFallbackFlattenExecution`
**cyc target:** 5
**Attribute:** `[MethodImpl(MethodImplOptions.NoInlining)]`

### Description

Extract the trigger/catch/fallback block from `ClosePositionsOnlyApexAccounts` into a dedicated
private helper. This is the highest-cyc extraction and contains:
- `if (!_pendingFlattenOps.IsEmpty)` — if branch (+1)
- `catch (InvalidOperationException ex) when (...)` — catch handler (+1)
- `when (ex.Message.Contains("TriggerCustomEvent"))` — exception filter (+1)
- `catch (Exception ex)` — catch handler (+1)
- Baseline (+1)

Resulting cyc = 5 for this extraction. The `isFlattenRunning` field mutations inside both catch
handlers remain in this helper — no synchronization changes. The else path (no-op or fallback log)
is also moved into this helper. This is a cold-path method; `[NoInlining]` prevents the JIT from
inlining exception handling into the hot path.

### Signature

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void TriggerOrFallbackFlattenExecution()
```

### Implementation Steps

1. Locate the `if (!_pendingFlattenOps.IsEmpty)` block with its try/catch/else body in
   `src/V12_002.SIMA.Flatten.cs`.
2. Create `TriggerOrFallbackFlattenExecution` with `[MethodImpl(MethodImplOptions.NoInlining)]`.
3. Move the entire `if (!IsEmpty) { try { ... } catch (InvalidOperationException ...) when (...) { ... }
   catch (Exception ...) { ... } } else { ... }` block into the helper.
4. Preserve all `isFlattenRunning` field writes at their current positions within the catch handlers.
5. In `ClosePositionsOnlyApexAccounts`, replace the block with:
   `TriggerOrFallbackFlattenExecution();`
6. Run `dotnet build src/` — zero errors required.
7. Add xUnit `[Fact]` tests covering: (a) normal trigger path, (b) InvalidOperationException
   with matching message, (c) general Exception fallback path.

### Acceptance Criteria

- [ ] `TriggerOrFallbackFlattenExecution` compiles with zero errors.
- [ ] `[MethodImpl(MethodImplOptions.NoInlining)]` attribute is present.
- [ ] cyc of `TriggerOrFallbackFlattenExecution` = 5.
- [ ] Both catch handlers (`InvalidOperationException when (...)` and `catch (Exception)`) preserved.
- [ ] `isFlattenRunning` field mutation locations are unchanged — inside catch handlers only.
- [ ] No `lock()` blocks introduced.
- [ ] ASCII-only identifiers.
- [ ] xUnit `[Fact]` tests for all 3 paths pass (`Assert.Equal` only, no NUnit/MSTest).
- [ ] `dotnet build src/` passes with zero errors.

---

## Post-Extraction Verification (Phase 5 engineer required steps)

After all 3 tickets are complete:

1. **Residual parent CYC check:** `ClosePositionsOnlyApexAccounts` must have cyc = 2
   (baseline + `if (!EnableSIMA)` early-return only).
2. **max_cyc_projected check:** Highest extracted helper cyc must be 5 (`TriggerOrFallbackFlattenExecution`).
3. **Build gate:** `dotnet build src/` — zero errors, zero warnings introduced by this epic.
4. **deploy-sync:** Run `bash deploy-sync.sh` (or equivalent) to re-synchronize NinjaTrader hard links.
5. **Jane Street compliance final check:**
   - Zero `lock()` blocks: `grep -n "lock(" src/V12_002.SIMA.Flatten.cs` → 0 results.
   - ASCII-only: no Unicode/emoji in new identifiers.
   - All helpers `[MethodImpl(MethodImplOptions.NoInlining)]` confirmed.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Epic** | EPIC-W7-100 |
| **Method** | ClosePositionsOnlyApexAccounts |
| **Source File** | src/V12_002.SIMA.Flatten.cs |
| **Phase** | 4 |
| **ticket_count** | 3 |
| **max_cyc_projected** | 5 |
| **Residual parent CYC** | 2 |
| **dna_verdict** | PASS |
