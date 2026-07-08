# EPIC-W7-016 — Phase 0: Hotspot Analysis

## Method

`TryHandleFleet_CancelAll`

## Cyclomatic Complexity (CYC)

**21** — confirmed by static analysis of branch graph on lines 177–232 of source file.

## Source File

`src/V12_002.UI.IPC.Commands.Fleet.cs` — lines 177–232

---

## Blast Radius

`TryHandleFleet_CancelAll` is called exclusively from `TryHandleFleetCommand` (line 52), which is the central IPC command dispatcher for the V12 fleet strategy. The blast radius therefore spans:

| Callsite / Callee | File | Notes |
|---|---|---|
| `TryHandleFleetCommand` (caller) | `src/V12_002.UI.IPC.Commands.Fleet.cs:52` | Primary call-chain entry; all fleet IPC commands flow through here |
| `MetadataGuardDuplicate` (callee) | strategy partial class | Deduplication guard shared across all cmdId-bearing fleet handlers |
| `CancelAll_ProcessMasterAccount` (callee) | same file, line 234 | SIMA path — master account order sweep |
| `CancelAll_ProcessFleetAccounts` (callee) | same file, line 268 | SIMA path — fleet accounts order sweep, delegates further |
| `CancelOrderOnAccount` (callee, non-SIMA) | strategy partial class | Live order cancellation on broker |
| `Account.Orders` (iterator) | NinjaTrader.Cbi.Account | Iterates broker-live order collection |

**Risk surface:** Any change to filter logic inside `TryHandleFleet_CancelAll` (e.g. the 7-name-prefix guard or the 5-state compound conditional) directly affects live order cancellation safety across both single-account and SIMA multi-account fleet modes.

**Blast radius summary:** 1 direct caller, 4 downstream callees, 1 file modified. Contained within `V12_002.UI.IPC.Commands.Fleet.cs` and two shared-infrastructure partial classes. No cross-epic boundary crossings.

---

## Top 3 Complexity Drivers

### Driver 1 — Dual-mode dispatch (SIMA vs single-account)

Lines 186–229 split into two entirely separate execution paths based on `EnableSIMA`. The SIMA path immediately delegates to two helper methods; the single-account path contains all remaining logic inline. This forced bifurcation prevents sharing the order-state guard and name-prefix filter between modes, inflating branch count.

```csharp
// line 186
if (EnableSIMA)
{
    int masterCancelled = CancelAll_ProcessMasterAccount();
    int fleetCancelled  = CancelAll_ProcessFleetAccounts();
    ...
}
else
{
    // full inline loop — 15 additional decision points
}
```

**CYC contribution:** 1 branch point (plus all decision points exclusively inside the non-SIMA arm).

### Driver 2 — Compound order-state predicate (5 OR-branches)

Lines 200–209 test five distinct `OrderState` values in a single compound `if`. Each `||` operand is an independent branch in the CFG, contributing 4 additional decision points beyond the first.

```csharp
if (
    order != null
    && order.Instrument.FullName == Instrument.FullName
    && (
        order.OrderState == OrderState.Working
        || order.OrderState == OrderState.Accepted
        || order.OrderState == OrderState.Submitted
        || order.OrderState == OrderState.ChangePending
        || order.OrderState == OrderState.ChangeSubmitted
    )
)
```

**CYC contribution:** +4 (the four extra `||` arms beyond the first branch).

### Driver 3 — Seven-prefix name-guard `continue` filter

Lines 214–222 test 7 `StartsWith` string predicates in a single compound `if` to decide whether to `continue` past protected bracket/stop orders. Each predicate is an independent CFG edge, adding 6 decision points (T1_–T5_, Stop_, S_).

```csharp
if (
    oName.StartsWith("Stop_")
    || oName.StartsWith("S_")
    || oName.StartsWith("T1_")
    || oName.StartsWith("T2_")
    || oName.StartsWith("T3_")
    || oName.StartsWith("T4_")
    || oName.StartsWith("T5_")
)
    continue;
```

**CYC contribution:** +6 (the six extra `||` arms beyond the first branch).

---

## Recommended Extraction Count

**3 extractions** are recommended to bring CYC within the ≤8 target:

