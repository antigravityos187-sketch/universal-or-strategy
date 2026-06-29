# EPIC-W7-025 — Phase 1: Scope Definition

## Single Method in Scope

| Field | Value |
|---|---|
| Method | `CheckFFMAConditions` |
| Source File | `src/V12_002.Entries.FFMA.cs` |
| Lines | 43–108 |
| Partial Class | `V12_002 : Strategy` |
| Current CYC | **2** |
| Target CYC | **≤ 8** (well within budget; no decomposition strictly required at this level) |

This document defines the **scope boundary** for Phase 1 of EPIC-W7-025.
Only the **single method** `CheckFFMAConditions` is subject to analysis,
planning, and any subsequent refactoring actions within this epic.

---

## Callers

Grep across all `.cs` files in the workspace identified **1 direct call site**:

| Caller File | Line | Caller Symbol | Context |
|---|---|---|---|
| `src/V12_002.BarUpdate.cs` | 334 | `OnBarUpdate` | Inside `if (isFFMAModeArmed && FFMAEnabled)` guard on the hot path |

**Callers count: 1**

The method is called exclusively from the `OnBarUpdate` hot path — every
armed bar update invokes it. There are no other call sites in the codebase,
confirming that the scope boundary is stable and that changes to
`CheckFFMAConditions` have a single propagation vector into the runtime.

---

## Why Other Methods Are NOT in Scope

Phase 7 (V12.23) partitioned the original monolithic `Entries.cs` into
dedicated node files. `src/V12_002.Entries.FFMA.cs` now hosts five methods:
`CheckFFMAConditions`, `ExecuteFFMAEntry`, `DeactivateFFMAMode`,
`ExecuteFFMALimitEntry`, and `ExecuteFFMAManualMarketEntry`.

**None of those sibling methods are in scope for EPIC-W7-025.** The reasons are:

1. **V12.23 partition boundary** — The V12.23 modular split assigned each
   method its own logical ownership unit. Post-partition, each method is
   treated as an independent refactoring target with its own epic, CYC
   measurement, and hotspot report. Bundling sibling methods into this epic
   would cross the V12.23 partition boundary and invalidate prior hotspot
   baselines.

2. **CYC scope isolation** — The CYC = 2 measurement recorded in Phase 0 is
   specific to `CheckFFMAConditions`. `ExecuteFFMAEntry` carries its own
   (higher) complexity score and is tracked under a separate epic. Merging
   them would produce a composite CYC that cannot be meaningfully attributed
   to either method for regression tracking.

3. **Blast-radius containment** — `CheckFFMAConditions` is a leaf trigger
   that delegates immediately to `ExecuteFFMAEntry`. Including downstream
   delegates in scope would expand the blast radius from 1 to ≥ 7 files and
   risk entangled changes across the `isFFMAModeArmed` state machine, the
   IPC snapshot serialiser, and the UI command handlers — all out of scope
   for a single-method CYC refactor.

4. **Single-responsibility principle for epics** — The engineering discipline
   for this wave requires one epic = one method = one deliverable. This keeps
   the scope boundary enforceable and the output reviewable in isolation.

---

## Method Structure Summary (from Phase 0 Hotspot Analysis)

```
CheckFFMAConditions()
├── Guard block (lines 45–50): isFFMAModeArmed, FFMAEnabled, null checks, bar count
├── try {
│   ├── SHORT branch (line 63): RSI > 80 + EMA distance + RED candle
│   │   ├── Print / string.Format (latency hotspot)
│   │   ├── Stop-distance clamp (lines 74–78) ← duplication with ExecuteFFMAEntry:128–138
│   │   └── ExecuteFFMAEntry(...)
│   └── LONG branch (line 84): RSI < 20 + EMA distance + GREEN candle
│       ├── Print / string.Format (latency hotspot)
│       ├── Stop-distance clamp (lines 95–99) ← same duplication
│       └── ExecuteFFMAEntry(...)
└── } catch (Exception ex) { Print("ERROR ..."); }
```

**Decision paths contributing to CYC = 2:** SHORT gate (line 63) and LONG
gate (line 84). The early-return guards are linear and do not increase CYC.

---

## Complexity Budget

| Metric | Value |
|---|---|
| Current CYC | 2 |
| Target CYC (maximum) | ≤ 8 |
| Budget headroom | +6 branches before action required |
| Recommended extraction | `ClampStopDistance` helper (stop-distance duplication) |
| Optional extraction | `EvaluateFFMAShort` / `EvaluateFFMALong` (only if CYC ≥ 4) |

The method is structurally sound at CYC = 2. The Phase 1 planning output
focuses on documenting the one extraction opportunity identified in Phase 0
(`ClampStopDistance`) without mandating an immediate restructuring.

---

## Scope Confirmation Checklist

- [x] single method identified: `CheckFFMAConditions`
- [x] source file confirmed: `src/V12_002.Entries.FFMA.cs`
- [x] current CYC recorded: 2
- [x] target CYC stated: ≤ 8
- [x] callers count established: 1
- [x] scope boundary declared and justified
- [x] sibling methods excluded with rationale (V12.23 partition)
- [x] Phase 0 hotspot data cross-referenced

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | `v12-phase1-scope` |
| Epic | EPIC-W7-025 |
| Wave | 7 |
| Phase | 1 — Scope Definition |
| Input | `00-hotspots.md`, `manifest.json`, grep caller search |
| Output | `00-scope.md` |
| Callers Found | 1 (`src/V12_002.BarUpdate.cs:334`) |
| Timestamp | 2025-07-11 |
