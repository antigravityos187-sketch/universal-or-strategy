# Phase 1: Scope Definition - EPIC-W7-152

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: N/A
- **Execution Time**: 2026-06-24T00:00:00Z

---

## Method Under Refactoring

| Field            | Value                                        |
|------------------|----------------------------------------------|
| **Method**       | `TryApplyConfigTarget_Value`                 |
| **File**         | `src/V12_002.UI.IPC.Commands.Config.cs`      |
| **Line**         | 209                                          |
| **Lines of Code**| 89                                           |
| **CYC (current)**| 22                                           |
| **CYC (target)** | ≤ 8                                          |

### Method Summary
`TryApplyConfigTarget_Value(string key, string val)` dispatches an IPC config key to
one of six named targets. Five branches (`T1`–`T5`) follow an identical pattern:

1. `double.TryParse` the string value
2. Call `ValidateIpcMultiplier` — print a rejection message on failure
3. On success, assign to the corresponding `TargetNValue` property
4. Unconditionally `return true`

The sixth branch (`CIT`) is a simple string assignment (`ChaseIfTouchPoints = val`) with no
validation, followed by `return true`. If no key matches, the method returns `false`.

The five validated-numeric branches are **structurally identical** (pure copy/paste with
different key strings and target properties), which is the sole source of the excess
CYC=22 (each `if` + `TryParse` + validation `if` contributes ~4 decision points per block).

---

## IN SCOPE — Extractions Required

The following helper methods shall be created to reduce the orchestrating method to CYC ≤ 8.

### Helper 1 — `ApplyValidatedTargetValue`

**Signature (proposed):**
```csharp
private void ApplyValidatedTargetValue(string label, double v, Action<double> assign)
```

**Responsibility:**  
Encapsulates the parse-validate-assign idiom shared by all five `Tx` branches:
- Calls `ValidateIpcMultiplier(v, out vmReason)`
- On failure: prints `[IPC REJECT] {label} value {v} rejected: {vmReason}`
- On success: invokes `assign(v)`

**CYC contribution:** 2 (one `if` for validate result)

---

### Helper 2 — `TryApplyNumericTarget`

**Signature (proposed):**
```csharp
private bool TryApplyNumericTarget(string key, string val, string label, Action<double> assign)
```

**Responsibility:**  
Handles one complete `Tx` branch:
- `double.TryParse(val, out double v)`
- If parse succeeds, delegates to `ApplyValidatedTargetValue(label, v, assign)`
- Returns `true` unconditionally (matching existing semantics)

**CYC contribution:** 2 (one `if` for TryParse result)

---

### Refactored Orchestrator — `TryApplyConfigTarget_Value` (after extraction)

```csharp
private bool TryApplyConfigTarget_Value(string key, string val)
{
    if (key == "T1") return TryApplyNumericTarget(key, val, "T1", v => Target1Value = v);
    if (key == "T2") return TryApplyNumericTarget(key, val, "T2", v => Target2Value = v);
    if (key == "T3") return TryApplyNumericTarget(key, val, "T3", v => Target3Value = v);
    if (key == "T4") return TryApplyNumericTarget(key, val, "T4", v => Target4Value = v);
    if (key == "T5") return TryApplyNumericTarget(key, val, "T5", v => Target5Value = v);
    if (key == "CIT") { ChaseIfTouchPoints = val; return true; }
    return false;
}
```

**Estimated CYC after extraction:** 7 (6 `if` branches + 1 base path) — within ≤ 8 threshold.

---

## OUT OF SCOPE

| Item                                              | Reason                                                   |
|---------------------------------------------------|----------------------------------------------------------|
| Public/private **signature** of `TryApplyConfigTarget_Value` | Must remain unchanged; callers (`TryApplyConfigTargets`) must not be touched |
| **Behavior change** of any kind                   | Refactoring only — identical observable outcomes required |
| `TryApplyConfigTargets`, `HandleConfigCommand`    | Caller methods are not modified                          |
| `TryApplyConfigTarget_Type`, `TryApplyConfigTarget_Count` | Sibling methods not in scope for this epic          |
| `ValidateIpcMultiplier`                           | Called by helper; its implementation is unchanged        |
| Any file outside `src/V12_002.UI.IPC.Commands.Config.cs` | Zero blast radius confirmed in Phase 0              |
| Test infrastructure / build scripts               | No build or test changes required by this phase          |
| Property declarations (`Target1Value`…`Target5Value`, `ChaseIfTouchPoints`) | Not modified |

---

## Extraction Plan

| Step | Action                                                                                 | Helpers Involved                  |
|------|----------------------------------------------------------------------------------------|-----------------------------------|
| 1    | Add private `ApplyValidatedTargetValue(string label, double v, Action<double> assign)` | `ApplyValidatedTargetValue`       |
| 2    | Add private `TryApplyNumericTarget(string key, string val, string label, Action<double> assign)` | `TryApplyNumericTarget` |
| 3    | Replace five `if (key == "Tx")` blocks in `TryApplyConfigTarget_Value` with single-line delegating calls | Both helpers |
| 4    | Retain the `CIT` branch and `return false` unchanged                                   | —                                 |
| 5    | Verify CYC ≤ 8 on all three affected methods via jCodemunch                            | —                                 |

### CYC Budget After Extraction

| Method                        | Estimated CYC |
|-------------------------------|---------------|
| `TryApplyConfigTarget_Value`  | 7             |
| `TryApplyNumericTarget`       | 2             |
| `ApplyValidatedTargetValue`   | 2             |
| **Max of any single method**  | **7 ≤ 8 ✅**  |

---

## Risk Assessment

| Risk                                  | Likelihood | Severity | Mitigation                                                    |
|---------------------------------------|------------|----------|---------------------------------------------------------------|
| `Action<double>` allocation overhead (closure capture) on hot path | LOW | LOW | Closures are stack-friendly; no heap allocation for non-capturing lambdas in .NET; `TargetNValue` setters are property fields, not captured from outer scope — each lambda captures `this` which is already on stack |
| Behavior drift during extraction      | LOW        | HIGH     | Extract one step at a time; each step is a strict mechanical substitution |
| Missed `return true` on parse failure | LOW        | MEDIUM   | `TryApplyNumericTarget` always returns `true` regardless of parse outcome — matches existing semantics exactly |
| Blast radius to callers               | NONE       | —        | Phase 0 confirmed 0 external dependents                       |

**Overall Risk: LOW** — The five duplicated blocks are structurally identical; extraction is mechanical.

---

## Success Criteria

1. `TryApplyConfigTarget_Value` CYC ≤ 8 as measured by jCodemunch
2. `TryApplyNumericTarget` CYC ≤ 8
3. `ApplyValidatedTargetValue` CYC ≤ 8
4. Method signature of `TryApplyConfigTarget_Value` is byte-for-byte unchanged
5. All six key branches (`T1`–`T5`, `CIT`) remain handled; `return false` fallback preserved
6. Zero changes to any caller or sibling method
7. No changes outside `src/V12_002.UI.IPC.Commands.Config.cs`

---

## Metadata
- **Epic ID**: EPIC-W7-152
- **Wave**: 7
- **Phase**: 1 (Scope Definition)
- **Status**: COMPLETED
- **Timestamp**: 2026-06-24T00:00:00Z
- **Analyzer**: v12-phase1-scope (Bob)
