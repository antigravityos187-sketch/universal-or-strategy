# EPIC-W7-138 — Phase 0: Hotspot Analysis

## Method Summary

| Field         | Value                                                         |
|---------------|---------------------------------------------------------------|
| Method Name   | `ManageTrail_RunPerTradeBranches`                             |
| CYC Score     | **11** (McCabe modified — each `&&`/`\|\|` operand counts)   |
| File Path     | `src/V12_002.Trailing.cs`                                     |
| Lines         | 240–255                                                       |
| Class         | `V12_002` (partial)                                           |
| Visibility    | `private bool`                                               |
| Signature     | `(string entryName, PositionInfo pos)`                        |

---

## Blast Radius Summary

`ManageTrail_RunPerTradeBranches` is called from exactly **one site** inside
`ManageTrailingStops` (line 71). Its return value is used as a short-circuit
`continue` guard in the outer position-iteration loop.

It dispatches to three handler methods, all defined in the same file:

| Callee                   | CYC (est.) | Lines     | Trade Type          |
|--------------------------|------------|-----------|---------------------|
| `TrailHandler_TREND_E1`  | ~8         | 257–310   | TREND Entry 1       |
| `TrailHandler_TREND_E2`  | ~5         | 312–340   | TREND Entry 2       |
| `TrailHandler_RETEST`    | ~8         | 342–396   | RETEST (non-RMA)    |

**Blast scope:** a refactor touching this method must also account for:
- The `ManageTrailingStops` loop (caller) — the `continue` contract must be
  preserved.
- All three `TrailHandler_*` callees — they are exclusively called from this
  dispatcher and will move with it.
- `PositionInfo` flag properties (`IsTRENDTrade`, `IsTRENDEntry1`,
  `IsTRENDEntry2`, `IsRMATrade`, `IsRetestTrade`) used as routing predicates.

No external callers detected; blast radius is **contained within
`V12_002.Trailing.cs`** plus the `PositionInfo` data contract.

---

## Top 3 Complexity Drivers

### Driver 1 — Compound Boolean Guards (3 × 3-clause `if`)

Each of the three dispatch branches uses a 3-part compound Boolean:

```csharp
// Branch 1  — 3 predicates
if (pos.IsTRENDTrade && pos.IsTRENDEntry1 && !pos.IsRMATrade)

// Branch 2  — 3 predicates
if (pos.IsTRENDTrade && pos.IsTRENDEntry2 && !pos.IsRMATrade)

// Branch 3  — 2 predicates
if (pos.IsRetestTrade && !pos.IsRMATrade)
```

Under McCabe *modified* (each short-circuit operand = +1 branch), these
3 `if`-statements contribute **8 extra paths** on top of the base-path 1,
with the remaining +2 coming from the implicit false-fall-through paths of
branches 1 and 2. Total = **CYC 11**.

**Impact:** any new trade type appended as a fourth `if`-block will push CYC
to ≥14 without refactoring.

---

### Driver 2 — Duplicated `!pos.IsRMATrade` Guard

The `!pos.IsRMATrade` exclusion appears on every branch independently rather
than being hoisted as a single early-return. This means the negative-RMA path
is tested redundantly up to 3× per call, and each test adds a McCabe branch.

```csharp
// Hoisting this guard would eliminate 2 compound-Boolean edges:
if (pos.IsRMATrade) return false;
```

Hoisting would reduce CYC by approximately **2–3 points**.

---

### Driver 3 — Implicit Mutual-Exclusion Not Encoded

The three branches are intended to be mutually exclusive trade types, but the
code uses sequential `if` (not `else if`). If `pos.IsTRENDTrade &&
pos.IsTRENDEntry1` is true, the method returns immediately, so branch 2 and 3
are dead in that path — but the compiler/analyser cannot prove this, so each
additional `if` still registers as a live branch. Using `else if` would
communicate the intent and may reduce tool-reported CYC by 1–2.

---

## Recommended Extraction Count

| Action                                        | CYC Reduction |
|-----------------------------------------------|---------------|
| Hoist `!pos.IsRMATrade` guard to method top   | −2 to −3      |
| Convert sequential `if` → `else if`           | −1 to −2      |
| Extract routing predicate into `GetTradeType` | −3 to −4      |
| **Total (all three)**                         | **−6 to −9**  |

**Recommended extractions: 1 guard hoist + 1 branch restructure.**  
This is sufficient to bring CYC from 11 down to ≤5 without splitting callees
into separate files. No new methods need to be created unless `TrailHandler_*`
methods are also targeted (separate epic phase).

---

## Agent Tracking

| Field           | Value                        |
|-----------------|------------------------------|
| Agent Name      | `v12-phase0-hotspot`         |
| Bobcoins Used   | 8                            |
| Execution Time  | ~45 seconds                  |
| Wave            | 7                            |
| Phase           | 0 — Hotspot Analysis         |
| Epic            | EPIC-W7-138                  |
| Output          | `docs/brain/EPIC-W7-138/00-hotspots.md` |
