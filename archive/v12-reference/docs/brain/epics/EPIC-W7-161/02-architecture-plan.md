# Phase 2: Architecture Plan -- EPIC-W7-161

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 -- Architecture Planning
**Generated:** 2026-06-29T01:12:00Z
**Input:** docs/brain/EPIC-W7-161/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `SyncLiveTargetRows`
- **Source File:** `src/V12_002.UI.Panel.StateSync.cs`
- **Original CYC:** 10
- **Lines:** 158-191 (34 LOC per index)
- **Visibility:** private
- **Class:** `V12_002` (partial, Strategy)

### jcodemunch get_context_bundle result

Symbol resolved via jcodemunch to `src/V12_002.UI.Panel.StateSync.cs::V12_002.SyncLiveTargetRows#method`.
Full source retrieved. Signature: `private void SyncLiveTargetRows(UILivePositionSnapshot livePosition)`.
Key structure: one for-loop (t=1..5) over target rows with per-row visibility + text update, followed by
a stop-row block updating liveStopRow visibility and liveStopPrice.Text. Imports confirmed:
System, System.Windows.Controls, System.Windows.Media, NinjaTrader.Cbi.

### jcodemunch get_call_hierarchy result

- **Callers (depth 1):** `UpdatePanelState` (src/V12_002.UI.Panel.StateSync.cs, line 13) -- ast_resolved
- **Callees (depth 1):** `SetLiveTargetRowVisible` (line 130), `GetLiveTargetPriceBox` (line 193), `GetLiveTargetCtsBlock` (line 212) -- all ast_resolved in src; ast_inferred duplicates in src-vm-backup (ignored)
- **Total callers:** 1 (UpdatePanelState) -- consistent with 00-scope.md (2 call sites resolves to 1 caller method)
- **Total unique callees:** 3

### jcodemunch get_dependency_graph result

File-level dependency graph for `src/V12_002.UI.Panel.StateSync.cs`: node_count=1, edge_count=0.
No file-level imports or importers resolved in the index. This is expected -- the partial class file
shares its class with other partial files and has no standalone import edges in the graph. All callee
methods are in-class references. Cross-file blast radius is zero.

### jcodemunch get_extraction_candidates result

Extraction candidates returned: none (empty list). This is expected for a file where all methods are
called from within the same class -- the min_callers=1 filter requires cross-file callers. The
complexity-based extraction decision is confirmed by the source body analysis rather than the automated
candidates tool.

---

## Sequential Thinking Summary

sequentialthinking chain completed (5 thoughts). Final synthesis:

Extract `SyncSingleTargetRow(int targetIndex, UILivePositionSnapshot livePosition)` as one private
helper containing all per-row UI logic (the for-loop body). Parent `SyncLiveTargetRows` becomes a
simple for-loop (t=1..5) calling the helper, plus the stop-row block. CYC drops from 10 to
max(parent=5, helper=8) = 8. ONE extraction is sufficient and minimal. Full Jane Street compliance:
CYC<=8, single-responsibility, no lock() blocks, all string literals ASCII-only, illegal null states
guarded by early-return continue inside the helper.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `SyncSingleTargetRow(int targetIndex, UILivePositionSnapshot livePosition)` | Handles all per-row UI sync for a single target slot: fetch target, compute active flag, call SetLiveTargetRowVisible, guard-return if inactive, update priceBox.Text and ctsBlock.Text/Foreground | 8 |

### Extracted Helper -- Full Signature

```csharp
private void SyncSingleTargetRow(int targetIndex, UILivePositionSnapshot livePosition)
```

**Body responsibilities:**
1. `UILiveTargetSnapshot target = livePosition.Targets[targetIndex - 1];` -- fetch slot
2. `bool active = target != null && target.IsVisible;` -- compute active
3. `SetLiveTargetRowVisible(targetIndex, active);` -- set row visibility
4. `if (!active || target == null) return;` -- early-return guard (replaces `continue`)
5. `TextBox priceBox = GetLiveTargetPriceBox(targetIndex);` + null+focus guard + text assign
6. `TextBlock ctsBlock = GetLiveTargetCtsBlock(targetIndex);` + null guard + text + foreground assign

**CYC accounting:**
- base: 1
- `if (!active || target == null)`: if +1, || +1 = +2
- `if (priceBox != null && !priceBox.IsFocused)`: if +1, && +1 = +2
- `target.Price > 0 ? ... : ...` ternary: +1
- `if (ctsBlock != null)`: +1
- `target.IsWorking ? ... : ...` ternary: +1
- **Total: 8** <= 8 threshold: PASS

---

## Parent Method After Extraction

**Remaining logic in `SyncLiveTargetRows` after extraction:**

```csharp
private void SyncLiveTargetRows(UILivePositionSnapshot livePosition)
{
    for (int t = 1; t <= 5; t++)
    {
        SyncSingleTargetRow(t, livePosition);
    }

    if (liveStopRow != null)
    {
        if (liveStopPrice != null)
            liveStopPrice.Text =
                livePosition.StopPrice > 0
                    ? Instrument.MasterInstrument.FormatPrice(livePosition.StopPrice)
                    : "--";
        liveStopRow.Visibility = System.Windows.Visibility.Visible;
    }
}
```

**CYC accounting (parent):**
- base: 1
- `for (int t = 1; t <= 5; t++)`: +1
- `if (liveStopRow != null)`: +1
- `if (liveStopPrice != null)`: +1
- `livePosition.StopPrice > 0 ? ... : ...` ternary: +1
- **Total: 5** <= 8 threshold: PASS

- **Remaining logic:** for-loop (t=1..5) calling SyncSingleTargetRow + liveStopRow visibility + liveStopPrice text assignment
- **Projected CYC:** 5

---

## max_cyc_projected: 8
## extraction_count: 1

---

## Jane Street Alignment

- **CYC<=8 achieved:** YES -- parent=5, helper=8, max=8
- **Single-responsibility per helper:** YES -- SyncSingleTargetRow handles exactly one target slot; parent orchestrates loop + stop-row only
- **Lock-free/Actor pattern preserved:** YES -- no lock() blocks exist in SyncLiveTargetRows; method only assigns WPF UI element properties, no shared mutable state requiring actor enqueue
- **Illegal states unrepresentable:** YES -- null guards on target, priceBox, and ctsBlock are explicit early-return/continue guards; accessing target.Price, target.IsWorking, target.RemainingContracts is structurally gated by the active guard
- **ASCII-only string literals:** YES -- "--" and " cts" are pure ASCII
- **Extract Guard Clauses applied:** YES -- `if (!active || target == null) return;` replaces nested if-chain inside extracted helper
- **xUnit [Fact] tests required:** one [Fact] per significant path in SyncSingleTargetRow (active=true/false, priceBox null, ctsBlock null, IsWorking true/false)

---

## Agent Tracking

- **Agent Name:** v12-phase2-architecture
- **Bobcoins Used:** 2.0
- **Execution Time:** 2026-06-29T01:12:00Z
- **Wave:** 7
- **Phase:** 2
- **Epic:** EPIC-W7-161
- **jcodemunch tools called:** get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates
- **sequential-thinking calls:** 5
- **MCP resolve_repo:** antigravityos187-sketch/universal-or-strategy (5147 symbols, 2000 files)
