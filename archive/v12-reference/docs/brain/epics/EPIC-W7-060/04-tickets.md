# EPIC-W7-060 — Phase 4: Ticket Definitions

**Agent Name:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Inputs:** docs/brain/EPIC-W7-060/02-architecture-plan.md + docs/brain/EPIC-W7-060/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Method** | `SweepTrackedOrders` |
| **File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Lines** | 1308–1353 (46 lines) |
| **CYC Baseline** | 11 (high) |
| **max_cyc_projected** | 5 |
| **Ticket Count** | 2 |
| **DNA Verdict** | PASS |
| **Ticket Ordering** | Sequential — T2 depends on T1 |

---

## MCP Evidence (Phase 4)

### resolve_repo
```json
{
  "found": true,
  "indexed": true,
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "symbol_count": 5147,
  "file_count": 2000,
  "status": "loadable"
}
```

### get_symbol_complexity
Symbol not surfaced by index name lookup (partial class limitation — consistent with Phase 2 finding).
**Phase 2 live MCP result used:** CYC = 11, max_nesting = 4, assessment = high.

### get_extraction_candidates
```json
{
  "file": "src/V12_002.SIMA.Lifecycle.cs",
  "candidates": [],
  "min_complexity": 5,
  "min_callers": 2
}
```
No candidates returned (partial class — only `SweepTrackedOrders` has a single caller `CancelAllV12GtcOrders`).
Extraction plan sourced directly from Phase 2 architecture evidence.

---

## Sequential Thinking Evidence

### Thought 1 — Ticket Count Decision

Two distinct, independent concerns identified in `SweepTrackedOrders`:

1. **Dict-array construction** (ternary force logic) — no knowledge of cancellation
2. **Per-dictionary sweep loop** (null guards, OrderState check, cancel call) — no knowledge of dict selection

Rule: one ticket = one concern. Result: **2 tickets**.

Ticket ordering is sequential: T2 depends on T1 having inserted `BuildTrackedDictList` before
T2 completes the parent refactor that calls both helpers.

### Thought 2 — Line Mapping and Helper Naming

**Ticket 1 — BuildTrackedDictList:**
- Lines extracted: 1312–1319 (force ternary + 7-element array construction)
- New method: `BuildTrackedDictList(bool force)`
- Returns: `ConcurrentDictionary<string, Order>[]`
- Insertion: after closing brace of `SweepTrackedOrders` (~line 1354)
- CYC projected: 2 (base=1, ternary=1)

**Ticket 2 — SweepDictionary + parent wiring:**
- Lines extracted: ~1321–1348 (inner foreach with null guards, OrderState check, try-cancel)
- New method: `SweepDictionary(ConcurrentDictionary<string, Order> dict)`
- Inline 5-condition OrderState check replaced by call to existing `IsOrderTerminal(ord)`
- Parent `SweepTrackedOrders` refactored to 3-line delegate body
- CYC projected: SweepDictionary=5, SweepTrackedOrders=2

### Thought 3 — CYC Compliance Verification

| Method | CYC After | Branches | <= 8? |
|---|---|---|---|
| `SweepTrackedOrders` (final) | 2 | base=1, foreach=1 | PASS ✅ |
| `BuildTrackedDictList` | 2 | base=1, ternary=1 | PASS ✅ |
| `SweepDictionary` | 5 | base=1, null-dict=1, foreach=1, null-ord=1, IsOrderTerminal=1 | PASS ✅ |

**max_cyc_projected = 5. All methods CYC <= 8. Threshold PASS.**

Intermediate state (after T1 only): `SweepTrackedOrders` CYC ~10 — not yet compliant.
T2 MUST execute to achieve full compliance. Sequential dependency enforced.

---

## Tickets

---

### Ticket 1 — Extract `BuildTrackedDictList`

| Field | Value |
|---|---|
| **Ticket ID** | EPIC-W7-060-T1 |
| **Title** | Extract `BuildTrackedDictList` from `SweepTrackedOrders` |
| **File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Parent Method** | `SweepTrackedOrders` (lines 1308–1353) |
| **Helper Name** | `BuildTrackedDictList` |
| **Helper Signature** | `private ConcurrentDictionary<string, Order>[] BuildTrackedDictList(bool force)` |
| **Lines Moved** | 1312–1319 (force ternary array construction) |
| **Insertion Point** | After closing brace of `SweepTrackedOrders` (~line 1354) |
| **CYC Before (parent)** | 11 |
| **CYC After (helper)** | 2 |
| **CYC After (parent, intermediate)** | ~10 (T2 required for full compliance) |
| **Dependency** | None — first ticket |
| **Caller Preserved** | `CancelAllV12GtcOrders` — DO NOT MODIFY |
| **Tests Required** | Yes — xUnit [Fact] (V12.32 mandate) |

