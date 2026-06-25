# Phase 1: Scope Definition - EPIC-W7-047

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **Execution Time**: 2026-06-24T00:00:00Z

---

## Method Under Refactoring

| Field | Value |
|---|---|
| **Method** | `CancelOrphanedTargets` |
| **File** | `src/V12_002.UI.Compliance.cs` |
| **Line** | 553 |
| **Signature** | `private int CancelOrphanedTargets(Account account)` |
| **Current CYC** | 13 |
| **Target CYC** | ≤ 8 (per method, including helpers) |
| **LOC** | 26 |
| **Max Nesting Depth** | 4 |

### Source (lines 553–578)

```csharp
private int CancelOrphanedTargets(Account account)
{
    int cancelledTargets = 0;
    foreach (Order o in account.Orders.ToArray())
    {
        if (o == null || o.Instrument?.FullName != Instrument?.FullName)
            continue;
        if (o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted)
            continue;
        if (
            o.Name != null
            && (
                o.Name.StartsWith("T1_")
                || o.Name.StartsWith("T2_")
                || o.Name.StartsWith("T3_")
                || o.Name.StartsWith("T4_")
                || o.Name.StartsWith("T5_")
            )
        )
        {
            CancelOrderOnAccount(o, account);
            cancelledTargets++;
        }
    }
    return cancelledTargets;
}
```

### Complexity Breakdown (CYC = 13)

| Decision Point | Source | Contribution |
|---|---|---|
| Base | method entry | +1 |
| `foreach` | loop | +1 |
| `o == null` | null guard `\|\|` | +1 |
| `o.Instrument?.FullName !=` | null-conditional `?.` | +1 |
| `o.OrderState != Working` | active-state guard | +1 |
| `&& o.OrderState != Accepted` | active-state guard (AND) | +1 |
| `o.Name != null` | name null guard | +1 |
| `StartsWith("T1_")` | target name check | +1 |
| `\|\| StartsWith("T2_")` | target name check | +1 |
| `\|\| StartsWith("T3_")` | target name check | +1 |
| `\|\| StartsWith("T4_")` | target name check | +1 |
| `\|\| StartsWith("T5_")` | target name check | +1 |
| **Total** | | **13** |

---

## IN SCOPE — Extractions

Two private helper methods will be extracted from the body of `CancelOrphanedTargets`.

### Helper 1 — `IsOrderEligibleForCancellation`

**Purpose**: Encapsulates the per-order eligibility guards (instrument match + active state).

**Proposed signature**:
```csharp
private bool IsOrderEligibleForCancellation(Order o)
```

**Logic extracted** (currently lines 558–561):
```csharp
if (o == null || o.Instrument?.FullName != Instrument?.FullName)
    return false;
if (o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted)
    return false;
return true;
```

**CYC of helper**: 1 (base) + 1 (null `||`) + 1 (`?.`) + 1 (Working) + 1 (Accepted) = **5** ✓

---

### Helper 2 — `IsOrphanedTargetOrder`

**Purpose**: Encapsulates the target-name recognition logic (T1–T5 prefix check).

**Proposed signature**:
```csharp
private bool IsOrphanedTargetOrder(Order o)
```

**Logic extracted** (currently lines 562–571):
```csharp
return o.Name != null
    && (
        o.Name.StartsWith("T1_")
        || o.Name.StartsWith("T2_")
        || o.Name.StartsWith("T3_")
        || o.Name.StartsWith("T4_")
        || o.Name.StartsWith("T5_")
    );
```

**CYC of helper**: 1 (base) + 1 (null guard) + 1 (T1) + 1 (T2) + 1 (T3) + 1 (T4) + 1 (T5) = **7** ✓

---

### Refactored `CancelOrphanedTargets` (post-extraction)

```csharp
private int CancelOrphanedTargets(Account account)
{
    int cancelledTargets = 0;
    foreach (Order o in account.Orders.ToArray())
    {
        if (!IsOrderEligibleForCancellation(o))
            continue;
        if (IsOrphanedTargetOrder(o))
        {
            CancelOrderOnAccount(o, account);
            cancelledTargets++;
        }
    }
    return cancelledTargets;
}
```

**CYC of refactored host**: 1 (base) + 1 (`foreach`) + 1 (`if !eligible`) + 1 (`if IsOrphaned`) = **4** ✓

**CYC summary after extraction**:

| Method | CYC Before | CYC After |
|---|---|---|
| `CancelOrphanedTargets` | 13 | **4** |
| `IsOrderEligibleForCancellation` | — | **5** |
| `IsOrphanedTargetOrder` | — | **7** |
| **Max across all** | **13** | **7** ✓ |

---

## OUT OF SCOPE

| Item | Reason |
|---|---|
| Signature of `CancelOrphanedTargets` — unchanged | Callers `HandleFleetStopFill` (L519) and `ProcessQueuedExecution_HandleFleetOCO` (L698) depend on it as-is |
| Return type and semantics of `CancelOrphanedTargets` | `int` count of cancelled orders; preserved exactly |
| `HandleFleetStopFill` | Caller; not modified |
| `ProcessQueuedExecution_HandleFleetOCO` | Indirect caller; not modified |
| `CancelOrderOnAccount` | Callee in external file; not modified |
| `IsOrderTerminal` | Callee in external file; not called by this method after refactoring |
| Any other method in `src/V12_002.UI.Compliance.cs` | Outside extraction boundary |
| `src-vm-backup/` directory | Backup copies; never touched |
| Behavioral change of any kind | Refactor is purely structural |
| New unit tests | Out of scope for Phase 1 scope definition |

---

## Extraction Plan

### Step-by-step (Phase 2 / Phase 3 execution)

1. **Insert** `IsOrderEligibleForCancellation(Order o)` immediately after `CancelOrphanedTargets` (line ~579) as a private helper within the same class.
2. **Insert** `IsOrphanedTargetOrder(Order o)` immediately after `IsOrderEligibleForCancellation` as a private helper.
3. **Replace** the body of `CancelOrphanedTargets` with the two-call refactored version shown above.
4. No other lines in the file are touched.

### Placement convention
Both helpers are `private`, no XML doc required (they are implementation details), placed in the same logical region as `CancelOrphanedTargets`.

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| Caller breakage | None | N/A | Signature unchanged |
| Behavioral regression | Very Low | High | Logic is a 1-to-1 structural lift; boolean short-circuit order preserved |
| `?.` null-conditional semantics change | None | N/A | `o.Instrument?.FullName` stays inside helper, evaluated identically |
| `o == null` guard bypass | None | N/A | Guard is first check inside helper, same as before |
| Hidden runtime dependency on helper locality | None | N/A | Helpers are pure predicates with no side effects |
| Merge conflict with `src-vm-backup/` | None | N/A | Backup files are never modified |

**Overall risk**: **LOW** — zero blast radius, 2 same-file callers, pure structural extraction.

---

## Success Criteria

- [ ] `CancelOrphanedTargets` CYC ≤ 8 (target: 4)
- [ ] `IsOrderEligibleForCancellation` CYC ≤ 8 (target: 5)
- [ ] `IsOrphanedTargetOrder` CYC ≤ 8 (target: 7)
- [ ] Signature `private int CancelOrphanedTargets(Account account)` unchanged
- [ ] Return value semantics unchanged (count of cancelled orders)
- [ ] No changes outside the three method bodies
- [ ] No changes to any file other than `src/V12_002.UI.Compliance.cs`
- [ ] No changes to `src-vm-backup/` directory
- [ ] All two callers (`HandleFleetStopFill`, `ProcessQueuedExecution_HandleFleetOCO`) compile without modification
