# Tickets — EPIC-W7-017

## Overview

| Field | Value |
|---|---|
| Epic | EPIC-W7-017 |
| Method | `TryApplyConfigTarget_Value` |
| File | `src/V12_002.UI.IPC.Commands.Config.cs` |
| Line | 209 |
| CYC (current) | 22 |
| ticket_count | 3 |
| projected_parent_cyc_after_all | 5 |
| max_cyc_projected | 6 |
| dna_verdict (Phase 3) | PASS |

---

## Ticket 1 — Extract `TryResolveTargetKeyIndex`

| Field | Value |
|---|---|
| ticket_id | T1 |
| helper_name | `TryResolveTargetKeyIndex` |
| concern | Map key string `"T1"`–`"T5"` to integer slot index; return false for any unrecognized key. No side effects, no parsing, no assignment. |
| lines_to_move | The 5 outer `if (key == "TN")` comparison blocks near the top of the method body (~10 lines). Each block currently sets a per-key double variable and falls through to a repeated parse+validate+assign chain; extract only the key-to-index resolution logic. |
| cyc_reduction | 5 (removes 5 if-check predicates from parent; parent calls one guard instead) |
| projected_helper_cyc | 6 |
| projected_helper_signature | `private bool TryResolveTargetKeyIndex(string key, out int index)` |

### Pseudocode

```csharp
private bool TryResolveTargetKeyIndex(string key, out int index)
{
    if (key == "T1") { index = 1; return true; }
    if (key == "T2") { index = 2; return true; }
    if (key == "T3") { index = 3; return true; }
    if (key == "T4") { index = 4; return true; }
    if (key == "T5") { index = 5; return true; }
    index = -1;
    return false;
}
// CYC = 1 (base) + 5 (if-checks) = 6
```

### Verify

- `[Fact]` test: keys "T1"–"T5" each return `true` with correct index 1–5.
- `[Fact]` test: keys "T6", "CIT", "" each return `false` with `index == -1`.

---

## Ticket 2 — Extract `TryParseAndValidateTargetValue`

| Field | Value |
|---|---|
| ticket_id | T2 |
| helper_name | `TryParseAndValidateTargetValue` |
| concern | Parse `val` as `double`; run `ValidateIpcMultiplier`; populate `rejectReason` on failure; return `true` only when both parse and validation succeed. No key routing, no property assignment. |
| lines_to_move | The repeated `double.TryParse` + `ValidateIpcMultiplier` check pattern that appears inside each T1–T5 block (~12 lines total for all 5 repetitions; collapsed to one 6-line helper). |
| cyc_reduction | 7 (collapses 5×2=10 branch repetitions down to 2 predicates in one helper; parent replaces 5 inline pairs with a single helper call) |
| projected_helper_cyc | 3 |
| projected_helper_signature | `private bool TryParseAndValidateTargetValue(string val, out double parsed, out string rejectReason)` |

### Pseudocode

```csharp
private bool TryParseAndValidateTargetValue(string val, out double parsed, out string rejectReason)
{
    rejectReason = null;
    if (!double.TryParse(val, out parsed))
    {
        return false;
    }
    if (!ValidateIpcMultiplier(parsed, out rejectReason))
    {
        return false;
    }
    return true;
}
// CYC = 1 (base) + 1 (TryParse check) + 1 (ValidateIpc check) = 3
```

### Verify

- `[Fact]` test: valid numeric string passes, `parsed` is set, `rejectReason` remains null.
- `[Fact]` test: non-numeric string fails, `parsed` is `0.0`, method returns `false`.
- `[Fact]` test: numeric but invalid multiplier fails, `rejectReason` is populated, method returns `false`.

---

## Ticket 3 — Extract `ApplyTargetValueByIndex`

| Field | Value |
|---|---|
| ticket_id | T3 |
| helper_name | `ApplyTargetValueByIndex` |
| concern | Switch on slot index 1–5 and assign `value` to the matching `TargetNValue` property. No parsing, no validation, no logging. |
| lines_to_move | The 5 per-key `TargetNValue = value` assignment blocks at the end of each T1–T5 inline chain (~14 lines). Extract into a single switch/case that maps index to property write. |
| cyc_reduction | 5 (removes 5 assignment-dispatch predicates from parent; parent calls one void helper instead) |
| projected_helper_cyc | 6 |
| projected_helper_signature | `private void ApplyTargetValueByIndex(int index, double value)` |

