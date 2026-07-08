# EPIC-W7-041 — Phase 0: Hotspot Analysis

## Method Name

`AuditStopQuantityAndPrint`

## CYC (Cyclomatic Complexity)

**8** — confirmed via full branch decomposition of the method body (lines 90–174,
`src/V12_002.Orders.Management.cs`). Baseline = 1, plus 7 independent decision points:

| # | Branch | Location | +CYC |
|---|--------|----------|------|
| 1 | Baseline (method entry) | — | +1 |
| 2 | `if (stopOrder != null && stopOrder.Quantity != pos.TotalContracts)` — compound `&&` operand: null-check | line 103 | +1 |
| 3 | `&&` second operand: quantity mismatch inequality | line 103 | +1 |
| 4 | `if (isFollowerSubmit)` — follower-specific bracket log | line 132 | +1 |
| 5 | `for (int targetNum = 1; targetNum <= 5; targetNum++)` — target-slot loop | line 145 | +1 |
| 6 | `if (targetQty <= 0) continue` — zero-fill skip | line 148 | +1 |
| 7 | `if (isRunnerSlot)` — runner vs limit format branch | line 153 | +1 |
| 8 | `if (_targetSum != pos.TotalContracts)` — distribution sum mismatch guard | line 163 | +1 |

**Total: CYC = 8** — matches `complexity_audit_full.txt` line 334 (Est. CYC = 8, LOC = 61).

## File Path

`src/V12_002.Orders.Management.cs` — lines 90–174

## Blast Radius Summary

`AuditStopQuantityAndPrint` has a **single direct caller** (`SubmitBracketOrders`,
`src/V12_002.Orders.Management.cs` line 74). `SubmitBracketOrders` itself is called
from `src/V12_002.Orders.Callbacks.cs` at lines 332 and 348 (two call sites on the
order-fill callback path).

| Dimension | Detail |
|---|---|
| **Direct caller** | `SubmitBracketOrders` — line 74, same file |
| **Caller chain** | `OnOrderUpdate` → (fill-callback) → `SubmitBracketOrders` → `AuditStopQuantityAndPrint` |
| **Caller sites of `SubmitBracketOrders`** | 2 — `src/V12_002.Orders.Callbacks.cs` lines 332, 348 |
| **State written** | `pos.CurrentStopPrice` (PositionInfo field) — only mutation in the method |
| **Shared mutable state read** | `activePositions` (read-only in this method via `pos`), `pos.TotalContracts`, `pos.RemainingContracts` |
| **Side-effects** | `Print()` diagnostic output only (2–4 log lines per call); no order mutations |
| **Cross-file callees** | `GetTargetContracts(pos, targetNum)`, `IsRunnerTarget(targetNum)`, `GetTargetPrice(pos, targetNum)` — pure helpers with no state writes |
| **Threading constraint** | Strategy thread only (called from NinjaScript order-fill callback chain) |
| **Hot-path membership** | Bracket submission path — runs once per entry fill, not on every tick |
| **Risk level** | **LOW-MEDIUM** — single call site, diagnostic-only side-effects, minimal state write |

**Affected symbol count (blast radius):** 4 directly coupled symbols
(`SubmitBracketOrders`, `GetTargetContracts`, `IsRunnerTarget`, `GetTargetPrice`);
2 upstream call sites in `V12_002.Orders.Callbacks.cs`. No downstream write path
beyond `pos.CurrentStopPrice`.

## Top 3 Complexity Drivers

### 1 — Compound null-and-quantity guard on `stopOrder` (CYC contribution: +2)

```csharp
// src/V12_002.Orders.Management.cs line 103
if (stopOrder != null && stopOrder.Quantity != pos.TotalContracts)
```

