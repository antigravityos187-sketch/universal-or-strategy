# Ph3.5 Ticket Review — BWAVE-REFACTOR Lane D
## Reviewer: ptt-ticket-reviewer

## Verdict: APPROVED with 1 correction

### Review Findings

1. **D-1**: No action needed confirmed. Tickets correctly identify it as complete.

2. **D-2**: CSharpier format approach correct and safe. Both files confirmed failing check.

3. **D-3**: Single line replacement. Correct fix.

4. **D-4a**: `WouldRecordBeTargetFill` confirmed `internal` — accessible without reflection flags workaround. Ticket uses `BindingFlags.NonPublic | BindingFlags.Instance` which is correct since it's on an instance.
   **APPROVED.**

5. **D-4b**: 
   - Rename approach is correct.
   - `TryFireFollowerBeRetry` confirmed `private void` taking `OrderEventArgs e` (from `NinjaTrader.Cbi`).
   - **CORRECTION**: Ticket uses `typeof(NinjaTrader.Cbi.OrderEventArgs)` but the test file already has `using NinjaTrader.Cbi;`, so engineer should write `typeof(OrderEventArgs)` instead.
   - **APPROVED with correction.**

6. **D-4c**: `CopyRule` is `internal readonly struct` nested inside `CopyEngine`. `GetNestedType` with `BindingFlags.NonPublic` is the correct approach. `Create` is `internal static`.
   **APPROVED.**

7. **No scope creep**: Tickets touch only test files. No .cs files outside Tests/ and CopyEngineTests.cs.

### Corrected D-4b structural test parameter type:
```csharp
Assert.Equal(typeof(OrderEventArgs), parms[0].ParameterType);
```
(not `typeof(NinjaTrader.Cbi.OrderEventArgs)`)

Engineer must apply this correction.
