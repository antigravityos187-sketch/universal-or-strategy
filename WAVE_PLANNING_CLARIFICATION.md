# Wave Planning Clarification - Epic Calculation Methodology

## Your Question

> "How come you are saying 25 epics when before you said 100+ epics? I'm guessing you were referring to 1 epic = 1 method and that is why it was 100+. Do we need 1 epic per 1 method? How did you come up with the 25 epics number?"

## Answer: Epic Grouping Strategy

### The Confusion

**You're absolutely right!** I was inconsistent. Let me clarify:

**Previous Statement** (100+ epics):
- Assumed: **1 epic = 1 method**
- Total methods >8: 180
- Therefore: 180 epics needed

**Current Statement** (25 epics):
- Assumed: **1 epic = ~7 methods** (grouped)
- Total methods >8: 180
- Therefore: 180 ÷ 7 = ~25 epics

### The Reality: We Have Options

**Option 1: 1 Epic = 1 Method** (Micro-Epics)
- **Total Epics**: 180
- **Tickets per Epic**: 1-3 (depending on method complexity)
- **Pros**: Simple, focused, easy to parallelize
- **Cons**: High overhead (180 Phase 0-4 cycles)

**Option 2: 1 Epic = ~7 Methods** (Grouped Epics)
- **Total Epics**: ~25
- **Tickets per Epic**: 7-10 (one per method)
- **Pros**: Lower overhead, better parallelism
- **Cons**: More complex planning

**Option 3: 1 Epic = File-Based** (Natural Grouping)
- **Total Epics**: Varies (depends on file clustering)
- **Tickets per Epic**: All methods in that file
- **Pros**: Logical grouping, easier to review
- **Cons**: Uneven epic sizes

---

## Recommended Strategy: Hybrid Approach

### Wave 3: File-Based Grouping (Natural Clusters)

**Rationale**: Methods in the same file often share context and dependencies.

**Example from Complexity Audit**:

#### Epic Group 1: SIMA.Fleet.cs (4 methods >8)
- `ShouldSkipFleet_RunHealthCheck` (CYC=31)
- `VerifyPhotonSlotIntegrity` (CYC=14)
- `InitializeFollowerBracketFSM` (CYC=13)
- `ProcessFleetSlot` (CYC=11)

**Epic**: EPIC-CCN-115 (SIMA Fleet Health)
**Tickets**: 4 (one per method)
**Estimated Time**: ~60 minutes

#### Epic Group 2: Orders.Callbacks.cs (6 methods >8)
- `HandleSecondaryOrderFilled` (CYC=21)
- `ProcessOnOrderUpdate` (CYC=19)
- `RequestStopCancelLifecycleSafe` (CYC=12)
- `HandleOrderRejected` (CYC=12)
- `HandleOrderPriceOrQuantityChanged` (CYC=11)
- `HandleOrderCancelled_ProcessStopReplacement` (CYC=10)

**Epic**: EPIC-CCN-116 (Order Callbacks Refactor)
**Tickets**: 6 (one per method)
**Estimated Time**: ~90 minutes

### Actual Epic Count Calculation

Let me analyze the complexity audit to give you the REAL number:

**Files with Methods >8**: Let me count...

From the audit output, I can see methods are spread across these files:
1. SignalBroadcaster.cs (1 method)
2. V12_002.BarUpdate.cs (2 methods)
3. V12_002.DrawingHelpers.cs (1 method)
4. V12_002.Entries.FFMA.cs (3 methods)
5. V12_002.Entries.MOMO.cs (1 method)
6. V12_002.Entries.OR.cs (1 method)
7. V12_002.Entries.Retest.cs (1 method)
8. V12_002.Entries.RMA.cs (1 method)
9. V12_002.Entries.Trend.cs (1 method)
10. V12_002.IPC.Hardening.cs (1 method)
11. V12_002.Lifecycle.cs (4 methods)
12. V12_002.Orders.Callbacks.AccountOrders.cs (14 methods)
13. V12_002.Orders.Callbacks.cs (6 methods)
14. V12_002.Orders.Callbacks.Execution.cs (5 methods)
15. V12_002.Orders.Callbacks.Propagation.cs (7 methods)
16. V12_002.Orders.Management.Cleanup.cs (7 methods)
17. V12_002.Orders.Management.cs (1 method)
18. V12_002.Orders.Management.Flatten.cs (4 methods)
19. V12_002.Orders.Management.StopSync.cs (5 methods)
20. V12_002.Perf.LogBuffer.cs (1 method)
21. V12_002.REAPER.Audit.cs (7 methods)
22. V12_002.REAPER.Repair.cs (1 method)
23. V12_002.Safety.Watchdog.cs (3 methods)
24. V12_002.SIMA.cs (1 method)
25. V12_002.SIMA.Dispatch.cs (3 methods)
26. V12_002.SIMA.Execution.cs (4 methods)
27. V12_002.SIMA.Flatten.cs (2 methods)
28. V12_002.SIMA.Fleet.cs (6 methods)
29. V12_002.SIMA.Lifecycle.cs (9 methods)
30. V12_002.SIMA.Shadow.cs (2 methods)
31. V12_002.StickyState.cs (1 method)
32. V12_002.Symmetry.BracketFSM.cs (5 methods)
33. V12_002.Symmetry.cs (2 methods)
34. V12_002.Symmetry.Follower.cs (3 methods)
35. V12_002.Symmetry.Replace.cs (5 methods)
36. V12_002.Trailing.Breakeven.cs (3 methods)
37. V12_002.Trailing.cs (3 methods)
38. V12_002.Trailing.StopUpdate.cs (4 methods)
39. V12_002.UI.Callbacks.cs (2 methods)
40. V12_002.UI.Compliance.cs (8 methods)
41. V12_002.UI.IPC.Commands.Config.cs (3 methods)
42. V12_002.UI.IPC.Commands.Fleet.cs (6 methods)
43. V12_002.UI.IPC.Commands.Misc.cs (2 methods)
44. V12_002.UI.IPC.Commands.Mode.cs (2 methods)
45. V12_002.UI.IPC.cs (5 methods)
46. V12_002.UI.IPC.Server.cs (2 methods)
47. V12_002.UI.Panel.Construction.cs (2 methods)
48. V12_002.UI.Panel.Handlers.cs (5 methods)
49. V12_002.UI.Panel.Helpers.cs (4 methods)
50. V12_002.UI.Panel.StateSync.cs (4 methods)
51. V12_002.UI.Sizing.cs (1 method)
52. V12_002.UI.Snapshot.cs (2 methods)
53. V12_002.UI.SnapshotPool.cs (1 method)

