# Phase 0: Hotspot Analysis — EPIC-W7-070

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 2.47
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:47:53Z
- **Wave**: 7 | **Phase**: 0

## Target Method
- **Method**: HydrateFSMsFromWorkingOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 787
- **Cyclomatic Complexity**: 13
- **Lines of Code**: 105

## Complexity Metrics (`get_symbol_complexity`)
| Metric            | Value  | Threshold | Status |
|-------------------|--------|-----------|--------|
| Cyclomatic (CYC)  | 13     | ≤8        | ❌ OVER |
| Max Nesting Depth | 4      | ≤3        | ⚠️ HIGH |
| Parameter Count   | 0      | —         | ✅ OK  |
| Lines of Code     | 105    | —         | —      |
| Assessment        | HIGH   | —         | ⚠️     |

## Hotspot Analysis (`get_hotspots`, top_n=50, days=90)
- **Hotspot Score**: 46.2195
- **Rank**: #36 out of top 50 repository hotspots
- **Churn (90 days)**: 34 commits
- **Risk Level**: HIGH

### Top 5 Hotspots for Context
| Rank | Method | CYC | Score |
|------|--------|-----|-------|
| 1 | HydrateFromOpenPositions | 34 | 120.88 |
| 2 | IsCommandForThisInstrument | 38 | 109.83 |
| 3 | HandleTerminated | 30 | 102.04 |
| 4 | SweepBrokerOrders | 28 | 99.55 |
| 35 | MapOrderStateToFSMState | 13 | 46.22 |
| **36** | **HydrateFSMsFromWorkingOrders** | **13** | **46.22** |

## Blast Radius (`get_blast_radius`, depth=1)
- **Direct Dependents**: 0
- **Importer Count**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

> **Analysis**: Zero external dependents — this method is contained entirely within
> `src/V12_002.SIMA.Lifecycle.cs`. **LOW RISK** from blast radius perspective.

## Call Hierarchy (`get_call_hierarchy`, direction=both, depth=3)

### Callers (3 total — all same-file)
1. **HydrateWorkingOrdersFromBroker** — depth 1, line 309 (`ast_resolved`)
2. **EnumerateApexAccounts** — depth 2, line 140 (`ast_resolved`)
3. **ProcessInitializeSIMA** — depth 3, line 90 (`ast_resolved`)

### Key Callees (33 total)
| Method | Line | Type |
|--------|------|------|
| MapOrderStateToFSMState | 469 | method |
| FindLivePosition | 605 | method |
| ResolveRemainingContracts | 532 | method |
| BuildFSM | 505 | method |
| LinkTargetOrderToFSM | 579 | method |
| RegisterFSM | 551 | method |
| HydrateFromOpenPositions | 625 | method |
| LogBuffer.Format | 28 | method |
| entryOrders, activePositions, stopOrders, _followerBrackets, target1-5Orders | — | constants |

## Risk Assessment

### Overall Risk: **MEDIUM**

| Factor | Level | Detail |
|--------|-------|--------|
| Blast Radius | LOW ✅ | 0 external dependents |
| Complexity | HIGH ⚠️ | CYC 13 vs Jane Street target ≤8 |
| Churn | HIGH ⚠️ | 34 commits in 90 days |
| Nesting | HIGH ⚠️ | Max depth 4 |
| Caller Scope | LOW ✅ | All callers in same file |
| Callee Count | HIGH ⚠️ | 33 callees — complex orchestration |

## Refactoring Recommendation
**PROCEED WITH CAUTION** — good candidate due to zero blast radius and high complexity.

Extract 2–3 helper methods targeting:
- FSM state mapping logic (`MapOrderStateToFSMState` dispatch)
- Position resolution logic (`FindLivePosition` + `ResolveRemainingContracts`)
- FSM build-and-register logic (`BuildFSM` + `LinkTargetOrderToFSM` + `RegisterFSM`)

## Jane Street Alignment
- **Current CYC**: 13 — GODMODE violation
- **Target CYC**: ≤8 — GODMODE compliant
- **Gap**: 5 points over threshold
- **Extraction Estimate**: 2–3 helper methods
