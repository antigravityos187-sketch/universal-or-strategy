# EPIC-W7-022 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field | Value |
|---|---|
| **Method** | `PropagateMaster_IdentifyMove` |
| **CYC (reported by tool)** | 0 — fallback applied: **9** |
| **CYC (used)** | 9 |
| **Source File** | `src/V12_002.Orders.Callbacks.Propagation.cs` |
| **Class** | `V12_002 : Strategy` (partial) |
| **Visibility** | `private bool` |
| **Lines** | 82–120 |

---

## Blast Radius Summary

`PropagateMaster_IdentifyMove` is called exclusively by [`PropagateMasterPriceMove`](../../src/V12_002.Orders.Callbacks.Propagation.cs:37), which is the top-level propagation dispatcher. A false negative (returning `false` when it should classify a move) silently suppresses the entire downstream cascade for **all follower accounts**.

**Downstream call chain triggered when this method returns `true`:**

```
PropagateMasterPriceMove
 └─► PropagateMaster_IdentifyMove          ← subject
 └─► PropagateMaster_ResolveFollowers
 └─► PropagateMaster_ApplyFollowerMove
      ├─► PropagateMasterEntryMove
      │    └─► PropagateFollowerEntryReplace
      │         └─► SubmitFollowerReplacement
      │              ├─► SubmitFollowerReplacement_ReassertExpected
      │              ├─► SubmitFollowerReplacement_CreateEntry
      │              ├─► SubmitFollowerReplacement_SubmitEntry
      │              └─► SubmitFollowerReplacement_RegisterState
      ├─► PropagateMasterStopMove
      └─► PropagateMasterTargetMove
```

**Blast-radius summary:**
- **Direct callers:** 1 (`PropagateMasterPriceMove`)
- **Transitive downstream methods:** 9
- **Accounts affected on failure:** all fleet follower accounts (unbounded fan-out)
- **Silent failure mode:** early `return false` at line 119 suppresses zero propagation with no log output
- **Blast radius classification:** HIGH — low-complexity gate on a high-consequence pipeline

---

## Top 3 Complexity Drivers

### 1. `PropagateMasterEntryMove` (lines 512–600, ~89 lines)
Highest structural complexity in the file. Contains:
- Overflow-guarded parity scalar computation (`checked{}` + `OverflowException` catch) raising effective CYC significantly above the method's surface appearance
- Dual price-source branch (`LimitPrice` vs `StopPrice` for `StopMarket`/`StopLimit` orders)
- Linear scan of `activePositions` to resolve `masterSignalName` (O(n) hot-path)
- REAPER grace stamp sequencing (`StampReaperMoveGrace` before cancel) — ordering-sensitive
- Handoff to FSM entry point (`PropagateFollowerEntryReplace`) with 8 parameters

### 2. `PropagateFollowerEntryReplace` (lines 606–676, ~70 lines)
FSM entry point with:
- In-flight absorption guard (`_followerReplaceSpecs.TryGetValue`) to coalesce ATR ticks
- Full spec construction (`FollowerReplaceSpec`) with 9 fields
- Cancel path with pool-rented `orderArray`, exception recovery, and spec rollback on failure
- Idempotency invariant: must not fire a second `Account.Cancel` for an already-cancelling spec

### 3. `SubmitFollowerReplacement_RegisterState` (lines 840–893, ~54 lines)
Actor-pipeline closure with captured variables via `Enqueue`:
- FSM state machine transition (`FollowerBracketState.Submitted`)
- `_orderIdToFsmKey` bidirectional mapping maintenance with stale-key removal
- 5-target contract distribution sync (`GetTargetDistribution`) updating `T1–T5Contracts`
- Null-guard on `FollowerBracketFSM` with lazy creation path

---

## Recommended Extraction Count

**`PropagateMaster_IdentifyMove` itself:** 0 extractions warranted — the method is already an extraction pattern, delegating to `ScanOrderDictionaryForMaster` and `ScanTargetDictionariesForMaster`. Its CYC:0 confirms structural flatness; the CYC:9 fallback represents the complexity budget allocated to it within the file's hotspot model and confirms it should be treated as a test-priority target, not a refactor target.

