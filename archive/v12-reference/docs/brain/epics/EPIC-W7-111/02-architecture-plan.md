# Phase 2: Architecture Plan -- EPIC-W7-111

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 -- Architecture Planning
**Generated:** 2026-06-29T01:30:00Z
**Input:** docs/brain/EPIC-W7-111/01-scope-boundary.md

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `HydrateExpectedPositionsFromBroker` |
| **Source File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Class** | `V12_002` (partial) |
| **Visibility** | `private void` |
| **Line Range** | 208-300 |
| **Original CYC** | **11** (conservative McCabe from 00-hotspots.md; liberal count = 15) |

### jcodemunch get_context_bundle result

Source confirmed via `get_context_bundle` (fallback from symbol ID ambiguity resolution via `search_symbols`). Full method body retrieved. Key findings:

- Two structurally parallel blocks: Block A iterates `Account.All` fleet accounts (L211-247); Block B handles master account directly (L253-299).
- Both blocks share identical structure: `foreach(Position pos in acct.Positions.ToArray())` + compound null/guard check + ternary qty + `Enqueue(ctx => ctx.AddOrUpdateExpectedPosition(...))` + `Print` + `hydratedCount++` + `break` + `catch(Exception ex)`.
- Block A uses explicit `pos.Instrument != null && pos.Instrument.FullName == ...`; Block B uses `pos.Instrument?.FullName == ...` null-conditional (inconsistency).
- All mutation paths routed through Actor-queue `Enqueue` -- zero lock blocks.
- Docstring confirms role: seeds `expectedPositions` from live broker state to prevent false REAPER DESYNC alerts on strategy restart.

### jcodemunch get_call_hierarchy result

- **Callers (depth=1):** `EnumerateApexAccounts` (src/V12_002.SIMA.Lifecycle.cs:140)
- **Callers (depth=2):** `ProcessInitializeSIMA` (src/V12_002.SIMA.Lifecycle.cs:90)
- **Callees (depth=1):** `IsFleetAccount`, `Enqueue`, `ExpKey`
- **Callees (depth=2):** `_cmdQueue`, `IsActorThread`, `TryDrain`, `ScheduleActorDrain`, `LogBuffer.Format`, `LogBuffer.ValidateThreadAffinity`, `LogBuffer.FormatInternal`
- Single entry point into this method confirms safe internal-only refactor; external contract unchanged.

### jcodemunch get_dependency_graph result

- `get_dependency_graph` returned: node_count=1, edge_count=0, imports=[], importers=[]
- The index reports no cross-file import edges for `src/V12_002.SIMA.Lifecycle.cs` at depth=1.
- This is consistent with NinjaTrader partial-class architecture where all `using` directives live in the root `V12_002.cs` and partial files share the same namespace without explicit import declarations.
- Conclusion: all helpers must remain in `src/V12_002.SIMA.Lifecycle.cs` (same partial class file) -- no new files required, V12.23 compliant.

### jcodemunch get_extraction_candidates result

- `get_extraction_candidates` returned: candidates=[] (complexity data not populated in index for this file).
- Complexity analysis deferred to manual McCabe count in `00-hotspots.md` (CYC=11-15, both above threshold).
- Extraction plan derived from manual analysis (see Extraction Plan section below).

---

## Sequential Thinking Summary

**5-thought sequentialthinking chain completed. Final thought (thought 5):**

Jane Street alignment final check:

1. CYC<=8: ALL symbols project to CYC=5. PASS.
2. Single responsibility per helper: `IsMatchingOpenPosition` answers only "is this position open and matching this instrument?" `HydrateSingleAccount` handles exactly "seed expectedPositions from one account's broker data." Both PASS.
3. Lock-free/Actor pattern preserved: All writes go through `Enqueue(...)` which is the Actor queue dispatch pattern. ZERO lock blocks. PASS.
4. Illegal states unrepresentable: `IsMatchingOpenPosition` consolidates both null guards AND the flat-position guard into one predicate -- impossible to reach `Enqueue` with null Position, null Instrument, instrument mismatch, or flat position. PASS.
5. ASCII-only string literals: No Unicode, no emoji, no curly quotes in any new string literals. PASS.
6. Extraction strategies applied: Guard Clause pattern in `IsMatchingOpenPosition` (early returns), Named Helper Methods (`HydrateSingleAccount`), Loop Body Extraction (foreach body moved to helper). All 3 Jane Street patterns correctly applied.
7. xUnit [Fact] tests: Phase 5 will generate tests for `IsMatchingOpenPosition` (null pos, null instrument, wrong instrument name, flat position, valid open position) and `HydrateSingleAccount` (no matching position, one matching position, exception path). Each test covers one code path.

**CONCLUSION:** Extraction plan sound. 2 helpers. max_cyc_projected=5. extraction_count=2.

---

## Extraction Plan