#### Implementation Instructions

**Step 1 — Read target method** (lines 1308–1353) before any edit.

**Step 2 — Add new private method** immediately after `SweepTrackedOrders` closing brace:

```csharp
private ConcurrentDictionary<string, Order>[] BuildTrackedDictList(bool force)
{
    return force
        ? new ConcurrentDictionary<string, Order>[]
          { entryOrders, stopOrders, target1Orders, target2Orders,
            target3Orders, target4Orders, target5Orders }
        : new ConcurrentDictionary<string, Order>[] { entryOrders };
}
```

**Step 3 — Replace** the ternary in `SweepTrackedOrders` body:

```csharp
// BEFORE (lines 1312–1319):
var trackedDicts = force
    ? new ConcurrentDictionary<string, Order>[]
      { entryOrders, stopOrders, target1Orders, target2Orders,
        target3Orders, target4Orders, target5Orders }
    : new ConcurrentDictionary<string, Order>[] { entryOrders };

// AFTER (single line):
var trackedDicts = BuildTrackedDictList(force);
```

**Step 4 — Verify** parent still compiles. The remainder of `SweepTrackedOrders` body is UNCHANGED.

**Step 5 — Run build:** `dotnet build src/` — zero errors required.

#### xUnit Tests (Ticket 1)

```csharp
[Fact]
public void BuildTrackedDictList_ForceTrue_ReturnsAllSevenDicts()
{
    // Arrange: instantiate partial class or test double with all 7 order dicts non-null
    // Act: var result = sut.BuildTrackedDictList(force: true);
    // Assert:
    Assert.Equal(7, result.Length);
}

[Fact]
public void BuildTrackedDictList_ForceFalse_ReturnsOnlyEntryOrders()
{
    // Arrange: instantiate partial class or test double
    // Act: var result = sut.BuildTrackedDictList(force: false);
    // Assert:
    Assert.Equal(1, result.Length);
    Assert.Same(entryOrders, result[0]);
}
```

#### Acceptance Criteria — Ticket 1

- [ ] `BuildTrackedDictList` private method present in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] CYC of `BuildTrackedDictList` = 2
- [ ] `SweepTrackedOrders` calls `BuildTrackedDictList(force)` (single-line assignment)
- [ ] `CancelAllV12GtcOrders` signature unchanged
- [ ] `dotnet build src/` passes — zero errors
- [ ] xUnit tests (2x [Fact]) present and passing
- [ ] No lock() blocks introduced
- [ ] ASCII-only code

---

### Ticket 2 — Extract `SweepDictionary` + Complete Parent Refactor

| Field | Value |
|---|---|
| **Ticket ID** | EPIC-W7-060-T2 |
| **Title** | Extract `SweepDictionary` and complete `SweepTrackedOrders` refactor |
| **File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Parent Method** | `SweepTrackedOrders` (post-T1 state) |
| **Helper Name** | `SweepDictionary` |
| **Helper Signature** | `private int SweepDictionary(ConcurrentDictionary<string, Order> dict)` |
| **Lines Moved** | Inner foreach block + null guards + cancel logic (post-T1 ~lines 1321–1348) |
| **Insertion Point** | After `BuildTrackedDictList` closing brace |
| **CYC Before (parent, post-T1)** | ~10 |
| **CYC After (helper)** | 5 |
| **CYC After (parent, final)** | 2 |
| **max_cyc_projected** | 5 |
| **Dependency** | Ticket 1 must be completed first |
| **Reuses** | `IsOrderTerminal(ord)` — already in call graph (src/V12_002.Orders.Management.Flatten.cs:698) |
| **Tests Required** | Yes — xUnit [Fact] (V12.32 mandate) |

#### Implementation Instructions

**Prerequisite:** Ticket 1 is complete. `BuildTrackedDictList` exists and parent calls it.

**Step 1 — Read current state** of `SweepTrackedOrders` (post-T1) before any edit.

**Step 2 — Add new private method** immediately after `BuildTrackedDictList`:

```csharp
private int SweepDictionary(ConcurrentDictionary<string, Order> dict)
{
    if (dict == null) return 0;
    int count = 0;
    foreach (var kvp in dict.ToArray())
    {
        Order ord = kvp.Value;
        if (ord == null || IsOrderTerminal(ord)) continue;
        try
        {
            CancelOrderOnAccount(ord, ord.Account);
            count++;
        }
        catch { }
    }
    return count;
}
```

**Step 3 — Replace** the inner foreach loop in `SweepTrackedOrders`:

