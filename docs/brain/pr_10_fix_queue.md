# PR #10 Fix Queue - Prioritized Repair List
Generated: 2026-05-31 18:46:00

## Instructions for v12-engineer

Process these fixes in priority order. Mark each as [x] FIXED after applying and verifying locally.

**CRITICAL**: Run `powershell -File .\scripts\pre_push_validation.ps1 -Fast` after each fix batch.

---

## Fix #1 - [P0] Race Condition in Stale Pending Handling
[x] **Priority:** CRITICAL (Correctness)
[x] **File:** `src/V12_002.Orders.Management.StopSync.cs:371`
[x] **Bot Consensus:** Amazon Q Developer
[x] **Jane Street Violation:** Lock-Free Concurrency

**Issue:**
`UpdateStopQuantity_HandleStalePending` returns `true` unconditionally, even if `TryRemove` fails. This triggers duplicate stop-resize cycles when removal fails.

**Fix:**
```csharp
// BEFORE (line 371):
pendingStopReplacements.TryRemove(entryName, out _);
return true;  // Always signals re-initiation

// AFTER:
if (pendingStopReplacements.TryRemove(entryName, out _))
{
    return true;  // Stale removed, re-initiate
}
return false;  // Removal failed, do not re-initiate
```

**Verification:**
- [x] Build succeeds
- [x] No new lint errors
- [x] Logic: Only re-initiate if removal succeeds

---

## Fix #2 - [P0] Missing Null Guard on Stop Order Creation
[x] **Priority:** CRITICAL (Correctness)
[x] **File:** `src/V12_002.SIMA.Dispatch.cs:708`
[x] **Bot Consensus:** Cubic Dev AI, Gemini Code Assist
[x] **Jane Street Violation:** Fail-Fast Isolation

**Issue:**
No null guard after `CreateOrder` for stop order. Null can propagate into submission/tracking paths.

**Fix:**
```csharp
// AFTER line 708 (after CreateOrder call):
Order stop = acct.CreateOrder(Instrument, exitAction, OrderType.StopMarket, ...);
if (stop == null)
{
    LogCritical($"[PublishPhoton_StopOrder] CreateOrder returned null for {fleetEntryName}");
    return null;  // Fail-fast
}
ordersToSubmit.Add(stop);
```

**Verification:**
- [x] Build succeeds
- [x] Null check before queueing
- [x] LogCritical includes context

---

## Fix #3 - [P0] Missing Null Guard on Target Order Creation
[x] **Priority:** CRITICAL (Correctness)
[x] **File:** `src/V12_002.SIMA.Dispatch.cs:774`
[x] **Bot Consensus:** Cubic Dev AI
[x] **Jane Street Violation:** Fail-Fast Isolation

**Issue:**
No null guard after `CreateOrder` for target orders. Null targets leak into `stagedTargets` and `ordersToSubmit`.

**Fix:**
```csharp
// AFTER line 774 (after CreateOrder call in target loop):
Order target = acct.CreateOrder(Instrument, exitAction, OrderType.Limit, ...);
if (target == null)
{
    dispatchLog.AppendLine($"[Target {targetNum}] CreateOrder returned null - skipping");
    continue;  // Skip this target, don't add to staged/submit lists
}
stagedTargets.Add(...);
ordersToSubmit.Add(target);
```

