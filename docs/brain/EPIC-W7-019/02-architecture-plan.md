# 02-Architecture Plan — EPIC-W7-019

## Agent Tracking
- **Agent Name**: v12-phase2-architecture
- **Epic ID**: EPIC-W7-019
- **Phase**: 2 — Architecture Planning
- **Bobcoins Used**: 7
- **Execution Time**: ~90s
- **Timestamp**: 2026-06-29

---

## 1. Original Method

| Attribute        | Value                                                                  |
|------------------|------------------------------------------------------------------------|
| **Method**       | `TryHandleFleet_MoveTarget`                                            |
| **File**         | `src/V12_002.UI.IPC.Commands.Fleet.cs`                                 |
| **Line**         | 645                                                                    |
| **End Line**     | 693                                                                    |
| **CYC (MCP)**    | 17 (assessment: high)                                                  |
| **Max Nesting**  | 5                                                                      |
| **Lines**        | 49                                                                     |
| **Params**       | 2 (`string action`, `string[] parts`)                                  |
| **Signature**    | `private bool TryHandleFleet_MoveTarget(string action, string[] parts)` |
| **Caller**       | `TryHandleFleetCommand` (line 37, same file)                           |

### Original Source (MCP-confirmed)

```csharp
private bool TryHandleFleet_MoveTarget(string action, string[] parts)
{
    if (!action.StartsWith("MOVE_TARGET") && action != "SET_TARGET_PRICE")
        return false;

    if (parts.Length >= 3)
    {
        string targetId = parts[1].Trim().ToUpperInvariant();
        string priceStr = parts[2].Trim();
        int targetNum = 0;
        if (
            targetId.Length >= 2
            && targetId.StartsWith("T")
            && int.TryParse(targetId.Substring(1), out targetNum)
            && targetNum >= 1
            && targetNum <= 5
        )
        {
            if (action == "SET_TARGET_PRICE")
            {
                // Build 1107: Absolute price move (from live control center)
                double absPrice;
                if (
                    double.TryParse(priceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out absPrice)
                    && absPrice > 0
                )
                {
                    absPrice = Instrument.MasterInstrument.RoundToTickSize(absPrice);
                    MoveSpecificTargetAbsolute(targetNum, absPrice);
                }
            }
            else
            {
                // Relative offset move (from context menu)
                string distance = priceStr.ToLowerInvariant();
                double profitPoints = 0;
                if (distance == "1pt")
                    profitPoints = 1.0;
                else if (distance == "2pt")
                    profitPoints = 2.0;
                else
                    return true;
                MoveSpecificTarget(targetNum, profitPoints);
            }
        }
    }

    return true;
}
```

---

## 2. Extraction Plan

| Helper Name                     | Responsibility                                                              | Lines Moved | Projected CYC |
|----------------------------------|-----------------------------------------------------------------------------|-------------|---------------|
| `TryParseTargetId`               | Validate `parts` length, extract `targetId` (T1–T5 format) + `priceStr`   | ~10         | 7             |
| `HandleSetTargetPriceAbsolute`   | Parse absolute price, round to tick, call `MoveSpecificTargetAbsolute`     | ~8          | 3             |
| `HandleMoveTargetRelative`       | Map distance string (1pt/2pt) to profitPoints, call `MoveSpecificTarget`   | ~10         | 4             |

### Helper Signatures

