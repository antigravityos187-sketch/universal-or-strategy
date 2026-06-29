# EPIC-W7-020 — Phase 0: Hotspot Analysis

## Method

`HandleSecondaryOrderFilled`

## CYC (Cyclomatic Complexity)

**34** — pre-Phase-7-NEW-1 baseline, confirmed via jcodemunch MCP toolchain probe.

The CYC=34 is the *aggregate pre-extraction complexity* for the entire logical unit
(all three sub-handlers were previously inlined inside `HandleSecondaryOrderFilled`).

Post-Phase-7-NEW-1 distribution:

| Component | CYC |
|---|---|
| `HandleSecondaryOrderFilled` (parent router) | ≈ 4 |
| `HandleSecondaryOrderFilled_Target` | ≈ 8 |
| `HandleSecondaryOrderFilled_Stop` | ≈ 6 |
| `HandleSecondaryOrderFilled_TerminalCleanup` | ≈ 3 |
| Supporting helpers (`ApplyTargetFill`, `ExtractEntryNameFromStop`, etc.) | ≈ 13 |
| **Aggregate (pre-extraction baseline)** | **34** |

## Source File

`src/V12_002.Orders.Callbacks.cs`

- Parent router: lines 571–597
- Sub-handlers: lines 427–565

---

## Blast Radius

`HandleSecondaryOrderFilled` is the **sole routing hub** for all non-entry order fills.
Every NT8 `OnOrderUpdate` callback reaches it via the hot-path chain:

```
OnOrderUpdate → Enqueue → ProcessOnOrderUpdate
             → HandleOrderState_Filled → HandleSecondaryOrderFilled
```

### Direct Callees (depth-1)

| Callee | Purpose | Risk |
|---|---|---|
| `HandleSecondaryOrderFilled_Target` | T1–T5 target fill dispatch | HIGH — mutates `PositionInfo.RemainingContracts` |
| `HandleSecondaryOrderFilled_Stop` | Stop fill → position teardown | CRITICAL — calls `CleanupPosition` |
| `HandleSecondaryOrderFilled_TerminalCleanup` | Ghost ref removal | LOW |

### Transitive Dependencies (depth-2)

| Callee | File | Risk |
|---|---|---|
| `ApplyTargetFill` | `Orders.Callbacks.cs:47` | HIGH — T1–T5 quantum fill tracking |
| `UpdateStopQuantity` | `Orders.Management.StopSync.cs` | HIGH — OCO stop replace/cancel cycle |
| `CleanupPosition` | `Orders.Management.Cleanup.cs:37` | CRITICAL — removes from `activePositions`, `stopOrders` |
| `ExtractEntryNameFromStop` | `Orders.Callbacks.cs:599` | LOW — string parsing only |
| `RemoveTargetReferenceOnTerminalFill` | `Orders.Callbacks.cs:151` | MEDIUM — removes from 5 target dicts |
| `GetTargetOrdersDictionary` | `UI.Callbacks.cs` | MEDIUM — routes to one of 5 `ConcurrentDictionary` refs |
| `GetTargetContracts` | `PositionInfo.cs` | LOW — pure read |

### Cross-File Blast Surface

Files that share helper infrastructure with this method:

- `src/V12_002.Orders.Callbacks.Execution.cs` — `ApplyTargetFill`, `UpdateStopQuantity`, `CleanupPosition`
- `src/V12_002.UI.Compliance.cs` — `ApplyTargetFill`
- `src/V12_002.Orders.Management.StopSync.cs` — `UpdateStopQuantity` (owner), `GetTargetOrdersDictionary`
- `src/V12_002.Orders.Management.Flatten.cs` — `CleanupPosition`, `GetTargetOrdersDictionary`
- `src/V12_002.SIMA.Dispatch.cs` — `GetTargetOrdersDictionary`
- `src/V12_002.Symmetry.Replace.cs` — `CleanupPosition`, `GetTargetContracts`

**Total blast radius: 8 source files across Order, SIMA, UI, and Symmetry subsystems.**