```csharp
// BEFORE (post-T1 parent body):
var trackedDicts = BuildTrackedDictList(force);
int trackedCancels = 0;
foreach (var dict in trackedDicts)
{
    if (dict == null) continue;
    foreach (var kvp in dict.ToArray())
    {
        Order ord = kvp.Value;
        if (ord == null) continue;
        if (
            ord.OrderState != OrderState.Working
            && ord.OrderState != OrderState.Accepted
            && ord.OrderState != OrderState.Submitted
            && ord.OrderState != OrderState.ChangePending
            && ord.OrderState != OrderState.ChangeSubmitted
        )
            continue;
        try
        {
            CancelOrderOnAccount(ord, ord.Account);
            trackedCancels++;
        }
        catch { }
    }
}
return trackedCancels;

// AFTER (final 3-line parent body):
var trackedDicts = BuildTrackedDictList(force);
int trackedCancels = 0;
foreach (var dict in trackedDicts)
    trackedCancels += SweepDictionary(dict);
return trackedCancels;
```

**Step 4 — Verify** `IsOrderTerminal` is accessible (same partial class or existing callee — confirmed by Phase 2 call graph). No import changes needed.

**Step 5 — Run build:** `dotnet build src/` — zero errors required.

**Step 6 — Run pre-push validation:** `powershell -File ./scripts/pre_push_validation.ps1 -Fast`

#### xUnit Tests (Ticket 2)

```csharp
[Fact]
public void SweepDictionary_NullDict_ReturnsZero()
{
    // Act: var result = sut.SweepDictionary(null);
    // Assert:
    Assert.Equal(0, result);
}

[Fact]
public void SweepDictionary_TerminalOrder_SkipsAndReturnsZero()
{
    // Arrange: dict with one order where IsOrderTerminal returns true
    // Act: var result = sut.SweepDictionary(dict);
    // Assert:
    Assert.Equal(0, result);
}

[Fact]
public void SweepDictionary_WorkingOrder_CancelsAndReturnsOne()
{
    // Arrange: dict with one working order (OrderState.Working)
    // Act: var result = sut.SweepDictionary(dict);
    // Assert:
    Assert.Equal(1, result);
    // Verify CancelOrderOnAccount called once
}
```

#### Acceptance Criteria — Ticket 2

- [ ] `SweepDictionary` private method present in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] CYC of `SweepDictionary` = 5
- [ ] CYC of `SweepTrackedOrders` final = 2
- [ ] `SweepTrackedOrders` body is 3 lines (BuildTrackedDictList + foreach + return)
- [ ] Inline 5-condition OrderState check removed; replaced by `IsOrderTerminal` call
- [ ] `IsOrderTerminal` signature unchanged
- [ ] `CancelAllV12GtcOrders` signature unchanged
- [ ] `dotnet build src/` passes — zero errors
- [ ] `pre_push_validation.ps1 -Fast` passes
- [ ] xUnit tests (3x [Fact]) present and passing
- [ ] No lock() blocks introduced
- [ ] ASCII-only code

---

## Full CYC Summary (Post All Tickets)

| Method | CYC Before | CYC After | Branches After | <= 8? |
|---|---|---|---|---|
| `SweepTrackedOrders` | 11 | 2 | base=1, foreach=1 | PASS ✅ |
| `BuildTrackedDictList` (new) | — | 2 | base=1, ternary=1 | PASS ✅ |
| `SweepDictionary` (new) | — | 5 | base=1, null=1, foreach=1, null=1, IsOrderTerminal=1 | PASS ✅ |

**max_cyc_projected = 5. CYC reduction: 11 → 5 = 55%.**

---

## Ticket Dependency Graph

```
T1: Extract BuildTrackedDictList
    |
    v
T2: Extract SweepDictionary + complete parent refactor
```

Sequential. T2 must NOT execute before T1 is verified complete.

---

## V12.23 Scope Compliance

| Check | Status |
|---|---|
| Each ticket targets one concern only | PASS |
| No cross-file changes | PASS |
| `CancelAllV12GtcOrders` not modified | PASS |
| `IsOrderTerminal` called as-is (no signature change) | PASS |
| xUnit only (no NUnit/MSTest) | PASS |
| No lock() blocks | PASS |
| ASCII-only code in all skeletons | PASS |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Epic** | EPIC-W7-060 |
| **Phase** | 4 |
| **Ticket Count** | 2 |
| **CYC Baseline** | 11 |
| **max_cyc_projected** | 5 |
| **Sequential Thinking Thoughts** | 3 |
| **MCP Tools Used** | resolve_repo, get_symbol_complexity, get_extraction_candidates, sequentialthinking (x3) |
| **DNA Verdict (Phase 3)** | PASS |
| **Scope Verdict** | PASS |
