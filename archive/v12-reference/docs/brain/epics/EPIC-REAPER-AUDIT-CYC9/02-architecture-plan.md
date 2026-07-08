# EPIC-REAPER-AUDIT-CYC9 -- Phase 2: Architecture Plan

**Protocol**: V12.25 Manifest-Based Independent Subtasks
**Agent**: v12-phase2-architecture
**Date**: 2026-06-15
**Depends on**: 01-scope-boundary.md (HOLD resolved -- IsWorkingOrderState rename adopted)

---

## 1. Overview

Reduce `AuditMaster_IsWorkingStopOrder` from CYC=9 to CYC=6 by extracting three
inline boolean expressions into private static expression-body helpers.

| Metric | Before | After |
|--------|--------|-------|
| Parent CYC | 9 | 6 |
| Helper count | 0 | 3 |
| Each helper CYC | -- | 2 |
| Files changed | 0 | 1 |
| Public API changes | -- | 0 |
| External callers | -- | 0 (blast radius ZERO) |

---

## 2. Target Method (BEFORE)

**File**: `src/V12_002.REAPER.Audit.cs`
**Lines**: 752-763 (comment + method)

```csharp
        // Extracted helper: evaluates whether a single order qualifies as an active protective stop.
        private bool AuditMaster_IsWorkingStopOrder(Order o, string instrName)
        {
            if (o == null || o.Instrument?.FullName != instrName)
            {
                return false;
            }
            bool isActive = o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted;
            bool isStop = o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit;
            bool isProtective = o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover;
            return isActive && isStop && isProtective;
        }
```

---

## 3. CYC=9 Branch-Count Proof (BEFORE)

| Branch | +CYC | Running Total |
|--------|------|---------------|
| Base | -- | 1 |
| `if (o == null ...)` if-statement | +1 | 2 |
| `o == null \|\|` short-circuit OR in guard | +1 | 3 |
| `o.Instrument?.FullName` null-conditional `?.` | +1 | 4 |
| `OrderState.Working \|\|` in isActive line | +1 | 5 |
| `OrderType.StopMarket \|\|` in isStop line | +1 | 6 |
| `OrderAction.Sell \|\|` in isProtective line | +1 | 7 |
| First `&&` in `return isActive && ...` | +1 | 8 |
| Second `&&` in `return ... isStop && ...` | +1 | **9** |

**CYC = 9. Threshold = 8. Violation confirmed.**

---

## 4. Three Private Static Helpers (AFTER -- new code)

All three helpers are placed immediately after the closing `}` of
`AuditMaster_IsWorkingStopOrder` (after line 763, before the blank line +
comment block of `AuditMasterAccountIfNeeded` at line 765).

```csharp
        private static bool IsWorkingOrderState(Order o) =>
            o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted;

        private static bool IsStopOrderType(Order o) =>
            o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit;

        private static bool IsProtectiveAction(Order o) =>
            o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover;
```

### CYC per Helper

| Helper | Branches | CYC |
|--------|----------|-----|
| `IsWorkingOrderState` | base(1) + one `\|\|`(+1) | **2** |
| `IsStopOrderType` | base(1) + one `\|\|`(+1) | **2** |
| `IsProtectiveAction` | base(1) + one `\|\|`(+1) | **2** |

All helpers CYC=2. Compliant (<=8).

---

## 5. Target Method (AFTER)

Replace lines 752-763 with the following:

```csharp
        // Extracted helper: evaluates whether a single order qualifies as an active protective stop.
        private bool AuditMaster_IsWorkingStopOrder(Order o, string instrName)
        {
            if (o == null || o.Instrument?.FullName != instrName)
            {
                return false;
            }
            return IsWorkingOrderState(o) && IsStopOrderType(o) && IsProtectiveAction(o);
        }

        private static bool IsWorkingOrderState(Order o) =>
            o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted;

        private static bool IsStopOrderType(Order o) =>
            o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit;

        private static bool IsProtectiveAction(Order o) =>
            o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover;
```

---

## 6. Parent CYC=6 Branch-Count Proof (AFTER)

| Branch | +CYC | Running Total |
|--------|------|---------------|
| Base | -- | 1 |
| `if (o == null ...)` if-statement | +1 | 2 |
| `o == null \|\|` short-circuit OR in guard | +1 | 3 |
| `o.Instrument?.FullName` null-conditional `?.` | +1 | 4 |
| First `&&` in `return IsWorkingOrderState(o) && ...` | +1 | 5 |
| Second `&&` in `return ... IsStopOrderType(o) && ...` | +1 | **6** |

**Parent CYC after extraction = 6. Compliant (<=8). Reduction: 9 -> 6 (-3).**

The three `||` operators previously counted in the parent are now each encapsulated
inside their respective private static helpers. They no longer contribute branches
to the parent method.

---

## 7. Name Collision Verification

