# EPIC-W7-123 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:15:00Z
**Input:** docs/brain/EPIC-W7-123/01-scope-boundary.md

---

## Target Method

| Field | Value |
|---|---|
| **Method** | `HandleMatchedFollowerOrder` |
| **File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **Line** | 472 |
| **Original CYC** | 14 |
| **Target CYC** | ≤ 8 |
| **Caller Count** | 3 (unchanged) |
| **Scope** | Private helpers, same partial class |

---

## Original Method Signature

```csharp
private void HandleMatchedFollowerOrder(
    string matchedEntry,
    PositionInfo matchedPos,
    Order order,
    string acctName,
    string reason
)
```

---

## CYC=14 Decomposition Analysis

The method contains the following branching contributions (from Sequential Thinking Thought 1):

| Branch Source | CYC Contribution |
|---|---|
| Base (method entry) | 1 |
| `ProcessFollowerCancellationSafe` early-return gate | 1 |
| Outer `if` compound condition (TryGetValue + null + OrderId + !EntryFilled) | 4 |
| LINQ `.Any` on `_followerBrackets` (null + AccountName + State Active + State Accepted) | 4 |
| Meta-purge guard nested `if` (TryGetValue + State==PendingCancel + CancellingOrderId) | 3 |
| Else branch (terminal path) | 1 |
| **Total** | **14** |

The method conflates 5 distinct responsibilities into a single body:
1. Entry-order identity resolution (compound 3-part condition)
2. FSM bracket active-state query (LINQ over `_followerBrackets`)
3. Meta-purge guard / spec-rescue decision (Build 973 / Build 947 logic)
4. Rollback + desync label render for cancelled unfilled entry
5. Terminal order ghost-reference cleanup

---

## Extraction Plan

### Helper 1 — `IsEntryOrderMatch`

```csharp
private bool IsEntryOrderMatch(
    string matchedEntry,
    PositionInfo matchedPos,
    Order order,
    out Order entryOrder
)
```

**Responsibility:** Encapsulates the compound entry-order identity check. Returns `true` iff
`entryOrders` contains the key, the stored order matches by reference or OrderId, and the
position has not yet been filled.

**Extracted Logic:**
```csharp
return entryOrders.TryGetValue(matchedEntry, out entryOrder)
    && (entryOrder == order || (entryOrder != null && entryOrder.OrderId == order.OrderId))
    && !matchedPos.EntryFilled;
```

| Field | Value |
|---|---|
| **Projected CYC** | 5 |
| **Jane Street** | `carl_cook`: hot-path zero-alloc predicate → `[AggressiveInlining]` candidate |
| **Jane Street** | `trading_billions`: single responsibility — identity resolution only |

---

### Helper 2 — `IsAnyFollowerBracketActive`

```csharp
private bool IsAnyFollowerBracketActive(string acctName)
```

**Responsibility:** Queries `_followerBrackets` to detect whether any bracket for the given
account is in `Active` or `Accepted` FSM state. Pure read predicate, zero side effects.

**Extracted Logic:**
```csharp
return _followerBrackets.Values.Any(f =>
    f != null
    && f.AccountName == acctName
    && (f.State == FollowerBracketState.Active || f.State == FollowerBracketState.Accepted)
);
```

| Field | Value |
|---|---|
| **Projected CYC** | 5 |
| **Jane Street** | `carl_cook`: hot-path LINQ predicate, `ConcurrentDictionary` ValueEnumerator is allocation-free → `[AggressiveInlining]` candidate |
| **Jane Street** | `gjengset`: pure read over `ConcurrentDictionary` — no false sharing, no cache-line ping-pong |

---

### Helper 3 — `ShouldRescuePendingCancelSpec`

```csharp
private bool ShouldRescuePendingCancelSpec(string matchedEntry, Order order)
```

**Responsibility:** Encapsulates the Build 973 / Build 947 meta-purge guard. Returns `true`
if a `PendingCancel` spec exists for this exact order and should fall through (rescued). On
the `false` path, eagerly removes orphaned `_followerReplaceSpecs` entries (Build 947 cleanup).

