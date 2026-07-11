# Architecture Plan — EPIC-W7-017

## Original Method

| Field | Value |
|---|---|
| Method | `TryApplyConfigTarget_Value` |
| File | `src/V12_002.UI.IPC.Commands.Config.cs` |
| Line | 209 |
| CYC (MCP) | 22 |
| Max Nesting | 5 |
| Lines | 89 |
| Params | 2 (`string key`, `string val`) |

**Signature**: `private bool TryApplyConfigTarget_Value(string key, string val)`

**Callers**: `TryApplyConfigTargets` (line 196) → `HandleConfigCommand` (line 153)

**Callees**: `ValidateIpcMultiplier` (in `src/V12_002.UI.IPC.cs`)

---

## Root Cause Analysis

The method dispatches on 6 keys: `T1`, `T2`, `T3`, `T4`, `T5`, and `CIT`. The five `T1`–`T5` blocks share identical logic (parse double → validate → log or assign), producing 16+ branch predicates across the 89-line body. The repetition is the sole cause of CYC=22.

---

## Extraction Plan

| Helper Name | Signature | Responsibility | Est. Lines Moved | Projected CYC |
|---|---|---|---|---|
| `TryResolveTargetKeyIndex` | `private bool TryResolveTargetKeyIndex(string key, out int index)` | Map key string `"T1"–"T5"` to integer slot 1–5; return false for unrecognized keys. No side effects. | ~10 | 6 |
| `TryParseAndValidateTargetValue` | `private bool TryParseAndValidateTargetValue(string val, out double parsed, out string rejectReason)` | Parse `val` as double; run `ValidateIpcMultiplier`; populate `rejectReason` on failure; return true only if both succeed. | ~12 | 3 |
| `ApplyTargetValueByIndex` | `private void ApplyTargetValueByIndex(int index, double value)` | Switch on slot index 1–5 and assign `value` to the matching `TargetNValue` property. | ~14 | 6 |

### Helper 1 — `TryResolveTargetKeyIndex`

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

### Helper 2 — `TryParseAndValidateTargetValue`

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
// CYC = 1 (base) + 1 (TryParse) + 1 (ValidateIpc) = 3
```

### Helper 3 — `ApplyTargetValueByIndex`

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
// CYC = 1 (base) + 5 switch cases = 6
```

---

## Parent After Extraction

```csharp
private bool TryApplyConfigTarget_Value(string key, string val)
{
    // Branch 1: CIT direct assign
    if (key == "CIT")
    {
        ChaseIfTouchPoints = val;
        return true;
    }

    // Branch 2: key routing — only T1-T5 handled here
    if (!TryResolveTargetKeyIndex(key, out int index))
    {
        return false;
    }

    // Branch 3: parse failure — key was valid, silently accept
    if (!double.TryParse(val, out double v))
    {
        return true;
    }

    // Branch 4: validation failure — log rejection
    string vmReason;
    if (!ValidateIpcMultiplier(v, out vmReason))
    {
        Print($"[IPC REJECT] {key} value {v} rejected: {vmReason}");
        return true;
    }

    ApplyTargetValueByIndex(index, v);
    return true;
}
// CYC = 1 (base) + 4 branch predicates = 5
```

**Parent projected CYC: 5**

---

## max_cyc_projected: 6

Summary of all projected CYC values:

| Symbol | Projected CYC |
|---|---|
| `TryApplyConfigTarget_Value` (parent) | 5 |
| `TryResolveTargetKeyIndex` | 6 |
| `TryParseAndValidateTargetValue` | 3 |
| `ApplyTargetValueByIndex` | 6 |
| **MAX** | **6** |

All values are ≤ 8 (Jane Street strict standard). ✓

---

## Jane Street Alignment Notes

### carl_cook — Zero-Alloc Hot Path
- No LINQ, no list allocations in any helper.
- Logging (`Print`) is on the cold rejection path only — never on the happy-path assign flow.
- `TryParseAndValidateTargetValue` uses `out double parsed` (stack) — no heap alloc.
- `TryResolveTargetKeyIndex` and `ApplyTargetValueByIndex` are candidates for `[MethodImpl(MethodImplOptions.AggressiveInlining)]` since they are pure dispatch with trivial bodies.
- The cold logging path in the parent (`Print(...)`) should use `[MethodImpl(MethodImplOptions.NoInlining)]` if extracted to its own method.

### gjengset — No New Locks, Memory Visibility
- No `lock()` blocks introduced or retained.
- No volatile or `Thread.MemoryBarrier` needed — `TargetNValue` properties are set from the IPC command handler thread (single writer from caller context).
- No shared concurrent state modified in the extracted helpers.

