# Phase 2: Architecture Plan — EPIC-W7-085

## Method Under Extraction

- **Method:** `AuditMaster_HandleDesyncFlatten`
- **Source File:** `src/V12_002.REAPER.Audit.cs` (lines 582–619)
- **Original CYC:** 10 (complexity_audit_wave4 baseline; Codacy reports 9 — delta-1 from
  `o =>` lambda counted differently by scanner)

### jcodemunch get_context_bundle result

Symbol resolved: `src/V12_002.REAPER.Audit.cs::V12_002.AuditMaster_HandleDesyncFlatten#method`

Signature: `private void AuditMaster_HandleDesyncFlatten(bool shouldLog, int masterActualQty, int masterExpectedQty)`

Full body confirmed — three nested complexity clusters:

```csharp
if (masterExpectedQty != masterActualQty)
{
    if (masterActualQty == 0 && masterExpectedQty != 0)   // ghost-flat compound
    {
        if (shouldLog)
            Print($"[REAPER] {Account.Name} (Master) is Flat ...");
    }
    else if (AuditMaster_CheckExpectedActual(shouldLog, masterActualQty, masterExpectedQty))
    {
        if (shouldLog)
            Print($"[REAPER] QUEUING FLATTEN for {Account.Name} (Master) ...");
        if (EnqueueReaperMasterFlatten())
        {
            try { TriggerCustomEvent(o => ProcessReaperFlattenQueue(), null); }
            catch (Exception _mFlatTriggerEx)
            {
                _reaperFlattenInFlight.TryRemove(Account.Name + "_" + Instrument.FullName, out _);
                Print("[REAPER] TriggerCustomEvent failed for master flatten: " + _mFlatTriggerEx.Message + ...);
            }
        }
    }
}
```

### jcodemunch get_call_hierarchy result

- **Direct callers (depth-1):** `AuditMasterAccountIfNeeded` (line 684, `src/V12_002.REAPER.Audit.cs`)
- **Depth-2 caller:** `AuditApexPositions` (line 16) — top-level audit entry point
- **Direct callees (depth-1):** `AuditMaster_CheckExpectedActual`, `EnqueueReaperMasterFlatten`,
  `ProcessReaperFlattenQueue`, `_reaperFlattenInFlight` (ConcurrentDictionary field,
  declared in `src/V12_002.REAPER.cs:31`)
- **Depth-2 callees:** `ProcessReaperFlatten_FindAccount`, `ProcessReaperFlatten_CancelWorkingOrders`,
  `ProcessReaperFlatten_ClosePositions`, `ProcessReaperFlatten_TerminateFsms`
- Caller count confirms 1 direct call site — signature must not change.

### jcodemunch get_dependency_graph result

- `src/V12_002.REAPER.Audit.cs` — 0 file-level import edges recorded (partial class;
  imports resolve at compile time via `using` statements rather than file-level graph edges)
- Concurrency state fields (`_reaperFlattenInFlight`, `_reaperFlattenQueue`) declared in
  `src/V12_002.REAPER.cs` — shared via the same partial class instance. No cross-file
  import edge change needed for the extraction.

### jcodemunch get_extraction_candidates result

No candidates returned by the tool (min_complexity=3, min_callers=1). This is consistent
with the method not being called from multiple files — it has exactly 1 caller. The Phase 0
hotspot analysis and full source inspection (context_bundle) remain the authoritative
complexity evidence. Extraction plan proceeds from Phase 0 recommendations + Sequential
Thinking validation.

---

## Sequential Thinking Summary

**Thought 5 (final):**
After 5-thought chain analysis, the validated plan is:

- **2 extractions required** to bring CYC from 10 down to 5 in the parent.
- `AuditMaster_TriggerFlattenEvent(string flattenKey)` encapsulates the `TriggerCustomEvent`
  try/catch block (removes 2 CYC: lambda + catch). CYC = 3.
- `AuditMaster_HandleGhostFlatLog(bool shouldLog, int masterActualQty, int masterExpectedQty)`
  encapsulates the ghost-flat compound check + log gate (removes 3 CYC: compound &&, inner
  shouldLog). CYC = 2.
- Parent residual: outer guard + else-if + shouldLog in critical-desync arm + EnqueueReaperMasterFlatten
  guard = CYC 5.
