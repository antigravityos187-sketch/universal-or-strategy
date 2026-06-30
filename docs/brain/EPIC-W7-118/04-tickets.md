# EPIC-W7-118 — Phase 4 Tickets

**Method**: `DeserializeSnapshot`
**Source**: `src/V12_002.StickyState.cs`
**CYC**: 0 (parse artefact; manual McCabe = 8)
**CYC Baseline (manual)**: 8
**CYC Target**: ≤ 8 per helper, ≤ 2 parent
**Lane**: P4-L7
**DNA Verdict**: PASS (Phase 3)

---

## Ticket Summary

| # | Ticket | Type | Helper Signature | CYC Projected | Priority |
|---|--------|------|-----------------|---------------|----------|
| 1 | Extract `ParseAccountPositions` | extraction | `private Dictionary<string, int> ParseAccountPositions(string json)` | 7 | P1 |
| 2 | Extract `HandleDeserializationFailure` | extraction | `private void HandleDeserializationFailure(string logContext, Exception ex)` | 1 | P2 |

**Post-extraction parent CYC**: 2 (base=1, catch=+1)
**max_cyc_projected**: 7 (`ParseAccountPositions`)

---

## Ticket 1 — Extract ParseAccountPositions

**Type**: extraction
**Target CYC**: ≤ 7
**Priority**: P1 (execute first — removes dominant complexity block from parent)

### Context

`DeserializeSnapshot` contains a deeply nested, 5-level AccountPositions hand-parser inline. The block begins at the `json.IndexOf("\"AccountPositions\"")` guard and ends after the `int.TryParse` loop. This is the primary source of the method's CYC=8 — it alone contributes 5–6 branches.

### Extraction Scope

Extract the following logic into a new private helper:

```csharp
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
```

### Parent Call Site Replacement

Replace the inline AccountPositions block in `DeserializeSnapshot` with:
```csharp
snapshot.AccountPositions = ParseAccountPositions(json);
```

### CYC Breakdown

| Branch | +CYC |
|--------|------|
| Base | +1 |
| `accountPosStart >= 0` | +1 |
| `objStart >= 0 && objEnd > objStart` (compound) | +2 |
| `foreach` loop | +1 |
| `colonIdx > 0` | +1 |
| `int.TryParse` success branch | +1 |
| **Total** | **7** |

### Jane Street Compliance

| Rule | Applied |
|------|---------|
| `[MethodImpl(NoInlining)]` on cold-path helper | ✓ deserialization is cold path |
| No `lock()` — lock-free design | ✓ no locking in parse helper |
| No LINQ | ✓ uses `Split`, `IndexOf`, `Substring` only |
| Returns empty dict not null | ✓ makes illegal null state unrepresentable |
| Single responsibility | ✓ parses AccountPositions only |
| CYC ≤ 8 | ✓ CYC = 7 |

### Verification Criteria

- [ ] New method `ParseAccountPositions` exists as `private` in `V12_002.StickyState.cs`
- [ ] Method decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`
- [ ] Return type is `Dictionary<string, int>` (never null — returns empty dict on missing section)
- [ ] Parent `DeserializeSnapshot` calls `ParseAccountPositions(json)` at the `snapshot.AccountPositions =` assignment
- [ ] Parent CYC reduced (no longer contains AccountPositions block inline)
- [ ] `dotnet build` passes with zero errors
- [ ] xUnit `[Fact]` tests cover: valid JSON with AccountPositions, JSON missing AccountPositions key, malformed AccountPositions block

---

## Ticket 2 — Extract HandleDeserializationFailure

**Type**: extraction
**Target CYC**: ≤ 1
**Priority**: P2 (execute after Ticket 1)

### Context

The original `DeserializeSnapshot` had duplicate (or near-duplicate) catch blocks, each performing:
1. `Interlocked.Increment(ref _stateCorruptionDetected)` — atomic state corruption counter
2. `Print(...)` — error logging

Extracting this into a shared `HandleDeserializationFailure` helper eliminates the duplication and collapses the parent to a single `catch (Exception ex)` body.

### Extraction Scope

Extract the catch-block body into a new private helper:

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void HandleDeserializationFailure(string logContext, Exception ex)
{
    Interlocked.Increment(ref _stateCorruptionDetected);
    Print(string.Format("{0} Deserialization failed: {1}", logContext, ex.Message));
}
```

### Parent Catch Block After Extraction

```csharp
catch (Exception ex)
{
    HandleDeserializationFailure("[STICKY_CORRUPT]", ex);
    return null;
}
```

### CYC Breakdown

| Branch | +CYC |
|--------|------|
| Base | +1 |
| No branches (sequential Interlocked + Print) | 0 |
| **Total** | **1** |

### Jane Street Compliance

| Rule | Applied |
|------|---------|
| `[MethodImpl(NoInlining)]` on cold error path | ✓ error path is never hot |
| Lock-free atomic increment | ✓ `Interlocked.Increment` — no `lock()` |
| ASCII-only string literals | ✓ `"[STICKY_CORRUPT]"`, `"Deserialization failed: {1}"` |
| Eliminates duplicated catch bodies | ✓ DRY — single canonical error handler |
| CYC ≤ 8 | ✓ CYC = 1 |

### Verification Criteria

- [ ] New method `HandleDeserializationFailure` exists as `private` in `V12_002.StickyState.cs`
- [ ] Method decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`
- [ ] Uses `Interlocked.Increment(ref _stateCorruptionDetected)` — no `lock()` blocks
- [ ] `Print` call uses ASCII-only string format: `"{0} Deserialization failed: {1}"`
- [ ] Parent `DeserializeSnapshot` catch block calls `HandleDeserializationFailure("[STICKY_CORRUPT]", ex)`
- [ ] Parent `DeserializeSnapshot` has exactly **one** catch block (not two)
- [ ] `dotnet build` passes with zero errors
- [ ] xUnit `[Fact]` tests cover: verifies `_stateCorruptionDetected` incremented on exception, verifies Print called with correct format string

---

## Post-Extraction CYC Summary

| Method | CYC Before | CYC After | Delta |
|--------|-----------|-----------|-------|
| `DeserializeSnapshot` (parent) | 8 | 2 | -6 |
| `ParseAccountPositions` (new) | — | 7 | new |
| `HandleDeserializationFailure` (new) | — | 1 | new |
| **max_cyc** | **8** | **7** | **-1** |

All methods satisfy Jane Street CYC ≤ 8 mandate. ✓

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-118 |
| **Method** | `DeserializeSnapshot` |
| **Source** | `src/V12_002.StickyState.cs` |
| **CYC Baseline** | 8 (manual McCabe; tool reported 0) |
| **max_cyc_projected** | 7 (`ParseAccountPositions`) |
| **Ticket Count** | 2 |
| **DNA Verdict** | PASS |
| **MCP Tools** | resolve_repo, get_symbol_complexity, sequentialthinking (×4) |