**Extracted Logic:**
```csharp
if (_followerReplaceSpecs.TryGetValue(matchedEntry, out var fsmGuard)
    && fsmGuard.State == FollowerReplaceState.PendingCancel
    && fsmGuard.CancellingOrderId == order.OrderId)
{
    Print("[META-PURGE GUARD] Rescuing PendingCancel spec " + matchedEntry
        + " despite no active FSM. Delegating to resubmit path.");
    return true;
}
_followerReplaceSpecs.TryRemove(matchedEntry, out _);
return false;
```

| Field | Value |
|---|---|
| **Projected CYC** | 4 |
| **Jane Street** | `trading_billions`: defense in depth — spec-level guard distinct from bracket-level guard |
| **Jane Street** | `trading_billions`: single responsibility — rescue/purge decision only |

---

### Helper 4 — `HandleEntryNotFilledRollback`

```csharp
private void HandleEntryNotFilledRollback(string matchedEntry, string acctName)
```

**Responsibility:** Performs the rollback and desync label for a cancelled unfilled entry.
Calls `HandleMatchedFollower_DeltaRollback`, emits a `Print` log, and renders the
`Draw.TextFixed` desync UI label. Cold path — UI operations present.

**Extracted Logic:**
```csharp
HandleMatchedFollower_DeltaRollback(matchedEntry);
Print(string.Format(
    "[SIMA] Follower entry cancelled: {0} on {1}. Reaper monitoring.",
    matchedEntry, acctName));
Draw.TextFixed(
    this,
    "SIMA_DESYNC_" + acctName,
    "(!) FOLLOWER DESYNC: " + acctName,
    TextPosition.TopLeft,
    Brushes.Red,
    new SimpleFont("Arial", 11),
    Brushes.Transparent,
    Brushes.Transparent,
    50);
```

| Field | Value |
|---|---|
| **Projected CYC** | 1 |
| **Jane Street** | `carl_cook`: cold-path extraction — `Draw.TextFixed` is UI; `[NoInlining]` candidate |
| **Jane Street** | `trading_billions`: single responsibility — rollback + visual alert only |

---

### Helper 5 — `HandleTerminalFollowerOrder`

```csharp
private void HandleTerminalFollowerOrder(Order order, string acctName, string reason)
```

**Responsibility:** Handles the else-branch for terminal (stop/target) orders already
processed by the top-level cancellation gate. Emits a terminal-state log and removes ghost
order references. Cold path — logging + cleanup only.

**Extracted Logic:**
```csharp
Print(string.Format(
    "[SIMA] Follower order terminal: {0} on {1} ({2}) | Id={3}",
    order.Name, acctName, reason, order.OrderId));
RemoveGhostOrderRef(order, reason);
```

| Field | Value |
|---|---|
| **Projected CYC** | 1 |
| **Jane Street** | `carl_cook`: cold-path logging/cleanup → `[NoInlining]` candidate |
| **Jane Street** | `trading_billions`: single responsibility — terminal cleanup only |

---

## Parent Method After Extraction

```csharp
private void HandleMatchedFollowerOrder(
    string matchedEntry,
    PositionInfo matchedPos,
    Order order,
    string acctName,
    string reason
)
{
    if (ProcessFollowerCancellationSafe(matchedEntry, matchedPos, order, acctName, reason))
        return;

    if (IsEntryOrderMatch(matchedEntry, matchedPos, order, out _))
    {
        entryOrders.TryRemove(matchedEntry, out _);
        if (!IsAnyFollowerBracketActive(acctName))
        {
            if (!ShouldRescuePendingCancelSpec(matchedEntry, order))
                return;
            // fall through: rescued PendingCancel spec
        }
        HandleEntryNotFilledRollback(matchedEntry, acctName);
    }
    else
    {
        HandleTerminalFollowerOrder(order, acctName, reason);
    }
}
```

**Projected CYC of parent:** 5

---

## CYC Validation Summary

| Method | Projected CYC | ≤ 8? |
|---|---|---|
| `HandleMatchedFollowerOrder` (parent) | 5 | **PASS** |
| `IsEntryOrderMatch` | 5 | **PASS** |
| `IsAnyFollowerBracketActive` | 5 | **PASS** |
| `ShouldRescuePendingCancelSpec` | 4 | **PASS** |
| `HandleEntryNotFilledRollback` | 1 | **PASS** |
| `HandleTerminalFollowerOrder` | 1 | **PASS** |
| **max_cyc_projected** | **5** | **PASS** |