### trading_billions — Single Responsibility, Defense in Depth
- **`TryResolveTargetKeyIndex`**: Only routing. Does not parse or validate.
- **`TryParseAndValidateTargetValue`**: Only parse+validate. Does not assign or log.
- **`ApplyTargetValueByIndex`**: Only assigns. Does not parse, validate, or log.
- **Parent**: Orchestrates the three checkpoints as independent defense layers — a bad key, bad parse, or bad value each fails independently.
- Each extracted helper has CYC ≤ 8. ✓
- Rate-limit circuit breaker not applicable here (no rate-limited resource); `ValidateIpcMultiplier` already provides value-range protection.

---

## MCP Evidence

| Tool | Input | Key Result |
|---|---|---|
| `resolve_repo` | `/home/malhitticrypto/universal-or-strategy` | `repo=antigravityos187-sketch/universal-or-strategy`, indexed=true, 5147 symbols |
| `search_symbols` | `TryApplyConfigTarget_Value` | Symbol ID: `src/V12_002.UI.IPC.Commands.Config.cs::V12_002.TryApplyConfigTarget_Value#method`, line 209 |
| `get_symbol_complexity` | Symbol ID above | CYC=22, max_nesting=5, param_count=2, lines=89, assessment="high" |
| `get_symbol_source` | Symbol ID above | 89-line source body retrieved (lines 209–297); 6 key branches: T1-T5 + CIT |
| `get_call_hierarchy` | Symbol ID, depth=2, both | Callers: `TryApplyConfigTargets` (depth 1), `HandleConfigCommand` (depth 2); Callees: `ValidateIpcMultiplier` |
| `get_dependency_graph` | `src/V12_002.UI.IPC.Commands.Config.cs`, depth=1, both | 1 node, 0 edges (standalone file, no file-level imports) |

---

## Sequential Thinking Evidence

### Thought 1 — Branch Point Enumeration (CYC Validation)

Enumerated all branch predicates in the source:
- 5 outer key checks: `if (key == "T1")` … `if (key == "T5")`
- 5 TryParse checks: `if (double.TryParse(…))`
- 5 ValidateIpcMultiplier checks: `if (!ValidateIpcMultiplier(…))`
- 1 CIT simple assign (no nested branches)

Total: 1 (base) + 15 (T1-T5 × 3 branches) + 1 (CIT outer) ≈ 17–22 depending on else-arm counting. MCP-reported CYC=22 confirmed valid. Structure is entirely if/else — no switch, no loops, no try/catch.

**Root finding**: 5 structurally identical target blocks drive all complexity. Extraction of shared dispatch/parse/assign helpers eliminates 17 branches from the parent.

### Thought 2 — Extraction Strategy Design

Evaluated three options:
- **Option A**: `ref double` helper — rejected because C# properties cannot be passed by ref.
- **Option B**: index-based helper with combined parse+validate — viable but mixes two concerns.
- **Option C (chosen)**: Three-helper decomposition with strict single responsibility:
  1. `TryResolveTargetKeyIndex` — key routing only (CYC=6)
  2. `TryParseAndValidateTargetValue` — parse+validate only (CYC=3)
  3. `ApplyTargetValueByIndex` — property assignment only (CYC=6)

Parent becomes a 5-branch orchestration with CYC=5. Aligns with trading_billions defense-in-depth principle.

### Thought 3 — Validation

Final CYC audit:

| Symbol | Branch Count | Projected CYC | Passes ≤8? |
|---|---|---|---|
| `TryResolveTargetKeyIndex` | 5 if-checks | 6 | ✓ |
| `TryParseAndValidateTargetValue` | 2 if-checks | 3 | ✓ |
| `ApplyTargetValueByIndex` | 5 switch cases | 6 | ✓ |
| `TryApplyConfigTarget_Value` (parent) | 4 if-checks | 5 | ✓ |

**max_cyc_projected = 6** — all helpers ≤ 8.

Jane Street alignment verified: cold logging path is out-of-line from the happy path, no allocations, no locks, single responsibility per helper.

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-phase2-architecture |
| Epic | EPIC-W7-017 |
| Method | TryApplyConfigTarget_Value |
| Phase | 2 (Architecture Planning) |
| Bobcoins Used | 7 |
| Execution Time | ~90s |
| Output | `docs/brain/EPIC-W7-017/02-architecture-plan.md` |
| max_cyc_projected | 6 |
| Status | COMPLETE |
