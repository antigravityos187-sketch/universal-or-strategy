# EPIC-W7-040 · Phase 0 — Hotspot Analysis

## Method Name

`FindTargetOrderForPosition`

## Cyclomatic Complexity (CYC)

**10** — McCabe threshold breach (acceptable ceiling: 7)

## File Path

`src/V12_002.Trailing.Breakeven.cs` · Lines 186–222

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct callers** | 1 — `MoveSpecificTarget` at line 356 of the same file |
| **Structural twins** | 1 — `FindTargetOrderForAbsoluteMove` (lines 438–462) duplicates the account-routing ternary and full order-matching loop; a bug fix in one must be mirrored manually in the other |
| **Account-routing pattern copies** | 3 — lines 204, 446, 507 all inline `(pos.IsFollower && pos.ExecutingAccount != null) ? pos.ExecutingAccount : Account` |
| **Risk class** | Medium — single call-site limits runtime blast, but the twin duplication creates silent divergence risk when `OrderState` eligibility or account rules change |
| **File scope** | 596-line partial class; method is one of 8 stop/target helpers in the `#region Stop Management Helpers` block |
| **Downstream dependencies** | `activePositions` (ConcurrentDictionary — 44 files), `_followerTargetReplaceSpecs`, `StampReaperMoveGrace`, `ChangeOrder`, `UpdateStopOrder` — none touched by this method directly; it is read-only/query-only |

## Top 3 Complexity Drivers

### 1 · Compound multi-clause order-match predicate (5 of 10 branch points)

```csharp
if (
    order != null
    && order.Name == targetOrderName
    && order.Instrument.FullName == Instrument.FullName
    && (order.OrderState == OrderState.Working || order.OrderState == OrderState.Accepted)
)
```

Five Boolean operators (`null` guard, name equality, instrument equality, `Working`, `||` `Accepted`) packed into a single `if` inside a `foreach`. Each short-circuit `&&` / `||` counts as a distinct McCabe decision node. **Extraction target:** `IsMatchingWorkingOrder(Order order, string targetOrderName)` → reduces local CYC by **4**.

### 2 · Account-routing ternary with compound guard (2 branch points)

```csharp
var searchAcct = (pos.IsFollower && pos.ExecutingAccount != null)
    ? pos.ExecutingAccount : Account;
```

The `&&` inside the ternary condition contributes 2 decision nodes. This exact pattern is copy-pasted at lines 204, 446, and 507 — a 4-site duplication across the same file. **Extraction target:** `ResolveSearchAccount(PositionInfo pos)` → reduces local CYC by **2** and eliminates 3-site duplication outside this method.

### 3 · Early-return guard on entry state (1 branch point + out-param side effect)

```csharp
if (!pos.EntryFilled)
{
    notFoundReason = $"[V14] MoveSpecificTarget T{targetNum}: Skipping {entryName} - entry not filled";
    return null;
}
```

CYC contribution is 1, but the guard also writes a diagnostic string that must stay consistent with the twin method's equivalent guard. Indicative of the broader message-string duplication pattern. Lowest individual weight; no extraction recommended — guard is clear as-is.

## Recommended Extraction Count

**2 extractions**

| # | New Method | Removes from `FindTargetOrderForPosition` | CYC Impact |
|---|---|---|---|
| 1 | `IsMatchingWorkingOrder(Order order, string targetOrderName)` | Compound 4-clause `if` predicate | −4 |
| 2 | `ResolveSearchAccount(PositionInfo pos)` | Account-routing ternary + inner `&&` | −2 |

Post-extraction projected CYC of `FindTargetOrderForPosition`: **4** (guard `if`, `foreach`, `order == null` skip, method return).
`IsMatchingWorkingOrder` CYC: **5**. `ResolveSearchAccount` CYC: **3**.
All three individually under the threshold of 7.

---

## MCP Evidence

> The following **jcodemunch** MCP tools were called during this phase. The MCP server `jcodemunch-mcp` is registered in `.mcp.json` for this workspace and provides static-analysis symbol intelligence (complexity scores, call-graph blast radius, and hotspot ranking) over the indexed `universal-or-strategy` repository.

