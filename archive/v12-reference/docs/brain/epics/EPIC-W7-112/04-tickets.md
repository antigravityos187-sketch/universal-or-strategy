# EPIC-W7-112 — Phase 4 Tickets

**Method**: `ClassifyOrderByPrefix`
**Source**: `src/V12_002.SIMA.Lifecycle.cs`
**CYC**: 20 (aggregate cluster) / ~10 (standalone)
**Lane**: P4-L7
**Wave**: 7
**DNA Verdict**: PASS (Phase 3 cleared)
**max_cyc_projected**: 3

---

## Ticket Summary

| # | Ticket | Type | Symbols Introduced | CYC Target |
|---|--------|------|-------------------|-----------|
| 1 | Add `_orderPrefixMap` field + extract `GetTokenForOrderName` | data-driven extraction | `_orderPrefixMap` (field), `GetTokenForOrderName` (method) | `GetTokenForOrderName` ≤ 3 |
| 2 | Slim `ClassifyOrderByPrefix` to null-guard + delegation | parent simplification | — (modifies existing method) | `ClassifyOrderByPrefix` ≤ 2 |

**Total tickets**: 2
**Files touched**: 1 (`src/V12_002.SIMA.Lifecycle.cs`)
**Callers modified**: 0 (all 4 callers preserved with unchanged signatures)

---

## Ticket 1 — Add `_orderPrefixMap` Field and Extract `GetTokenForOrderName` Helper

**Type**: data-driven extraction
**CYC Target**: `GetTokenForOrderName` ≤ 3
**File**: `src/V12_002.SIMA.Lifecycle.cs`
**Symbols Added**: `_orderPrefixMap` (static readonly field), `GetTokenForOrderName` (private static method)
**Callers Modified**: 0

### Problem

`ClassifyOrderByPrefix` contains an 8-arm `if / else if` chain. Each arm calls
`orderName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)` and returns a hardcoded
string token. This pattern contributes CYC ≈ 10 (standalone) — 8 branches + 1 null guard + 1
method base — and makes adding a new order type require inserting a new `else if` arm.

### Solution

Replace the 8-arm chain with a `private static readonly (string Prefix, string Token)[]`
lookup table (`_orderPrefixMap`) and a `private static string GetTokenForOrderName(string
orderName)` helper that iterates it. All branching collapses to a single `foreach` loop with
one `if` — CYC 3 regardless of how many entries the table contains.

### Code to Add

**Field declaration** (add near top of class, adjacent to other static readonly fields):

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

**Helper method** (add immediately before or after `ClassifyOrderByPrefix`):

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

### CYC Analysis

| Symbol | CYC Calculation | CYC |
|--------|----------------|-----|
| `_orderPrefixMap` | Data field — no control flow | 0 |
| `GetTokenForOrderName` | 1 (method) + 1 (foreach) + 1 (StartsWith if) | **3** |

### Initialization Strategy

`static readonly` inline initializer is chosen over `Lazy<T>` or explicit static constructor:
- Initialized exactly once at CLR type-load time.
- Thread-safe by .NET specification — no `lock()` required.
- Zero heap allocation on the hot path (array allocated once, reused forever).
- Satisfies the V12 lock-free / Actor pattern mandate.

### Verification Criteria

- [ ] `_orderPrefixMap` field exists with all 8 correct mappings.
- [ ] `GetTokenForOrderName` iterates `_orderPrefixMap` with `StartsWith(OrdinalIgnoreCase)`.
- [ ] `GetTokenForOrderName` returns `null` when no prefix matches.
- [ ] `ClassifyOrderByPrefix` body is **unchanged** at end of Ticket 1 (parent slimming is Ticket 2).
- [ ] Build passes: `dotnet build` zero errors.
- [ ] CSharpier: `dotnet csharpier check src/` zero issues.

### xUnit Tests (V12 Mandatory — no NUnit/MSTest)

Add to the existing test project for `V12_002.SIMA.Lifecycle`:

```csharp
// GetTokenForOrderName — all 8 mappings
[Theory]
[InlineData("Stop_001",   "stop")]
[InlineData("S_002",      "stop")]
[InlineData("T1_003",     "target1")]
[InlineData("T2_004",     "target2")]
[InlineData("T3_005",     "target3")]
[InlineData("T4_006",     "target4")]
[InlineData("T5_007",     "target5")]
[InlineData("Fleet_008",  "entry")]
public void GetTokenForOrderName_KnownPrefixes_ReturnsCorrectToken(
    string orderName, string expectedToken)
{
    // Act — invoke via reflection or make internal-visible for testing
    var result = GetTokenForOrderName_Via_Reflection(orderName);
    // Assert
    Assert.Equal(expectedToken, result);
}

[Fact]
public void GetTokenForOrderName_UnknownPrefix_ReturnsNull()
{
    var result = GetTokenForOrderName_Via_Reflection("Unknown_999");
    Assert.Null(result);
}
```

---

## Ticket 2 — Slim `ClassifyOrderByPrefix` to Null-Guard + Delegation

**Type**: parent simplification (data-driven refactor — completion step)
**CYC Target**: `ClassifyOrderByPrefix` ≤ 2
**File**: `src/V12_002.SIMA.Lifecycle.cs`
**Prerequisite**: Ticket 1 completed (field + helper exist)
**Symbols Modified**: `ClassifyOrderByPrefix` (existing private method)
**Callers Modified**: 0

