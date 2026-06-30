# EPIC-W7-097 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Epic ID:** EPIC-W7-097
**Method:** `ExecuteRMAEntryV2`
**Source File:** [`src/V12_002.SIMA.Execution.cs`](src/V12_002.SIMA.Execution.cs)
**Generated:** 2026-06-29T01:20:00Z
**Inputs:** `docs/brain/EPIC-W7-097/02-architecture-plan.md`, `docs/brain/EPIC-W7-097/03-audit-report.md`

---

## Summary

`ExecuteRMAEntryV2` (lines 686–844, ~158 lines) has an actual CYC of ~9, which is 1 over the Jane Street threshold of 8. Two surgical extraction tickets bring the orchestrator to **CYC = 8**:

- **ticket_count:** 2
- **projected_parent_cyc_after_all:** 8
- **dna_verdict (Phase 3):** PASS — zero violations

Execution order: Ticket-1 first (cold-path logging extraction, no CYC impact), then Ticket-2 (predicate extraction, CYC −1).

---

## Tickets

---

### Ticket 1 — Extract `BuildRmaForensicPulseReport`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-097-T1 |
| **helper_name** | `BuildRmaForensicPulseReport` |
| **concern** | Cold-path forensic logging extraction (carl_cook `[NoInlining]` rule) |
| **source_file** | [`src/V12_002.SIMA.Execution.cs`](src/V12_002.SIMA.Execution.cs) |
| **parent_method** | `ExecuteRMAEntryV2` |
| **lines_to_move** | ~17 AppendLine statements + surrounding StringBuilder block (~20 lines from orchestrator) |
| **cyc_reduction** | 0 (pure LOC isolation — no branches removed; reduces orchestrator line count by ~20) |
| **projected_helper_cyc** | 1 |
| **projected_parent_cyc_after_ticket** | 9 (unchanged — this ticket does not remove decision points) |
| **jane_street_rule** | carl_cook — cold-path logging MUST be extracted with `[NoInlining]` |
| **attributes** | `[MethodImpl(MethodImplOptions.NoInlining)]` |

#### Signature

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void BuildRmaForensicPulseReport(
    StringBuilder dispatchLog,
    int fleetOk,
    int fleetSkip,
    double setupMs,
    double localMs,
    double loopMs,
    double totalMs)
