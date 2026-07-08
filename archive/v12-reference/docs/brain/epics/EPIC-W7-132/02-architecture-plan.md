# EPIC-W7-132 — Phase 2: Architecture Plan

**Agent Name: v12-phase2-architecture**
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Epic:** EPIC-W7-132
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-132/01-scope-boundary.md

---

## MCP Evidence Summary

| Tool | Result |
|------|--------|
| `mcp__jcodemunch-mcp__resolve_repo` | Repo resolved: `antigravityos187-sketch/universal-or-strategy`, 5147 symbols indexed |
| `mcp__jcodemunch-mcp__get_context_bundle` | Symbol retrieved: `src/V12_002.Symmetry.Replace.cs::V12_002.SymmetryNormalizeTradeType#method`, lines 322–341 |
| `mcp__jcodemunch-mcp__get_call_hierarchy` | 1 caller (`SymmetryInferTradeType` at line 304, same file), 0 callees — leaf method |
| `mcp__jcodemunch-mcp__get_dependency_graph` | `node_count=1, edge_count=0` — no cross-file import/export edges; blast radius fully contained in `src/V12_002.Symmetry.Replace.cs` |

### Source Confirmed via get_context_bundle

```csharp
private string SymmetryNormalizeTradeType(string raw)
{
    if (string.IsNullOrEmpty(raw))
        return "GENERIC";

    string t = raw.ToUpperInvariant();
    if (t.StartsWith("TREND", StringComparison.Ordinal))
        return "TREND";
    if (t.StartsWith("RETEST", StringComparison.Ordinal))
        return "RETEST";
    if (t.StartsWith("FFMA", StringComparison.Ordinal))
        return "FFMA";
    if (t.StartsWith("MOMO", StringComparison.Ordinal))
        return "MOMO";
    if (t.StartsWith("RMA", StringComparison.Ordinal))
        return "RMA";
    if (t.StartsWith("OR", StringComparison.Ordinal) || t.Contains("ORLONG") || t.Contains("ORSHORT"))
        return "OR";
    return "GENERIC";
}
```

---

## Sequential Thinking Analysis

Sequential (`sequentialthinking`) validation performed across 4 thoughts:

1. **Probe** — Task framing and tool selection confirmed.
2. **CYC Count from Source** — Manual branch-count from get_context_bundle source:
   - Base: 1
   - `IsNullOrEmpty` if: +1
   - 5x `StartsWith` ifs: +5
   - `StartsWith("OR",...)`: +1
   - `|| t.Contains("ORLONG")`: +1 (McCabe strict: boolean operator)
   - `|| t.Contains("ORSHORT")`: +1 (McCabe strict: boolean operator)
   - **Total CYC = 10** (strict) / **CYC = 8** (Phase 0 loose count treating compound || as single decision)
3. **Extraction Strategy** — Minimal extraction of compound OR predicate into `SymmetryIsOrTradeType(string t)` reduces parent CYC from 10 to 8, extracted helper CYC = 3.
4. **CYC Validation** — Post-extraction CYC confirmed: parent = 8, helper = 3. Both within ≤ 8 threshold. ✅

---

## CYC Analysis

| Metric | Value |
|--------|-------|
| CYC before (strict McCabe) | **10** |
| CYC before (Phase 0 loose) | **8** |
| CYC threshold (Jane Street) | **8** |
| Status before extraction | **EXCEEDS or BORDERLINE** |
| max_cyc_projected | **8** |

---

## Extraction Plan

### Strategy: Extract Compound OR Predicate (Minimal Targeted Extraction)

The compound `||` predicate on line 338 is the sole source of CYC excess under strict counting. Extracting it into a private static helper achieves:
- Parent method CYC reduced to exactly 8 (threshold boundary)
- Helper has single responsibility (classify OR trade type) — CYC = 3
- Zero behavioral change — pure refactor with identical semantics
- No allocation overhead — all string comparisons operate on existing string instances

### Extraction Table

| # | New Method | Signature | Extracted From | Lines | CYC After | Jane Street Notes |
|---|-----------|-----------|----------------|-------|-----------|-------------------|
| 1 | `SymmetryIsOrTradeType` | `private static bool SymmetryIsOrTradeType(string t)` | Line 338 compound OR predicate | ~4 | 3 | `AggressiveInlining` (hot-path, zero-alloc, no side effects) |

