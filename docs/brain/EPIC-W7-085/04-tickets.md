# Phase 4: Tickets — EPIC-W7-085

## Epic Summary

| Key | Value |
|---|---|
| **Epic** | EPIC-W7-085 |
| **Method** | `AuditMaster_HandleDesyncFlatten` |
| **Source File** | `src/V12_002.REAPER.Audit.cs` (lines 582–619) |
| **Original CYC** | 10 |
| **extraction_count** | 2 |
| **max_cyc_projected** | 5 |
| **DNA Verdict** | PASS |
| **Wave** | 7 / Lane P4-L5 |

---

## Ticket W7-085-T1: Extract `AuditMaster_TriggerFlattenEvent`

**Title:** Extract flatten event dispatch into `AuditMaster_TriggerFlattenEvent(string flattenKey)` (CYC=3)

**Description:**
The `TriggerCustomEvent` try/catch block inside `AuditMaster_HandleDesyncFlatten` is a distinct
responsibility: safely dispatch the flatten event to the Actor queue and recover the in-flight
guard (`_reaperFlattenInFlight.TryRemove`) on failure. This extraction isolates the dispatch
concern into a named private helper with a `flattenKey` parameter, eliminating the risk of a
permanently-blocked flatten cycle when `TriggerCustomEvent` throws.

The `flattenKey` string (`Account.Name + "_" + Instrument.FullName`) is pre-allocated at the
call site — no new heap allocations are introduced by this extraction.

**Acceptance Criteria:**
- [ ] New private method `AuditMaster_TriggerFlattenEvent(string flattenKey)` exists in
  `src/V12_002.REAPER.Audit.cs`.
- [ ] Method contains the `TriggerCustomEvent(o => ProcessReaperFlattenQueue(), null)` call
  and the `catch (Exception _mFlatTriggerEx)` block with `_reaperFlattenInFlight.TryRemove`.
- [ ] No `lock()` blocks — ConcurrentDictionary atomic `TryRemove` used exclusively.
- [ ] All string literals are ASCII-only (no Unicode or curly quotes).
- [ ] Build passes: `dotnet build src/` with zero errors and zero new warnings.

**CYC Impact:**
- Helper `AuditMaster_TriggerFlattenEvent`: **CYC = 3** (lambda `+1`, `if EnqueueReaperMasterFlatten` `+1`, catch `+1`)
- Parent CYC reduced by 3 (lambda + catch removed from parent body).

---

## Ticket W7-085-T2: Extract `AuditMaster_HandleGhostFlatLog` [NoInlining]

**Title:** Extract ghost-flat detection into `AuditMaster_HandleGhostFlatLog` with `[MethodImpl(MethodImplOptions.NoInlining)]` (CYC=2)

**Description:**
The ghost-flat compound check (`masterActualQty == 0 && masterExpectedQty != 0`) and its
conditional log print represent a cold-path classification concern that is semantically distinct
from the critical-desync flatten arm. This extraction isolates ghost-flat logic into a named
private helper.

The `[MethodImpl(MethodImplOptions.NoInlining)]` attribute is **mandatory**: this is a cold
path that must not be inlined into the hot-path parent, keeping the hot-path JIT footprint
minimal and enabling the JIT to optimize the critical-desync arm independently.

**Acceptance Criteria:**
- [ ] New private method `AuditMaster_HandleGhostFlatLog(bool shouldLog, int masterActualQty, int masterExpectedQty)`
  exists in `src/V12_002.REAPER.Audit.cs`.
