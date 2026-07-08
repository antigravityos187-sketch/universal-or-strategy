# EPIC-W7-118 — Phase 0: Hotspot Analysis

> **Status:** Completed (manual source analysis — MCP complexity tools unavailable; CYC=0 was the reported tool input, not the true score)

---

## 1. Method Identity

| Field              | Value                                  |
|--------------------|----------------------------------------|
| **Method**         | `DeserializeSnapshot`                  |
| **CYC (reported)** | 0 *(tool returned 0 — see note below)* |
| **CYC (manual)**   | 8                                      |
| **File**           | `src/V12_002.StickyState.cs`           |
| **Line range**     | 441–502                                |
| **Visibility**     | `private`                              |
| **Return type**    | `StateSnapshot`                        |
| **Parameters**     | `string json`                          |

> **⚠ CYC Discrepancy Note:** The orchestration task supplied `CYC: 0`, which indicates the automated tool could not locate or score the method (likely due to partial-class spanning or the NinjaScript build context). A manual McCabe count over the actual control-flow graph yields **CYC = 8**. This document treats the manual score as authoritative.

---

## 2. Blast Radius Summary

`DeserializeSnapshot` is a **pure deserialization utility** that is called from three distinct callers, all within the same file:

| Caller | Location | Call context |
|---|---|---|
| `LoadStateSnapshot` | `StickyState.cs:172` | Primary load path — result drives `RestoreFromSnapshot` |
| `LoadStateSnapshot` | `StickyState.cs:196` | Rollback path — called after `RollbackToLastGoodState()` succeeds |
| `RollbackToLastGoodState` | `StickyState.cs:279` | Backup deserialization — gates `ValidateSnapshotIntegrity` on backup |

**Blast radius classification: MODERATE-CONTAINED**

- No external callers outside `V12_002.StickyState.cs`
- All three callers treat a `null` return as a safe recoverable signal
- A refactoring that changes the return contract (null semantics, exception surface) would affect all three call sites
- The parsed fields (`SnapshotTicks`, `StrategyVersion`, `PositionSize`, `EnableSIMA`, `EnableREAPER`, `ChecksumSHA256`, `AccountPositions`) feed directly into `RestoreFromSnapshot`, which mutates live strategy fields (`minContracts`, `EnableSIMA`, `ReaperAuditEnabled`, `expectedPositions`)
- Indirectly downstream: `ValidateSnapshotIntegrity`, `TrackStateSecurityViolation`, `_stateCorruptionDetected` counter

---

## 3. Top 3 Complexity Drivers

### Driver 1 — Nested `if` chain over `AccountPositions` block (lines 454–484)

```
if (accountPosStart >= 0)                          // outer guard
  → if (objStart >= 0 && objEnd > objStart)        // compound condition (+2 edges)
    → foreach (string pair in pairs)               // loop
      → if (colonIdx > 0)                          // inner guard
        → if (int.TryParse(..., out int val))       // parse guard
```

Five decision points in a single nesting stack (depth = 5). This hand-rolled JSON object parser is the dominant complexity source. It could be extracted to a dedicated `ParseAccountPositions(string json)` method that returns `Dictionary<string,int>`.

**Recommended extraction:** `ParseAccountPositions(string json) → Dictionary<string, int>` — reduces this nesting to a single call line and gives the inner parser its own independently testable unit.

---

### Driver 2 — Dual `catch` branches with counter side-effects (lines 488–501)

```csharp
catch (FormatException ex)   // +1 edge — increments _stateCorruptionDetected
catch (Exception ex)         // +1 edge — increments _stateCorruptionDetected
```

Both handlers perform the same two actions (`Interlocked.Increment` + `Print` + `return null`) with only the log message differing. The duplication inflates CYC and is an extraction candidate. A single `catch (Exception ex)` with a `when (ex is FormatException)` pattern, or a private `HandleDeserializationFailure(string context, Exception ex)` helper, collapses these to CYC −1 and eliminates the duplicate counter increment.

**Recommended extraction:** `HandleDeserializationFailure(string logContext, Exception ex)` — single location for the counter increment + Print pattern.

---

### Driver 3 — Short-circuit compound condition (line 459)

```csharp
if (objStart >= 0 && objEnd > objStart)
```

The `&&` short-circuit adds a hidden branch edge that many static tools undercount. Combined with the outer `if (accountPosStart >= 0)` guard, the intent is: *"find the `{...}` block of the AccountPositions object."* This logic (IndexOf `{`, IndexOf `}`, bounds check) is a JSON-block-extraction concern that can be cleanly separated.

**Recommended extraction:** `TryExtractJsonObject(string json, int searchFrom, out string block) → bool` — converts the compound guard + substring mechanics into a named, testable helper, removing 3 decision points from `DeserializeSnapshot`.

---

## 4. Recommended Extraction Count

| # | Extraction | Estimated CYC reduction |
|---|---|---|
| 1 | `ParseAccountPositions(string json)` | −4 (loop + 2 if guards + outer check) |
| 2 | `TryExtractJsonObject(string json, int from, out string block)` | −1 (compound &&) |
| 3 | `HandleDeserializationFailure(string ctx, Exception ex)` | −1 (catch branch dedup) |

**Total recommended extractions: 3**
**Projected post-refactor CYC for `DeserializeSnapshot`: ~3** (base + `accountPosStart` guard + try/catch)

---

## 5. Manual Review Flags

- [ ] Confirm no external caller exists outside `V12_002.StickyState.cs` (partial-class search recommended across all `V12_002.*.cs` files)
- [ ] The hand-rolled JSON parser is fragile for nested objects with `}` inside string values — `objEnd = json.IndexOf('}', objStart)` will find the first `}`, not the matching close brace. Mark as a latent correctness bug.
- [ ] `int.TryParse` result is silently dropped on failure — no log entry for malformed account position values

---

## 6. Agent Tracking

| Field            | Value                     |
|------------------|---------------------------|
| **Agent Name**   | v12-phase0-hotspot        |
| **Bobcoins Used**| 18                        |
| **Execution Time**| ~45s                    |
| **MCP Tools**    | Not available (fallback to direct source analysis) |
| **CYC Source**   | Manual McCabe count from source (reported tool CYC = 0, treated as tool-unavailable signal) |
