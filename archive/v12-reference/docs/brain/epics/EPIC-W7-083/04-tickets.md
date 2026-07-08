# EPIC-W7-083 — Phase 4: Ticket Generation

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:15:00Z
**Input:** docs/brain/EPIC-W7-083/02-architecture-plan.md, docs/brain/EPIC-W7-083/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-083 |
| **Method** | `AuditMaster_CheckExpectedActual` |
| **Source File** | `src/V12_002.REAPER.Audit.cs` |
| **Current CYC** | 13 |
| **Max CYC Projected** | 4 |
| **Extraction Count** | 3 |
| **Ticket Count** | 6 |
| **DNA Verdict** | PASS |

All tickets target a single file: `src/V12_002.REAPER.Audit.cs`. One PR. Zero cross-file blast radius.

---

## Ticket W7-083-T1: Extract `AuditMaster_IsInFillGrace` [AggressiveInlining]

**Title:** Extract hot-path fill grace predicate `AuditMaster_IsInFillGrace` with `[AggressiveInlining]`

**Description:**
Extract the fill grace time-window check from `AuditMaster_CheckExpectedActual` into a standalone
private helper `AuditMaster_IsInFillGrace`. This extraction isolates the lock-free `Interlocked.Read`
atomic read and the ticks comparison into a single-responsibility predicate. The `[AggressiveInlining]`
attribute is mandated by Jane Street alignment (carl_cook pattern): hot-path predicates must be
inlined to eliminate call overhead and keep the JIT instruction cache clean.

**Signature to add:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private bool AuditMaster_IsInFillGrace()
{
    long stampTicks = Interlocked.Read(ref _lastExpectedPositionSetTicks);
    return stampTicks > 0 && (DateTime.UtcNow.Ticks - stampTicks) < ReaperFillGraceTicks;
}
```

**Acceptance Criteria:**
- [ ] Method `AuditMaster_IsInFillGrace` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] Decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Uses `Interlocked.Read` (no `lock()` blocks)
- [ ] Returns `bool` with zero side effects
- [ ] Build passes: `dotnet build` → zero errors

**CYC Impact:** New helper CYC = 2 (<=8 mandate: PASS). Contributes to parent extraction target.

---

## Ticket W7-083-T2: Extract `AuditMaster_IsCriticalDesync` [AggressiveInlining]

**Title:** Extract hot-path critical desync predicate `AuditMaster_IsCriticalDesync` with `[AggressiveInlining]`

**Description:**
Extract the critical desync quantity-mismatch evaluation into a standalone private helper
`AuditMaster_IsCriticalDesync`. This extraction isolates the two-branch compound predicate
(side-flip check and zero-vs-nonzero check) into a single-responsibility pure function with
no side effects. The `[AggressiveInlining]` attribute is required per Jane Street carl_cook
alignment: this predicate is called on the hot path and must not introduce call overhead.
The extraction makes the desync logic independently testable and verifiable.

**Signature to add:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private bool AuditMaster_IsCriticalDesync(int masterActualQty, int masterExpectedQty)
{
    return (masterActualQty != 0 && masterExpectedQty == 0)
        || (Math.Sign(masterActualQty) != Math.Sign(masterExpectedQty) && masterExpectedQty != 0);
}
```

**Acceptance Criteria:**
- [ ] Method `AuditMaster_IsCriticalDesync` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] Decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Pure function: no side effects, no `Print`, no state mutation
- [ ] Accepts `int masterActualQty, int masterExpectedQty` parameters
- [ ] Build passes: `dotnet build` → zero errors

**CYC Impact:** New helper CYC = 3 (<=8 mandate: PASS). Second of 3 extractions reducing parent CYC.

---

## Ticket W7-083-T3: Extract `AuditMaster_LogDesyncState` [NoInlining]

**Title:** Extract cold-path desync logging sink `AuditMaster_LogDesyncState` with `[NoInlining]`

