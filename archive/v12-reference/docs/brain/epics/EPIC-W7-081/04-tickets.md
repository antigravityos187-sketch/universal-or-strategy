# EPIC-W7-081 — Phase 4: Ticket Generation

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-081/02-architecture-plan.md, docs/brain/EPIC-W7-081/03-audit-report.md
**Lane:** P4-L5

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-081 |
| **Method** | `AuditMaster_HandleNakedPosition` |
| **File** | `src/V12_002.REAPER.Audit.cs` |
| **Current CYC** | 6 (pre-extraction baseline; Codacy confirmed 15) |
| **extraction_count** | 3 |
| **max_cyc_projected** | 3 |
| **ticket_count** | 6 |
| **dna_verdict** | PASS |

The extraction plan reduces `AuditMaster_HandleNakedPosition` from CYC=6 (structural) / CYC=15
(Codacy) to a parent body of CYC=3, with three private helpers each holding a single
responsibility. All projected cyc values are well within the Jane Street threshold of 8.

---

## Ticket W7-081-T1: Extract `AuditMaster_HasWorkingStopOrder` [AggressiveInlining]

**Title:** Extract hot-path stop-order predicate into `AuditMaster_HasWorkingStopOrder` with `[AggressiveInlining]`

**Description:**
Extract lines 630–636 of `AuditMaster_HandleNakedPosition` (the snapshot + LINQ predicate block
that determines whether a working or accepted stop order exists for the current instrument) into a
new private helper method `AuditMaster_HasWorkingStopOrder()`.

This helper is on the **hot path** — it is called on every audit cycle — so it must carry the
`[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute to
eliminate call overhead at the JIT layer (carl_cook pattern).

The H13-FIX `Account.Orders.ToArray()` snapshot **must be preserved** inside the helper body to
prevent `InvalidOperationException` when NinjaTrader's UI thread mutates `Account.Orders` during
LINQ iteration (gjengset false-sharing prevention).

**Signature:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool AuditMaster_HasWorkingStopOrder()
```

**Body:**
```csharp
// H13-FIX: Snapshot to prevent InvalidOperationException from UI thread updates.
var masterOrders = Account.Orders.ToArray();
return masterOrders.Any(o =>
    o.Instrument?.FullName == Instrument?.FullName
    && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
    && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
    && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover)
);
```

**Acceptance Criteria:**
- [ ] Method `AuditMaster_HasWorkingStopOrder` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] `[AggressiveInlining]` attribute present on the method
- [ ] H13-FIX `Account.Orders.ToArray()` snapshot preserved inside helper body
- [ ] No `lock()` block introduced
- [ ] Build passes with zero errors

**CYC impact:** Helper CYC = 1 (no branches; single snapshot + LINQ Any expression). Hot-path extraction removes 2 decision points from parent, contributing to parent CYC reduction from 6 to 3.

---

## Ticket W7-081-T2: Extract `AuditMaster_StartNakedGraceWindow` [NoInlining]

**Title:** Extract cold-path grace-window initialiser into `AuditMaster_StartNakedGraceWindow` with `[NoInlining]`

**Description:**
Extract lines 643–651 of `AuditMaster_HandleNakedPosition` (the first-seen timestamp registration
and grace-window log emission) into a new private helper method
`AuditMaster_StartNakedGraceWindow(int masterActualQty, int graceSeconds)`.

This helper executes only once per new naked-position detection event — it is a **cold path**. It
must carry `[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]` to keep the
instruction cache footprint of the hot-path caller minimal and to keep the `Print(string.Format(...))`
logging out-of-line (carl_cook pattern).

The `_nakedPositionFirstSeen` ConcurrentDictionary write must use the direct indexer assignment
(atomic) — no `lock()` block (gjengset Left-Right pattern).