**Total Files with Methods >8**: ~53 files

---

## Corrected Wave Planning

### Strategy: 1 Epic = 1 File (or File Group)

**Total Epics Needed**: **~53 epics** (not 25, not 180)

**Why 53?**
- Each file becomes 1 epic
- Methods in same file share context
- Natural grouping by concern

### Wave Distribution

**Wave 3**: 20 epics (highest priority files)
- Focus on files with most methods >8
- Example: Orders.Callbacks.AccountOrders.cs (14 methods)

**Wave 4**: 20 epics (medium priority files)
- Files with 3-6 methods >8

**Wave 5**: 13 epics (remaining files)
- Files with 1-2 methods >8

**Total Waves**: **3 waves** (not counting Wave 2)

---

## Time Estimates (Revised)

### Per Epic (File-Based)
- **Phase 0-4**: ~55 minutes (planning)
- **Phase 5**: ~10 minutes per method (execution)
- **Phase 5.V**: ~5 minutes per method (verification)
- **Phase 6**: ~10 minutes (review)

**Example**: Orders.Callbacks.AccountOrders.cs (14 methods)
- Planning: 55 min
- Execution: 14 × 10 = 140 min
- Verification: 14 × 5 = 70 min
- Review: 10 min
- **Total**: 275 minutes (~4.5 hours)

### Total Time (53 Epics)

**Sequential Execution**:
- Average 6 methods per epic
- ~150 minutes per epic
- 53 epics × 150 min = 7,950 minutes
- **Total**: ~132 hours

**With Optimizations** (2 VMs, 10 agents each):
- Parallel epic execution: 2x speedup
- Parallel ticket execution: 1.5x speedup
- 7-phase consolidation: 10% speedup
- **Total**: ~40 hours

---

## Why I Said 25 Epics (My Mistake)

I incorrectly calculated:
- 180 methods ÷ 7 methods/epic = ~25 epics

**What I Should Have Said**:
- 180 methods across 53 files
- 1 epic per file (or file group)
- **53 epics total**

---

## Final Answer

### How Many Waves?

**Option 1: Conservative** (20 epics per wave)
- Wave 3: 20 epics
- Wave 4: 20 epics
- Wave 5: 13 epics
- **Total**: 3 waves

**Option 2: Aggressive** (30 epics per wave)
- Wave 3: 30 epics
- Wave 4: 23 epics
- **Total**: 2 waves

**Option 3: Single Wave** (53 epics)
- Wave 3: All 53 epics in parallel
- Requires 3+ VMs
- **Total**: 1 wave (~40 hours with full parallelism)

### Recommendation

**Start with Option 1** (3 waves, 20 epics each):
- More manageable
- Easier to course-correct
- Lower risk
- Can accelerate if successful

---

## Summary

**Your Question**: "How did you come up with 25 epics?"

**My Answer**: I made a calculation error. The correct number is:

- **53 epics** (1 per file with methods >8)
- **3 waves** (20 + 20 + 13 epics)
- **~40 hours** (with optimizations)

**Not**:
- ❌ 180 epics (1 per method) - too granular
- ❌ 25 epics (7 methods each) - arbitrary grouping
- ✅ 53 epics (1 per file) - natural grouping

Does this clarify the wave planning?