- [ ] Method is decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`.
- [ ] Method contains the compound check `masterActualQty == 0 && masterExpectedQty != 0`
  and the guarded `Print(...)` call.
- [ ] No `lock()` blocks — cold path is stateless read-only logic.
- [ ] All string literals are ASCII-only.
- [ ] Build passes: `dotnet build src/` with zero errors and zero new warnings.

**CYC Impact:**
- Helper `AuditMaster_HandleGhostFlatLog`: **CYC = 2** (compound `&&` `+1`, `if (shouldLog)` `+1`)
- Parent CYC reduced by 2 (compound check + shouldLog branch removed from parent body).

---

## Ticket W7-085-T3: Refactor Parent `AuditMaster_HandleDesyncFlatten` to Call Helpers (CYC 10→5)

**Title:** Refactor `AuditMaster_HandleDesyncFlatten` to delegate to extracted helpers (parent CYC 10→5)

**Description:**
After the two extractions in T1 and T2, the parent method `AuditMaster_HandleDesyncFlatten`
must be updated to call the helpers instead of containing the extracted logic inline. The
refactored parent retains only the structural skeleton: outer desync guard, ghost-flat arm
delegation, critical-desync else-if arm, shouldLog print, and EnqueueReaperMasterFlatten guard.
This is a pure behavior-preserving refactoring — no logic changes, only delegation.

The caller `AuditMasterAccountIfNeeded` (line 684) is unaffected — the parent method signature
`private void AuditMaster_HandleDesyncFlatten(bool shouldLog, int masterActualQty, int masterExpectedQty)`
is unchanged.

**Acceptance Criteria:**
- [ ] Parent method body matches the architecture-plan skeleton: outer guard, ghost-flat helper
  call, else-if critical-desync arm, shouldLog print, EnqueueReaperMasterFlatten guard,
  TriggerFlattenEvent helper call.
- [ ] No inline try/catch in the parent body (moved to `AuditMaster_TriggerFlattenEvent`).
- [ ] No compound `&&` in the parent body (moved to `AuditMaster_HandleGhostFlatLog`).
- [ ] Caller `AuditMasterAccountIfNeeded` signature and call site are unchanged.
- [ ] Build passes: `dotnet build src/` with zero errors and zero new warnings.
- [ ] `dotnet csharpier check src/` reports zero formatting issues.

**CYC Impact:**
- Parent `AuditMaster_HandleDesyncFlatten`: **CYC = 5** (base `+1`, outer guard `+1`,
  else-if `+1`, shouldLog `+1`, EnqueueReaperMasterFlatten guard `+1`)
- Reduction from original CYC 10 to projected CYC 5 confirmed.

---

## Ticket W7-085-T4: Verify CYC Compliance (max_cyc_projected=5, all methods ≤8)

**Title:** Verify cyclomatic complexity compliance for all three post-extraction methods

**Description:**
Run `complexity_audit.py` against `src/V12_002.REAPER.Audit.cs` to verify that all three
post-extraction methods satisfy the V12 Jane Street CYC ≤ 8 threshold. The max_cyc_projected
across all extracted and refactored methods must be 5. No method introduced or modified by
this epic may exceed CYC 8.

**Acceptance Criteria:**
- [ ] `python scripts/complexity_audit.py` reports `AuditMaster_HandleDesyncFlatten` CYC ≤ 8
  (projected: 5).
- [ ] `python scripts/complexity_audit.py` reports `AuditMaster_TriggerFlattenEvent` CYC ≤ 8
  (projected: 3).
- [ ] `python scripts/complexity_audit.py` reports `AuditMaster_HandleGhostFlatLog` CYC ≤ 8
  (projected: 2).
- [ ] max_cyc_projected across all three methods = 5 (confirmed ≤ 8 Jane Street threshold).
- [ ] `powershell -File .\scripts\pre_push_validation.ps1 -Fast` passes all blocking checks.
- [ ] Zero `lock()` blocks confirmed via `grep -r "lock(" src/V12_002.REAPER.Audit.cs`.

**CYC Impact:**
- All three methods: **max CYC = 5** (well within ≤ 8 threshold).
- Original CYC 10 eliminated from the codebase.

---

## Ticket W7-085-T5: Update Manifest

**Title:** Update `docs/brain/EPIC-W7-085/manifest.json` with phase_5 completion markers

**Description:**
After T1–T4 are completed and all acceptance criteria pass, update the epic manifest to record
phase_5 completion. Set each ticket status to `completed` with its corresponding output
artifact path. This closes the Phase 5 execution loop for EPIC-W7-085.

**Acceptance Criteria:**
- [ ] `docs/brain/EPIC-W7-085/manifest.json` contains `phase_5.status = "completed"`.
- [ ] Manifest records `ticket-1-completion.md`, `ticket-2-completion.md`,
  `ticket-3-completion.md` output paths.
- [ ] Manifest records `phase_5.cyc_after = 5` and `phase_5.extraction_count = 2`.
- [ ] All upstream phase statuses (phase_0 through phase_4) are `"completed"`.

**CYC Impact:**
- Administrative ticket — no CYC change. Confirms the epic extraction is recorded in the
  manifest state machine for wave-level rollup reporting.

---

## CYC Reduction Summary

| Method | Before | After | Delta |
|---|---|---|---|
| `AuditMaster_HandleDesyncFlatten` (parent) | 10 | 5 | -5 |
| `AuditMaster_TriggerFlattenEvent` (new) | — | 3 | new |
| `AuditMaster_HandleGhostFlatLog` (new) | — | 2 | new |
| **max_cyc_projected** | **10** | **5** | **-5** |

All methods satisfy the V12 Jane Street CYC ≤ 8 standard.
Total CYC budget for this epic's three methods: 5 + 3 + 2 = 10 (same total; distributed
across three focused, single-responsibility methods instead of one god function).

---

## Agent Tracking

| Key | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-085 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Lane** | P4-L5 |
| **Execution Time** | 2026-06-29T02:20:00Z |
| **Output** | docs/brain/EPIC-W7-085/04-tickets.md |