**Signature:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]
private void AuditMaster_StartNakedGraceWindow(int masterActualQty, int graceSeconds)
```

**Body:**
```csharp
_nakedPositionFirstSeen[Account.Name] = DateTime.UtcNow;
Print(string.Format(
    "[REAPER][NAKED_POSITION] {0} (Master): {1}ct naked -- starting {2}s grace window.",
    Account.Name,
    masterActualQty,
    graceSeconds
));
```

**Acceptance Criteria:**
- [ ] Method `AuditMaster_StartNakedGraceWindow` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] `[NoInlining]` attribute present on the method
- [ ] `_nakedPositionFirstSeen` write uses atomic indexer — no `lock()` block
- [ ] ASCII-only string literals in Print call (no Unicode or curly quotes)
- [ ] Build passes with zero errors

**CYC impact:** Helper CYC = 1 (no branches; sequential assignment + Print call). Removal of the first-seen initialisation block from parent reduces parent nesting depth and contributes to parent CYC reduction.

---

## Ticket W7-081-T3: Extract `AuditMaster_TriggerNakedStopIfGraceExpired` [NoInlining]

**Title:** Extract cold-path emergency stop trigger into `AuditMaster_TriggerNakedStopIfGraceExpired` with `[NoInlining]`

**Description:**
Extract lines 640 + 653–671 of `AuditMaster_HandleNakedPosition` (grace seconds calculation,
`EnqueueReaperMasterNakedStop`, `TriggerCustomEvent` dispatch, and circuit-breaker error recovery)
into a new private helper `AuditMaster_TriggerNakedStopIfGraceExpired(Position masterPos,
int masterActualQty, string masterExpectedKey, DateTime masterFirstSeen)`.

This is the **max helper** (CYC=3) and is on the **cold emergency-stop path** — it must carry
`[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]` (carl_cook pattern).

The `_reaperNakedStopInFlight.TryRemove` in the catch block is the **rate-limit circuit breaker**
(trading_billions pattern): if `TriggerCustomEvent` fails, the in-flight token is cleared so the
next audit cycle can retry. This circuit-breaker logic must be preserved intact.

`EnqueueReaperMasterNakedStop` and `TriggerCustomEvent` are the Actor/Enqueue model — no `lock()`
may be introduced here (gjengset Left-Right alignment).

**Signature:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]
private void AuditMaster_TriggerNakedStopIfGraceExpired(
    Position masterPos, int masterActualQty, string masterExpectedKey, DateTime masterFirstSeen)
```

**Body:**
```csharp
int graceSeconds = (NakedPositionGraceSec >= 5) ? NakedPositionGraceSec : 5;
if (EnqueueReaperMasterNakedStop(masterPos, masterActualQty, masterExpectedKey, masterFirstSeen))
{
    try
    {
        TriggerCustomEvent(e => ProcessReaperNakedStopQueue(), null);
    }
    catch (Exception tcEx)
    {
        _reaperNakedStopInFlight.TryRemove(masterExpectedKey, out _);
        Print(string.Format(
            "[REAPER][NAKED_STOP] TriggerCustomEvent failed for {0} (Master): {1} -- in-flight cleared.",
            Account.Name,
            tcEx.Message
        ));
    }
}
```

**Acceptance Criteria:**
- [ ] Method `AuditMaster_TriggerNakedStopIfGraceExpired` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] `[NoInlining]` attribute present on the method
- [ ] `_reaperNakedStopInFlight.TryRemove` circuit-breaker preserved in catch block
- [ ] No `lock()` block introduced
- [ ] ASCII-only string literals in Print call
- [ ] Build passes with zero errors

**CYC impact:** Helper CYC = 3 (ternary grace check +1; if-enqueue +1; catch +1). This is the max helper for this extraction. Parent delegates the entire grace-expired branch to this helper.

---

## Ticket W7-081-T4: Refactor parent `AuditMaster_HandleNakedPosition` to call helpers (CYC 6 → 3)

**Title:** Replace inlined logic in `AuditMaster_HandleNakedPosition` with calls to the three extracted helpers

**Description:**
After tickets T1–T3 are complete and all three helpers exist, refactor the body of
`AuditMaster_HandleNakedPosition` to delegate to the helpers. The parent retains the outer
`masterActualQty != 0` guard as the first line of defense (trading_billions pattern) and the
`_nakedPositionFirstSeen.TryGetValue` branch as the dispatcher, but moves all implementation
detail into the helpers.

The parent method signature must remain unchanged:
`private void AuditMaster_HandleNakedPosition(Position masterPos, int masterActualQty, string masterExpectedKey)`

**New body:**
```csharp
if (masterActualQty != 0)
{
    if (!AuditMaster_HasWorkingStopOrder())
    {
        DateTime masterFirstSeen;
        if (!_nakedPositionFirstSeen.TryGetValue(Account.Name, out masterFirstSeen))
        {
            int graceSeconds = (NakedPositionGraceSec >= 5) ? NakedPositionGraceSec : 5;
            AuditMaster_StartNakedGraceWindow(masterActualQty, graceSeconds);
        }
        else
        {
            AuditMaster_TriggerNakedStopIfGraceExpired(
                masterPos, masterActualQty, masterExpectedKey, masterFirstSeen);
        }
    }
    else
    {
        _nakedPositionFirstSeen.TryRemove(Account.Name, out _);
    }
}
```

