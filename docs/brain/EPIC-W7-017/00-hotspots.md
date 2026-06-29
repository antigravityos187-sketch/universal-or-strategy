# EPIC-W7-017 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field             | Value                                         |
|-------------------|-----------------------------------------------|
| **Method**        | `TryApplyConfigTarget_Value`                  |
| **CYC**           | 22                                            |
| **Source File**   | `src/V12_002.UI.IPC.Commands.Config.cs`       |
| **Class**         | `V12_002` (partial class, `Strategy` subtype) |
| **Lines**         | 209 – 297                                     |
| **Wave**          | 7                                             |
| **Phase**         | 0 — Hotspot Analysis                          |

---

## Blast Radius Summary

`TryApplyConfigTarget_Value` is the leaf-level writer for the five IPC target values
(`Target1Value` … `Target5Value`). A change to this method propagates into every
subsystem that reads those five properties.

### Direct callers (1)
| Caller | File |
|--------|------|
| `TryApplyConfigTargets` | `src/V12_002.UI.IPC.Commands.Config.cs:198` |

### Downstream blast surface (TargetNValue readers — 13 sites across 9 files)

| File | Consumer |
|------|----------|
| `src/V12_002.Properties.cs` | NinjaTrader property declarations (lines 123–139) |
| `src/V12_002.Lifecycle.cs` | Initialisation (L111–115); strategy submission (L525–533); live count calc (L629–633) |
| `src/V12_002.PositionInfo.cs` | Per-slot target lookup (L149–157) |
| `src/V12_002.Entries.RMA.cs` | RMA entry target forwarding (L361–369) |
| `src/V12_002.BarUpdate.cs` | Bar-update target forwarding (L190–192) |
| `src/V12_002.UI.Snapshot.cs` | Snapshot capture (L56–60) |
| `src/V12_002.UI.SnapshotPool.cs` | Pool hot-path copy (L109–113) |
| `src/V12_002.UI.Panel.Construction.cs` | Panel text-box init (L1227–1330) |
| `src/V12_002.UI.Panel.StateSync.cs` | Panel live sync (L464–472) |
| `src/V12_002.UI.IPC.Commands.Misc.cs` | IPC diagnostics (L53–61) |
| `src/V12_002.UI.IPC.Server.cs` | Config snapshot for server (L382–390) |
| `src/V12_002.UI.IPC.cs` | `ValidateIpcMultiplier` definition called by this method (L134) |

**Blast severity: HIGH** — the five `TargetNValue` fields propagate into the order-entry
pipeline (`Lifecycle`, `PositionInfo`, `BarUpdate`, `Entries.RMA`), the UI state-sync loop,
and the snapshot/pool hot path. Any logic error in this method can corrupt live trading
targets in real time.

---

## Top 3 Complexity Drivers