**Verification:**
- [x] Build succeeds
- [x] Null check before staging
- [x] Loop continues on null (doesn't break)

---

## Fix #4 - [P0] stopPrice Parameter Ignored
[x] **Priority:** CRITICAL (API Contract)
[x] **File:** `src/V12_002.SIMA.Dispatch.cs:690-694`
[x] **Bot Consensus:** Sourcery AI, Gitar, Cubic Dev AI, CodeRabbit AI (4 bots)
[x] **Jane Street Violation:** Correctness by Construction

**Issue:**
`PublishPhoton_StopOrder` accepts `stopPrice` parameter but ignores it, using `fleetPos.CurrentStopPrice` instead. If they diverge, wrong price used silently.

**Fix (Option 1 - Use the parameter):**
```csharp
// Line 694 - BEFORE:
double validatedStop = ValidateStopPrice(fleetPos.Direction, fleetPos.CurrentStopPrice);

// Line 694 - AFTER:
double validatedStop = ValidateStopPrice(fleetPos.Direction, stopPrice);
```

**Fix (Option 2 - Remove parameter if truly unused):**
```csharp
// Line 690 - Remove stopPrice parameter:
private Order PublishPhoton_StopOrder(
    Account acct,
    OrderAction exitAction,
    PositionInfo fleetPos,
    string fleetEntryName,
    string ocoId,
    // REMOVE: double stopPrice,
    ref List<Order> ordersToSubmit
)

// Line 484 - Update caller to not pass stopPrice
```

**Recommendation:** Use Option 1 (use the parameter) - caller explicitly passes it.

**Verification:**
- [x] Build succeeds
- [x] Parameter is used (Option 1 applied)
- [x] Caller passes stopPrice correctly

---

## Fix #5 - [P0] Orphaned Documentation Comment
[x] **Priority:** CRITICAL (Maintainability)
[x] **File:** `src/V12_002.Orders.Management.StopSync.cs:338-351`
[x] **Bot Consensus:** CodeRabbit AI
[x] **Jane Street Violation:** Documentation Correctness

**Issue:**
XML doc comment for `UpdateStopQuantity` (lines 338-346) is now attached to `UpdateStopQuantity_HandleStalePending` instead of the intended method at line 493.

**Fix:**
```csharp
// STEP 1: Remove lines 338-346 from before UpdateStopQuantity_HandleStalePending

// STEP 2: Add immediately before UpdateStopQuantity at line 493:
/// <summary>
/// Updates the stop order quantity after a partial target fill.
/// </summary>
/// <remarks>
/// V12.Audit [C-08]: Callers MUST ensure the <paramref name="pos"/> reference is
/// read under <c>stateLock</c> or from within a callback that is already serialized
/// by the NinjaTrader dispatch thread. Passing a stale <paramref name="pos"/> can
/// result in the stop being undersized relative to actual remaining contracts.
/// </remarks>
private void UpdateStopQuantity(string entryName, PositionInfo pos)
```

**Verification:**
- [x] Build succeeds
- [x] IntelliSense shows correct doc on UpdateStopQuantity
- [x] UpdateStopQuantity_HandleStalePending has its own helper summary

---

## Fix #6 - [P1] Emergency Flatten Logic Too Restrictive
[x] **Priority:** HIGH (False Positives)
[x] **File:** `src/V12_002.Orders.Management.StopSync.cs:444-450`
[x] **Bot Consensus:** Codacy Production, Gemini Code Assist
[x] **Jane Street Violation:** Correctness by Construction

**Issue:**
Emergency flatten check uses exact name match (`o.Name == entryName`), but stop orders use prefix `"S_" + entryName` (may be truncated to 50 chars or include suffixes). This causes false-positive liquidations.

**Fix:**
```csharp
// BEFORE (line 444-450):
bool hasActiveStop = acct.Orders.Any(o =>
    o.OrderState == OrderState.Working &&
    o.IsStopMarket &&
    o.Name == entryName  // Exact match - too narrow
);

// AFTER:
// Compute expected stop name prefix (as SubmitStopOrderToBroker does)
string stopPrefix = "S_" + entryName;
if (stopPrefix.Length > 50)
{
    stopPrefix = stopPrefix.Substring(0, 50);
}

bool hasActiveStop = acct.Orders.Any(o =>
    o.OrderState == OrderState.Working &&
    o.IsStopMarket &&
    o.Name != null &&
    o.Name.StartsWith(stopPrefix)  // Prefix match - catches truncated/suffixed names
);
```

**Verification:**
- [x] Build succeeds
- [x] Prefix logic matches SubmitStopOrderToBroker
- [x] Handles truncation to 50 chars

---

## Fix #7 - [P1] PublishPhoton_EntryOrder is No-Op
[x] **Priority:** HIGH (Dead Code)
[x] **File:** `src/V12_002.SIMA.Dispatch.cs:674-678`
[x] **Bot Consensus:** Sourcery AI, Gitar, CodeRabbit AI
[x] **Jane Street Violation:** Cognitive Clarity

**Issue:**
Method does nothing (entry already in list from caller). Adds indirection without value.

**Fix (Option 1 - Remove method):**
```csharp
// STEP 1: Delete lines 674-678 (PublishPhoton_EntryOrder method)

// STEP 2: Remove invocation at line 479:
// BEFORE:
PublishPhoton_EntryOrder(entry, ref ordersToSubmit);

// AFTER:
// Entry already added to ordersToSubmit by caller - no additional MMIO publish needed
```

**Fix (Option 2 - Mark as TODO if future logic planned):**
```csharp
/// <summary>
/// Phase 7 NEW-3 Helper 1: Publish entry order to MMIO.
/// TODO: Implement MMIO publish when architecture requires it.
/// Entry is currently added to ordersToSubmit by caller.
/// </summary>
private void PublishPhoton_EntryOrder(Order entry, ref List<Order> ordersToSubmit)
{
    // No additional MMIO publishing needed yet
    // Future: Add MMIO mirror write here if required
}
```

**Recommendation:** Use Option 1 (remove) unless future MMIO logic is planned.

**Verification:**
- [x] Build succeeds
- [x] Method removed (Option 1 applied)
- [x] Caller updated with inline comment

---

## Fix #8 - [P1] DateTime.Now vs DateTime.UtcNow
[x] **Priority:** HIGH (Reliability)
[x] **File:** `src/V12_002.Orders.Management.StopSync.cs` (staleness tracking)
[x] **Bot Consensus:** Gemini Code Assist
[x] **Jane Street Violation:** Timezone-Independent Behavior

**Issue:**
Using `DateTime.Now` instead of `DateTime.UtcNow` for staleness checks. Timezone-dependent behavior is unreliable in HFT systems.

**Fix:**
```csharp
// Find all instances of DateTime.Now in staleness logic and replace with DateTime.UtcNow
// Example locations:
// - UpdateStopQuantity_HandleStalePending (staleness check)
// - Any other time-based comparisons

// BEFORE:
DateTime now = DateTime.Now;

// AFTER:
DateTime now = DateTime.UtcNow;
```

**Verification:**
- [x] Build succeeds
- [x] All DateTime.Now replaced with DateTime.UtcNow in staleness logic
- [x] Grep confirms no remaining DateTime.Now in StopSync.cs

---

## Fix #9 - [P2] Trailing Comment Cleanup (Non-Blocking)
[x] **Priority:** LOW (Style)
[x] **File:** `src/V12_002.SIMA.Dispatch.cs:1074`
[x] **Bot Consensus:** Gitar
[x] **Jane Street Violation:** None (style only)

**Issue:**
`// Made with Bob` comment at EOF is non-functional noise.

**Fix:**
```csharp
// Remove line 1074:
// Made with Bob
```

**Verification:**
- [x] Build succeeds
- [x] No functional impact

---

## Post-Fix Validation Checklist

After completing all fixes:

- [ ] Run: `powershell -File .\scripts\pre_push_validation.ps1`
- [ ] Verify: Zero build errors
- [ ] Verify: Zero new lint errors
- [ ] Verify: All P0 fixes applied
- [ ] Verify: All P1 fixes applied
- [ ] Run: `powershell -File .\deploy-sync.ps1` (hard-link sync)
- [ ] Test: F5 in NinjaTrader (smoke test)
- [ ] Commit: "fix(phase7-new3): Address 8 bot findings from PR #10 forensics"

---

## Status Tracking

**Total Fixes:** 9  
**P0 (Critical):** 5  
**P1 (High):** 3  
**P2 (Low):** 1  

**Completion Status:**
- [x] All P0 fixes applied (5/5)
- [x] All P1 fixes applied (3/3)
- [x] P2 fix applied (1/1)
- [ ] Pre-push validation passed (PENDING - requires Orchestrator)
- [ ] Hard-link sync completed (PENDING - requires Orchestrator)
- [ ] Smoke test passed (PENDING - requires Orchestrator)

**Next Step:** Return control to Orchestrator for Step 3 (Verification)