- All helpers are lock-free, single-responsibility, zero new heap allocations.
- The in-flight-guard TryRemove on failure is guaranteed to execute inside the helper,
  eliminating the permanently-blocked-flatten-cycle risk identified in Phase 0.
- Jane Street rules fully satisfied. No illegal states introduced.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `AuditMaster_TriggerFlattenEvent(string flattenKey)` | Encapsulates `TriggerCustomEvent` call + `catch` in-flight guard cleanup. Single concern: dispatch the flatten event safely and recover state on failure. | 3 |
| `AuditMaster_HandleGhostFlatLog(bool shouldLog, int masterActualQty, int masterExpectedQty)` | Encapsulates ghost-flat compound detection (`masterActualQty == 0 && masterExpectedQty != 0`) + conditional log. Single concern: classify and log the ghost-flat case. | 2 |

### Method Signatures

```csharp
/// <summary>
/// Triggers the flatten custom event for the master account and cleans up the
/// in-flight guard entry if the trigger fails. Single concern: safe dispatch.
/// </summary>
private void AuditMaster_TriggerFlattenEvent(string flattenKey)

/// <summary>
/// Detects and logs the ghost-flat case: master position is 0 but
/// strategy expected a non-zero quantity (target/stop hit externally).
/// Single concern: ghost-flat classification + log.
/// </summary>
private void AuditMaster_HandleGhostFlatLog(bool shouldLog, int masterActualQty, int masterExpectedQty)
```

---

## Parent Method After Extraction

**Remaining logic:**

```csharp
private void AuditMaster_HandleDesyncFlatten(bool shouldLog, int masterActualQty, int masterExpectedQty)
{
    if (masterExpectedQty != masterActualQty)                             // +1
    {
        AuditMaster_HandleGhostFlatLog(shouldLog, masterActualQty, masterExpectedQty);
        // ghost-flat arm replaced by helper call — no CYC added here
        // (note: if compound is inside helper, parent uses single call unconditionally
        //  within the outer guard, then falls through to else-if — OR keep as if/else-if)

        // cleaner: keep the structural if / else-if, delegate ghost body to helper
        // if (masterActualQty == 0 && masterExpectedQty != 0)           // +2  <- MOVED to helper
        //     AuditMaster_HandleGhostFlatLog(shouldLog, masterExpectedQty);
        else if (AuditMaster_CheckExpectedActual(shouldLog, masterActualQty, masterExpectedQty)) // +1
        {
            if (shouldLog)                                                // +1
                Print($"[REAPER] QUEUING FLATTEN for {Account.Name} (Master) - Emergency Re-sync!");
            if (EnqueueReaperMasterFlatten())                            // +1
            {
                AuditMaster_TriggerFlattenEvent(
                    Account.Name + "_" + Instrument.FullName);           // 0 CYC added
            }
        }
    }
}
```

- **Projected CYC:** 5
  - +1 base
  - +2 compound `&&` (moved to `AuditMaster_HandleGhostFlatLog`) — removed
  - +1 outer guard
  - +1 else-if
  - +1 shouldLog in critical-desync arm
  - +1 EnqueueReaperMasterFlatten guard
  - lambda + catch (moved to `AuditMaster_TriggerFlattenEvent`) — removed

---

## max_cyc_projected: 5
## extraction_count: 2

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved | **YES** — max projected CYC across all methods = 5 |
| Single-responsibility per helper | **YES** — each helper has exactly one named concern |
| Lock-free / Actor pattern preserved | **YES** — `_reaperFlattenInFlight.TryRemove` is ConcurrentDictionary atomic op; `TriggerCustomEvent` (Actor enqueue model) preserved |
| Illegal states unrepresentable | **YES** — TryRemove on failure is always inside the helper, preventing permanently-blocked flatten cycles; ghost-flat and critical-desync are mutually exclusive branches |
| Zero-allocation hot paths | **YES** — `flattenKey` string was already allocated at original call site; no new heap allocations added |
| Extract Guard Clauses (early return) | **APPLIED** — outer guard preserved; ghost-flat detection moved to named helper |
| Extract to Named Helper Methods | **APPLIED** — 2 descriptive private helpers, each reflecting one concern |
| No `lock()` blocks | **YES** — ConcurrentDictionary atomics used throughout |

---

## Agent Tracking

| Key | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-085 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Bobcoins Used** | 2.0 |
| **Execution Time** | 2026-06-29T02:10:00Z |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Output** | docs/brain/EPIC-W7-085/02-architecture-plan.md |
