# EPIC-W7-037 — Phase 1: Scope Definition

---

## Single Method in Scope

This phase targets a **single method**: `SymmetryNormalizeTradeType`.

| Field             | Value                               |
|-------------------|-------------------------------------|
| **Method**        | `SymmetryNormalizeTradeType`        |
| **File**          | `src/V12_002.Symmetry.Replace.cs`   |
| **Lines**         | 322–341                             |
| **Visibility**    | `private`                           |
| **Return type**   | `string`                            |
| **Current CYC**   | 9 (project-canonical per hotspots)  |
| **Target CYC**    | ≤ 8                                 |

---

## Scope Boundary

The **scope boundary** for this epic is strictly limited to `SymmetryNormalizeTradeType` and the
private static helpers that will be extracted from it. No other methods, files, or call paths fall
inside this scope boundary.

Only the following artifacts are in scope:

1. The **single method** `SymmetryNormalizeTradeType` (definition at
   `src/V12_002.Symmetry.Replace.cs` line 322).
2. Any new `private static` helper methods extracted directly from its body during Phase 2
   (projected: `IsOrTradeType` and `NormalizeTradeTypeKernel`), which will be co-located in the
   same file and class.

Everything outside these two items is explicitly out of scope.

---

## Callers

Grep across `src/**/*.cs` returned **3 direct call sites** (1 definition + 3 references).
All call sites are documented here for traceability; none of them are modified by this epic.

| Call Site # | Method                          | File                              | Line | Role                                       |
|-------------|----------------------------------|-----------------------------------|------|--------------------------------------------|
| 1           | `SymmetryInferTradeType`         | `src/V12_002.Symmetry.Replace.cs` | 319  | Calls with raw entry name; result forwarded to dispatch |
| 2           | `SymmetryGuardBeginDispatch`     | `src/V12_002.Symmetry.cs`         | 146  | Calls with typed string; result stored as `ctx.TradeType` |
| 3           | `SymmetryFindDispatchForMasterFill` | `src/V12_002.Symmetry.cs`      | 332  | Calls with typed string; result compared against live `ctx.TradeType` values |

**Total callers: 3** (across 2 source files). No callers are altered by this epic.

---

## Why Other Methods Are NOT in Scope

Per rule **V12.23**, a refactor epic targeting a **single method** must preserve a hard scope
boundary and must not absorb adjacent methods, callers, or transitive dependencies into the work
item. Expanding scope mid-phase invalidates the CYC contract established in Phase 0 and violates
the incremental-delivery invariant enforced by the Wave 7 plan.

Specific exclusions and rationale:

| Method                              | File                              | Exclusion Reason (V12.23)                                                                                          |
|--------------------------------------|-----------------------------------|--------------------------------------------------------------------------------------------------------------------|
| `SymmetryInferTradeType`             | `src/V12_002.Symmetry.Replace.cs` | Direct caller, not a complexity hotspot; touching it would widen scope beyond the single-method boundary           |
| `SymmetryGuardBeginDispatch`         | `src/V12_002.Symmetry.cs`         | Direct caller; its correctness depends on the normalized return value, not the internal structure of normalization  |
| `SymmetryFindDispatchForMasterFill`  | `src/V12_002.Symmetry.cs`         | Direct caller; fill-resolution logic is a separate concern from trade-type normalization                           |
| `SymmetryGuardResolveMasterFill`     | `src/V12_002.Symmetry.cs`         | Transitive caller; two hops removed; V12.23 prohibits transitive expansion                                         |
| `ExecuteSmartDispatchEntry`          | `src/V12_002.SIMA.Dispatch.cs`    | Transitive caller in a separate subsystem file; V12.23 hard excludes cross-file transitive scope creep             |
| *(RMA execution path)*               | `src/V12_002.SIMA.Execution.cs`   | Transitive caller; separate execution-layer file; out of scope by V12.23 cross-file rule                           |

Rule V12.23 applies unconditionally: any method not identified as the target in the epic's
manifest `method` field is outside the scope boundary for Phase 1 through Phase 3.

---

## CYC Reduction Plan (Preview)

| Artifact                       | Current CYC | Projected Post-Refactor CYC |
|--------------------------------|-------------|------------------------------|
| `SymmetryNormalizeTradeType`   | 9           | 2 (null-guard + delegate)    |
| `IsOrTradeType` *(new)*        | —           | 3                            |
| `NormalizeTradeTypeKernel` *(new)* | —       | 6                            |
| **Target (central method)**    | **9**       | **≤ 8** ✓                   |

The target CYC of ≤ 8 is met at the single method level. Detailed extraction design is deferred
to Phase 2 (Implementation).

---

## Confirmation Checklist

- [x] Single method in scope: `SymmetryNormalizeTradeType`
- [x] Source file confirmed: `src/V12_002.Symmetry.Replace.cs`
- [x] Current CYC per hotspots: **9**
- [x] Target CYC: **≤ 8**
- [x] Callers counted from live grep: **3 direct call sites**
- [x] Scope boundary defined and documented
- [x] Out-of-scope methods listed with V12.23 rationale
- [x] No caller methods modified by this epic

---

## Agent Tracking

| Field             | Value                                  |
|-------------------|----------------------------------------|
| **Agent Name**    | v12-phase1-scope                       |
| **Wave**          | 7                                      |
| **Phase**         | 1 — Scope Definition (REDO)            |
| **Epic**          | EPIC-W7-037                            |
| **Output**        | `docs/brain/EPIC-W7-037/00-scope.md`   |
| **Bobcoins Used** | 0                                      |
| **Inputs**        | `00-hotspots.md`, `manifest.json`, live grep of `src/**/*.cs` |
| **Scope Rule**    | V12.23 — single-method scope boundary enforced |
