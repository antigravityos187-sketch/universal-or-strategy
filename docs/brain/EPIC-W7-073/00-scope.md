# Phase 1: Scope Definition — EPIC-W7-073

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Source Epic**: EPIC-W7-073
- **Input**: 00-hotspots.md
- **Output**: 00-scope.md
- **Execution Time**: 2026-06-23

---

## Method Under Refactoring

| Property | Value |
|---|---|
| **Method** | `DeserializeSnapshot` |
| **File** | `src/V12_002.StickyState.cs` |
| **Line** | 441 |
| **Signature** | `private StateSnapshot DeserializeSnapshot(string json)` |
| **Current CYC** | 8 |
| **Max Nesting Depth** | 7 |
| **Lines of Code** | 62 |
| **Target CYC** | ≤ 8 per extracted method |

### Callers (all within same file — zero external blast radius)
1. `LoadStateSnapshot` — `src/V12_002.StickyState.cs:153`
2. `RollbackToLastGoodState` — `src/V12_002.StickyState.cs:258`
3. `LoadStickyState` — `src/V12_002.StickyState.cs:369` *(indirect, depth 2)*

---

## IN SCOPE — Extractions

The high nesting depth (7 levels) is the primary driver. The method performs manual JSON parsing, accumulating nested `if`/`try` blocks. The following logical seams are identified for extraction into private helper methods within the same class:

### 1. `ParseSnapshotHeader`
- **Responsibility**: Extract and validate the top-level JSON object boundary check (nesting levels 1–2). Confirm the JSON string is non-null/non-empty and opens with a valid root object token.
- **Reduces**: 1 nesting level from `DeserializeSnapshot`.
- **Estimated CYC contribution**: ≤ 3.

### 2. `ParsePositionFields`
- **Responsibility**: Parse all numeric position-related fields (longs and ints) out of the JSON token stream — delegates to existing callees `ParseJsonLong` and `ParseJsonInt`. Encapsulates the innermost nesting block (levels 4–7) that iterates position field tokens.
- **Reduces**: 3–4 nesting levels from `DeserializeSnapshot`.
- **Estimated CYC contribution**: ≤ 5.

### 3. `ParseStateFlags`
- **Responsibility**: Parse all boolean state flag fields via existing callee `ParseJsonBool`, isolated from numeric parsing. Mirrors the structure of `ParsePositionFields` for flag tokens.
- **Reduces**: 1–2 nesting levels from `DeserializeSnapshot`.
- **Estimated CYC contribution**: ≤ 3.

### 4. `ParseStringFields`
- **Responsibility**: Parse all string-typed snapshot fields via existing callee `ParseJsonString`. Keeps string-field handling in one place, separate from numeric and boolean sections.
- **Reduces**: 1 nesting level from `DeserializeSnapshot`.
- **Estimated CYC contribution**: ≤ 3.

### Post-extraction `DeserializeSnapshot` shape
After extraction the orchestrating method becomes a linear sequence of four calls (`ParseSnapshotHeader`, `ParsePositionFields`, `ParseStateFlags`, `ParseStringFields`) plus object construction, bringing its nesting depth to ≤ 3 and CYC to ≤ 5.

---

## OUT OF SCOPE

| Item | Reason |
|---|---|
| Signature of `DeserializeSnapshot` | Must remain `private StateSnapshot DeserializeSnapshot(string json)` — callers are not touched. |
| Behavior / return value | No observable behavior change; purely structural. |
| Callers (`LoadStateSnapshot`, `RollbackToLastGoodState`, `LoadStickyState`) | Not modified in any phase. |
| Existing parse primitives (`ParseJsonLong`, `ParseJsonString`, `ParseJsonInt`, `ParseJsonBool`) | Reused as-is; their bodies are untouched. |
| `LogBuffer` utilities (`Format`, `ValidateThreadAffinity`, `FormatInternal`) | Not touched; continue to be called naturally through extracted helpers if needed. |
| All other methods in `V12_002.StickyState.cs` | Zero-touch. |
| Any file outside `src/V12_002.StickyState.cs` | Zero-touch. |
| Error-handling strategy | No changes to exception/null-handling policy. |
| Accessibility modifiers | All extracted helpers are `private`. |

---

## Extraction Plan

```
DeserializeSnapshot(string json)           [CYC target: ≤ 5, depth target: ≤ 3]
├── ParseSnapshotHeader(string json)       [CYC target: ≤ 3, depth target: ≤ 2]
│   └── validates root JSON token, returns parsed root node / throws on invalid input
├── ParsePositionFields(...)               [CYC target: ≤ 5, depth target: ≤ 3]
│   └── calls ParseJsonLong, ParseJsonInt; populates position sub-struct
├── ParseStateFlags(...)                   [CYC target: ≤ 3, depth target: ≤ 2]
│   └── calls ParseJsonBool; populates flag fields
└── ParseStringFields(...)                 [CYC target: ≤ 3, depth target: ≤ 2]
    └── calls ParseJsonString; populates string fields
```

### Extraction Order
1. `ParseSnapshotHeader` — removes outermost guard block, lowest risk, easiest to verify.
2. `ParsePositionFields` — targets the deepest nesting cluster (levels 4–7), highest CYC reduction.
3. `ParseStateFlags` — straightforward boolean block extraction.
4. `ParseStringFields` — final string block extraction.

Each extraction is independently verifiable against the 3 in-file callers.

---

## Risk Assessment

| Risk | Severity | Mitigation |
|---|---|---|
| Accidental behavior change in parsing logic | LOW | Extract verbatim; no logic rewrite. Verify by inspection against 3 callers. |
| Parameter threading (intermediate parse state) | LOW | Identify all mutable locals in each extraction target before splitting. |
| CYC of a helper exceeding 8 | LOW | `ParsePositionFields` is the only helper with elevated CYC; review field count before extraction. |
| Regression in `LoadStateSnapshot` / `RollbackToLastGoodState` | LOW | Both callers are in-file; full-file inspection confirms no signature dependency. |
| Nesting depth not reduced sufficiently | LOW | Extraction order prioritises the deepest block first (`ParsePositionFields`). |

**Overall Risk: LOW** — consistent with Phase 0 finding (blast radius = 0, all callers in same file).

---

## Success Criteria

1. `DeserializeSnapshot` CYC ≤ 8 after extraction (target ≤ 5).
2. `DeserializeSnapshot` max nesting depth ≤ 3 after extraction (down from 7).
3. Each extracted helper method has CYC ≤ 8 (target ≤ 5).
4. Signature of `DeserializeSnapshot` is byte-for-byte identical to pre-refactor: `private StateSnapshot DeserializeSnapshot(string json)`.
5. Zero changes outside `src/V12_002.StickyState.cs`.
6. All 3 callers (`LoadStateSnapshot`, `RollbackToLastGoodState`, `LoadStickyState`) remain unmodified.
7. Existing primitive helpers (`ParseJsonLong`, `ParseJsonString`, `ParseJsonInt`, `ParseJsonBool`) remain unmodified.
8. No new public or internal API surface introduced.