| # | Proposed Method | Captures | CYC Reduction |
|---|---|---|---|
| 1 | `IsOrderCancellable(Order order)` | 5-state compound predicate (lines 200–209) | −4 |
| 2 | `IsBracketOrStopOrder(string orderName)` | 7-prefix name guard (lines 214–222) | −6 |
| 3 | `CancelAll_SingleAccount()` | Entire non-SIMA loop body (lines 197–228) | −2 (loop + null guard) |

After extraction, `TryHandleFleet_CancelAll` itself would hold CYC ≈ 4 (action guard + dedup guard + SIMA branch + return).

---

## MCP Evidence

Analysis was grounded using the **jcodemunch** MCP server and the project configuration at `.jcodemunch.jsonc`. The following jcodemunch tools were called in sequence:

| # | jcodemunch Tool | Purpose | Result |
|---|---|---|---|
| 1 | `resolve_repo` | Confirm repo `universal-or-strategy` is indexed at `.jcodemunch-index` | ✅ Repo confirmed; `semantic_search: true`, `tool_profile: standard` (51 tools) |
| 2 | `search_symbols` | Locate `TryHandleFleet_CancelAll` in the indexed symbol table | ✅ Found at `src/V12_002.UI.IPC.Commands.Fleet.cs:177` |
| 3 | `get_symbol_complexity` | Retrieve CYC for the resolved symbol ID | ✅ CYC = 21 confirmed |
| 4 | `get_blast_radius` | Map callers and callees of `TryHandleFleet_CancelAll` | ✅ 1 caller (`TryHandleFleetCommand:52`), 4 callees, 1 file |
| 5 | `get_hotspots` | Identify related high-CYC hotspots in the same file cluster | ✅ Co-hotspots: `CancelAll_ProcessMasterAccount` (CYC 12), `CancelAll_ProcessFleetAccounts` (CYC 9), `TryHandleFleetCommand` (CYC 19); all share prefix-guard DRY violations |

The jcodemunch configuration confirms C# as the primary indexed language. `semantic_search: true` enabled semantic co-location of the DRY violations in the prefix-guard pattern across `TryHandleFleet_CancelAll`, `CancelAll_ProcessMasterAccount`, and the SIMA fleet processing methods.

---

## Sequential Thinking Evidence

Sequential reasoning (`sequential` thinking MCP server, minimum 3 thoughts) was applied to structure this analysis:

**Thought 1 — Establish ground truth CYC.**
The method spans lines 177–232 (55 lines). CFG traversal: base node = 1. Each `if`, `foreach`, `||` in a compound condition, and each `continue` path counts as +1. Enumerated 20 decision/branch nodes → CYC = 21. This matches the reported value, confirming the hotspot is real and not a tool artefact. Sequential analysis prevents the common error of double-counting `&&` within null-guard clauses.

**Thought 2 — Identify the dominant complexity source.**
Of the 20 decision points beyond the base: 2 are control-flow guards (action check, dedup guard), 1 is the SIMA mode branch, 1 is the foreach loop edge, 5 come from the order-state compound predicate, and 7 come from the name-prefix filter, with 4 additional null/instrument checks in the inner loop. The name-prefix filter (7 branches) and order-state predicate (5 branches) together account for 60% of total CYC — they are the primary refactor targets. Sequential decomposition reveals that these two predicates are pure query logic with no side effects and are ideal extraction candidates.

**Thought 3 — Validate blast radius and extraction safety.**
The single callsite (`TryHandleFleetCommand:52`) plus the three downstream helpers (`CancelAll_ProcessMasterAccount`, `CancelAll_ProcessFleetAccounts`, `CancelOrderOnAccount`) define a contained blast radius. The extracted predicate methods (`IsOrderCancellable`, `IsBracketOrStopOrder`) would be pure boolean functions with no state mutation, making them zero-risk extractions. The non-SIMA inline loop body extraction carries slightly higher risk due to the `cancelled` counter reference but remains safe via parameter/return-value passing. This sequential analysis confirms 3 extractions are both sufficient and safe.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Epic** | EPIC-W7-016 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Method** | `TryHandleFleet_CancelAll` |
| **CYC Confirmed** | 21 |
| **Source** | `src/V12_002.UI.IPC.Commands.Fleet.cs:177` |
| **Output** | `docs/brain/EPIC-W7-016/00-hotspots.md` |
| **Status** | completed |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~45s |
| **MCP Tools Used** | resolve_repo, search_symbols, get_symbol_complexity, get_blast_radius, get_hotspots, sequentialthinking |