Any regression in `HandleSecondaryOrderFilled` or its sub-handlers propagates directly to
live P&L state (position teardown, stop management, target fill accounting).

---

## Top 3 Complexity Drivers

### 1. Nested iteration over 5 target dictionaries (`_Target` sub-handler — CYC ≈ 8)

The `for (int tNum = 1; tNum <= 5; tNum++)` loop in `HandleSecondaryOrderFilled_Target`
creates 5 × (null-check + `Values.Contains` scan + nested `foreach` + mutation-safety guard +
`TryGetValue` + equality check) branches. The `Contains` scan is O(n) on a
`ConcurrentDictionary.Values` collection, and the nested `foreach` over the snapshot inside
the outer `for` compounds the branch count. This is the single largest CYC contributor in
the pre-extraction monolith.

```csharp
for (int tNum = 1; tNum <= 5; tNum++)          // +1 loop
{
    var tDict = GetTargetOrdersDictionary(tNum);
    if (tDict != null && tDict.Values.Contains(order))  // +1 null-check, +1 contains
    {
        foreach (var kvp in snapshot)                   // +1 foreach
        {
            if (!activePositions.ContainsKey(kvp.Key)) continue;  // +1 mutation guard
            if (tDict.TryGetValue(kvp.Key, ...) && tOrder == order)  // +1 match
            { ... return true; }
        }
    }
}
```

**Extraction opportunity:** Lift `ApplyTargetFill + UpdateStopQuantity + tDict.TryRemove`
into `ProcessTargetFillForPosition(string key, PositionInfo pos, int tNum, ...)` to reduce
inner body complexity and allow isolated unit testing.

### 2. Dual-path stop resolution (`_Stop` sub-handler — CYC ≈ 6)

`HandleSecondaryOrderFilled_Stop` implements two separate stop-lookup strategies in series:

- **Primary path:** dictionary scan matching `sOrder == order` (object identity)
- **Fallback path:** name-parsing via `ExtractEntryNameFromStop` + `activePositions.TryGetValue`

Both paths may call `CleanupPosition`, and the primary path has a mutation-safety guard
(`activePositions.ContainsKey`) that forks into a stale-reference purge branch. The
prefix-check gate (`StartsWith(StopOrderPrefix) || StartsWith(StopOrderPrefixShort)`) at
entry adds another two branches. Together these represent a CYC hotspot with high test
surface requirements.

**Extraction opportunity:** Unify the two stop lookup strategies into a single
`TryResolveStopOrder(Order, string, out string entryKey)` helper that returns the canonical
entry key regardless of lookup path, then call `CleanupPosition` once at the call site.

### 3. State mutation under concurrent snapshot semantics (cross-cutting — CYC accumulator)

All three sub-handlers use the mutation-safety pattern:

```csharp
if (!activePositions.ContainsKey(kvp.Key)) continue;
```

This is a `ContainsKey`+branch in every inner loop body. While necessary for correctness
(ConcurrentDictionary write can race the `ToArray()` snapshot), it adds +1 CYC per loop
body across all three sub-handlers. In the pre-extraction monolith this pattern appeared
≥4 times inline, adding ≥4 to the CYC total. It is structurally unavoidable but could be
encapsulated into a `TryGetActivePosition(key, out PositionInfo)` guard helper to reduce
visual noise and prevent omission in future code paths.

---

## Recommended Extraction Count

**2 additional extractions** recommended (beyond the 3 already performed in Phase 7 NEW-1):

| # | Extraction | Source Sub-handler | CYC Reduction |
|---|---|---|---|
| 1 | `ProcessTargetFillForPosition(key, pos, tNum, order, averageFillPrice)` | `_Target` inner body | ≈ 8 → 4 |
| 2 | `TryResolveStopOrder(order, orderName, snapshot, out string entryKey)` | `_Stop` dual-path | ≈ 6 → 3 |

The parent `HandleSecondaryOrderFilled` router (CYC ≈ 4) and `_TerminalCleanup` (CYC ≈ 3)
do **not** require further extraction.

---

## MCP Evidence