```

#### Extraction Steps

1. Locate the `StringBuilder(1024)` forensic report block inside `ExecuteRMAEntryV2` (~17 consecutive `AppendLine` calls).
2. Create new private method `BuildRmaForensicPulseReport` with the signature above, decorated `[MethodImpl(MethodImplOptions.NoInlining)]`.
3. Move all `dispatchLog.AppendLine(...)` statements into the new helper body verbatim.
4. Replace the extracted block in `ExecuteRMAEntryV2` with a single call: `BuildRmaForensicPulseReport(dispatchLog, fleetOk, fleetSkip, setupMs, localMs, loopMs, totalMs);`
5. Verify: `grep BuildRmaForensicPulseReport src/V12_002.SIMA.Execution.cs` → exactly 2 matches (definition + call site).
6. Build: `dotnet build` → zero errors.

#### Verify Criterion

- `[MethodImpl(MethodImplOptions.NoInlining)]` present on helper definition.
- Zero `AppendLine` calls remain inline in `ExecuteRMAEntryV2` body for the forensic report block.
- Orchestrator LOC reduced by ~20 lines; CYC remains ~9 (unchanged by design).

---

### Ticket 2 — Extract `IsEligibleFleetAccount`

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-097-T2 |
| **helper_name** | `IsEligibleFleetAccount` |
| **concern** | Fleet-account eligibility predicate extraction (trading_billions single-responsibility rule) |
| **source_file** | [`src/V12_002.SIMA.Execution.cs`](src/V12_002.SIMA.Execution.cs) |
| **parent_method** | `ExecuteRMAEntryV2` |
| **lines_to_move** | 2 inline guard conditions (`!IsFleetAccount(acct)` + `acct == this.Account`) → merged into 1 predicate call |
| **cyc_reduction** | 1 (merges 2 decision points into 1 predicate call, reducing orchestrator CYC 9 → 8) |
| **projected_helper_cyc** | 2 |
| **projected_parent_cyc_after_ticket** | 8 |
| **jane_street_rule** | trading_billions — single-responsibility predicates; CYC ≤ 8 mandate |
| **attributes** | _(none — hot predicate, trivially inlineable by JIT)_ |

#### Signature

```csharp
private bool IsEligibleFleetAccount(Account acct)
```

#### Body

```csharp
private bool IsEligibleFleetAccount(Account acct)
{
    return IsFleetAccount(acct) && acct != this.Account;
}
```

#### Extraction Steps

1. Locate the `foreach (Account acct in Account.All)` loop inside `ExecuteRMAEntryV2`.
2. Identify the two consecutive guard checks:
   ```csharp
   if (!IsFleetAccount(acct)) continue;
   if (acct == this.Account) continue;
   ```
3. Create new private method `IsEligibleFleetAccount(Account acct)` returning `bool` with body: `return IsFleetAccount(acct) && acct != this.Account;`
4. Replace the two guard lines in the fleet loop with: `if (!IsEligibleFleetAccount(acct)) continue;`
5. Verify: `grep IsEligibleFleetAccount src/V12_002.SIMA.Execution.cs` → exactly 2 matches (definition + call site in loop).
6. Verify: the dual-guard pattern (`IsFleetAccount(acct)) continue` + `this.Account) continue`) no longer appears inline.
7. Build: `dotnet build` → zero errors.

#### Verify Criterion

- Orchestrator fleet loop contains exactly one guard: `if (!IsEligibleFleetAccount(acct)) continue;`
- `IsEligibleFleetAccount` helper has exactly 1 return statement with the `&&` predicate.
- Post-extraction CYC of orchestrator = **8** (at Jane Street threshold — PASS).

---

## CYC Reduction Summary

| Ticket | Helper | cyc_reduction | Parent CYC After |
|---|---|---|---|
| T1 | `BuildRmaForensicPulseReport` | 0 (LOC isolation only) | 9 |
| T2 | `IsEligibleFleetAccount` | 1 | **8** |
| **Total** | — | **1** | **8** |

**projected_parent_cyc_after_all: 8**

---

## Post-Extraction Method CYC Table

| Method | CYC |
|---|---|
| `ExecuteRMAEntryV2` (orchestrator, post-extraction) | 8 |
| `BuildRmaForensicPulseReport` | 1 |
| `IsEligibleFleetAccount` | 2 |
| All methods ≤ 8 Jane Street threshold | **PASS** |

---

## Sequential Thinking Evidence

**Thought 1:** Confirmed ticket scope — 2 extractions required. Ticket-1 is LOC-only (no CYC impact), Ticket-2 is CYC-reducing (-1 branch). Tickets are logically independent but sequenced T1 → T2 for clean compile at each step.

**Thought 2:** Validated lines_to_move estimates: T1 moves ~17-20 lines (AppendLine block), T2 moves 2 guard lines replacing with 1 predicate call. Both helpers comply with Phase 3 DNA audit (PASS, violations=[]).

**Thought 3:** Confirmed execution order and verify criteria. Post T1+T2: orchestrator CYC=8, BuildRmaForensicPulseReport CYC=1, IsEligibleFleetAccount CYC=2. All at or below threshold. ticket_count=2, projected_parent_cyc_after_all=8. Ticket breakdown validated.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **MCP Tools Used** | resolve_repo, sequentialthinking (3 thoughts) |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 8 |
| **Jane Street KB Applied** | carl_cook (NoInlining cold logging), trading_billions (CYC<=8, single responsibility) |
| **dna_verdict (Phase 3)** | PASS |
