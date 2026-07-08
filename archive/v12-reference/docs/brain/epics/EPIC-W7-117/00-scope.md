# Phase 1: Scope Definition -- EPIC-W7-117

**Agent**: v12-phase1-scope (orchestrator direct -- MCP available in parent context)
**Wave**: 7 | **Phase**: 1
**Generated**: 2026-06-26T02:35:31Z

---

## Single Method in Scope

| Field | Value |
|-------|-------|
| **Method Name** | `SymmetryGuardReplaceExistingFollowerTarget` |
| **Current CYC** | 17 |
| **Target CYC** | <= 8 |
| **File** | `src/V12_002.Symmetry.Replace.cs` |
| **Callers Count** | 6 |

---

## Scope Boundary Statement

> **Only `SymmetryGuardReplaceExistingFollowerTarget` and its new extracted helper methods are in scope.**

This epic covers exclusively the cyclomatic complexity reduction of `SymmetryGuardReplaceExistingFollowerTarget` in `src/V12_002.Symmetry.Replace.cs`.
No other methods will be modified. No cross-file changes beyond adding `private` helper methods
within the same partial class file. The public/internal signature of `SymmetryGuardReplaceExistingFollowerTarget` must remain
identical after refactoring to preserve all 6 call site(s).

---

## Complexity Analysis

- **Current CYC**: 17 -- exceeds Jane Street strict threshold of 8
- **Target CYC**: <= 8
- **Reduction required**: 9 complexity points to extract
- **Strategy**: Extract cohesive logical sub-sections into `private` helper methods

---

## Sequential Thinking (3 inline thoughts)

**Thought 1**: `SymmetryGuardReplaceExistingFollowerTarget` in `src/V12_002.Symmetry.Replace.cs` has CYC=17, which exceeds the Jane Street threshold of <=8. The scope must be strictly limited to extracting helper methods from `SymmetryGuardReplaceExistingFollowerTarget` only -- no scope creep.

**Thought 2**: With 6 caller(s), the method signature must remain unchanged post-refactor to preserve all call sites. Extracted helpers will be `private` methods in the same partial class.

**Thought 3**: Scope boundary confirmed. Only `SymmetryGuardReplaceExistingFollowerTarget` and its newly extracted private helper methods are in scope. scope_confirmed_single_method = true.

---

## Scope Checklist

- [x] Single method identified: `SymmetryGuardReplaceExistingFollowerTarget`
- [x] Source file confirmed: `src/V12_002.Symmetry.Replace.cs`
- [x] CYC verified: 17 (target <= 8)
- [x] Callers enumerated: 6
- [x] Scope boundary defined: extract-only, no signature changes
- [x] No other methods in scope
- [x] scope_confirmed_single_method: true

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase1-scope |
| **Bobcoins Used** | 0.5 |
| **Execution Time** | 2026-06-26T02:35:31Z |
| **Wave** | 7 |
| **Phase** | 1 |
| **Method** | SymmetryGuardReplaceExistingFollowerTarget |
| **Output** | docs/brain/EPIC-W7-117/00-scope.md |