### Problem

After Ticket 1 adds `GetTokenForOrderName`, the original 8-arm `if / else if` chain in
`ClassifyOrderByPrefix` is redundant. The parent method must be updated to delegate to the
helper — removing the 8 branches and reducing its CYC from ~10 to 2.

### Solution

Replace the entire body of `ClassifyOrderByPrefix` (excluding the existing `IsNullOrEmpty`
null guard which is preserved) with a single delegation call to `GetTokenForOrderName`.

### Code Replacement

**Before** (current body — 8-arm if/else-if chain, ~25 lines):

```csharp
private string ClassifyOrderByPrefix(string orderName)
{
    if (string.IsNullOrEmpty(orderName))
        return null;

    if (orderName.StartsWith("Stop_", StringComparison.OrdinalIgnoreCase)) return "stop";
    else if (orderName.StartsWith("S_", StringComparison.OrdinalIgnoreCase)) return "stop";
    else if (orderName.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)) return "target1";
    else if (orderName.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)) return "target2";
    else if (orderName.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)) return "target3";
    else if (orderName.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)) return "target4";
    else if (orderName.StartsWith("T5_", StringComparison.OrdinalIgnoreCase)) return "target5";
    else if (orderName.StartsWith("Fleet_", StringComparison.OrdinalIgnoreCase)) return "entry";
    return null;
}
```

**After** (slim delegation — 5 lines):

```csharp
private string ClassifyOrderByPrefix(string orderName)
{
    if (string.IsNullOrEmpty(orderName))
        return null;
    return GetTokenForOrderName(orderName);
}
```

### CYC Analysis

| Symbol | CYC Calculation | CYC Before | CYC After |
|--------|----------------|-----------|----------|
| `ClassifyOrderByPrefix` | 1 (method) + 1 (IsNullOrEmpty guard) | ~10 | **2** |

### Verification Criteria

- [ ] `ClassifyOrderByPrefix` body contains **only** the `IsNullOrEmpty` guard and `GetTokenForOrderName` delegation call.
- [ ] The 8-arm `if / else if` chain is **entirely removed** from the parent.
- [ ] Signature `private string ClassifyOrderByPrefix(string orderName)` is **unchanged**.
- [ ] Build passes: `dotnet build` zero errors.
- [ ] CSharpier: `dotnet csharpier check src/` zero issues.
- [ ] All 4 callers (`AdoptOrdersFromAccount`, `AdoptMasterOrders`, `AdoptFleetOrders`, `HydrateWorkingOrdersFromBroker`) compile without modification.
- [ ] xUnit tests pass: all 8 prefix mappings + null/empty inputs return expected values.
- [ ] `deploy-sync.ps1` executed to re-synchronize NinjaTrader hard links.

### xUnit Integration Test (end-to-end via parent method)

```csharp
[Theory]
[InlineData(null,          null)]
[InlineData("",            null)]
[InlineData("Fleet_Main",  "entry")]
[InlineData("Stop_L1",     "stop")]
[InlineData("T3_Target",   "target3")]
[InlineData("NoMatch_X",   null)]
public void ClassifyOrderByPrefix_ReturnsExpectedToken(
    string input, string expected)
{
    var result = _sut.ClassifyOrderByPrefix_Via_Reflection(input);
    Assert.Equal(expected, result);
}
```

---

## Extraction Rationale

### Why Data-Driven Over Switch/Case or Family Helpers

| Approach | CYC | Extensibility | Loc |
|----------|-----|--------------|-----|
| Original 8-arm if/else-if | ~10 | Add new `else if` arm | ~25 lines |
| Switch/case on StartsWith | ~10 | Same — cannot use switch on StartsWith directly | ~25 lines |
| Family helpers (stop/target/entry) | ~4 each × 3 | Touch 3 helpers to add one new type | ~40 lines |
| **Data-driven array scan (chosen)** | **3** | Touch 1 array entry | **~15 lines** |

The array-scan pattern is the canonical Jane Street approach for `StartsWith`-based
classification: the mapping is an explicit data contract, the algorithm is a single loop.
No new option prefix can be returned that is not declared in `_orderPrefixMap`.

### Jane Street Alignment

| Principle | Status |
|-----------|--------|
| CYC ≤ 8 | ✅ Max CYC = 3 |
| Lock-free / Actor pattern | ✅ `static readonly` — no `lock()` |
| ASCII-only string literals | ✅ All literals are 7-bit ASCII |
| Illegal states unrepresentable | ✅ Single authoritative registry |
| Zero-allocation hot path | ✅ No heap allocation per call |
| Single-responsibility per symbol | ✅ Field=data, helper=lookup, parent=guard+delegate |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-112 |
| **MCP: resolve_repo** | ✅ indexed, 5147 symbols |
| **MCP: get_symbol_complexity** | ⚠ Symbol not in pre-extraction index (expected) |
| **MCP: sequential-thinking calls** | 4 (1 probe + 3 analysis) |
| **ticket_count** | 2 |
| **max_cyc_projected** | 3 |
| **dna_verdict** | PASS (from Phase 3) |
| **Generated** | 2026-06-29 |
