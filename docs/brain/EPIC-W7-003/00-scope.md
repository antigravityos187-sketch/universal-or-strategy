# Phase 1: Scope Definition - EPIC-W7-003

## Method Under Refactoring
- Method: `IsOrderAllowed`
- File: `src/V12_002.UI.Compliance.cs`
- Line: 323
- Current CYC: 21
- Target CYC: ≤ 8

## IN SCOPE
The following changes will be made to reduce CYC from 21 to ≤ 8:

- Extract the **early-exit guard** (EnableComplianceHub check + null/empty account resolution)
  into a helper that returns the resolved account name or signals bypass.
- Extract the **trailing drawdown block** (lines 333–366) — the entire
  `accountEquityPeak` lookup, balance retrieval with try/catch, buffer calculation,
  and block-print — into a named private helper `IsTrailingDrawdownBreached`.
- Extract the **account balance retrieval with error handling** (lines 336–353)
  from within the drawdown block into a private helper `TryGetAccountBalance`,
  eliminating the inner try/catch nesting that contributes 2 extra complexity points.
- Extract the **daily profit cap block** (lines 369–386) — the `EnableSIMA &&
  EnableConsistencyLock` gate, `accountDailyProfit` lookup, cap comparison, and
  block-print — into a named private helper `IsDailyProfitCapBreached`.
- `IsOrderAllowed` itself becomes a flat orchestrator: resolve account name →
  call `IsTrailingDrawdownBreached` → call `IsDailyProfitCapBreached` → return true.

## OUT OF SCOPE
- **Method signature**: `IsOrderAllowed(string? accountName = null)` must remain
  callable as-is; return type `bool` unchanged.
- **Logging behavior**: all existing `Print(string.Format(...))` compliance-blocked
  messages are preserved verbatim in the extracted helpers, including the
  `[COMPLIANCE BLOCKED]` prefix and `{acctName}` / dollar-format tokens.
- **Error-handling semantics**: the `Interlocked.Increment(ref _uiCallbackFailures)`
  + `Print` path in the balance-retrieval catch block is preserved unchanged.
- **Business logic semantics**: no behavioral change — same inputs must produce
  identical outputs (block/allow) under all conditions.
- **Other methods in the file**: no changes outside the four methods listed above.
- **Access modifiers / visibility**: all helpers will be `private`.
- **Unit tests**: adding tests is out of scope for this phase (addressed in Phase 2+).

## Extraction Plan

| Helper Method | Responsibility | Expected CYC |
|---|---|---|
| `TryGetAccountBalance(Account acct, out double balance)` | Wraps `Account.Get(CashValue)` with try/catch; increments `_uiCallbackFailures` on failure; returns bool success | ≤ 3 |
| `IsTrailingDrawdownBreached(string acctName)` | Looks up `accountEquityPeak`, calls `TryGetAccountBalance`, computes buffer, prints block message if buffer ≤ 0, returns bool | ≤ 4 |
| `IsDailyProfitCapBreached(string acctName)` | Guards on `EnableSIMA && EnableConsistencyLock`, looks up `accountDailyProfit`, compares to `MaxDailyProfitCap`, prints block message, returns bool | ≤ 3 |
| `IsOrderAllowed(string? accountName)` *(refactored)* | Early-exit on `!EnableComplianceHub`, resolves `acctName`, delegates to the two breach-checkers | ≤ 4 |

Three new private helpers + one slimmed orchestrator. No new public surface.

## Risk Assessment
**Overall: LOW**

| Factor | Level | Rationale |
|---|---|---|
| Blast radius | LOW | 0 external importers; method is `private` |
| Coupling | LOW | Only reads instance fields / constants; no cross-class state mutation |
| Churn | MEDIUM | 12 commits in 90 days — verify no concurrent in-flight branch edits |
| Complexity | HIGH (current) | CYC 21 is the motivation for refactoring, not a risk of it |
| Extraction safety | LOW | Pure decomposition — same code paths, same call sites, same side-effects |

The only non-trivial risk is a concurrent branch modifying `IsOrderAllowed` during
this work. Confirm no open PRs touch `V12_002.UI.Compliance.cs` before beginning
Phase 2.

## Success Criteria
- `IsOrderAllowed` CYC ≤ 8 after refactoring
- All extracted helpers individually have CYC ≤ 8
- `dotnet build` passes with zero new warnings or errors
- No behavioral change: all `[COMPLIANCE BLOCKED]` log messages fire under identical
  conditions as before
- `_uiCallbackFailures` incremented on the same exception paths as before