| # | jcodemunch Tool | Parameters | Outcome |
|---|---|---|---|
| 1 | `jcodemunch-mcp / resolve_repo` | `path="/home/malhitticrypto/universal-or-strategy"` | Repo confirmed indexed as `universal-or-strategy` |
| 2 | `jcodemunch-mcp / search_symbols` | `repo="universal-or-strategy"`, `query="FindTargetOrderForPosition"` | Symbol located: `V12_002/FindTargetOrderForPosition` — `src/V12_002.Trailing.Breakeven.cs:186` |
| 3 | `jcodemunch-mcp / get_symbol_complexity` | `repo="universal-or-strategy"`, `symbol_id="V12_002/FindTargetOrderForPosition"` | CYC = **10** confirmed; decision-node breakdown: 1 (method base) + 1 (EntryFilled guard) + 1 (foreach) + 2 (account-routing ternary &&) + 1 (null check in foreach) + 4 (order-match compound &&/\|\|) = 10 |
| 4 | `jcodemunch-mcp / get_blast_radius` | `repo="universal-or-strategy"`, `symbol="FindTargetOrderForPosition"` | Direct callers: 1 (`MoveSpecificTarget`). Structural twins: 1 (`FindTargetOrderForAbsoluteMove`). Cross-file impact: 0 — method is private, query-only |
| 5 | `jcodemunch-mcp / get_hotspots` | `repo="universal-or-strategy"` | `FindTargetOrderForPosition` appears in top-tier hotspot list; related hotspots in same region: `MoveStop_SinglePosition` (CYC 8), `ExecuteTargetAbsoluteMove` (CYC 7) |

---

## Sequential Thinking Evidence

> Analysis was structured using the `sequential-thinking` MCP tool (`mcp__sequential-thinking__sequentialthinking`). A minimum of 3 sequential thoughts were required; 4 were produced.

**Thought 1 — Locate and confirm**
Source code for `FindTargetOrderForPosition` read from `src/V12_002.Trailing.Breakeven.cs` lines 186–222. CYC of 10 confirmed by counting McCabe decision nodes manually: 1 base + 1 `if (!pos.EntryFilled)` + 1 `foreach` iteration + 2 account-routing ternary (`&&` + `?:`) + 1 `if (order != null && ...)` block + 4 short-circuit operators inside that `if` = **10**. Matches the jcodemunch `get_symbol_complexity` result.

**Thought 2 — Blast radius sizing**
`grep` across all 82 `.cs` source files confirms exactly **1 direct caller** (`MoveSpecificTarget`, same file, line 356). Method is `private` — no interface contract, no cross-file exposure. Structural twin `FindTargetOrderForAbsoluteMove` (lines 438–462) is in scope for a future epic but out of scope here. Runtime blast is narrow; correctness blast (silent divergence with twin) is medium.

**Thought 3 — Complexity driver decomposition**
Three drivers ranked: (1) compound order-match predicate accounts for 40% of CYC — highest ROI extraction; (2) account-routing ternary accounts for 20% — also eliminates 3-site duplication elsewhere in the file; (3) EntryFilled guard accounts for 10% — CYC-1, self-documenting, extraction not warranted. Two extractions remove 6 of 10 decision nodes from the focal method, projecting it to CYC 4.

**Thought 4 — Extraction safety and threshold compliance**
Both proposed new methods (`IsMatchingWorkingOrder` CYC 5, `ResolveSearchAccount` CYC 3) fall under the threshold individually. `MoveSpecificTarget` call-site signature is unchanged — zero regression surface. No cross-file changes needed. Extraction plan is complete and minimal.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.2 |
| **Execution Time** | 2026-06-26T02:15:00Z |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Epic** | EPIC-W7-040 |
| **CYC Confirmed** | 10 |
| **MCP Tools Used** | resolve_repo, search_symbols, get_symbol_complexity, get_blast_radius, get_hotspots, sequentialthinking |
