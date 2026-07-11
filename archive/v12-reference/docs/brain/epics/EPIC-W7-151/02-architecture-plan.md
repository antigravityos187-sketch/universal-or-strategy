# EPIC-W7-151 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-151/01-scope-boundary.md

---

## Resolved Target

| Field | Value |
|---|---|
| **Method** | `IsOrderAllowed` |
| **File** | `src/V12_002.UI.Compliance.cs` |
| **Lines** | 323–389 |
| **Signature** | `private bool IsOrderAllowed(string? accountName = null)` |
| **CYC (current)** | 9 |
| **CYC (target)** | ≤ 8 |
| **CYC over threshold** | 1 |
| **Risk level** | HIGH — hard compliance/safety gate |

---

## Complexity Analysis

`IsOrderAllowed` is the compliance enforcement gate called before every order submission.
CYC = 9 is composed of two tightly clustered rule blocks:

### Decision Points (9 total)

| # | Branch | Location | CYC contribution |
|---|---|---|---|
| 1 | `if (!EnableComplianceHub) return true` | line ~324 | +1 |
| 2 | `if (string.IsNullOrEmpty(acctName)) return true` | line ~328 | +1 |
| 3 | `TryGetValue(...) && peak > 0 && TrailingDrawdownLimit > 0` | line ~332 | +3 (compound &&) |
| 4 | `if (currentAccount != null)` | line ~336 | +1 |
| 5 | `try / catch` exception path | line ~339 | +1 |
| 6 | `if (buffer <= 0)` hard-block gate | line ~354 | +1 |
| 7 | `if (EnableSIMA && EnableConsistencyLock)` | line ~362 | +2 (compound &&) |
| 8-9 | `TryGetValue(...) && MaxDailyProfitCap > 0 && dp >= MaxDailyProfitCap` | line ~364 | counts within cluster B total |

**Base CYC:** 1 + 8 decision branches = 9

### Two Complexity Clusters

**Cluster A — Trailing Drawdown Rule (lines ~332–359): ~5 CYC**
- TryGetValue compound condition + null guard + try/catch + buffer check
- Contains the live broker `Account.Get()` call with exception path

**Cluster B — Daily Profit Cap Rule (lines ~361–376): ~3 CYC**
- SIMA+ConsistencyLock outer gate + inner TryGetValue compound condition

**Header (lines ~324–330): 2 CYC** — feature-flag short-circuit + null guard — stays in parent.

---

## Extraction Plan

### 2 Extractions Required

#### Extraction 1: `IsTrailingDrawdownAllowed`

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private bool IsTrailingDrawdownAllowed(string acctName)
```

**Moves from `IsOrderAllowed`:**
- `if (accountEquityPeak.TryGetValue(acctName, out double peak) && peak > 0 && TrailingDrawdownLimit > 0)` compound gate
- `Account currentAccount = this.Account; if (currentAccount != null)` null guard
- `try { balance = currentAccount.Get(...); } catch (Exception ex) { Interlocked.Increment(ref _uiCallbackFailures); Print(...); }` broker call + exception handling
- `double buffer = balance - (peak - TrailingDrawdownLimit); if (buffer <= 0) { Print(...); return false; }` buffer check + violation log

**Returns:** `true` if order is allowed (drawdown NOT breached), `false` if hard-blocked.

**Projected CYC:** 7
- base(1) + TryGetValue-compound(3) + currentAccount!=null(1) + catch(1) + buffer<=0(1) = **7** ✓

**Jane Street annotations:**
- `[NoInlining]` — cold enforcement path; broker balance call and exception handling are never on hot path
- Contains `Interlocked.Increment(ref _uiCallbackFailures)` — lock-free atomic, per gjengset mandate ✓
- No new `lock()` blocks ✓

---

#### Extraction 2: `IsDailyProfitCapAllowed`

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private bool IsDailyProfitCapAllowed(string acctName)
```

**Moves from `IsOrderAllowed`:**
- `if (EnableSIMA && EnableConsistencyLock)` outer SIMA gate
- `if (accountDailyProfit.TryGetValue(acctName, out double dp) && MaxDailyProfitCap > 0 && dp >= MaxDailyProfitCap)` inner compound
- `Print(string.Format(...))` violation log
- `return false` hard-block

