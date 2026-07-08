# EPIC-W7-118 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Epic:** EPIC-W7-118
**Method:** `DeserializeSnapshot`
**Source File:** `src/V12_002.StickyState.cs`
**CYC Baseline:** 8 (manual McCabe; tool reported 0 — tool-unavailable signal)
**CYC Target:** ≤ 8

---

## Extraction Plan

| # | New Helper | Signature | Extracted Logic | CYC Projected | Jane Street Attribute |
|---|-----------|-----------|-----------------|---------------|----------------------|
| 1 | `ParseAccountPositions` | `private Dictionary<string, int> ParseAccountPositions(string json)` | Entire AccountPositions block: `accountPosStart` guard, `objStart/objEnd` extraction, foreach loop, colonIdx split, `int.TryParse` per pair. Returns populated dict or empty dict on missing/malformed section. | 7 | `[MethodImpl(NoInlining)]` — cold deserialization path |
| 2 | `HandleDeserializationFailure` | `private void HandleDeserializationFailure(string logContext, Exception ex)` | `Interlocked.Increment(ref _stateCorruptionDetected)` + `Print(...)`. Eliminates duplicated catch bodies; collapses parent to single `catch (Exception)`. | 1 | `[MethodImpl(NoInlining)]` — error path only |

### Parent Method CYC After Extraction

| Path | Branches | Count |
|------|----------|-------|
| Base | +1 | 1 |
| `ParseAccountPositions(json)` call (no branch in parent) | — | 1 |
| Single `catch (Exception ex)` | +1 | 2 |

**max_cyc_projected = 2** ✓ (parent); **7** (ParseAccountPositions); **1** (HandleDeserializationFailure)

---

## Refactored Method Sketch

```csharp
private StateSnapshot DeserializeSnapshot(string json)
{
    StateSnapshot snapshot = new StateSnapshot();
    try
    {
        snapshot.SnapshotTicks = ParseJsonLong(json, "SnapshotTicks");
        snapshot.StrategyVersion = ParseJsonString(json, "StrategyVersion");
        snapshot.PositionSize = ParseJsonInt(json, "PositionSize");
        snapshot.EnableSIMA = ParseJsonBool(json, "EnableSIMA");
        snapshot.EnableREAPER = ParseJsonBool(json, "EnableREAPER");
        snapshot.ChecksumSHA256 = ParseJsonString(json, "ChecksumSHA256");
        snapshot.AccountPositions = ParseAccountPositions(json);
        return snapshot;
    }
    catch (Exception ex)
    {
        HandleDeserializationFailure("[STICKY_CORRUPT]", ex);
        return null;
    }
}

[MethodImpl(MethodImplOptions.NoInlining)]
private Dictionary<string, int> ParseAccountPositions(string json)
{
    var result = new Dictionary<string, int>();
    int accountPosStart = json.IndexOf("\"AccountPositions\"", StringComparison.Ordinal);
    if (accountPosStart >= 0)
    {
        int objStart = json.IndexOf('{', accountPosStart);
        int objEnd = json.IndexOf('}', objStart);
        if (objStart >= 0 && objEnd > objStart)
        {
            string accountsBlock = json.Substring(objStart + 1, objEnd - objStart - 1);
            string[] pairs = accountsBlock.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string pair in pairs)
            {
                int colonIdx = pair.IndexOf(':');
                if (colonIdx > 0)
                {
                    string key = pair.Substring(0, colonIdx).Trim().Trim('"');
                    string valStr = pair.Substring(colonIdx + 1).Trim();
                    if (int.TryParse(valStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int val))
                        result[key] = val;
                }
            }
        }
    }
    return result;
}

[MethodImpl(MethodImplOptions.NoInlining)]
private void HandleDeserializationFailure(string logContext, Exception ex)
{
    Interlocked.Increment(ref _stateCorruptionDetected);
    Print(string.Format("{0} Deserialization failed: {1}", logContext, ex.Message));
}
```

---

## MCP Evidence

| Tool | Key Finding |
|------|-------------|
| `get_context_bundle` | Full source retrieved — 62-line method, CYC=8 (manual), nested AccountPositions hand-parser confirmed, dual catch blocks confirmed |
| `get_call_hierarchy` | 3 callers: `LoadStateSnapshot` (line 153, 2 sites), `RollbackToLastGoodState` (line 258). All within same file. 14 callees including `ParseJsonLong/String/Int/Bool` helpers, `LogBuffer.Format` |

---

## Sequential Thinking Evidence

| Thought | Finding |
|---------|---------|
| 1 — Complexity Drivers | 8 branches: accountPosStart guard, compound objStart&&objEnd, foreach loop, colonIdx guard, TryParse branch, 2x catch. Dominant source: 5-level nested AccountPositions parser block. |
| 2 — Extraction Strategy | Extract `ParseAccountPositions` (removes 5 branches from parent, keeps them isolated at CYC=7). Collapse dual catch to single + `HandleDeserializationFailure` helper (CYC=1). Parent reduces to CYC=2. |
| 3 — CYC Validation | Parent=2 ✓; ParseAccountPositions=7 ✓; HandleDeserializationFailure=1 ✓. All ≤ 8. Jane Street: NoInlining on both cold helpers, lock-free via Interlocked, no LINQ. |

---

## Jane Street Compliance

| Rule | Applied |
|------|---------|
| AggressiveInlining hot / NoInlining cold | Both helpers are cold-path: `NoInlining` ✓ |
| No new `lock()` blocks | `Interlocked.Increment` is lock-free ✓ |
| Single responsibility per helper | `ParseAccountPositions`: parse only; `HandleDeserializationFailure`: error accounting only ✓ |
| Each helper CYC ≤ 8 | 7 and 1 ✓ |
| Avoid LINQ | No LINQ in any helper ✓ |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase2-architecture |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-118 |
| **CYC Baseline** | 8 (manual) |
| **max_cyc_projected** | 7 (ParseAccountPositions) |
| **Extractions** | 2 |