### Parent Method After Extraction

```csharp
private string SymmetryNormalizeTradeType(string raw)
{
    if (string.IsNullOrEmpty(raw))
        return "GENERIC";

    string t = raw.ToUpperInvariant();
    if (t.StartsWith("TREND", StringComparison.Ordinal))
        return "TREND";
    if (t.StartsWith("RETEST", StringComparison.Ordinal))
        return "RETEST";
    if (t.StartsWith("FFMA", StringComparison.Ordinal))
        return "FFMA";
    if (t.StartsWith("MOMO", StringComparison.Ordinal))
        return "MOMO";
    if (t.StartsWith("RMA", StringComparison.Ordinal))
        return "RMA";
    if (SymmetryIsOrTradeType(t))
        return "OR";
    return "GENERIC";
}
```

CYC = 1 + 1 (IsNullOrEmpty) + 5 (StartsWith) + 1 (SymmetryIsOrTradeType call) = **8** ✅

### Extracted Helper

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private static bool SymmetryIsOrTradeType(string t)
{
    return t.StartsWith("OR", StringComparison.Ordinal)
        || t.Contains("ORLONG")
        || t.Contains("ORSHORT");
}
```

CYC = 1 + 2 (|| boolean operators) = **3** ✅

---

## CYC Projection Summary

| Method | CYC Before | CYC After | Status |
|--------|-----------|-----------|--------|
| `SymmetryNormalizeTradeType` | 10 (strict) / 8 (loose) | **8** | ✅ At threshold |
| `SymmetryIsOrTradeType` (new) | — | **3** | ✅ Well within threshold |

**max_cyc_projected: 8**

---

## Blast Radius Confirmation

From `get_dependency_graph`: `node_count=1, edge_count=0`
- No cross-file imports or importers detected
- All changes confined to `src/V12_002.Symmetry.Replace.cs`
- Method signature of `SymmetryNormalizeTradeType` unchanged: `private string SymmetryNormalizeTradeType(string raw)`
- Callers (confirmed via `get_call_hierarchy`): `SymmetryInferTradeType` (same file, line 304) — unaffected

---

## Jane Street Compliance Notes

| Principle | Source | Application | Status |
|-----------|--------|-------------|--------|
| Zero-alloc hot path | carl_cook | Both methods use only string comparison on existing instances — no heap allocation | ✅ |
| `AggressiveInlining` on hot path | carl_cook | `SymmetryIsOrTradeType` marked `AggressiveInlining` — small, frequently called predicate | ✅ |
| No new `lock()` blocks | gjengset | Pure string comparison, no shared state, no synchronization needed | ✅ |
| Volatile + MemoryBarrier N/A | gjengset | No mutable state in extracted methods | ✅ |
| Single responsibility per helper | trading_billions | `SymmetryIsOrTradeType` has exactly one concern: detect OR trade type variants | ✅ |
| Each helper CYC <= 8 | trading_billions | max_cyc_projected = 8 (parent), 3 (helper) | ✅ |
| Avoid LINQ | carl_cook | No LINQ introduced — raw string methods only | ✅ |

---

## Implementation Constraints

- **File scope only:** All changes in `src/V12_002.Symmetry.Replace.cs` (V12.23 No Scope Creep Protocol)
- **Signature unchanged:** `SymmetryNormalizeTradeType(string raw)` contract preserved
- **Private static:** `SymmetryIsOrTradeType` is `private static` — no visibility leak
- **ASCII-only strings:** All string literals are ASCII-safe (`"OR"`, `"ORLONG"`, `"ORSHORT"`)
- **No behavioral change:** Return values and fallback `"GENERIC"` are identical

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-132 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **MCP Tools Called** | `resolve_repo`, `get_context_bundle`, `get_call_hierarchy`, `get_dependency_graph` (jcodemunch), `sequentialthinking` (sequential-thinking) |
| **CYC Before** | 10 (strict) / 8 (Phase 0 manual) |
| **max_cyc_projected** | 8 |
| **Extractions** | 1 (`SymmetryIsOrTradeType`) |
| **Bobcoins Used** | 2 |
| **Status** | Completed |
