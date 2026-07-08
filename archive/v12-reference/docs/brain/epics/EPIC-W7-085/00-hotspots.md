# EPIC-W7-085 — Phase 0: Hotspot Analysis

## Method Name

`AuditMaster_HandleDesyncFlatten`

## CYC

**10** (tool baseline: `complexity_audit_wave4.txt`; Codacy independently reports 9 —
delta of 1 from the `o =>` lambda counted as a branch by the static-analysis scanner)

## File Path

`src/V12_002.REAPER.Audit.cs` — lines 582–619

## Blast Radius Summary

The method is a pure dispatcher called exclusively by `AuditMasterAccountIfNeeded`
(line 700). It touches shared concurrency state that fans across **5 files**:

| File | Role |
|---|---|
| `src/V12_002.REAPER.Audit.cs` | Definition + sole call site (`AuditMasterAccountIfNeeded`) |
| `src/V12_002.REAPER.cs` | `_reaperFlattenInFlight`, `_reaperFlattenQueue` field declarations |
| `src/V12_002.REAPER.FlattenQueue.cs` | `ProcessReaperFlattenQueue` — enqueued target, reads the same queue |
| `src/V12_002.cs` | `TriggerCustomEvent` dispatcher; strategy-thread boundary |
| `src/V12_002.Properties.cs` | `AutoFlattenDesync` parameter (gate read by `AuditMaster_CheckExpectedActual`) |

Direct callers: **1** (`AuditMasterAccountIfNeeded`).
Shared-state dependents: **4 additional files**.
Risk level: **Low-Medium** — the method itself only enqueues; no order submission occurs
here. The most dangerous coupling is the `_reaperFlattenInFlight` in-flight guard; if its
`TryRemove` in the catch block is lost, future flatten cycles are permanently blocked for
the master account.

## Top 3 Complexity Drivers

### 1 — Three-level nested `if` / `else if` desync classification (CYC contribution ~4)

```csharp
if (masterExpectedQty != masterActualQty)            // outer guard
{
    if (masterActualQty == 0 && masterExpectedQty != 0)  // ghost-flat branch (+2 for &&)
    { … }
    else if (AuditMaster_CheckExpectedActual(…))     // critical-desync delegate
    { … }
}
```

Three decision points are stacked within a single method that conceptually only needs
to answer "which desync category am I in?" The outer guard, the ghost-flat compound,
and the `else if` delegate call are three independent reasons to reach different
terminal states.

### 2 — Double `shouldLog` branches inside each arm (CYC contribution ~2)

```csharp
if (shouldLog)                                          // arm 1 log gate
    Print($"[REAPER] … is Flat (Target/Stop hit)…");
…
if (shouldLog)                                          // arm 2 log gate
    Print($"[REAPER] QUEUING FLATTEN …");
```

Each arm independently re-gates on `shouldLog`, adding two extra paths while providing
no structural value beyond throttled console output. Extracting a `PrintIfLog(bool,
string)` helper — or collapsing logging into the callee — would eliminate both.

### 3 — Exception-recovery path on `TriggerCustomEvent` (CYC contribution ~2)

```csharp
try
{
    TriggerCustomEvent(o => ProcessReaperFlattenQueue(), null);  // lambda = +1
}
catch (Exception _mFlatTriggerEx)                               // catch = +1
{
    _reaperFlattenInFlight.TryRemove(Account.Name + "_" + Instrument.FullName, out _);
    Print("[REAPER] TriggerCustomEvent failed for master flatten: " + …);
}
```

The `try/catch` block (including the lambda) contributes 2 cyclomatic units. Identical
in-flight-cleanup-on-failure patterns exist in `AuditFleet_HandleCriticalDesyncFlatten`,
`AuditFleet_HandleDesyncRepair`, and `AuditFleet_HandleNakedPosition` — a shared helper
would unify all four sites and remove this driver entirely from each.

## Recommended Extraction Count

**2 extractions** — targeting a post-refactor CYC of ≤ 5 for the residual dispatcher body:

| # | Proposed Method | Eliminates |
|---|---|---|
| 1 | `AuditMaster_TriggerFlattenEvent(string flattenKey)` | `TriggerCustomEvent` + catch + in-flight `TryRemove` — mirrors pattern in `AuditFleet_HandleCriticalDesyncFlatten` (~2 CYC) |
| 2 | `AuditMaster_HandleGhostFlatLog(bool shouldLog, int masterExpectedQty)` | Ghost-flat `if` arm + its `shouldLog` log gate, leaving the outer guard and critical-desync arm only (~2 CYC) |

---

## Agent Tracking

| Key | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~40s |
| **Epic** | EPIC-W7-085 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Source Tool** | Bob (native file tools + grep + sequential analysis) |
| **CYC Confirmed** | 0 (wave7-epic-list baseline) / 10 (complexity_audit_wave4 + cross-reference) |
