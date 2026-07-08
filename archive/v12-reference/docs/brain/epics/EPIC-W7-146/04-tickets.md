# EPIC-W7-146 — Phase 4: Ticket Generation

**agent_name:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-146/02-architecture-plan.md + docs/brain/EPIC-W7-146/03-audit-report.md

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-146 |
| **Bobcoins Used** | 1.0 |

---

## Summary

| Field | Value |
|---|---|
| **Target Method** | `CancelOrphanedTargets` |
| **Source File** | `src/V12_002.UI.Compliance.cs` |
| **Lines** | 553–578 |
| **CYC Baseline** | 13 |
| **CYC Target** | 7 |
| **dna_verdict** | PASS |
| **Ticket Count** | 2 |
| **Max CYC Projected** | 7 |

---

## Sequential Thinking Evidence

**ST-thought-1:** CancelOrphanedTargets CYC=13 decomposed into 6 drivers: base(1)+foreach(1)+null/instrument guard(2)+state guard(2)+name null(1)+5-way OR prefix chain(5). Dominant driver = 5-way OR prefix chain (StartsWith T1_–T5_) contributing +5 CYC. Primary extraction target: factor the 5-way OR into `IsTargetOrderName(string name)`. After extraction parent CYC=7, helper CYC=6.

**ST-thought-2:** Designed 2-ticket extraction plan. T1 creates `IsTargetOrderName` as `[MethodImpl(AggressiveInlining)]` private bool with CYC=6 and full xUnit coverage (5 positive prefix tests + 1 negative). T2 refactors `CancelOrphanedTargets` to call `IsTargetOrderName`, reducing parent CYC from 13 to 7. T2 depends on T1. No cross-file changes. Caller `HandleFleetStopFill` contract unchanged.

**ST-thought-3:** Verification confirmed all CYC targets satisfied (max=7 ≤ 8). All 6 DNA checks from Phase 3 audit carry forward as PASS. Jane Street KB rules satisfied: AggressiveInlining, zero-alloc, no LINQ, single responsibility. 2-ticket plan approved for execution.

---

## Extraction Tickets

### Ticket T1 — Extract `IsTargetOrderName` Helper

| Field | Value |
|---|---|
| **ID** | T1 |
| **Type** | extraction |
| **Priority** | P0 (blocker for T2) |
| **CYC Target** | 6 |
| **File** | `src/V12_002.UI.Compliance.cs` |
| **Depends On** | — (no dependencies) |

**Title:** Extract `IsTargetOrderName(string name)` from the 5-way OR prefix chain in `CancelOrphanedTargets`

**Description:**

The inline 5-way `StartsWith` OR chain inside `CancelOrphanedTargets` contributes +5 CYC to the parent (lines 553–578). This ticket extracts that logic into a dedicated private helper method `IsTargetOrderName(string name)` placed in the same partial class.

The new helper:
- Is marked `[MethodImpl(MethodImplOptions.AggressiveInlining)]` per Jane Street carl_cook hot-path rule
- Returns `bool` — true if `name` starts with any of `T1_`, `T2_`, `T3_`, `T4_`, or `T5_`
- Has CYC=6 (base=1 + five StartsWith OR branches = 5)
- Introduces zero new allocations

**Implementation:**

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool IsTargetOrderName(string name)
{
    return name.StartsWith("T1_")
        || name.StartsWith("T2_")
        || name.StartsWith("T3_")
        || name.StartsWith("T4_")
        || name.StartsWith("T5_");
}
```

**Acceptance Criteria:**

- [ ] `IsTargetOrderName(string name)` is added to `src/V12_002.UI.Compliance.cs` in the same partial class as `CancelOrphanedTargets`
- [ ] Method is `private bool` with `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute
- [ ] Method handles exactly: `T1_`, `T2_`, `T3_`, `T4_`, `T5_` prefixes via `StartsWith`
- [ ] CYC of `IsTargetOrderName` = 6 (verified by complexity audit)
- [ ] xUnit [Fact] tests written: 5 positive cases (one per prefix) + 1 negative case = 6 tests minimum
- [ ] Build passes: `dotnet build` zero errors
- [ ] CSharpier passes: `dotnet csharpier check src/`
- [ ] ASCII-only string literals confirmed (`"T1_"` through `"T5_"`)