**Description:**
Extract all desync-state `Print` logging calls from `AuditMaster_CheckExpectedActual` into a
standalone cold-path method `AuditMaster_LogDesyncState`. This extraction moves all string
formatting and print calls out of the hot path per the Jane Street carl_cook cold-path extraction
pattern. The `[NoInlining]` attribute is mandatory: cold-path logging must never be inlined into
the hot-path instruction cache. This method is only called when `shouldLog == true`, keeping all
string allocation and formatting off the critical execution path.

**Signature to add:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private void AuditMaster_LogDesyncState(
    bool isCriticalDesync,
    bool inFillGrace,
    int masterExpectedQty,
    int masterActualQty)
{
    if (inFillGrace)
    {
        Print($"[REAPER] {Account.Name} (Master): Fill grace active -- desync check suppressed.");
        return;
    }
    if (isCriticalDesync)
    {
        Print(
            $"[REAPER] CRITICAL DESYNC on {Account.Name} (Master): Expected={masterExpectedQty}, Actual={masterActualQty}"
        );
        return;
    }
    Print(
        $"[REAPER] Minor Desync on {Account.Name} (Master): Expected={masterExpectedQty}, Actual={masterActualQty}"
    );
}
```

**Acceptance Criteria:**
- [ ] Method `AuditMaster_LogDesyncState` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] Decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`
- [ ] All `Print` calls from original method consolidated here
- [ ] No string formatting outside this method in the parent
- [ ] Build passes: `dotnet build` → zero errors

**CYC Impact:** New helper CYC = 3 (<=8 mandate: PASS). Final extraction completing the 3-helper set.

---

## Ticket W7-083-T4: Refactor Parent `AuditMaster_CheckExpectedActual` to Delegate to Helpers

**Title:** Rewrite parent `AuditMaster_CheckExpectedActual` to call extracted helpers (CYC 13 → 4)

**Description:**
Replace the body of `AuditMaster_CheckExpectedActual` with calls to the 3 extracted helpers
produced in T1, T2, and T3. This is the core CYC reduction step: the parent method retains
only orchestration logic (evaluate fill grace, evaluate desync, dispatch logging, return flatten
decision). All conditional complexity is delegated to helpers. The method signature is unchanged
— callers `AuditMaster_HandleDesyncFlatten` (line 582 area) and `AuditMaster_AccountIfNeeded`
(line 684 area) require no modification.

**Target body after extraction:**
```csharp
private bool AuditMaster_CheckExpectedActual(bool shouldLog, int masterActualQty, int masterExpectedQty)
{
    bool inFillGrace = AuditMaster_IsInFillGrace();
    bool isCriticalDesync = !inFillGrace && AuditMaster_IsCriticalDesync(masterActualQty, masterExpectedQty);
    if (shouldLog)
    {
        AuditMaster_LogDesyncState(isCriticalDesync, inFillGrace, masterExpectedQty, masterActualQty);
    }
    if (isCriticalDesync && AutoFlattenDesync)
    {
        return true;
    }
    return false;
}
```

**Acceptance Criteria:**
- [ ] Parent method body matches the target above (or behaviorally equivalent)
- [ ] Method signature unchanged: `bool AuditMaster_CheckExpectedActual(bool shouldLog, int masterActualQty, int masterExpectedQty)`
- [ ] All original callers unchanged (`AuditMaster_HandleDesyncFlatten`, `AuditMaster_AccountIfNeeded`)
- [ ] No `lock()` blocks introduced
- [ ] Build passes: `dotnet build` → zero errors
- [ ] `dotnet csharpier check src/` passes (no formatting violations)

**CYC Impact:** Parent CYC reduces 13 → 4 (<=8 mandate: PASS). Primary CYC reduction ticket.

---

## Ticket W7-083-T5: Verify CYC Compliance (max_cyc_projected = 4, all symbols <= 8)

**Title:** Verify CYC compliance for all 4 symbols post-extraction

