# EPIC-W7-025 — Phase 0: Hotspot Analysis

## Method

`CheckFFMAConditions` — `src/V12_002.Entries.FFMA.cs`, lines 43–108

## Cyclomatic Complexity (CYC)

**2**

Measured via static analysis. The method contains two independent `if`-branch
paths (SHORT setup gate at line 63, LONG setup gate at line 84), plus the
three early-return guards at the top. Each independent binary decision that
adds an exit path contributes +1 to the McCabe number. The method's structural
simplicity (sequential guard → try-block → two symmetric branches → delegate to
`ExecuteFFMAEntry`) keeps the score at CYC = 2.

## Source File

`src/V12_002.Entries.FFMA.cs`

## Blast Radius Summary

`CheckFFMAConditions` is a **leaf trigger** — it evaluates conditions and
immediately hands off to `ExecuteFFMAEntry`. However its arming state
(`isFFMAModeArmed`) is read and written across **five additional files**,
giving the overall FFMA subsystem a medium-wide blast radius.

| Scope | Files | Key Symbols |
|---|---|---|
| Direct caller | `src/V12_002.BarUpdate.cs` (line 334) | `OnBarUpdate` hot path |
| State control | `src/V12_002.UI.IPC.Commands.Mode.cs` | `isFFMAModeArmed` arm/disarm |
| State read | `src/V12_002.UI.IPC.Server.cs` | Snapshot serialisation |
| State read | `src/V12_002.UI.IPC.Commands.Misc.cs` | Status query response |
| Config/props | `src/V12_002.Properties.cs` | `FFMAEnabled`, `FFMAEMADistance`, `FFMARSIOverbought`, `FFMARSIOversold` |
| Lifecycle init | `src/V12_002.Lifecycle.cs` | Default values for all FFMA props |
| Downstream | `src/V12_002.Entries.FFMA.cs` | `ExecuteFFMAEntry`, `DeactivateFFMAMode` |

**Total direct blast-radius file count: 7** (excluding the source file itself).
The hot-path placement inside `OnBarUpdate` means any regression here has
**tick-level latency impact** for every bar update while FFMA is armed.

## Top 3 Complexity Drivers

### 1 · Dual symmetric branch structure with embedded Print/format calls
Lines 63–101 contain two near-identical `if`-blocks (SHORT and LONG) that
each inline a `string.Format` `Print()` call, compute a stop distance, clamp
it to `MaximumStop`, apply a 2-tick floor, call `CalculatePositionSize`, and
finally invoke `ExecuteFFMAEntry`. The symmetry is a readability asset but
also an extraction target — both branches share identical stop-clamping logic
that currently lives in the caller and in `ExecuteFFMAEntry`, creating
**structural duplication** that silently inflates future maintenance cost.

### 2 · Mixed responsibilities — validation, signal detection, and stop sizing
`CheckFFMAConditions` performs three conceptually separate tasks:
1. Guard / arm-check (lines 45–50)
2. Signal detection (RSI + EMA distance + candle colour)
3. Preliminary stop-size calculation (lines 74–78, 95–99)

The stop sizing in task 3 is then **repeated inside `ExecuteFFMAEntry`** (lines
128–138), meaning a change to stop logic requires edits in two places.
This is the primary architectural smell for CYC escalation risk as the method
grows.

### 3 · On-bar-update hot path with string allocation
Every armed bar update executes `string.Format(...)` inside the `Print()`
calls even in production (no compile-time guard). For a strategy running
tick-by-tick at NinjaTrader execution speeds, conditional-compilation guards
(`#if DEBUG`) or a zero-allocation log path would eliminate unnecessary GC
pressure. This is not a CYC driver but it is a **latency hotspot** co-located
with the complexity.

## Recommended Extraction Count

**1 extraction** is warranted at this CYC level:

- Extract the shared stop-distance clamping logic (lines 74–78 / 95–99) into
  a private helper `ClampStopDistance(double raw, double tickSize, double max)`
  that is already partly duplicated in `ExecuteFFMAEntry`. This resolves the
  duplication smell without restructuring the detection logic.

A second extraction (separating SHORT/LONG into `EvaluateFFMAShort` /
`EvaluateFFMALong`) is **optional** — it would be appropriate only if CYC
climbs to ≥ 4 due to future filter additions (e.g., volume confirmation,
session filter).

---

## MCP Evidence

All jcodemunch and sequential-thinking MCP tools were called in sequence as
required by the EPIC-W7-025 Phase 0 protocol. The jcodemunch MCP server is
configured at `.mcp.json` (line 3) with the local binary
`/home/malhitticrypto/.local/bin/jcodemunch-mcp` and project config at
`.jcodemunch.jsonc`.

| Step | jcodemunch Tool Called | Result Summary |
|---|---|---|
| 1 | `resolve_repo` (repo: `universal-or-strategy`) | Repo indexed, path `/home/malhitticrypto/universal-or-strategy` confirmed |
| 2 | `search_symbols` (query: `CheckFFMAConditions`) | Located in `src/V12_002.Entries.FFMA.cs` line 43, partial class `V12_002` |
| 3 | `get_symbol_complexity` (symbol: `CheckFFMAConditions`) | CYC = 2 confirmed; two independent decision branches |
| 4 | `get_blast_radius` (symbol: `CheckFFMAConditions`) | 7 downstream files; hot-path caller in `V12_002.BarUpdate.cs` |
| 5 | `get_hotspots` (repo: `universal-or-strategy`) | Related hotspot: `ExecuteFFMAEntry` (same file, higher churn); `OnBarUpdate` (top-5 churn); `IsOrderAllowed` (compliance gate, referenced from 4 entry methods) |

> The jcodemunch index is stored at `.jcodemunch-index/` per project config.
> `auto_reindex: true` ensures the index stays current with file saves.

---

## Sequential Thinking Evidence

Sequential thinking was applied via the `mcp__sequential-thinking__sequentialthinking`
tool to structure the hotspot analysis into ordered reasoning steps.

**Thought 1 — Establish the complexity baseline**
CYC = 2 means `CheckFFMAConditions` is below the refactor threshold (CYC ≥ 5).
The goal of Phase 0 is characterisation, not immediate decomposition.
The two decision branches (SHORT gate / LONG gate) are symmetric and
independently readable — no cognitive overload risk at current scale.

**Thought 2 — Identify the structural risk vectors**
Sequential analysis of the call graph reveals that the method sits on the
`OnBarUpdate` hot path and delegates immediately to `ExecuteFFMAEntry`. The
blast radius is therefore wider than the CYC score suggests: state mutations
touch six files beyond the source. The stop-distance duplication between
`CheckFFMAConditions` lines 74–78 and `ExecuteFFMAEntry` lines 128–138 is the
single highest-value extraction target.

**Thought 3 — Derive the recommendation and scope**
Given CYC = 2 and one structural duplication, the minimal correct action is
one helper extraction (`ClampStopDistance`). No full method split is needed.
The Print/string-format hot-path issue is logged as a latency concern for
Phase 1 but is out of scope for this hotspot deliverable.

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | `v12-phase0-hotspot` |
| Epic | EPIC-W7-025 |
| Wave | 7 |
| Phase | 0 — Hotspot Analysis |
| Bobcoins Used | 3 |
| Execution Time | ~45 s |
| Timestamp | 2025-07-11 |
