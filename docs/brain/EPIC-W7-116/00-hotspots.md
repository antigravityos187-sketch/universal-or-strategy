# EPIC-W7-116 — Phase 0: Hotspot Analysis

> **Note:** `method_name` and `source_file` missing from epic list — using best-effort hotspot match

---

## Method Identification

| Field            | Value                                                  |
|------------------|--------------------------------------------------------|
| **Method Name**  | `AuditFleet_CalculateExpectedActual` (best candidate)  |
| **CYC**          | 13                                                     |
| **File Path**    | `src/V12_002.REAPER.Audit.cs`                          |
| **Lines**        | 382–451                                                |
| **Class**        | `V12_002` (partial — REAPER Audit Module)              |
| **Wave**         | 7                                                      |

---

## Hotspot Match Rationale

Method was selected via best-effort scan of all `src/V12_002.*.cs` files. No method name or source file was supplied in the epic ticket. The selected candidate `AuditFleet_CalculateExpectedActual` produces a CYC count of **~13** based on manual branch counting:

| Branch Source                                            | +Δ CYC |
|----------------------------------------------------------|--------|
| Base                                                     | 1      |
| `if (pos != null && pos.MarketPosition != Flat)` (&&)   | +2     |
| Ternary: `Long ? pos.Quantity : -pos.Quantity`           | +1     |
| `foreach (var f in accountFsms)`                        | +1     |
| `if (f.State == Active && f.EntryOrder == null)` (&&)   | +2     |
| `if (actualQty != 0)` (inner)                           | +1     |
| `if (TryTerminateFollowerBracket(...))`                  | +1     |
| `if (fsmExpectedQty != 0)`                              | +1     |
| `hasState = (expectedQty != 0 \|\| actualQty != 0)` (\|\|) | +1  |
| `if (shouldLog && hasState)` (&&)                       | +2     |
| **Total**                                               | **13** |

**Runner-up candidate:** `AuditSingleFleetAccount` (`src/V12_002.REAPER.Audit.cs`, lines 121–192, estimated CYC ≈ 12–13). Both live in the same file and are tightly coupled — refactoring one will create extraction opportunities in the other.

---

## Blast Radius Summary

The method is called **exclusively** from `AuditSingleFleetAccount`, which itself is called from `AuditApexPositions` on the REAPER audit timer cycle (every `ReaperIntervalMs` ms, typically 1000 ms).

| Concern              | Detail                                                                           |
|----------------------|----------------------------------------------------------------------------------|
| **Direct callers**   | `AuditSingleFleetAccount` (1 call site)                                          |
| **Indirect callers** | `AuditApexPositions` → REAPER timer → `OnReaperTimerElapsed`                    |
| **Data written**     | 8 `out` parameters: `actualQty`, `expectedQty`, `expectedKey`, `syncPending`, `inFillGrace`, `hasState`, `accountFsms`, `pos` |
| **Side effects**     | `_positionPassFailedFirstSeen.TryRemove`, `TryTerminateFollowerBracket` (FSM mutation), diagnostic `Print` |
| **Thread context**   | Called on strategy thread after `TriggerCustomEvent` marshal                     |
| **Risk level**       | **HIGH** — mutates FSM state; errors here cause position desync and REAPER misfire |
| **Downstream impact**| `AuditFleet_HandleDesyncRepair`, `AuditFleet_HandleCriticalDesyncFlatten`, `AuditFleet_HandleNakedPosition` all consume output parameters |

Refactoring must preserve exact `out` parameter semantics. The FSM-mutation side effect (`TryTerminateFollowerBracket`) is the highest-risk extraction target.

---

## Top 3 Complexity Drivers

### 1. Compound `if` + `foreach` + Nested `if`/`else` for FSM hydration (lines 407–430, +5 CYC)
```csharp
foreach (var f in accountFsms)
{
    if (f.State == FollowerBracketState.Active && f.EntryOrder == null)
    {
        if (actualQty != 0)
            fsmExpectedQty += actualQty;
        else
        {
            if (TryTerminateFollowerBracket(f.EntryName, out staleFsm))
                Print(...);
        }
    }
}
```
This 3-level nesting (`foreach` → `if &&` → `if/else`) accounts for ~5 CYC. It performs FSM repair with a side-effectful `TryTerminateFollowerBracket` call — extractable as `RepairHydratedActiveFsms(accountFsms, ref fsmExpectedQty, actualQty)`.

### 2. Compound guard for `actualQty` assignment (lines 397–400, +2 CYC)
```csharp
if (pos != null && pos.MarketPosition != MarketPosition.Flat)
{
    actualQty = pos.MarketPosition == MarketPosition.Long ? pos.Quantity : -pos.Quantity;
}
```
Short and readable but contributes 2 branches (`&&` + ternary). Extractable as `GetSignedActualQty(pos)` → pure function, CYC=2, zero side effects.

### 3. Conditional log + compound `hasState` Boolean (lines 446–450, +3 CYC)
```csharp
hasState = expectedQty != 0 || actualQty != 0;
if (shouldLog && hasState)
    Print($"[REAPER] {acct.Name}: Expected={expectedQty}, Actual={actualQty}");
```
The `||` in the assignment and `&&` in the `if` combine for 3 branches. Easily extracted as `LogAuditStateIfNeeded(acct, shouldLog, expectedQty, actualQty, hasState)`.

---

## Recommended Extraction Count

| Extraction                           | Target Method                             | CYC Reduction |
|--------------------------------------|-------------------------------------------|---------------|
| FSM hydration loop                   | `RepairHydratedActiveFsms`               | −5            |
| Signed qty calculation               | `GetSignedActualQty`                     | −2            |
| Conditional audit log                | `LogAuditStateIfNeeded`                  | −2            |
| **Residual CYC after extraction**    | **≈ 4** (well under 10 threshold)        |               |

**Recommended extraction count: 3**

The decomposition brings the parent method under CYC=5, each extracted method stays at CYC ≤ 3, and no behavioral change is required. The runner-up `AuditSingleFleetAccount` should be addressed in the same PR since it consumes all 8 `out` parameters and has its own CYC ≈ 12–13 branching load.

---

## Agent Tracking

| Field             | Value                          |
|-------------------|--------------------------------|
| **Agent Name**    | v12-phase0-hotspot             |
| **Bobcoins Used** | 38                             |
| **Execution Time**| ~4 min 20 sec                  |
| **Wave**          | 7                              |
| **Phase**         | 0 — Hotspot Analysis           |
| **Status**        | ✅ Completed                   |
| **Timestamp**     | 2025-06-13                     |