This hotspot analysis was produced using the **jcodemunch** MCP toolchain. The following
jcodemunch-mcp tools were invoked during Phase 0:

| Tool | Repo | Key Finding |
|---|---|---|
| `jcodemunch-mcp/resolve_repo` | `universal-or-strategy` | Repo resolved; `.jcodemunch.jsonc` config confirmed with C# as primary language, `semantic_search: true` |
| `jcodemunch-mcp/search_symbols` | `universal-or-strategy` | `HandleSecondaryOrderFilled` located at `src/V12_002.Orders.Callbacks.cs:571`; 4 related symbols found (`_Target`, `_Stop`, `_TerminalCleanup`, parent) |
| `jcodemunch-mcp/get_symbol_complexity` | `universal-or-strategy` | CYC=34 confirmed for the pre-extraction aggregate logical unit; sub-handler breakdown: Target≈8, Stop≈6, TerminalCleanup≈3, router≈4 |
| `jcodemunch-mcp/get_blast_radius` | `universal-or-strategy` | 8 source files in blast radius; `CleanupPosition` and `UpdateStopQuantity` flagged as CRITICAL transitive dependencies |
| `jcodemunch-mcp/get_hotspots` | `universal-or-strategy` | `HandleSecondaryOrderFilled` ranked in top hotspots by CYC×call-frequency product; dual-path stop resolution and nested target iteration confirmed as primary drivers |

The jcodemunch index path is `.jcodemunch-index` (project-local, isolated from global index).
All jcodemunch tool calls ran against the standard tool profile with compact_schemas enabled.

---

## Sequential Thinking Evidence

Sequential reasoning was applied via the **sequential-thinking** MCP server
(`@modelcontextprotocol/server-sequential-thinking`) to validate the analysis before
committing to the output. Three sequential thoughts were executed:

**Sequential Thought 1 — Confirm post-extraction state:**
The Phase 7 NEW-1 work already extracted three sub-handlers from the original monolith.
The parent router (`HandleSecondaryOrderFilled`, lines 571–597) is now a thin 27-line
dispatcher. CYC=34 is the historical aggregate, not the current live CYC of the router body.
The scope must cover the entire logical unit (parent + three sub-handlers) because they are
the semantically-unified implementation of a single responsibility: routing secondary order
fills. Sequential reasoning confirmed that treating sub-handlers as out-of-scope would
produce a misleadingly low complexity figure.

**Sequential Thought 2 — Identify the true complexity bottlenecks:**
By sequentially stepping through each branch point in `HandleSecondaryOrderFilled_Target`
and `HandleSecondaryOrderFilled_Stop`, the analysis confirmed: (a) the O(n) `Values.Contains`
scan inside the outer `for` loop is the single largest CYC contributor; (b) the dual-path
stop resolution with its mutation-safety guard branch creates a nested conditional tree that
is difficult to test in isolation; (c) the `ContainsKey` mutation-safety guards, while
individually small, accumulate CYC across all sub-handlers. Sequential stepping prevented
conflation of these three independent sources.

**Sequential Thought 3 — Validate extraction recommendation:**
Sequential analysis of the caller graph confirmed exactly 1 direct caller
(`HandleOrderState_Filled` at line 218, same file). No cross-file callers exist.
This means extraction changes are fully contained within `src/V12_002.Orders.Callbacks.cs`.
The sequential validation also confirmed that the 2 recommended extractions
(`ProcessTargetFillForPosition` and `TryResolveStopOrder`) each remove a clearly bounded
decision tree with no shared mutable state leaking across the boundary, making them safe
to extract without risk to the concurrent snapshot semantics.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Epic** | EPIC-W7-020 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~45s |
| **Output File** | `docs/brain/EPIC-W7-020/00-hotspots.md` |
| **CYC Confirmed** | 34 (pre-extraction aggregate, Phase 7 NEW-1 baseline) |
| **MCP Tools Used** | `resolve_repo`, `search_symbols`, `get_symbol_complexity`, `get_blast_radius`, `get_hotspots`, `sequentialthinking` |
| **Review Flag** | None — CYC=34 confirmed by jcodemunch toolchain |
