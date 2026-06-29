# Phase 2: Architecture Plan — EPIC-W7-112

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-112/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `ClassifyOrderByPrefix`
- **Source File:** `src/V12_002.SIMA.Lifecycle.cs`
- **Symbol ID:** `src/V12_002.SIMA.Lifecycle.cs::V12_002.ClassifyOrderByPrefix#method`
- **Line:** 1262
- **Original CYC:** 20 (aggregate cluster); standalone method CYC ≈ 10
- **Signature:** `private string ClassifyOrderByPrefix(string orderName)`

### jcodemunch get_context_bundle result

Source confirmed via MCP. The method is 25 lines: one `string.IsNullOrEmpty` null-guard
followed by a flat 8-arm `if / else if` chain. Each arm calls `orderName.StartsWith(prefix,
StringComparison.OrdinalIgnoreCase)` and returns one of eight string tokens:
- `"Stop_"` → `"stop"`, `"S_"` → `"stop"`
- `"T1_"` → `"target1"`, `"T2_"` → `"target2"`, `"T3_"` → `"target3"`
- `"T4_"` → `"target4"`, `"T5_"` → `"target5"`
- `"Fleet_"` → `"entry"`
- No match → `null`

The docstring confirms: *pure function — no state mutations, no concurrency concerns.*

### jcodemunch get_call_hierarchy result

| Depth | Caller | Resolution |
|---|---|---|
| 1 | `AdoptOrdersFromAccount` (line 930) | ast_resolved |
| 1 | `AdoptMasterOrders` (line 1195) | ast_resolved |
| 2 | `AdoptFleetOrders` (line 903) | ast_resolved |
| 2 | `HydrateWorkingOrdersFromBroker` (line 309) | ast_resolved |

**Callees:** None (leaf method — pure classification, no downstream calls).

The 4 direct/indirect callers match the scope boundary. No caller will be modified.

### jcodemunch get_dependency_graph result

`src/V12_002.SIMA.Lifecycle.cs` has no import edges resolved in the index (self-contained
C# partial class). External framework dependencies (NinjaTrader, System.Collections.Concurrent,
etc.) are resolved at compile time, not tracked as file-level graph edges. Blast radius is
fully contained to this single file.

### jcodemunch get_extraction_candidates result

No extraction candidates returned by the tool (min_callers=1, min_complexity=3). This is
expected: the index's complexity data is pre-extraction. The extraction plan is derived from
the confirmed source body and hotspot analysis rather than automated candidates.

---

## Sequential Thinking Summary

**5-thought chain completed.** Final conclusion (Thought 5):

> *Replace the 8-arm if/else-if chain in `ClassifyOrderByPrefix` with a `static readonly`
> `(string Prefix, string Token)[] _orderPrefixMap` field and a `private static`
> `GetTokenForOrderName` helper that iterates it via a single foreach loop.
> The parent method becomes a 2-CYC null-guard + single delegation call.
> The helper is a 3-CYC loop. Both satisfy CYC ≤ 8.*
>
> *All 4 callers are unaffected (signature unchanged). Static readonly array is thread-safe
> by .NET spec — no locking required. No heap allocation on the hot path. Single authoritative
> source for all prefix→token mappings. Adding a new order type requires touching exactly
> one place: the static array. Plan is valid, minimal, and fully Jane Street-aligned.*

Key decisions validated:
1. **Lookup-table over Dictionary**: `Dictionary<string,string>` requires exact-key match; `StartsWith` is substring logic. Array scan is the correct data structure.
2. **Static readonly**: Initialized once at type load, read-only thereafter — zero-allocation, thread-safe without locks.
3. **Option A (array scan) over Option C (family helpers)**: One helper + one field is less code than three family-group helpers; equally readable; superior for extensibility.

---

## Extraction Plan

| Helper | Kind | Responsibility | Projected CYC |
|---|---|---|---|
| `_orderPrefixMap` | `private static readonly (string Prefix, string Token)[]` field | Authoritative ordered lookup table: 8 prefix→token mappings. Zero branching — pure data. | 0 (field, not method) |
| `GetTokenForOrderName` | `private static string` method | Iterates `_orderPrefixMap`; returns first matching token via `StartsWith(OrdinalIgnoreCase)`, or `null`. Single concern: lookup. | **3** |

### `_orderPrefixMap` — field definition

```csharp
private static readonly (string Prefix, string Token)[] _orderPrefixMap =
{
    ("Stop_",  "stop"),
    ("S_",     "stop"),
    ("T1_",    "target1"),
    ("T2_",    "target2"),
    ("T3_",    "target3"),
    ("T4_",    "target4"),
    ("T5_",    "target5"),
    ("Fleet_", "entry"),
};
```

### `GetTokenForOrderName` — extracted helper

```csharp
private static string GetTokenForOrderName(string orderName)
{
    foreach ((string prefix, string token) in _orderPrefixMap)
    {
        if (orderName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return token;
    }
    return null;
}
```

CYC = 1 (method) + 1 (foreach loop) + 1 (StartsWith if) = **3**.

---

## Parent Method After Extraction

### Remaining logic

```csharp
private string ClassifyOrderByPrefix(string orderName)
{
    if (string.IsNullOrEmpty(orderName))
        return null;
    return GetTokenForOrderName(orderName);
}
```

- **Guard clause:** `string.IsNullOrEmpty` early-return (unchanged).
- **Delegation:** Single call to `GetTokenForOrderName` — no branching in parent.
- **Signature:** Unchanged — all 4 callers preserved.
- **Side effects:** None (pure function, unchanged).

**Projected CYC:** 1 (method) + 1 (null guard) = **2**.

---

## max_cyc_projected: 3
## extraction_count: 1

*(1 new private static method: `GetTokenForOrderName`. `_orderPrefixMap` is a field, not counted in extraction_count.)*

---

## Jane Street Alignment

| Principle | Status | Evidence |
|---|---|---|
| **CYC <= 8 achieved** | YES | Parent = 2, `GetTokenForOrderName` = 3; max = 3 |
| **Single-responsibility per helper** | YES | `_orderPrefixMap` owns data only; `GetTokenForOrderName` owns lookup only; parent owns null-guard + delegation only |
| **Lock-free / Actor pattern preserved** | YES | `static readonly` field: thread-safe by .NET spec; no `lock()`, no mutable state |
| **Zero-allocation hot path** | YES | Static readonly array (no alloc); foreach with value-tuple destructuring (stack-only); token strings are compile-time string constants (interned); no LINQ, no substring allocation |
| **Illegal states unrepresentable** | YES | Single authoritative prefix→token registry; new order types require exactly one edit location; return type is `string` (nullable by convention) — no invalid token can be returned that is not declared in the map |
| **Lookup table / Strategy pattern** | YES | 8-arm if/else-if replaced by static array scan — canonical lookup table pattern for `StartsWith` classification |
| **Extract guard clauses** | YES | `IsNullOrEmpty` early-return preserved and isolated as sole concern of parent |
| **FSM decomposition** | N/A | Pure classification function — no state machine; FSM pattern not applicable |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | search_symbols, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **max_cyc_projected** | 3 |
| **extraction_count** | 1 |