All V12_002.*.cs files are `public partial class V12_002`. Any duplicate member
name across ANY partial file produces CS0111.

| Helper Name | Collision Found | Notes |
|-------------|-----------------|-------|
| `IsWorkingOrderState` | **NO** | Distinct from `IsActiveOrderState` (SIMA.Lifecycle.cs:490) |
| `IsStopOrderType` | **NO** | No match found anywhere in src/ |
| `IsProtectiveAction` | **NO** | No match found anywhere in src/ |

The Phase 1.5 scope boundary identified `IsActiveOrderState` as a collision target.
This plan adopts `IsWorkingOrderState` as the renamed replacement. No further
collision risk.

---

## 8. Insertion Location

```
src/V12_002.REAPER.Audit.cs

  Line 752  // comment (existing)
  Line 753  private bool AuditMaster_IsWorkingStopOrder(...)    <- REPLACE body
  Line 763  }                                                    <- last line of method
  Line 764  (blank)                                             <- INSERT 3 helpers here
  Line 765  // Build 935 [REAPER-B935-004]: ...                 <- next method comment (unchanged)
```

The 3 helpers are inserted between the closing `}` of `AuditMaster_IsWorkingStopOrder`
and the comment block for `AuditMasterAccountIfNeeded`. This groups them with their
logical parent (AuditMaster_IsWorkingStopOrder) and matches the Jane Street helper
co-location pattern (same class, immediately adjacent).

---

## 9. Files Changed

| File | Change |
|------|--------|
| `src/V12_002.REAPER.Audit.cs` | Replace 3 local bool lines + return; insert 3 helper methods |

**No other file is touched.** This is a single-file, zero-blast-radius refactor.

---

## 10. Constraints Checklist

| Constraint | Status |
|------------|--------|
| `QueuedAccountOrderUpdate` is a struct (use `.` not `?.`) | N/A -- helpers take `Order` (ref type) |
| No `lock()` | Confirmed -- no lock in method or helpers |
| xUnit only for tests | Phase 5 must write `[Fact]` tests for all 3 helpers |
| ASCII only (no em-dash, curly quotes, Unicode > 0x7F) | Confirmed in this doc and all code |
| Private helpers only (no public surface) | `private static bool` -- compliant |
| Expression-body (`=>` syntax) | All 3 helpers use `=>` -- compliant |
| CYC <= 8 (parent) | Parent = 6 after extraction -- compliant |
| CYC <= 8 (each helper) | Each helper = 2 -- compliant |
| Blast radius | ZERO -- no external callers |
| Single-file change | YES -- src/V12_002.REAPER.Audit.cs only |

---

## 11. Phase 5 Test Requirements

Phase 5 (ticket execution) MUST write one `[Fact]` test per helper at minimum.

| Test Name | Scenario |
|-----------|----------|
| `IsWorkingOrderState_WhenWorking_ReturnsTrue` | Order.OrderState == OrderState.Working |
| `IsWorkingOrderState_WhenAccepted_ReturnsTrue` | Order.OrderState == OrderState.Accepted |
| `IsWorkingOrderState_WhenFilled_ReturnsFalse` | Order.OrderState == OrderState.Filled |
| `IsStopOrderType_WhenStopMarket_ReturnsTrue` | Order.OrderType == OrderType.StopMarket |
| `IsStopOrderType_WhenStopLimit_ReturnsTrue` | Order.OrderType == OrderType.StopLimit |
| `IsStopOrderType_WhenMarket_ReturnsFalse` | Order.OrderType == OrderType.Market |
| `IsProtectiveAction_WhenSell_ReturnsTrue` | Order.OrderAction == OrderAction.Sell |
| `IsProtectiveAction_WhenBuyToCover_ReturnsTrue` | Order.OrderAction == OrderAction.BuyToCover |
| `IsProtectiveAction_WhenBuy_ReturnsFalse` | Order.OrderAction == OrderAction.Buy |

Test project: `tests/V12_Performance.Tests/`
Framework: xUnit ([Fact], Assert.Equal / Assert.True / Assert.False)

---

## 12. Agent Tracking

| Step | Tool Used | Result |
|------|-----------|--------|
| Read verbatim method body | read_file src/V12_002.REAPER.Audit.cs:745-780 | Confirmed lines 753-763 |
| Read Phase 0 hotspot analysis | read_file 00-hotspots.md | CYC=9 branch count confirmed |
| Read Phase 1.5 boundary | read_file 01-scope-boundary.md | Name collision + rename decision confirmed |
| Architecture validation | sequentialthinking (4 thoughts) | BEFORE/AFTER design verified, CYC math correct |
| Collision re-check | From 01-scope-boundary.md | IsWorkingOrderState is collision-free |

**Validated by**: v12-phase2-architecture (Sequential Thinking + jCodemunch MCP)
**Next phase**: Phase 3 (DNA Audit) or Phase 4 (Ticket Generation)