---

### Ticket T2 — Refactor `CancelOrphanedTargets` to Call `IsTargetOrderName`

| Field | Value |
|---|---|
| **ID** | T2 |
| **Type** | extraction |
| **Priority** | P1 |
| **CYC Target** | 7 |
| **File** | `src/V12_002.UI.Compliance.cs` |
| **Depends On** | T1 (IsTargetOrderName must exist) |

**Title:** Reduce `CancelOrphanedTargets` CYC from 13 to 7 by delegating to `IsTargetOrderName`

**Description:**

With `IsTargetOrderName` in place (T1), replace the inline 5-way OR prefix chain in `CancelOrphanedTargets` with a single call to `IsTargetOrderName(o.Name)`. This collapses the +5 CYC contribution into +0 (predicate delegation), reducing the parent from CYC=13 to CYC=7.

All existing guards are preserved in the refactored parent:
- `o == null || o.Instrument?.FullName != Instrument?.FullName` → `continue`
- `o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted` → `continue`
- `o.Name != null && IsTargetOrderName(o.Name)` → cancel + increment

The caller `HandleFleetStopFill` is unaffected — signature and call site unchanged.

**Refactored Parent Implementation:**

```csharp
private int CancelOrphanedTargets(Account account)
{
    int cancelledTargets = 0;
    foreach (Order o in account.Orders.ToArray())
    {
        if (o == null || o.Instrument?.FullName != Instrument?.FullName)
            continue;
        if (o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted)
            continue;
        if (o.Name != null && IsTargetOrderName(o.Name))
        {
            CancelOrderOnAccount(o, account);
            cancelledTargets++;
        }
    }
    return cancelledTargets;
}
```

**CYC After:** base(1) + foreach(1) + null/instrument guard(2) + state guard(2) + name null(1) = **7**

**Acceptance Criteria:**

- [ ] `CancelOrphanedTargets` in `src/V12_002.UI.Compliance.cs` is refactored to delegate prefix check to `IsTargetOrderName(o.Name)`
- [ ] Inline 5-way `StartsWith` OR chain is removed from the parent body
- [ ] All three guard conditions preserved (`null/instrument`, `state`, `name null + prefix`)
- [ ] CYC of `CancelOrphanedTargets` = 7 after refactor (verified by complexity audit)
- [ ] Caller `HandleFleetStopFill` requires zero modifications
- [ ] Return type `int` (cancelledTargets) preserved
- [ ] Build passes: `dotnet build` zero errors
- [ ] CSharpier passes: `dotnet csharpier check src/`
- [ ] Pre-push validation passes: `powershell -File ./scripts/pre_push_validation.ps1 -Fast`
- [ ] `deploy-sync.ps1` executed to re-synchronize NinjaTrader hard links

---

## CYC Reduction Summary

| Method | CYC Before | CYC After | Threshold | Status |
|---|---|---|---|---|
| `CancelOrphanedTargets` | 13 | 7 | ≤8 | **PASS** |
| `IsTargetOrderName` | N/A (new) | 6 | ≤8 | **PASS** |
| **Max CYC Projected** | — | **7** | ≤8 | **PASS** |

---

## Execution Order

```
T1 (IsTargetOrderName extraction) → T2 (CancelOrphanedTargets refactor)
```

T2 has a hard dependency on T1. T1 must be committed and verified before T2 begins.

---

## Phase 3 DNA Audit Carry-Forward

| Check | Status |
|---|---|
| Zero `lock()` blocks | PASS |
| ASCII-only string literals | PASS |
| No scope creep | PASS |
| xUnit [Fact] tests (never NUnit/MSTest) | PASS |
| max_cyc_projected ≤ 8 | PASS (max=7) |
| dna_verdict | **PASS** |