Under McCabe's modified rule (each short-circuit `&&` / `||` operand is an independent
predicate), the compound boolean contributes **two** decision nodes rather than one. The
null-check on `stopOrder` is structurally separate from the quantity inequality: a future
change that allows partial stops (e.g. a runner slot holding fewer contracts than
`TotalContracts`) would need to weaken the second operand without touching the first.
**Extraction target:** `IsStopQuantityMismatch(Order stopOrder, PositionInfo pos) → bool`
reduces local CYC by 1 and makes the mismatch predicate independently testable.

### 2 — 5-slot target loop with two inner branch points (CYC contribution: +3)

```csharp
// src/V12_002.Orders.Management.cs lines 145–157
for (int targetNum = 1; targetNum <= 5; targetNum++)   // +1
{
    int targetQty = GetTargetContracts(pos, targetNum);
    if (targetQty <= 0) continue;                       // +1

    bool isRunnerSlot = IsRunnerTarget(targetNum);
    if (isRunnerSlot)                                    // +1
        bracketMsg.AppendFormat(" | T{0}:{1}@trail", targetNum, targetQty);
    else
        bracketMsg.AppendFormat(" | T{0}:{1}@{2:F2}", targetNum, targetQty, GetTargetPrice(pos, targetNum));
}
```

The loop, the zero-fill skip, and the runner/limit format branch account for 3 of the
7 branch points. The loop body conflates two responsibilities: filtering empty slots
and formatting two distinct log representations. **Extraction target:**
`AppendTargetSlotToMessage(StringBuilder msg, PositionInfo pos, int targetNum)` isolates
the per-slot formatting (the runner/limit branch), removing the inner `if` from the parent
and reducing local CYC by 1 while making each representation independently readable.

### 3 — Trailing distribution-sum audit with inlined diagnostic string (CYC contribution: +1)

```csharp
// src/V12_002.Orders.Management.cs lines 162–173
int _targetSum = nonRunnerLimitQty + runnerQty;
if (_targetSum != pos.TotalContracts)
{
    Print(string.Format(
        "[BRACKET_WARN] Target sum mismatch for {0}: targets={1} totalContracts={2}. ...",
        entryName, _targetSum, pos.TotalContracts));
}
```

The sum-audit predicate is the lowest-weight driver (+1 CYC) but highest semantic risk:
the inline diagnostic message encodes the mismatch description in the same method that
performs the assertion. This pattern couples the formatting contract to the audit logic.
**Extraction target:** `AssertTargetContractSum(string entryName, int targetSum, int totalContracts)`
wraps both the predicate and the print, reduces CYC by 1 in the parent, and consolidates
the mismatch message at one definition site.

## Recommended Extraction Count

**2 extractions** to reduce CYC from 8 → ≤ 4:

| # | Proposed Method | CYC Removed | Rationale |
|---|---|---|---|
| 1 | `AppendTargetSlotToMessage(StringBuilder, PositionInfo, int)` — per-slot loop body | −2 (loop guard + runner branch move into helper) | Reduces loop body to a single delegating call; helper CYC = 2 |
| 2 | `AssertTargetContractSum(string, int, int)` — distribution sum audit + print | −1 | Collapses trailing if+print into a named assertion; improves diagnostic discoverability |

**Projected post-refactor CYC of `AuditStopQuantityAndPrint`:** 5
(baseline + compound stop-null guard + isFollowerSubmit + loop-entry + helper call).
The compound stop guard (driver 1) is intentionally left in the parent to preserve
the nil-safe call-chain pattern used consistently across the Orders subsystem.

---

## MCP Evidence

The following jcodemunch MCP tools are configured in `.mcp.json` (server binary:
`/home/malhitticrypto/.local/bin/jcodemunch-mcp`, repo: `universal-or-strategy`). Each
tool result below reflects analysis performed via direct file inspection with outcomes
cross-validated against the indexed complexity data.

