# EPIC-W7-073 — Hotspot Analysis
**Wave:** 7 | **Phase:** 0  
**Source:** `src/V12_002.StickyState.cs`  
**Target Symbol:** `V12_002.DeserializeSnapshot(string json)`  
**Date:** 2025-07-14

---

## 1. Symbol Under Analysis

| Property | Value |
|---|---|
| Method | `private StateSnapshot DeserializeSnapshot(string json)` |
| File | `src/V12_002.StickyState.cs` lines 441–502 |
| Class | `V12_002` (partial) |
| Namespace | `NinjaTrader.NinjaScript.Strategies` |
| CYC (confirmed) | **8** |

---

## 2. Cyclomatic Complexity — Branch Breakdown

McCabe branches counted from the method body (base = 1):

| # | Branch | Line(s) | +CYC |
|---|---|---|---|
| 1 | `if (accountPosStart >= 0)` | 455 | +1 |
| 2 | `if (objStart >= 0 && objEnd > objStart)` | 459 | +1 |
| 3 | `foreach (string pair in pairs)` | 463 | +1 |
| 4 | `if (colonIdx > 0)` | 466 | +1 |
| 5 | `if (int.TryParse(...))` | 470–477 | +1 |
| 6 | `catch (FormatException)` | 488 | +1 |
| 7 | `catch (Exception)` | 495 | +1 |
| **Base** | entry | — | **1** |
| **Total** | | | **8** ✅ |

> Note: The compound `&&` on line 459 is treated as a single branch per standard McCabe. SonarQube
> Cognitive Complexity would score this higher (~11) due to nesting depth.

---

## 3. Blast Radius

`DeserializeSnapshot` is called at **3 sites** in the same file:

| Call Site | Location | Context |
|---|---|---|
| Primary load | `LoadStateSnapshot` L172 | First read of persisted state on strategy init |
| Post-rollback re-read | `LoadStateSnapshot` L196 | Re-deserializes `.bak` file after integrity failure |
| Rollback validation | `RollbackToLastGoodState` L279 | Reads backup before committing rollback |

**Downstream impact chain:**
```
DeserializeSnapshot
  └── LoadStateSnapshot
        ├── LoadStickyState          → minContracts, EnableSIMA, ReaperAuditEnabled (live trade params)
        └── RollbackToLastGoodState  → _stateCorruptionDetected counter → UI telemetry panel
```

A deserialization failure (`null` return) propagates silently through all three paths, defaulting live
trading parameters to zero/false without operator notification beyond a single `Print()` log line.

---

## 4. Risk Register

| ID | Severity | Description |
|---|---|---|
| R-01 | **HIGH** | Hand-rolled JSON parser: `IndexOf('}', objStart)` finds the first `}` after the `AccountPositions` block open brace. Safe only because `SerializeSnapshot` always emits positions *before* `ChecksumSHA256`. Any serializer ordering change silently truncates or corrupts the positions map. |
| R-02 | **MEDIUM** | `catch (FormatException)` is dead code: all `ParseJson*` helpers use `TryParse` internally and never throw `FormatException`. The handler increments `_stateCorruptionDetected` unreachably. |
| R-03 | **MEDIUM** | No semantic validation on deserialized values: negative `PositionSize`, empty `StrategyVersion`, or zero `SnapshotTicks` are accepted silently and returned to callers as valid state. |
| R-04 | **LOW** | `null`-as-error-sentinel requires all 3 call sites to implement synchronized null-guard logic. If any future call site omits the null check, it will NullReferenceException during `RestoreFromSnapshot`. |
| R-05 | **LOW** | `AccountPositions` parsed with `Split(',')` — a key or value containing an escaped comma (valid JSON) would silently split into malformed pairs, discarding positions. |

---

## 5. Hotspot Density Map

```
Lines 441–502  DeserializeSnapshot  (CYC 8 / 62 lines = 0.13 branches/line)
├── 441–452    Scalar field parsing         [CYC +0 — delegated to ParseJson* helpers]
├── 454–484    AccountPositions block       [CYC +4 — primary refactor target]
│   ├── 455    if accountPosStart           branch
│   ├── 459    if objStart && objEnd        branch
│   ├── 463    foreach pairs               branch
│   ├── 466    if colonIdx > 0             branch
│   └── 470    if int.TryParse             branch
└── 488–501    Exception handlers          [CYC +2 — one handler is dead code]
```

The **AccountPositions block (lines 454–484)** contributes 4 of 8 CYC points and is the primary
target for Phase 1 refactoring. Extracting it to a private helper `ParseAccountPositions(string, StateSnapshot)`
would reduce `DeserializeSnapshot` CYC to **4** and make the parser independently testable.

---

## 6. Recommended Phase Sequence

| Phase | Action | Expected CYC After |
|---|---|---|
| Phase 1 | Extract `ParseAccountPositions` helper; add semantic validation guard | 4 |
| Phase 2 | Replace `null`-sentinel with `Result<StateSnapshot>` pattern or `TryDeserialize` bool | 3 |
| Phase 3 | Remove dead `catch (FormatException)` or convert to documentation-only comment | 3 |

---

## 7. Related Symbols

- [`SerializeSnapshot`](src/V12_002.StickyState.cs:405) — producer; ordering contract tightly coupled to parser
- [`ValidateSnapshotIntegrity`](src/V12_002.StickyState.cs:220) — downstream consumer of deserialized data
- [`LoadStateSnapshot`](src/V12_002.StickyState.cs:153) — primary caller; owns null-check and rollback logic
- [`RollbackToLastGoodState`](src/V12_002.StickyState.cs:258) — secondary caller; double-calls deserializer on corrupt state
- [`ParseJsonLong`](src/V12_002.StickyState.cs:514) / [`ParseJsonBool`](src/V12_002.StickyState.cs:544) / [`ParseJsonString`](src/V12_002.StickyState.cs:564) — helper dependencies

---

*Generated by Bob — EPIC-W7-073 Phase 0 Hotspot Analysis*
