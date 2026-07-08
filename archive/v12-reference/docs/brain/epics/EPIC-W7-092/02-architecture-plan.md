# EPIC-W7-092 — Phase 2: Architecture Plan

**Agent Name:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T02:30:00Z
**Input:** docs/brain/EPIC-W7-092/01-scope-boundary.md

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-092 |

---

## MCP Evidence

### Repo Resolution

| Field | Value |
|---|---|
| **Repo** | antigravityos187-sketch/universal-or-strategy |
| **Status** | indexed (loadable) |
| **Symbol Count** | 5,147 |
| **Source Root** | /home/malhitticrypto/universal-or-strategy |
| **Indexed At** | 2026-06-29T01:05:21Z |

### Target Symbol (jCodemunch `get_symbol_source`)

- **Symbol ID:** `src/V12_002.SIMA.cs::V12_002.SetRmaAnchorFromIpc#method`
- **File:** `src/V12_002.SIMA.cs`
- **Lines:** 241–264
- **Signature:** `private void SetRmaAnchorFromIpc(string anchorStr)`
- **CYC (baseline):** 13

### RmaAnchorType Enum (jCodemunch `get_symbol_source`)

- **Symbol ID:** `src/V12_002.cs::V12_002.RmaAnchorType#type`
- **File:** `src/V12_002.cs` lines 288–296
- **Members:** `Ema30`, `Ema65`, `Ema200`, `OrHigh`, `OrLow`, `Manual` (6 values — exact match to the 6-branch if/else-if chain)

### Caller Confirmed

- `src/V12_002.UI.IPC.Commands.Mode.cs::V12_002.TryHandleRisk_SetAnchor#method` (line 362)
- Caller signature: `private bool TryHandleRisk_SetAnchor(string action, string[] parts)` — **not modified by this epic**

---

## Sequential Thinking Evidence

### Thought 1 — Complexity Drivers

The `SetRmaAnchorFromIpc` method has CYC=13 driven by:

| Complexity Source | CYC Contribution |
|---|---|
| Base path | +1 |
| `if (anchorStr == "EMA30")` | +1 |
| `else if (anchorStr == "EMA65")` | +1 |
| `else if (anchorStr == "EMA200")` | +1 |
| `else if (anchorStr == "OR_HIGH")` | +1 |
| `else if (anchorStr == "OR_LOW")` | +1 |
| `else if (anchorStr == "MANUAL")` | +1 |
| `try` block | +1 |
| `catch (Exception ex)` | +1 |
| Unmatched-string fall-through paths (tool estimate) | +4 |
| **Total** | **13** |

The dominant driver is the **6-branch if/else-if string dispatch chain** — a textbook O(N) linear
comparison anti-pattern for enum mapping. Adding new anchor types would extend this chain further.

### Thought 2 — Extraction Strategy

Replace the 6-branch chain with a `static readonly Dictionary<string, RmaAnchorType>` field.
Extract a single private static helper `TryParseRmaAnchorType` that delegates to `Dictionary.TryGetValue`.

The orchestrator `SetRmaAnchorFromIpc` retains the `try/catch` shell and the `Print` log call,
but all dispatch logic moves to the helper + dictionary.

**Jane Street alignment (carl_cook):** `static readonly Dictionary` is allocated once at class
initialization — zero heap allocation at call-time. `Dictionary.TryGetValue` is O(1) and
alloc-free. This satisfies the zero-alloc hot path rule.

### Thought 3 — CYC Validation

| Method | CYC (projected) | Passes <= 8? |
|---|---|---|
| `RmaAnchorLookup` (static field) | N/A (field, not counted) | N/A |
| `TryParseRmaAnchorType` | 1 | PASS |
| `SetRmaAnchorFromIpc` (refactored) | 4 | PASS |

**max_cyc_projected = 4** — 69% reduction from baseline CYC 13.
All methods well under the Jane Street CYC <= 8 threshold.

---

## Extraction Plan

| # | Artifact | Kind | Location | CYC (projected) | Notes |
|---|---|---|---|---|---|
| 1 | `RmaAnchorLookup` | `private static readonly Dictionary<string, RmaAnchorType>` field | `src/V12_002.SIMA.cs` (same partial class) | N/A | Zero-alloc at call time; allocated once at class init |
| 2 | `TryParseRmaAnchorType(string key, out RmaAnchorType result)` | `private static bool` method | `src/V12_002.SIMA.cs` (same partial class) | 1 | Single-expression TryGetValue delegate |
| 3 | `SetRmaAnchorFromIpc(string anchorStr)` | refactored orchestrator | `src/V12_002.SIMA.cs` line 241 | 4 | Retains try/catch + Print; dispatches via helper |