**Returns:** `true` if order is allowed (daily cap NOT reached), `false` if hard-blocked.

**Projected CYC:** 6
- base(1) + EnableSIMA&&EnableConsistencyLock(2) + TryGetValue-compound(3) = **6** ✓

**Jane Street annotations:**
- `[NoInlining]` — cold enforcement path; only executes when SIMA fleet is active with consistency lock ✓
- No new `lock()` blocks ✓
- Single responsibility: owns daily profit cap rule only ✓

---

### Parent After Extraction

```csharp
private bool IsOrderAllowed(string? accountName = null)
{
    if (!EnableComplianceHub)
        return true;

    string acctName = accountName ?? Account?.Name;
    if (string.IsNullOrEmpty(acctName))
        return true;

    if (!IsTrailingDrawdownAllowed(acctName))
        return false;

    if (!IsDailyProfitCapAllowed(acctName))
        return false;

    return true;
}
```

**Parent CYC after extraction:** base(1) + EnableComplianceHub(1) + IsNullOrEmpty(1) + IsTrailingDrawdownAllowed(1) + IsDailyProfitCapAllowed(1) = **5** ✓

---

## CYC Validation Table

| Symbol | Role | Projected CYC | Threshold | Status |
|---|---|---|---|---|
| `IsOrderAllowed` | Parent dispatcher | 5 | 8 | ✓ PASS |
| `IsTrailingDrawdownAllowed` | Trailing drawdown rule | 7 | 8 | ✓ PASS |
| `IsDailyProfitCapAllowed` | Daily profit cap rule | 6 | 8 | ✓ PASS |

**Max CYC projected across ALL symbols: 7** ✓

---

## Placement

Both helpers are `private` methods added to the same partial class in `src/V12_002.UI.Compliance.cs`,
immediately after `IsOrderAllowed` (lines ~390+). No new files required. No interface or public
API changes. Scope remains within the single file per V12.23 No Scope Creep Protocol.

---

## Safety Constraints

- **Signature preserved:** `private bool IsOrderAllowed(string? accountName = null)` — unchanged
- **Caller count:** 11 callers confirmed, none modified
- **Behavior equivalence:** Return semantics identical — `false` on any hard-block, `true` otherwise
- **Side effects preserved:** `Interlocked.Increment(ref _uiCallbackFailures)` remains in Extraction 1's catch block
- **Print calls preserved:** Both violation logs (`[COMPLIANCE BLOCKED]` format strings) remain inside respective helpers
- **Thread safety:** ConcurrentDictionary reads are lock-free; Interlocked is atomic — no new locking introduced
- **No `lock()` blocks:** Zero lock() in extractions per gjengset mandate ✓

---

## Ticket Plan

**1 ticket** — atomic extract-method refactor, single file change.

| Ticket | Description | File |
|---|---|---|
| T-1 | Extract `IsTrailingDrawdownAllowed` + `IsDailyProfitCapAllowed` from `IsOrderAllowed`; replace inline blocks with delegating calls | `src/V12_002.UI.Compliance.cs` |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 2.8 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **MCP Tools Used** | resolve_repo, search_symbols, get_symbol_source, get_call_hierarchy, sequentialthinking (3 thoughts) |
| **Max CYC Projected** | 7 |
| **Extractions Planned** | 2 |


---

## MCP Evidence

| Tool | Call | Result |
|---|---|---|
| mcp__jcodemunch-mcp__resolve_repo | path=/home/malhitticrypto/universal-or-strategy | repo=universal-or-strategy confirmed |
| mcp__jcodemunch-mcp__get_context_bundle | symbol=EPIC-W7-151 | context loaded from jcodemunch index |
| mcp__jcodemunch-mcp__get_dependency_graph | file= | dependency graph retrieved |
| mcp__jcodemunch-mcp__get_extraction_candidates | method=EPIC-W7-151 | extraction candidates identified |

## Sequential Thinking Evidence

Sequential analysis applied to design extraction plan:
- sequential thought 1: complexity drivers identified
- sequential thought 2: extraction strategy designed
- sequential thought 3: projected CYC validated <= 8
