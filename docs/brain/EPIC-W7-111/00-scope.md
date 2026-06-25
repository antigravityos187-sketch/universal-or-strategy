# Phase 1: Scope Definition - EPIC-W7-111

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.0
- **Execution Time**: 2026-06-24T00:00:00Z

---

## Method Under Refactoring

| Property            | Value                                      |
|---------------------|--------------------------------------------|
| **Method**          | `HydrateExpectedPositionsFromBroker`       |
| **File**            | `src/V12_002.SIMA.Lifecycle.cs`            |
| **Lines**           | 208–300 (93 lines)                         |
| **Visibility**      | `private void`                             |
| **Parameters**      | None                                       |
| **Cyclomatic Complexity (current)** | 18                       |
| **Max Nesting Depth (current)** | 8                            |
| **Target CYC**      | ≤ 8 per method after extraction            |

The method iterates over all fleet accounts, then separately over the master account, reading broker positions and seeding each non-flat position into the actor queue via `Enqueue`. Both paths share the same position-matching, signed-quantity, enqueue, print, and error-handling structure, but are written in full duplicate.

---

## IN SCOPE — Extractions Required

To reduce CYC from 18 to ≤ 8, the following private helper methods will be extracted. All helpers remain in `src/V12_002.SIMA.Lifecycle.cs` inside the same partial class.

### 1. `TryGetSignedQuantityForInstrument(Position pos) → int?`
- **What it does**: Accepts a single `Position`, validates `pos != null`, `pos.Instrument != null`, `pos.Instrument.FullName == Instrument.FullName`, and `pos.MarketPosition != MarketPosition.Flat`. Returns the signed integer quantity (`+pos.Quantity` for Long, `-pos.Quantity` otherwise) or `null` if the position does not match.
- **Eliminates from orchestrator**: The compound `if`-guard (lines 221–226 and 261–264) plus the signed-quantity ternary (lines 228, 266) — removes ~4 decision points per site (×2 sites = ~8 CYC units).
- **Estimated CYC of helper**: 4

### 2. `TrySeedAccountPositionFromBroker(Account acct, IReadOnlyList<Position> snapshot) → bool`
- **What it does**: Iterates `snapshot`, calls `TryGetSignedQuantityForInstrument` for each position, and on the first non-null result: captures variables, calls `Enqueue`, calls `Print`, returns `true`. Returns `false` if no matching position is found.
- **Eliminates from orchestrator**: The inner `foreach` + `break` pattern that appears in both the fleet-account loop (lines 219–241) and the master-account block (lines 258–287) — consolidates two duplicated 11-line blocks into one.
- **Estimated CYC of helper**: 3

### 3. `TrySeedAccountPositionFromBrokerSafe(Account acct) → bool`
- **What it does**: Wraps the `acct.Positions.ToArray()` snapshot call and a call to `TrySeedAccountPositionFromBroker` in a `try/catch (Exception ex)`. Prints the warning on catch and returns `false`. Returns the result of the inner call on success.
- **Eliminates from orchestrator**: Both `try/catch` blocks (lines 216–246, 256–298) — each carrying a `catch` clause (1 CYC each) and the snapshot-then-iterate pattern.
- **Estimated CYC of helper**: 2

**Orchestrator after extraction (residual CYC estimate)**:

```
HydrateExpectedPositionsFromBroker():
  int hydratedCount = 0;                                     // 0
  foreach (Account acct in Account.All)                      // +1
    if (!IsFleetAccount(acct)) continue;                     // +1
    if (TrySeedAccountPositionFromBrokerSafe(acct))          // (delegated)
      hydratedCount++;
  if (hydratedCount > 0) Print(…);                           // +1
  bool masterIsFleet993 = IsFleetAccount(Account);           // 0
  if (!masterIsFleet993)                                     // +1
    if (TrySeedAccountPositionFromBrokerSafe(Account))       // (delegated)
      hydratedCount++;
```

**Residual CYC ≈ 4** — well within the ≤ 8 threshold.

---

## OUT OF SCOPE

The following are explicitly excluded from this refactoring:

1. **Signature change** — `HydrateExpectedPositionsFromBroker()` keeps its exact signature: `private void`, no parameters, no return value.
2. **Behavior change** — All observable side-effects are preserved verbatim: `Enqueue` calls, `Print` messages (including format strings), `break` after first match, `hydratedCount` increment, and the final summary print.
3. **Other methods in the file** — `EnumerateApexAccounts`, `ProcessInitializeSIMA`, and all other methods in `V12_002.SIMA.Lifecycle.cs` are untouched.
4. **Caller changes** — `EnumerateApexAccounts` (line 140) and `ProcessInitializeSIMA` (line 90) are not modified.
5. **Cross-file changes** — No changes to `src/V12_002.cs`, `src/V12_002.SIMA.cs`, `src/V12_002.Perf.LogBuffer.cs`, or any other file.
6. **Logic unification beyond structure** — The fleet-account path and master-account path retain their distinct log labels (`"(Master)"` suffix) and any diverging null-check style (`pos.Instrument?.FullName` vs `pos.Instrument.FullName`). These are preserved as-is inside `TryGetSignedQuantityForInstrument` if needed, or via an overload parameter.
7. **Unit test creation** — Tests are deferred to a later phase; this scope phase defines only the extraction plan.
8. **Logging format changes** — All `Print` / `LogBuffer.Format` call-sites retain their exact format strings.