**Helpers extracted: 1** (TryParseRmaAnchorType)
**Static fields added: 1** (RmaAnchorLookup)
**max_cyc_projected: 4**

---

## Refactored Orchestrator Skeleton

```csharp
// Static readonly lookup table — allocated once, zero-alloc at call time (carl_cook)
private static readonly Dictionary<string, RmaAnchorType> RmaAnchorLookup =
    new Dictionary<string, RmaAnchorType>
    {
        { "EMA30",   RmaAnchorType.Ema30   },
        { "EMA65",   RmaAnchorType.Ema65   },
        { "EMA200",  RmaAnchorType.Ema200  },
        { "OR_HIGH", RmaAnchorType.OrHigh  },
        { "OR_LOW",  RmaAnchorType.OrLow   },
        { "MANUAL",  RmaAnchorType.Manual  },
    };

// CYC=1: single expression, no branches (trading_billions: single responsibility)
private static bool TryParseRmaAnchorType(string key, out RmaAnchorType result)
    => RmaAnchorLookup.TryGetValue(key, out result);

// CYC=4: base(1) + if(1) + try(1) + catch(1)
private void SetRmaAnchorFromIpc(string anchorStr)
{
    try
    {
        if (TryParseRmaAnchorType(anchorStr, out RmaAnchorType anchor))
            currentRmaAnchor = anchor;

        Print("IPC SET ANCHOR: " + anchorStr);
    }
    catch (Exception ex)
    {
        Print("Error SetRmaAnchorFromIpc: " + ex.Message);
    }
}
```

---

## Jane Street Compliance Table

| Principle | Rule | Status | Evidence |
|---|---|---|---|
| **carl_cook** | Zero-alloc hot path | PASS | `static readonly Dictionary` allocated once at class init; `TryGetValue` is alloc-free at call time |
| **carl_cook** | AggressiveInlining on hot | N/A | Helper is CYC=1 expression-bodied method — JIT will inline automatically; no attribute needed |
| **carl_cook** | Extract cold logging out-of-line | PASS | `Print(...)` calls remain cold-path; only executed after dispatch, not inside helper |
| **carl_cook** | Avoid LINQ | PASS | No LINQ used; Dictionary lookup only |
| **gjengset** | No new lock() blocks | PASS | `Dictionary` is read-only after class init; `TryGetValue` is safe for concurrent reads with no locks |
| **gjengset** | volatile + Thread.MemoryBarrier | N/A | No shared mutable state modified by helpers |
| **trading_billions** | Single responsibility per helper | PASS | `TryParseRmaAnchorType` does one thing: key-to-enum lookup |
| **trading_billions** | Each helper CYC <= 8 | PASS | TryParseRmaAnchorType=1, SetRmaAnchorFromIpc=4 |
| **trading_billions** | Defense in depth | PASS | `TryGetValue` returns bool — unrecognized keys silently no-op (no exception thrown) |

---

## Boundary Constraints

| Constraint | Status |
|---|---|
| **V12.23 No Scope Creep** | PASS — single method targeted + 1 new private helper in same file |
| **Caller signature unchanged** | PASS — `SetRmaAnchorFromIpc(string anchorStr)` signature identical |
| **Blast radius** | LOW — `src/V12_002.SIMA.cs` only; callers (`TryHandleRisk_SetAnchor`) untouched |
| **Cross-file changes** | NONE — all changes in `src/V12_002.SIMA.cs` |
| **Public API changes** | NONE — method is `private` |
| **Phase 1.5 boundary_verdict** | PASS |

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-092 |
| **Target Method** | `SetRmaAnchorFromIpc` in `src/V12_002.SIMA.cs` |
| **CYC Baseline** | 13 |
| **max_cyc_projected** | 4 |
| **Helpers Extracted** | 1 (`TryParseRmaAnchorType`) |
| **Static Fields Added** | 1 (`RmaAnchorLookup`) |
| **Complexity Reduction** | 69% (13 → 4) |
| **Jane Street Compliant** | YES |
| **Boundary Verdict** | PASS |