```csharp
// HELPER 1 — target-id parsing + parts validation
// CYC: 1(base) + 1(length>=3) + 1(length>=2) + 1(StartsWith T) + 1(TryParse) + 1(>=1) + 1(<=5) = 7
private bool TryParseTargetId(string[] parts, out int targetNum, out string priceStr)
{
    targetNum = 0;
    priceStr = string.Empty;
    if (parts.Length < 3)
        return false;
    string targetId = parts[1].Trim().ToUpperInvariant();
    priceStr = parts[2].Trim();
    return targetId.Length >= 2
        && targetId.StartsWith("T")
        && int.TryParse(targetId.Substring(1), out targetNum)
        && targetNum >= 1
        && targetNum <= 5;
}

// HELPER 2 — absolute price move (SET_TARGET_PRICE path)
// CYC: 1(base) + 1(TryParse) + 1(&&absPrice>0) = 3
private void HandleSetTargetPriceAbsolute(int targetNum, string priceStr)
{
    double absPrice;
    if (
        double.TryParse(priceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out absPrice)
        && absPrice > 0
    )
    {
        absPrice = Instrument.MasterInstrument.RoundToTickSize(absPrice);
        MoveSpecificTargetAbsolute(targetNum, absPrice);
    }
}

// HELPER 3 — relative distance move (MOVE_TARGET path)
// CYC: 1(base) + 1(1pt) + 1(2pt) + 1(else unrecognized) = 4
private bool HandleMoveTargetRelative(int targetNum, string priceStr)
{
    string distance = priceStr.ToLowerInvariant();
    double profitPoints;
    if (distance == "1pt")
        profitPoints = 1.0;
    else if (distance == "2pt")
        profitPoints = 2.0;
    else
        return true;   // unrecognized distance — no-op, return consumed
    MoveSpecificTarget(targetNum, profitPoints);
    return true;
}
```

### Parent After Extraction

```csharp
// Parent CYC: 1(base) + 2(action guard: if + &&) + 1(parse check) + 1(SET_TARGET_PRICE dispatch) = 5
private bool TryHandleFleet_MoveTarget(string action, string[] parts)
{
    if (!action.StartsWith("MOVE_TARGET") && action != "SET_TARGET_PRICE")
        return false;

    if (!TryParseTargetId(parts, out int targetNum, out string priceStr))
        return true;

    if (action == "SET_TARGET_PRICE")
        HandleSetTargetPriceAbsolute(targetNum, priceStr);
    else
        return HandleMoveTargetRelative(targetNum, priceStr);

    return true;
}
```

---

## 3. Complexity Summary

| Symbol                          | Before | After | Status   |
|---------------------------------|--------|-------|----------|
| `TryHandleFleet_MoveTarget`     | 17     | 5     | REDUCED  |
| `TryParseTargetId` (new)        | —      | 7     | NEW      |
| `HandleSetTargetPriceAbsolute`  | —      | 3     | NEW      |
| `HandleMoveTargetRelative`      | —      | 4     | NEW      |

**max_cyc_projected: 7** (from `TryParseTargetId`) — satisfies Jane Street threshold <= 8

---

## 4. Jane Street Alignment

### carl_cook (zero-alloc hot path)
- `TryParseTargetId` uses `out int targetNum, out string priceStr` — no heap allocation for parsed values
- No LINQ in any helper
- `AggressiveInlining` candidate for `HandleSetTargetPriceAbsolute` (short, hot path for live control center)
- `HandleMoveTargetRelative` uses `NoInlining` consideration as cold path (context menu only)
- `string.Empty` instead of `""` for out param default (minor zero-alloc discipline)

### gjengset (no new lock() blocks)
- No `lock()` blocks introduced in any helper
- No `volatile` or threading primitives needed — this is a UI command handler on the UI thread
- No shared state mutation; all state changes delegated to `MoveSpecificTargetAbsolute` / `MoveSpecificTarget` (pre-existing callee chain, unchanged)

### trading_billions (single responsibility, CYC <= 8, defense in depth)
- **Single responsibility**: `TryParseTargetId` validates only; `HandleSetTargetPriceAbsolute` executes only absolute; `HandleMoveTargetRelative` executes only relative
- **Defense in depth**: Early-return guard on action prefix preserved in parent; `TryParseTargetId` returns false on invalid format preventing downstream mutation
- **CYC <= 8**: All helpers at CYC 7, 3, 4 — all <= 8 threshold ✓
- **Rate-limit/circuit breaker**: N/A for this command handler; existing callee chain handles order execution safety

---

## 5. MCP Evidence

### Tool: `resolve_repo`
```
repo: antigravityos187-sketch/universal-or-strategy
symbol_count: 5147
file_count: 2000
indexed_at: 2026-06-29T01:05:21
```

### Tool: `search_symbols` (query="TryHandleFleet_MoveTarget")
```
id: src/V12_002.UI.IPC.Commands.Fleet.cs::V12_002.TryHandleFleet_MoveTarget#method
file: src/V12_002.UI.IPC.Commands.Fleet.cs
line: 645
signature: private bool TryHandleFleet_MoveTarget(string action, string[] parts)
```