**Original CYC:** 14 → **Max projected CYC:** 5 — reduction of 9 points (64%)

---

## Jane Street Alignment Notes

### gjengset — Cache Line / False Sharing
- `IsAnyFollowerBracketActive` reads `_followerBrackets.Values` via `ConcurrentDictionary`'s
  lock-free `ValueEnumerator`. No writes during the read path.
- `ShouldRescuePendingCancelSpec` has a single conditional `TryRemove` write. This is a
  well-scoped single-writer mutation; no concurrent writes to `_followerReplaceSpecs` within
  the same call context.
- No new shared mutable state introduced by any helper.

### carl_cook — Hot-Path Zero-Alloc + InliningStrategy
- **AggressiveInlining candidates:** `IsEntryOrderMatch`, `IsAnyFollowerBracketActive`
  (pure predicates, zero heap allocation, on the decision hot-path)
- **NoInlining candidates:** `HandleEntryNotFilledRollback`, `HandleTerminalFollowerOrder`
  (cold paths: UI rendering, logging, ghost-ref cleanup)
- `ShouldRescuePendingCancelSpec`: standard inline (semi-cold error/meta path)

### trading_billions — Defense in Depth + Single Responsibility
- Three-layer defense preserved:
  1. `ProcessFollowerCancellationSafe` — cancellation gate (already extracted)
  2. `IsAnyFollowerBracketActive` — bracket-level FSM state guard
  3. `ShouldRescuePendingCancelSpec` — spec-level meta-purge guard (Build 973/947)
- Each helper has exactly one responsibility. No helper mixes query + mutation + UI.
- Build 973 / Build 947 semantics are preserved intact inside `ShouldRescuePendingCancelSpec`.

---

## MCP Evidence

### jCodemunch — resolve_repo
- Repo: `antigravityos187-sketch/universal-or-strategy`
- Indexed: true | Symbol count: 5,147 | File count: 2,000

### jCodemunch — get_context_bundle
- Symbol found at `src/V12_002.Orders.Callbacks.AccountOrders.cs:472`
- Full method body retrieved and analyzed

### jCodemunch — get_call_hierarchy
- **Callers (3):** `ProcessQueuedAccountOrder` (depth 1), `ProcessAccountOrderQueue` (depth 2),
  `ProcessAccountOrder_EnqueueTerminalUpdate` (depth 3) — all in same file; signatures unchanged
- **Callees (direct):** `ProcessFollowerCancellationSafe`, `HandleMatchedFollower_DeltaRollback`,
  `RemoveGhostOrderRef`, `entryOrders` (dict), `_followerReplaceSpecs` (dict), `_followerBrackets` (dict)

### jCodemunch — get_dependency_graph
- File has 0 cross-file imports in the indexed graph (C# partial class — imports via using directives)
- Blast radius confirmed: zero cross-file edges to break

### jCodemunch — get_extraction_candidates
- No pre-existing extraction candidates (method not yet split in index)
- Confirms fresh extraction opportunity for all 5 helpers

---

## Sequential Thinking Evidence

| Thought | Focus | Outcome |
|---|---|---|
| 1 | CYC=14 decomposition — identify 5 logical sub-concerns | Mapped 14 branches to 5 responsibilities |
| 2 | Design 5 helpers — signatures, CYC projections, Jane Street patterns | All projected CYCs ≤ 8; parent CYC = 5 |
| 3 | Validation — CYC compliance, gjengset/carl_cook/trading_billions alignment | All PASS; NoInlining/AggressiveInlining guidance confirmed |

---

## V12.23 Scope Compliance

| Check | Status |
|---|---|
| Methods touched: 1 target + 5 new helpers | PASS |
| All helpers private, same partial class | PASS |
| No caller signature changes (3 callers) | PASS |
| No cross-file changes | PASS |
| max_cyc_projected ≤ 8 | PASS |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 2.5 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **Extraction Count** | 5 |
| **max_cyc_projected** | 5 |
| **Original CYC** | 14 |
| **CYC Reduction** | 64% (14 → 5) |
