# EPIC-W7-132 · Phase 0 — Hotspot Analysis

> Wave 7 | Phase 0 | Agent: v12-phase0-hotspot

---

## 1. Method Identity

| Field        | Value                                                    |
|--------------|----------------------------------------------------------|
| Method Name  | `SymmetryNormalizeTradeType`                             |
| File         | `src/V12_002.Symmetry.Replace.cs`                        |
| Lines        | 322 – 341 (20 lines)                                     |
| Visibility   | `private`                                                |
| Return Type  | `string`                                                 |
| Signature    | `private string SymmetryNormalizeTradeType(string raw)`  |

### CYC Score

| Source          | CYC |
|-----------------|-----|
| MCP tool result | **0** (method not resolved by `mcp__jcodemunch-mcp` — requires manual review) |
| Manual analysis | **8** |

> **⚠ Manual Review Required.**  
> The `mcp__jcodemunch-mcp__get_symbol_complexity` tool returned CYC = 0 because the symbol
> could not be resolved from the index (partial-class file pattern `V12_002.Symmetry.Replace.cs`
> may not be indexed). Manual branch-count over the method body yields **CYC = 8**:
> 1 base + 7 `if`-branches (the last branch uses a compound `||` expression counting as one
> decision point under McCabe's strict definition).

---

## 2. Blast Radius

### Direct Callers (2 call sites, 2 files)

| Caller Method                         | File                          | Line | Role                                                           |
|---------------------------------------|-------------------------------|------|----------------------------------------------------------------|
| `SymmetryInferTradeType`              | `V12_002.Symmetry.Replace.cs` | 319  | Fallback normaliser when `PositionInfo` flags are absent       |
| `SymmetryGuardBeginDispatch`          | `V12_002.Symmetry.cs`         | 146  | Normalises trade type before creating a dispatch context       |
| `SymmetryFindDispatchForMasterFill`   | `V12_002.Symmetry.cs`         | 332  | Normalises for dispatch lookup/matching on master fill         |

### Indirect Blast Radius

```
SymmetryNormalizeTradeType
 ├── SymmetryInferTradeType              (Replace.cs:304)
 │    └── called from Symmetry.cs:282   (SymmetryGuardTryResolveFollowersForDispatch → infer path)
 ├── SymmetryGuardBeginDispatch          (Symmetry.cs:139)
 │    └── creates SymmetryDispatchContext stored in symmetryDispatchById ConcurrentDict
 └── SymmetryFindDispatchForMasterFill   (Symmetry.cs:326)
      └── matched against ctx.TradeType for bracket linking
```

**Scope summary:** Any refactor of `SymmetryNormalizeTradeType` affects trade-type string routing
across the entire Symmetry dispatch pipeline — dispatch creation, dispatch lookup, and follower
resolution. The normalised `string` it returns is persisted in `SymmetryDispatchContext.TradeType`
and compared via `string.Equals(..., Ordinal)`. Any change in return values must be reflected
in all stored dispatch contexts and their comparisons.

**Risk level: MEDIUM** — logic is leaf-level and pure (no I/O, no side effects), but its return
values are hard-compared across the dispatch lifecycle. A rename of any token string would be a
silent semantic break.

---

## 3. Top 3 Complexity Drivers

### Driver 1 — Linear `if`-chain over string prefixes (lines 328–339)

```csharp
if (t.StartsWith("TREND",  StringComparison.Ordinal)) return "TREND";
if (t.StartsWith("RETEST", StringComparison.Ordinal)) return "RETEST";
if (t.StartsWith("FFMA",   StringComparison.Ordinal)) return "FFMA";
if (t.StartsWith("MOMO",   StringComparison.Ordinal)) return "MOMO";
if (t.StartsWith("RMA",    StringComparison.Ordinal)) return "RMA";
if (t.StartsWith("OR", StringComparison.Ordinal) || t.Contains("ORLONG") || t.Contains("ORSHORT"))
    return "OR";
```

Six sequential early-return branches, each contributing +1 CYC. There is no structural grouping —
any new trade type requires another `if` appended here and in every sister method
(`SymmetryInferTradeType` has a parallel `if` chain over `PositionInfo` flags).

### Driver 2 — Compound `||` predicate in the OR branch (line 338)

```csharp
if (t.StartsWith("OR", StringComparison.Ordinal) || t.Contains("ORLONG") || t.Contains("ORSHORT"))
```

The `OR` trade type requires three distinct string tests because its naming is inconsistent:
some entry names use the bare prefix `OR`, others embed `ORLONG` / `ORSHORT` as substrings
rather than prefixes. This is both a readability issue and a latent correctness risk — a signal
named e.g. `"MORNING_ORLONG_ENTRY"` would match `Contains("ORLONG")` even if it is not an OR trade.

### Driver 3 — Duplication with `SymmetryInferTradeType` (lines 304–320)

`SymmetryInferTradeType` contains a parallel classification chain using `PositionInfo` boolean
flags. Both methods must be kept in sync when trade types are added or renamed. This is an
**implicit coupling** not visible in CYC but is the primary maintainability driver and the
root cause of why a new trade type requires edits in at least two places.

---

## 4. Recommended Extraction Count

| Extraction | Description                                                                                       | Priority |
|------------|---------------------------------------------------------------------------------------------------|----------|
| **1**      | Extract the six `StartsWith`/`Contains` tests into a `static readonly Dictionary<string,string>` or a `switch` expression (C# 8+). Reduces method CYC to ~2 and makes the trade-type registry a single, visible list. | High     |
| **2**      | Consolidate `SymmetryNormalizeTradeType` + `SymmetryInferTradeType` into a single `SymmetryResolveTradeType(string entryName, PositionInfo pos)` that checks flags first, then falls through to prefix matching. Eliminates dual-maintenance. | Medium   |
| **3**      | Introduce a `TradeTypeToken` constant class (or `enum` + `ToString()`) to eliminate the scattered magic string literals (`"TREND"`, `"RETEST"`, etc.) that propagate into `SymmetryDispatchContext.TradeType` and all `string.Equals` comparisons. | Low      |

**Total recommended extractions: 3**

---

## 5. Agent Tracking

| Field            | Value                              |
|------------------|------------------------------------|
| Agent Name       | v12-phase0-hotspot                 |
| Epic             | EPIC-W7-132                        |
| Wave             | 7                                  |
| Phase            | 0 — Hotspot Analysis               |
| Bobcoins Used    | 6                                  |
| Execution Time   | ~90 seconds                        |
| MCP Tools Called | `search_symbols` (not resolved), `get_symbol_complexity` (CYC=0, fallback to manual), `get_blast_radius`, `get_hotspots`, `sequentialthinking` |
| CYC Override     | MCP=0 → Manual=8 (requires manual review flag set) |
| Status           | ✅ Completed with manual-review annotation |
