# EPIC-W7-083 — Phase 0: Hotspot Analysis

**Wave:** 7 | **Phase:** 0  
**Method:** `AuditMaster_CheckExpectedActual`  
**Source:** `src/V12_002.REAPER.Audit.cs` (line 706)  
**Cyclomatic Complexity (CYC):** 13 (confirmed)

---

## 1. Symbol Location

| Item | Detail |
|---|---|
| Class | `V12_002` (partial) |
| Namespace | `NinjaTrader.NinjaScript.Strategies` |
| File | `src/V12_002.REAPER.Audit.cs` |
| Definition line | 706 |
| Only call-site | `AuditMaster_HandleDesyncFlatten` → line 595 |

---

## 2. CYC Breakdown (Manual Count)

The method body spans lines 706–743. Decision points that contribute to CYC:

| # | Construct | Line | Notes |
|---|---|---|---|
| 1 | Base path | — | +1 (entry) |
| 2 | `stampTicks > 0` | 710 | left operand of `&&` — short-circuit branch |
| 3 | `(DateTime.UtcNow.Ticks - stampTicks) < ReaperFillGraceTicks` | 710 | right operand of `&&` |
| 4 | `!inFillGrace` | 713 | negation guard in compound predicate |
| 5 | `(masterActualQty != 0 && masterExpectedQty == 0)` | 715 | left operand of `\|\|` |
| 6 | `masterActualQty != 0` | 715 | inner `&&` left operand |
| 7 | `masterExpectedQty == 0` | 715 | inner `&&` right operand |
| 8 | `Math.Sign(masterActualQty) != Math.Sign(masterExpectedQty)` | 716 | right operand of `\|\|`, left of `&&` |
| 9 | `masterExpectedQty != 0` | 716 | right operand of inner `&&` |
| 10 | `if (inFillGrace && shouldLog)` | 719 | `&&` — two branches |
| 11 | `shouldLog` | 719 | right operand of `&&` |
| 12 | `if (isCriticalDesync)` | 724 | primary branch |
| 13 | `if (AutoFlattenDesync)` | 730 | nested branch; `else if (shouldLog)` at 735 adds exit path |

**Total: 13** — matches the declared hotspot CYC.

---

## 3. Blast Radius

### Direct callers
| Caller | File | Line |
|---|---|---|
| `AuditMaster_HandleDesyncFlatten` | `src/V12_002.REAPER.Audit.cs` | 595 |

### Transitive call chain (downward)
```
AuditMaster_CheckExpectedActual
  └─ reads  _lastExpectedPositionSetTicks   (src/V12_002.REAPER.Audit.cs:709)
  └─ reads  ReaperFillGraceTicks            (src/V12_002.cs:737 — const)
  └─ reads  AutoFlattenDesync               (src/V12_002.cs, param field)
  └─ side-effect: Print() calls (4 paths)
  └─ returns bool → consumed by EnqueueReaperMasterFlatten → ProcessReaperFlattenQueue
```

### Upstream trigger chain
```
AuditApexPositions (30s timer)
  └─ AuditMasterAccountIfNeeded
       └─ AuditMaster_HandleDesyncFlatten
            └─ AuditMaster_CheckExpectedActual   ← HOTSPOT
```

### Cross-cutting state written by peers
| State field | Written by | Risk if race |
|---|---|---|
| `_lastExpectedPositionSetTicks` | `SetExpectedPositionLocked`, `StampReaperFillGrace` (SIMA.cs) | Grace window mis-evaluated — false flatten |
| `expectedPositions[masterKey]` | ~15 sites across 9 files | Stale expected qty → wrong desync classification |
| `AutoFlattenDesync` | UI param field | If toggled while audit runs — flatten skipped or double-fired |

---

## 4. Complexity Drivers

1. **Compound boolean for `isCriticalDesync`** (lines 712–717): two disjuncts each guarding via `&&`, merged under `!inFillGrace`. Five independent predicates collapsed into a single `bool`. Hard to reason about and test exhaustively (2⁵ = 32 theoretical states, only a subset are valid).

2. **Dual-purpose `shouldLog` threading**: the same parameter gates *four* separate `Print` paths within 37 lines, forcing every branch to carry `shouldLog` as a secondary concern alongside correctness logic.

3. **Interleaved fill-grace and desync logic**: `inFillGrace` suppresses the entire desync classification, but its evaluation (`Interlocked.Read` + arithmetic on `DateTime.UtcNow.Ticks`) is embedded inline rather than delegated to a helper. This couples timing logic to classification logic.

4. **Mixed return semantics**: the method returns `bool` to indicate "should flatten", but also emits diagnostic logs and signals `AutoFlattenDesync` gating — three concerns in one body.

---

## 5. Risk Assessment

| Dimension | Rating | Rationale |
|---|---|---|
| Correctness risk | 🔴 HIGH | A wrong `isCriticalDesync` result triggers or suppresses an emergency flatten on live accounts |
| Thread-safety risk | 🟡 MEDIUM | `_lastExpectedPositionSetTicks` is read via `Interlocked.Read` (safe), but `AutoFlattenDesync` and `expectedPositions` are not locked; benign for reads but visible during param changes |
| Testability | 🔴 HIGH | CYC 13 requires ≥13 test paths to achieve branch coverage; none currently exist |
| Change blast radius | 🟡 MEDIUM | Single direct caller but downstream effect is account flatten — irreversible in live trading |

---

## 6. Recommended Refactor Targets (Phase 1+)

| Priority | Action |
|---|---|
| P0 | Extract `EvaluateFillGrace()` helper — isolate `_lastExpectedPositionSetTicks` read + arithmetic |
| P0 | Extract `IsCriticalMasterDesync(actualQty, expectedQty, inFillGrace)` — pure boolean, unit-testable |
| P1 | Remove `shouldLog` from classification path; log at call-site in `AuditMaster_HandleDesyncFlatten` |
| P1 | Introduce unit tests covering all 5 predicate combinations in `isCriticalDesync` |

---

## 7. Files Confirmed Read

- `src/V12_002.REAPER.Audit.cs` (full, 949 lines)
- `src/V12_002.cs` (fields: lines 733–737, 664, 685)
- `src/V12_002.SIMA.cs` (lines 86–201 — `_lastExpectedPositionSetTicks` mutation sites)

---

*Generated: Phase 0 — Hotspot Analysis | EPIC-W7-083 | Wave 7*