**Description:**
After completing T1–T4, run the complexity audit to confirm all 4 symbols meet the Jane Street
CYC <= 8 mandate. Use `python scripts/complexity_audit.py` scoped to `src/V12_002.REAPER.Audit.cs`
and validate the CYC values match projections from the architecture plan. Also run the full pre-push
validation suite (`powershell -File .\scripts\pre_push_validation.ps1 -Fast`) to confirm build,
format, and lint gates pass. Confirm zero `lock()` patterns remain in the file.

**Acceptance Criteria:**
- [ ] `AuditMaster_CheckExpectedActual` CYC = 4 (projected: 4)
- [ ] `AuditMaster_IsInFillGrace` CYC = 2 (projected: 2)
- [ ] `AuditMaster_IsCriticalDesync` CYC = 3 (projected: 3)
- [ ] `AuditMaster_LogDesyncState` CYC = 3 (projected: 3)
- [ ] max_cyc across all 4 symbols = 4 (<= 8: PASS)
- [ ] `grep -c "lock(" src/V12_002.REAPER.Audit.cs` returns 0
- [ ] `dotnet build` → zero errors, zero warnings
- [ ] `dotnet csharpier check src/` → zero issues
- [ ] Pre-push validation `-Fast` → all blocking checks green

**CYC Impact:** Verification ticket — confirms max_cyc_projected = 4, epic CYC target achieved.

---

## Ticket W7-083-T6: Update Manifest

**Title:** Update `docs/brain/EPIC-W7-083/manifest.json` to reflect Phase 5 completion

**Description:**
After T5 verification passes, update `docs/brain/EPIC-W7-083/manifest.json` to mark
`phase_5` as completed and record the extraction outputs. Set `epic_status` to reflect
wave-level completion state. This ticket closes the EPIC-W7-083 execution loop.

**Acceptance Criteria:**
- [ ] `manifest.json` phase_5 status = "completed"
- [ ] `manifest.json` records ticket_count = 6
- [ ] `manifest.json` records extraction_count = 3
- [ ] `manifest.json` records max_cyc_achieved = 4
- [ ] manifest committed alongside src/ changes in the same PR

**CYC Impact:** Manifest-only ticket — no CYC change. Closes epic tracking.

---

## CYC Budget Summary

| Symbol | Current CYC | Projected CYC | Ticket | Status |
|---|---|---|---|---|
| `AuditMaster_CheckExpectedActual` (parent) | 13 | 4 | T4 | PASS (<=8) |
| `AuditMaster_IsInFillGrace` | — | 2 | T1 | PASS (<=8) |
| `AuditMaster_IsCriticalDesync` | — | 3 | T2 | PASS (<=8) |
| `AuditMaster_LogDesyncState` | — | 3 | T3 | PASS (<=8) |
| **max_cyc_projected** | | **4** | T5 | **PASS (<=8)** |

---

## Execution Order

```
T1 (extract IsInFillGrace)
T2 (extract IsCriticalDesync)
T3 (extract LogDesyncState)
T4 (refactor parent — depends on T1, T2, T3)
T5 (verify compliance — depends on T4)
T6 (update manifest — depends on T5)
```

T1, T2, T3 may be executed in sequence within a single Phase 5 session.
T4 must follow T1–T3. T5 must follow T4. T6 must follow T5.

---

## Jane Street Alignment Summary

| Helper | Attribute | Rationale |
|---|---|---|
| `AuditMaster_IsInFillGrace` | `[AggressiveInlining]` | Hot-path predicate, zero-alloc atomic read |
| `AuditMaster_IsCriticalDesync` | `[AggressiveInlining]` | Hot-path predicate, pure function, zero-alloc |
| `AuditMaster_LogDesyncState` | `[NoInlining]` | Cold-path logging, string formatting, keep off hot JIT cache |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Epic** | EPIC-W7-083 |
| **Phase** | 4 |
| **Method** | AuditMaster_CheckExpectedActual |
| **Source File** | src/V12_002.REAPER.Audit.cs |
| **Ticket Count** | 6 |
| **Extraction Count** | 3 |
| **Max CYC Projected** | 4 |
| **Output** | docs/brain/EPIC-W7-083/04-tickets.md |