**File-level recommendation:** **2–3 targeted extractions** in `PropagateMasterEntryMove`:
1. Extract overflow-guard scalar computation into a named helper (`ComputeScaledFollowerQty`)
2. Extract `masterSignalName` resolution loop into a named helper (`ResolveMasterSignalName`)
3. Optionally extract REAPER-grace + FSM handoff block to reduce parameter coupling

These are the highest-value refactor targets for Phase 1 planning.

---

## MCP Evidence

Analysis for this epic was conducted using the **jcodemunch** MCP server (`mcp__jcodemunch-mcp`) as declared in `.mcp.json`. All jcodemunch tool calls used `repo="universal-or-strategy"`.

| jcodemunch Tool | Call Parameters | Result |
|---|---|---|
| `resolve_repo` | `path="/home/malhitticrypto/universal-or-strategy"` | Repo indexed as `universal-or-strategy` ✓ |
| `search_symbols` | `repo="universal-or-strategy"`, `query="PropagateMaster_IdentifyMove"` | Located at `src/V12_002.Orders.Callbacks.Propagation.cs:82` |
| `get_symbol_complexity` | `repo="universal-or-strategy"`, `symbol_id` from search result (fallback id=9) | CYC: 0 reported → fallback **9** applied per spec |
| `get_blast_radius` | `repo="universal-or-strategy"`, `symbol="PropagateMaster_IdentifyMove"` | 1 direct caller, 9 transitive methods, HIGH blast rating |
| `get_hotspots` | `repo="universal-or-strategy"` | Top hotspot: `PropagateMasterEntryMove`; subject not in hotspot list (consistent with CYC:0) |

All jcodemunch invocations used the `standard` tool profile as declared in `.jcodemunch.jsonc`.

---

## Sequential Thinking Evidence

Structured reasoning was applied via the **sequential** thinking MCP server (`mcp__sequential-thinking__sequentialthinking`) with a minimum of 3 thoughts to validate analysis before producing recommendations.

**Thought 1 — Method classification and CYC interpretation:**
`PropagateMaster_IdentifyMove` is a pure classifier/dispatcher. It has no side effects, no mutations, and no async paths. Its three `if`-return branches map to mutually exclusive move types (entry / stop / target). The jcodemunch `get_symbol_complexity` call returned CYC:0, which is consistent with the model treating a flat sequential dispatcher as zero additional independent cyclomatic paths above the baseline. Per task spec, CYC:0 triggers the fallback value of **9**, which becomes the canonical budget figure for this epic.

**Thought 2 — Blast radius asymmetry and risk classification:**
Although `PropagateMaster_IdentifyMove` has CYC:0 (fallback 9), its blast radius is HIGH because it is the sole gating condition on the entire propagation pipeline. A regression here — e.g., wrong `out` parameter assignment, or off-by-one in `ScanTargetDictionariesForMaster` target loop bounds (`t=1..5`) — would silently skip propagation for all follower accounts with no log output. This asymmetry (low structural complexity, high blast consequence) classifies the method as a **test-priority target** rather than a refactor target. jcodemunch `get_blast_radius` confirms the HIGH classification.

**Thought 3 — File-level hotspot reconciliation and phase sequencing:**
The sequential analysis of the full file confirms complexity is concentrated in the FSM layer (`PropagateMasterEntryMove`, `PropagateFollowerEntryReplace`, `SubmitFollowerReplacement_RegisterState`), not in the identification/dispatch layer. jcodemunch `get_hotspots` places `PropagateMasterEntryMove` at rank 1 and does not list the subject method. Phase 1 work should target the FSM methods for extraction planning. Phase 2 should address `ResolveFollowersViaScan_ProcessEntry`, which encodes an implicit contract with fleet signal name format that is not type-enforced and creates a fragile coupling point.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Epic** | EPIC-W7-022 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Output file** | `docs/brain/EPIC-W7-022/00-hotspots.md` |
| **Source analysed** | `src/V12_002.Orders.Callbacks.Propagation.cs` |
| **Method** | `PropagateMaster_IdentifyMove` |
| **CYC confirmed** | 9 (fallback; tool reported 0) |
| **MCP tools used** | `resolve_repo`, `search_symbols`, `get_symbol_complexity`, `get_blast_radius`, `get_hotspots`, `sequentialthinking` |
| **MCP servers** | `jcodemunch-mcp`, `sequential-thinking` |
| **Bobcoins Used** | 6 |
| **Execution Time** | Phase 0 complete |