### Tool: `get_symbol_complexity`
```
cyclomatic: 17
max_nesting: 5
param_count: 2
lines: 49
assessment: high
```

### Tool: `get_symbol_source`
```
line: 645
end_line: 693
source: (full 49-line body captured above)
content_hash: b7a174fb65decbd84b54c01572148f64...
```

### Tool: `get_call_hierarchy`
```
callers (depth=1): TryHandleFleetCommand (line 37, same file)
callees (depth=1): MoveSpecificTargetAbsolute, MoveSpecificTarget
callees (depth=2): ValidateTargetMoveAbsoluteRequest, FindTargetOrderForAbsoluteMove,
                   ValidateMoveTargetRequest, FindTargetOrderForPosition,
                   CalculateAndValidateNewTargetPrice, ExecuteFollowerTargetMove,
                   ExecuteMasterTargetMove, ExecuteTargetAbsoluteMove, LogBuffer.Format
```

### Tool: `get_dependency_graph`
```
file: src/V12_002.UI.IPC.Commands.Fleet.cs
direction: imports
depth: 1
edge_count: 0 (partial class — no explicit using imports in standalone file)
```

---

## 6. Sequential Thinking Evidence

### Thought 1 — Branch Point Enumeration
Enumerated all 13+ branch points in the original 49-line method:
- Action guard: `if (!...StartsWith && ...!=)` → 2 branches
- Parts length: `if (parts.Length >= 3)` → 1 branch
- Compound target-id validation: 5 short-circuit AND conditions → 5 branches
- Action dispatch: `if (action == "SET_TARGET_PRICE")` → 1 branch
- Absolute price: `if (TryParse ... && > 0)` → 2 branches
- Relative distance: `if (1pt)` + `else if (2pt)` + `else` → 2 branches
- **MCP-confirmed CYC=17** accepted as authoritative (MCP counts short-circuit operators individually)

### Thought 2 — Extraction Strategy
Identified 3 cohesive sub-blocks with natural seams:
1. **TryParseTargetId** — parts validation + target-id format check → CYC 7
2. **HandleSetTargetPriceAbsolute** — absolute price path → CYC 3
3. **HandleMoveTargetRelative** — relative distance path → CYC 4

Parent collapses to 4-line dispatch after extraction → CYC 5.

### Thought 3 — Parent CYC Validation
Verified post-extraction parent structure:
- base=1 + 2(action guard) + 1(parse check) + 1(action dispatch) = **CYC 5**
- All helpers: max(7, 3, 4) = **CYC 7**
- **max_cyc_projected = 7 <= 8** ✓
- No lock() blocks, no LINQ, out params for zero-alloc parse, single responsibility per helper

---

## 7. Scope Boundary Compliance

- **In scope**: `TryHandleFleet_MoveTarget` only (lines 645–693)
- **Out of scope**: Callee methods `MoveSpecificTargetAbsolute`, `MoveSpecificTarget` — not modified
- **No scope creep**: Pre-existing compilation state untouched; helpers are private to the same partial class
- **One concern per PR**: Fleet move-target complexity reduction only

---

## 8. Ticket Blueprint

### Ticket 1: Extract `TryParseTargetId`
- Extract compound target-id validation into `private bool TryParseTargetId(string[] parts, out int targetNum, out string priceStr)`
- Verify: build passes, CYC of new method = 7

### Ticket 2: Extract `HandleSetTargetPriceAbsolute`
- Extract absolute-price path into `private void HandleSetTargetPriceAbsolute(int targetNum, string priceStr)`
- Verify: build passes, CYC = 3

### Ticket 3: Extract `HandleMoveTargetRelative`
- Extract relative-distance path into `private bool HandleMoveTargetRelative(int targetNum, string priceStr)`
- Verify: build passes, CYC = 4

### Ticket 4: Rewrite parent `TryHandleFleet_MoveTarget`
- Replace body with 4-line dispatch calling the 3 helpers
- Verify: build passes, parent CYC = 5, max_cyc_projected = 7

---

*Phase 2 complete. max_cyc_projected = 7. Ready for Phase 3 (epic-scan).*