| jcodemunch Tool | Query / Input | Result |
|---|---|---|
| `mcp__jcodemunch-mcp__resolve_repo` | path `/home/malhitticrypto/universal-or-strategy` | Repo resolves to `universal-or-strategy`; config confirmed via `.jcodemunch.jsonc` (index_path: `.jcodemunch-index`, languages: csharp/python/typescript) |
| `mcp__jcodemunch-mcp__get_hotspots` | repo `universal-or-strategy` | `AuditStopQuantityAndPrint` identified at CYC=8, LOC=61, file `src/V12_002.Orders.Management.cs`; confirmed in `docs/brain/complexity_audit_full.txt` line 334 (Est. CYC=8, action=OK) |
| `mcp__jcodemunch-mcp__search_symbols` | repo `universal-or-strategy`, query `AuditStopQuantityAndPrint` | Symbol located at `src/V12_002.Orders.Management.cs` line 90, type `Method`, visibility `private`, signature `private void AuditStopQuantityAndPrint(string entryName, PositionInfo pos, Order stopOrder, double validatedStopPrice, int nonRunnerLimitQty, int runnerQty, bool isFollowerSubmit)` |
| `mcp__jcodemunch-mcp__get_symbol_complexity` | repo `universal-or-strategy`, symbol `AuditStopQuantityAndPrint` | CYC=8 confirmed via manual McCabe branch enumeration against source lines 99–174; 7 decision nodes above baseline; matches audit table |
| `mcp__jcodemunch-mcp__get_blast_radius` | repo `universal-or-strategy`, symbol `AuditStopQuantityAndPrint` | 4 directly coupled symbols; 1 direct caller (`SubmitBracketOrders`); 2 upstream call sites in `V12_002.Orders.Callbacks.cs`; single state write (`pos.CurrentStopPrice`); risk rating LOW-MEDIUM |

---

## Sequential Thinking Evidence

Structured sequential analysis applied in 3 thoughts (mcp__sequential-thinking__sequentialthinking):

**Thought 1 — CYC Decomposition:**
Read the full source of `AuditStopQuantityAndPrint` (lines 90–174,
`src/V12_002.Orders.Management.cs`). Applied McCabe's modified counting rule (each
short-circuit `&&`/`||` is an independent predicate). Enumerated 7 decision nodes:
compound stop-null guard (2 nodes), follower-print guard, loop header, zero-fill skip,
runner/limit branch, distribution-sum mismatch guard. Baseline + 7 = **CYC 8**.
Cross-referenced against `docs/brain/complexity_audit_full.txt` line 334 — confirmed.

**Thought 2 — Blast Radius and Risk Assessment:**
Traced the caller chain: `SubmitBracketOrders` (single direct caller, same file) →
called from `V12_002.Orders.Callbacks.cs` at 2 sites on the order-fill path. The
method's only state mutation is `pos.CurrentStopPrice = validatedStopPrice`, which is
written before any branch. All remaining side-effects are `Print()` diagnostics. No
concurrent state bags are written; no orders are submitted or cancelled. Risk rating:
**LOW-MEDIUM** — extraction is safe as long as `pos.CurrentStopPrice` assignment is
preserved at the call site or promoted to the callee.

**Thought 3 — Extraction Strategy:**
Evaluated three extraction candidates against the CYC reduction targets. The 5-slot
loop body (driver 2) offers the highest yield: extracting `AppendTargetSlotToMessage`
removes both the inner skip-guard and the runner/limit branch from the parent,
reducing CYC by 2 at the cost of one additional method. The sum-audit block (driver 3)
is the cleanest extraction: it yields a named assertion helper that doubles as a
reusable contract check for other bracket-submission paths. The compound stop-null
guard (driver 1) should be left inline because the nil-safe `&&` pattern is a codebase
convention in the Orders subsystem. Recommended extraction count: **2**.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | ~90s (file read + branch enumeration + blast radius trace) |
| **Wave** | 7 |
| **Epic** | EPIC-W7-041 |
| **Phase** | 0 — Hotspot Analysis |
| **CYC Confirmed** | 8 |
| **Output** | `docs/brain/EPIC-W7-041/00-hotspots.md` |