| Helper Method Name | Responsibility | Signature | Projected CYC |
|---|---|---|---|
| `IsMatchingOpenPosition` | Guard predicate: validates pos != null, Instrument != null, FullName matches, MarketPosition != Flat. Normalizes the two inconsistent guard variants from Block A and Block B into one canonical predicate. | `private bool IsMatchingOpenPosition(Position pos)` | **5** |
| `HydrateSingleAccount` | Iterates one account's positions, finds the first `IsMatchingOpenPosition` match, calculates signed qty, routes seed through `Enqueue -> AddOrUpdateExpectedPosition`, logs result, increments hydratedCount. Contains the single try/catch. | `private void HydrateSingleAccount(Account acct, ref int hydratedCount)` | **5** |

### IsMatchingOpenPosition -- Detailed Design

```csharp
private bool IsMatchingOpenPosition(Position pos)
{
    if (pos == null)
        return false;
    if (pos.Instrument == null)
        return false;
    if (pos.Instrument.FullName != Instrument.FullName)
        return false;
    if (pos.MarketPosition == MarketPosition.Flat)
        return false;
    return true;
}
```

CYC breakdown: base 1 + 4 guard-clause returns = **5**.

### HydrateSingleAccount -- Detailed Design

```csharp
private void HydrateSingleAccount(Account acct, ref int hydratedCount)
{
    try
    {
        foreach (Position pos in acct.Positions.ToArray())
        {
            if (!IsMatchingOpenPosition(pos))
                continue;
            int qty = pos.MarketPosition == MarketPosition.Long ? pos.Quantity : -pos.Quantity;
            var capturedAcct = acct.Name;
            var capturedQty = qty;
            Enqueue(ctx =>
                ctx.AddOrUpdateExpectedPosition(ExpKey(capturedAcct), capturedQty, v => capturedQty)
            );
            Print($"[SIMA HYDRATE] {acct.Name}: Seeded expected={qty} from broker ({pos.MarketPosition} {pos.Quantity})");
            hydratedCount++;
            break;
        }
    }
    catch (Exception ex)
    {
        Print($"[SIMA HYDRATE] WARNING: Could not read positions for {acct.Name}: {ex.Message}");
    }
}
```

CYC breakdown: base 1 + foreach 1 + if(!IsMatchingOpenPosition) 1 + ternary qty 1 + catch 1 = **5**.

---

## Parent Method After Extraction

### Remaining Logic

The parent method becomes a pure orchestration shell:
1. Initialize `hydratedCount = 0`
2. Iterate `Account.All` with fleet-account guard (`if (!IsFleetAccount(acct)) continue`)
3. Delegate to `HydrateSingleAccount(acct, ref hydratedCount)` for each fleet account
4. Print summary if `hydratedCount > 0`
5. Check `masterIsFleet993 = IsFleetAccount(Account)`
6. If master is not a fleet account, call `HydrateSingleAccount(Account, ref hydratedCount)`

```csharp
private void HydrateExpectedPositionsFromBroker()
{
    int hydratedCount = 0;
    foreach (Account acct in Account.All)
    {
        if (!IsFleetAccount(acct))
            continue;
        HydrateSingleAccount(acct, ref hydratedCount);
    }
    if (hydratedCount > 0)
        Print($"[SIMA HYDRATE] Hydrated {hydratedCount} account(s) with live broker positions");

    bool masterIsFleet993 = IsFleetAccount(Account);
    if (!masterIsFleet993)
        HydrateSingleAccount(Account, ref hydratedCount);
}
```

CYC breakdown: base 1 + foreach 1 + if(!IsFleetAccount) 1 + if(hydratedCount > 0) 1 + if(!masterIsFleet993) 1 = **5**.

- **Remaining logic:** Loop orchestration + fleet guard + summary print + master account delegation
- **Projected CYC:** **5**

---

## max_cyc_projected: 5
## extraction_count: 2

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved for all symbols | **YES** -- max projected CYC = 5 |
| Single-responsibility per helper | **YES** -- predicate vs. hydration are distinct concerns |
| Lock-free/Actor pattern preserved | **YES** -- all Enqueue calls remain in HydrateSingleAccount; zero lock blocks |
| Guard Clause pattern applied | **YES** -- IsMatchingOpenPosition uses 4 early returns |
| Loop Body Extraction applied | **YES** -- foreach body extracted to HydrateSingleAccount |
| Named Helper Methods (private scope) | **YES** -- both helpers are private, same partial class file |
| Illegal states unrepresentable | **YES** -- null positions and null instruments cannot reach Enqueue; flat positions cannot reach Enqueue |
| ASCII-only string literals | **YES** -- no Unicode, no emoji, no curly quotes in any string |
| Structural duplication eliminated | **YES** -- Block A and Block B both delegate to HydrateSingleAccount |
| Inconsistent null-guard patterns normalized | **YES** -- IsMatchingOpenPosition uses explicit null checks (no ?. inconsistency) |
| xUnit [Fact] tests required (Phase 5) | YES -- IsMatchingOpenPosition x5 paths, HydrateSingleAccount x3 paths |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 3.0 |
| **Execution Time** | 2026-06-29T01:30:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **MCP resolve_repo** | antigravityos187-sketch/universal-or-strategy (5147 symbols, indexed) |
| **Input** | docs/brain/EPIC-W7-111/01-scope-boundary.md |
| **Output** | docs/brain/EPIC-W7-111/02-architecture-plan.md |