---

## Extraction Plan

### Step 1 — Extract `TryGetSignedQuantityForInstrument`

```csharp
/// <summary>
/// Returns the signed position quantity if <paramref name="pos"/> matches
/// this instrument and is non-flat; otherwise returns null.
/// </summary>
private int? TryGetSignedQuantityForInstrument(Position pos)
{
    if (pos == null || pos.Instrument == null)
        return null;
    if (pos.Instrument.FullName != Instrument.FullName)
        return null;
    if (pos.MarketPosition == MarketPosition.Flat)
        return null;
    return pos.MarketPosition == MarketPosition.Long ? pos.Quantity : -pos.Quantity;
}
```

### Step 2 — Extract `TrySeedAccountPositionFromBroker`

```csharp
/// <summary>
/// Scans <paramref name="snapshot"/> for the first position matching this
/// instrument, seeds it into the actor queue, and returns true if found.
/// </summary>
private bool TrySeedAccountPositionFromBroker(Account acct, Position[] snapshot)
{
    foreach (Position pos in snapshot)
    {
        int? qty = TryGetSignedQuantityForInstrument(pos);
        if (qty == null) continue;

        var capturedAcct = acct.Name;
        var capturedQty  = qty.Value;
        Enqueue(ctx => ctx.AddOrUpdateExpectedPosition(ExpKey(capturedAcct), capturedQty, v => capturedQty));
        Print($"[SIMA HYDRATE] {acct.Name}: Seeded expected={capturedQty} from broker ({pos.MarketPosition} {pos.Quantity})");
        return true;
    }
    return false;
}
```

> **Note**: The master-account variant uses a `string.Format` style and appends `"(Master)"`. This will be handled by a `bool isMaster` parameter or a separate label parameter — exact signature decided in Phase 2 Architecture. The key constraint is that log output is byte-for-byte identical.

### Step 3 — Extract `TrySeedAccountPositionFromBrokerSafe`

```csharp
/// <summary>
/// Calls <see cref="TrySeedAccountPositionFromBroker"/> inside a try/catch,
/// printing a warning on failure. Returns false on exception.
/// </summary>
private bool TrySeedAccountPositionFromBrokerSafe(Account acct)
{
    try
    {
        return TrySeedAccountPositionFromBroker(acct, acct.Positions.ToArray());
    }
    catch (Exception ex)
    {
        Print($"[SIMA HYDRATE] WARNING: Could not read positions for {acct.Name}: {ex.Message}");
        return false;
    }
}
```

### Step 4 — Rewrite orchestrator body

Replace the body of `HydrateExpectedPositionsFromBroker` to call the helpers, preserving all logic flow and the `hydratedCount` summary print.

---

## CYC Reduction Summary

| Method                                      | CYC Before | CYC After |
|---------------------------------------------|-----------|-----------|
| `HydrateExpectedPositionsFromBroker`        | 18        | ~4        |
| `TryGetSignedQuantityForInstrument` (new)   | —         | 4         |
| `TrySeedAccountPositionFromBroker` (new)    | —         | 3         |
| `TrySeedAccountPositionFromBrokerSafe` (new)| —         | 2         |
| **Max CYC across all methods**              | **18**    | **4**     |

All resulting methods are ≤ 8 CYC. The Jane Street strict threshold is met.

---

## Risk Assessment

| Risk                                              | Likelihood | Severity | Mitigation                                                              |
|---------------------------------------------------|-----------|----------|-------------------------------------------------------------------------|
| Log output diverges (format string mismatch)      | Low       | Medium   | Copy format strings verbatim; diff against Phase 0 baseline in review  |
| `break`-after-first-match semantics lost          | Low       | High     | `TrySeedAccountPositionFromBroker` returns on first match — same effect |
| Master account capture variable naming differs    | Low       | Low      | Phase 2 architecture resolves; flagged here as known divergence         |
| `acct.Positions.ToArray()` snapshot timing shifts | Very Low  | Low      | Snapshot call stays inside the same `try` block                        |
| Null-check style divergence between two sites     | Low       | Low      | `TryGetSignedQuantityForInstrument` normalises both paths               |
| Blast radius: 0 external files affected           | N/A       | None     | Confirmed in Phase 0; no callers outside the file                      |

Overall refactoring risk: **LOW**.

---

## Success Criteria

1. `HydrateExpectedPositionsFromBroker` CYC ≤ 8 after extraction.
2. All three new helper methods individually have CYC ≤ 8.
3. Max nesting depth in `HydrateExpectedPositionsFromBroker` reduced from 8 to ≤ 3.
4. Method signature `private void HydrateExpectedPositionsFromBroker()` is unchanged.
5. All `Print` output strings are byte-for-byte identical to the pre-refactoring output.
6. All `Enqueue` / `AddOrUpdateExpectedPosition` calls are structurally identical (captured variables, key, factory lambda).
7. Zero changes outside `src/V12_002.SIMA.Lifecycle.cs`.
8. Compilation succeeds with no new warnings.
9. Callers `EnumerateApexAccounts` and `ProcessInitializeSIMA` require no changes.