### Pseudocode

```csharp
private void ApplyTargetValueByIndex(int index, double value)
{
    switch (index)
    {
        case 1: Target1Value = value; break;
        case 2: Target2Value = value; break;
        case 3: Target3Value = value; break;
        case 4: Target4Value = value; break;
        case 5: Target5Value = value; break;
    }
}
// CYC = 1 (base) + 5 (switch cases) = 6
```

### Verify

- `[Fact]` test: index 1–5 each sets the correct property via reflection or accessible accessor.
- `[Fact]` test: index 0 and 6 produce no assignment (no crash, no side effect).

---

## Parent After All Extractions

```csharp
private bool TryApplyConfigTarget_Value(string key, string val)
{
    if (key == "CIT")
    {
        ChaseIfTouchPoints = val;
        return true;
    }

    if (!TryResolveTargetKeyIndex(key, out int index))
    {
        return false;
    }

    if (!double.TryParse(val, out double v))
    {
        return true;
    }

    string vmReason;
    if (!ValidateIpcMultiplier(v, out vmReason))
    {
        Print($"[IPC REJECT] {key} value {v} rejected: {vmReason}");
        return true;
    }

    ApplyTargetValueByIndex(index, v);
    return true;
}
// CYC = 1 (base) + 4 (CIT check + key-guard + TryParse + ValidateIpc) = 5
```

**projected_parent_cyc_after_all = 5**

---

## CYC Summary Table

| Symbol | Projected CYC | Passes <= 8? |
|---|---|---|
| `TryApplyConfigTarget_Value` (parent) | 5 | YES |
| `TryResolveTargetKeyIndex` | 6 | YES |
| `TryParseAndValidateTargetValue` | 3 | YES |
| `ApplyTargetValueByIndex` | 6 | YES |
| **MAX** | **6** | **YES** |

---

## Sequential Thinking Evidence

### Thought 1 — Ticket Count

3 tickets required — one per extracted helper. Each helper owns exactly one concern (key routing, parse+validate, property assignment). V12.23 No Scope Creep: ONE TICKET = ONE CONCERN. No ticket may combine two helper extractions. Extraction order: T1 → T2 → T3 (independent but ordered for readability).

### Thought 2 — Per-Ticket Line and CYC Analysis

- T1 (`TryResolveTargetKeyIndex`): 5 if-checks extracted from parent → helper CYC=6, parent loses 5 predicates.
- T2 (`TryParseAndValidateTargetValue`): 5×2 repeated parse+validate pairs collapse to 2 if-checks in helper → helper CYC=3, parent loses the 10 repeated inline branches.
- T3 (`ApplyTargetValueByIndex`): 5 per-key assignment blocks → switch with 5 cases in helper → helper CYC=6, parent loses 5 predicates.

### Thought 3 — CYC Validation

All helpers ≤ 8 (max=6). Parent after all extractions = CYC 5. Jane Street strict standard met. DNA audit (Phase 3) already confirmed PASS with no violations. Extraction plan is safe to execute.

---

## MCP Evidence

| Tool | Input | Key Result |
|---|---|---|
| `resolve_repo` | `/home/malhitticrypto/universal-or-strategy` | `repo=antigravityos187-sketch/universal-or-strategy`, indexed=true, 5147 symbols |
| `search_symbols` | `TryApplyConfigTarget_Value`, kind=method | Symbol ID: `src/V12_002.UI.IPC.Commands.Config.cs::V12_002.TryApplyConfigTarget_Value#method`, line 209 |
| `get_symbol_complexity` | Symbol ID above | CYC=22, max_nesting=5, param_count=2, lines=89, assessment="high" |
| `get_extraction_candidates` | `src/V12_002.UI.IPC.Commands.Config.cs` | 0 candidates (index complexity data not fully populated; CYC=22 confirmed via get_symbol_complexity) |

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-phase4-tickets |
| Epic | EPIC-W7-017 |
| Method | `TryApplyConfigTarget_Value` |
| Phase | 4 (Ticket Generation) |
| Bobcoins Used | 8 |
| Execution Time | ~75s |
| Output | `docs/brain/EPIC-W7-017/04-tickets.md` |
| ticket_count | 3 |
| projected_parent_cyc_after_all | 5 |
| max_cyc_projected | 6 |
| Status | COMPLETE |
