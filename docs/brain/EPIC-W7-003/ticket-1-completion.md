# EPIC-W7-003 Ticket 1 Completion

## Agent Tracking

| Field | Value |
|---|---|
| Epic | EPIC-W7-003 |
| Ticket | 1 of 3 |
| Cluster | S3_UI_IO |
| Engineer | V12 Photon Engineer (Phase 5) |
| Mode | v12-engineer |
| Status | COMPLETED |

## Ticket Spec

| Field | Value |
|---|---|
| helper_name | TryGetAccountBalance |
| concern | Safe broker API call with error isolation |
| signature | `private bool TryGetAccountBalance(Account acct, out double balance)` |
| source_file | src/V12_002.UI.Compliance.cs |
| parent_method | IsOrderAllowed |

## Changes Made

### New Method: TryGetAccountBalance

Added to [`src/V12_002.UI.Compliance.cs`](../../src/V12_002.UI.Compliance.cs) in the `#region Snapshot & Enforcement` block, placed immediately before `IsOrderAllowed`.

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining
)]
private bool TryGetAccountBalance(Account acct, out double balance)
{
    balance = 0;
    if (acct == null)
        return false;
    try
    {
        balance = acct.Get(
            NinjaTrader.Cbi.AccountItem.CashValue,
            NinjaTrader.Cbi.Currency.UsDollar
        );
        return true;
    }
    catch (Exception ex)
    {
        Interlocked.Increment(ref _uiCallbackFailures);
        Print($"[UI_CALLBACK] Account balance retrieval failed: {ex.Message}");
        return false;
    }
}
```

### Caller Replacement in IsOrderAllowed

Replaced 19-line try/catch block (lines 336-354 original) with one call:

```csharp
TryGetAccountBalance(this.Account, out double balance);
```

The `out double balance` declaration preserves downstream use:
```csharp
double buffer = balance - (peak - TrailingDrawdownLimit);
```

## Complexity Results

| Method | CYC Before | CYC After | Status |
|---|---|---|---|
| IsOrderAllowed | 16 | 14 | REFACTOR (tickets 2+3 continue) |
| TryGetAccountBalance | N/A | 3 | OK |

## Validation

| Check | Result |
|---|---|
| dotnet csharpier format src/ | 83 files formatted |
| dotnet build Linting.csproj | 0 errors, 0 warnings |
| TryGetAccountBalance CYC | 3 (target <= 8) |
| lock() usage | 0 (Interlocked.Increment only) |
| ASCII-only strings | confirmed |
| No scope creep | confirmed — ONE concern extracted |

## DNA Compliance

- [x] No `lock()` — `Interlocked.Increment` used
- [x] ASCII-only strings in all Print calls
- [x] `[MethodImpl(NoInlining)]` on cold-path error handler
- [x] Zero logic drift — pure structural extraction
- [x] Single concern: broker API call with error isolation

## Output

```json
{
  "status": "success",
  "helper_name": "TryGetAccountBalance",
  "cyc_achieved": 3,
  "build_passed": true
}
```