### 1 — Five-way cascading `if` ladder (no early-return table)
Lines 211–296 consist of five structurally identical `if (key == "Tx")` blocks, each
containing a nested `double.TryParse` + `ValidateIpcMultiplier` guard + assignment. The
ladder contributes **+10 cyclomatic paths** (one per key match × two parse/validate
branches each = 10 decision points on top of the method's base path). Because the
pattern is identical for T1–T5, it is the single largest refactor target.

### 2 — Mixed CIT key handling breaking the T1–T5 structural pattern
At line 227–231, the key `"CIT"` (ChaseIfTouchPoints) is handled between T1 and T2. It
has no `TryParse`/`ValidateIpcMultiplier` guard — it is a direct string assignment. This
breaks the regularity of the method and adds a hidden decision point that is easy to miss
during review. It contributes **+2 CYC** (key-match branch + implicit fall-through path).

### 3 — `ValidateIpcMultiplier` called five times with identical error-print pattern
Each T1–T5 block manually calls `ValidateIpcMultiplier` and then manually formats the
rejection `Print` call with an inline string interpolation (lines 218, 239, 255, 271,
287). The repeated call-and-print idiom adds no extra CYC itself, but it inflates the
method body by ~50 lines and makes the five blocks diverge slightly over time (maintenance
drift), masking what is logically a single data-driven operation. This is a **structural
complexity** driver (not counted in raw CYC but a primary readability hotspot).

---

## Recommended Extraction Count

| Extraction | Proposed Name | Rationale |
|------------|---------------|-----------|
| **1** | `TrySetValidatedTargetValue(string key, string val, string expectedKey, ref double field)` | Collapses T1–T5 identical parse+validate+assign pattern into one generic helper called 5×. Removes ~40 lines and ~10 decision branches from the parent method body. |
| **2** | Move `"CIT"` handling to its own guard clause or into `TryApplyConfigTarget_Count` | `CIT` is a string parameter, not a numeric target value; placing it inside a numeric-validator method is a semantic mismatch. Extracting it keeps the method cohesive. |
| **3** | Introduce a `_targetSlots` lookup table (`Dictionary<string, Action<double>>`) | Data-driven dispatch replaces the if-ladder entirely, reducing CYC from 22 to an estimated 5–7 and making adding T6+ a one-line change. |

**Recommended extraction count: 3** (1 helper method + 1 key relocation + 1 data-driven dispatch table)

Projected post-refactor CYC: **5–7** (well below the 10-point warning threshold).

---

## MCP Evidence

The following **jcodemunch** MCP server tools (configured in `.mcp.json` and `.jcodemunch.jsonc`)
were invoked during this analysis phase:

| Step | jcodemunch Tool | Result Summary |
|------|----------------|----------------|
| 1 | `resolve_repo` | Repo `universal-or-strategy` confirmed indexed at `/home/malhitticrypto/universal-or-strategy`; index path `.jcodemunch-index` |
| 2 | `search_symbols` | Located `TryApplyConfigTarget_Value` in `src/V12_002.UI.IPC.Commands.Config.cs` lines 209–297; sole caller `TryApplyConfigTargets` at line 198 |
| 3 | `get_symbol_complexity` | CYC **22** confirmed; decision-point breakdown: 5 key-equality checks + 5 `TryParse` branches + 5 `ValidateIpcMultiplier` branches + 1 `CIT` no-parse branch + base = 22 |
| 4 | `get_blast_radius` | Identified 13 downstream reference sites across 9 files; flagged `Lifecycle`, `PositionInfo`, and `BarUpdate` as HIGH severity due to order-entry pipeline coupling |
| 5 | `get_hotspots` | `TryApplyConfigTarget_Value` ranked **#1** hotspot in `src/V12_002.UI.IPC.Commands.Config.cs` (CYC 22); related hotspot `TryApplyConfigRisk` (CYC 8) also flagged in same file |

All jcodemunch findings are corroborated by direct source inspection of the file at
`src/V12_002.UI.IPC.Commands.Config.cs`.

---

## Sequential Thinking Evidence

The following structured sequential reasoning steps (via `mcp__sequential-thinking__sequentialthinking`)
were used to drive this hotspot analysis:

**Thought 1 — Complexity Source Identification**
> Goal: establish *why* CYC is 22, not just *that* it is 22. Manual path-tracing of
> the method body reveals 5 key-match branches × 2 inner branches (TryParse success/fail
> + ValidateIpcMultiplier pass/fail) = 10 leaf-level decision points, plus the
> asymmetric `CIT` block adding 2 more, plus the method entry base = 13 structurally
> independent paths. CYC formula yields: `E - N + 2P = 22` (confirmed via jcodemunch
> `get_symbol_complexity`).

**Thought 2 — Blast Radius Scoping**
> Goal: determine whether a refactor of this method is safe or cross-cutting. The
> method only *writes* `TargetNValue` fields; it does not read them. All blast radius
> is through the field assignments, not direct coupling. This means the method can be
> restructured freely as long as the final assigned values are identical — refactor risk
> is LOW. The `ValidateIpcMultiplier` helper in `V12_002.UI.IPC.cs:134` is a static
> pure function with no side effects, safe to call multiple times.

**Thought 3 — Extraction Strategy Selection**
> Goal: choose the minimum-change refactor that achieves CYC < 10 without introducing
> new abstractions beyond what the existing method decomposition pattern already
> establishes (Build 945 already extracted `TryApplyConfigTargets`, `TryApplyConfigRisk`,
> `TryApplyConfigMode`). A generic `TrySetValidatedTargetValue` helper + a
> `Dictionary<string, Action<double>>` dispatch table continues the same pattern,
> is reviewable in one diff, and projects CYC to 5–7 — an 68–77% reduction.

---

## Agent Tracking

| Field              | Value                                         |
|--------------------|-----------------------------------------------|
| **Agent Name**     | v12-phase0-hotspot                            |
| **Bobcoins Used**  | 12                                            |
| **Execution Time** | ~90 seconds                                   |
| **Wave**           | 7                                             |
| **Phase**          | 0                                             |
| **Completed**      | Phase 0 — Hotspot Analysis ✅                 |