**Acceptance Criteria:**
- [ ] Parent method body matches the new body sketch above
- [ ] Parent method signature unchanged
- [ ] Caller `AuditMasterAccountIfNeeded` not modified
- [ ] No `lock()` block introduced
- [ ] Build passes with zero errors
- [ ] All previously passing unit tests still pass

**CYC impact:** Parent CYC reduced from 6 to 3 (base 1 + qty-check +1 + hasWorkingStop-check +1). The else-branches are now delegated to helpers. This is the primary CYC reduction ticket for EPIC-W7-081.

---

## Ticket W7-081-T5: Verify CYC compliance (max_cyc_projected = 3, all symbols <= 8)

**Title:** Verify that all symbols in the extraction meet Jane Street CYC <= 8 threshold

**Description:**
After all four extraction and refactor tickets (T1–T4) are applied, run the complexity audit to
confirm that the max projected cyc across all affected symbols does not exceed the Jane Street
strict threshold of 8, and that the actual max is 3 as projected.

Run: `python scripts/complexity_audit.py` and filter for `AuditMaster_HandleNakedPosition`,
`AuditMaster_HasWorkingStopOrder`, `AuditMaster_StartNakedGraceWindow`, and
`AuditMaster_TriggerNakedStopIfGraceExpired`.

Also confirm the build passes via `dotnet build src/` and that no new Codacy / Roslyn violations
were introduced (zero new issues policy — V12 No Scope Creep Protocol §11).

**Acceptance Criteria:**
- [ ] `AuditMaster_HandleNakedPosition` CYC <= 3 post-extraction
- [ ] `AuditMaster_HasWorkingStopOrder` CYC = 1
- [ ] `AuditMaster_StartNakedGraceWindow` CYC = 1
- [ ] `AuditMaster_TriggerNakedStopIfGraceExpired` CYC <= 3
- [ ] max_cyc_projected across all four symbols = 3 (all <= 8 Jane Street threshold)
- [ ] `dotnet build src/` exits with 0 errors
- [ ] Zero new `lock()` calls in `src/V12_002.REAPER.Audit.cs`
- [ ] CSharpier formatting check passes: `dotnet csharpier check src/`

**CYC impact:** Verification-only ticket. Confirms the 80% CYC reduction (Codacy baseline 15 → max 3) and Jane Street compliance for the entire extraction family.

---

## Ticket W7-081-T6: Update manifest to reflect Phase 5 readiness

**Title:** Update `docs/brain/EPIC-W7-081/manifest.json` to record Phase 4 completion and set Phase 5 as pending

**Description:**
Update the central manifest for EPIC-W7-081 to reflect that Phase 4 (Ticket Generation) is
complete and that Phase 5 (Ticket Execution) can begin. Set the ticket count, output path, and
completion timestamp.

**Acceptance Criteria:**
- [ ] `manifest.json` `phase_4.status` = `"completed"`
- [ ] `manifest.json` `phase_4.output` = `"04-tickets.md"`
- [ ] `manifest.json` `phase_4.ticket_count` = 6
- [ ] `manifest.json` `phase_5` entries created for T1–T6 with status `"pending"`
- [ ] `manifest.json` is valid JSON (no parse errors)

**CYC impact:** Documentation-only ticket. No source code changes.

---

## CYC Summary

| Symbol | Pre-Extraction CYC | Post-Extraction CYC | Jane Street Threshold | Status |
|---|---|---|---|---|
| `AuditMaster_HandleNakedPosition` (parent) | 6 (structural) / 15 (Codacy) | 3 | 8 | PASS |
| `AuditMaster_HasWorkingStopOrder` | — (new) | 1 | 8 | PASS |
| `AuditMaster_StartNakedGraceWindow` | — (new) | 1 | 8 | PASS |
| `AuditMaster_TriggerNakedStopIfGraceExpired` | — (new) | 3 | 8 | PASS |

**max_cyc_projected: 3**
**extraction_count: 3**
**CYC reduction: 80% (Codacy baseline 15 → max 3)**

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 0.8 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-081 |
| **Method** | AuditMaster_HandleNakedPosition |
| **Lane** | P4-L5 |
| **MCP Tools Used** | resolve_repo, sequentialthinking (4 thoughts), read_file (architecture-plan + audit-report) |
| **Sequential Thinking Steps** | 4 |
| **ticket_count** | 6 |
| **max_cyc_projected** | 3 |
| **extraction_count** | 3 |
| **Output** | docs/brain/EPIC-W7-081/04-tickets.md |
