# B62-LaneA Orchestrator Prompt

## Block: B62 — Entry order drag sync + dedup fix

**DW item**: DW-B62-01 (live test confirmed: dragging a leader limit order spawns duplicate PTT-Copy
orders on the follower because the 10-second dedup expiry allows re-entry, and no entry-order drag
sync path exists — follower does not move its working PTT-Copy order when the leader drags).

**Root cause** (confirmed from NinjaTrader Grid 2026-08-11 logs, Sim102 accumulated 41L vs 10L):
1. `IsDedup` prunes cache by time (`>10s`). A drag after >10s adds a new entry and passes Gate 5,
   spawning a second `PTT-Copy` order instead of moving the existing one.
2. No code path exists that detects "this orderId is already in the cache + state=Accepted + Limit"
   and diverts to an `acc.Change()` call.

**Option chosen**: Option B — real-time entry drag sync (mirror of `HandleBracketChange` pattern).

---

## PIPELINE CHAIN (all 7 phases mandatory — none skippable — none combinable)

```
Ph1  ptt-architect       -> docs/brain/B62-LaneA/02-architecture-plan.md
Ph2  ptt-plan-reviewer   -> docs/brain/B62-LaneA/02-plan-review.md       (gate: REVIEW_PASS)
Ph3  ptt-architect       -> docs/brain/B62-LaneA/04-tickets.md
Ph3.5 ptt-ticket-reviewer -> docs/brain/B62-LaneA/04-ticket-review.md   (gate: TICKET_REVIEW_PASS)
Ph4a ptt-engineer        -> src .cs edits + docs/brain/B62-LaneA/ticket-1-completion.md
Ph4b ptt-verifier        -> docs/brain/B62-LaneA/ticket-1-verification.md (gate: VERIFY_PASS)
Ph5  ptt-plan-reviewer   -> docs/brain/B62-LaneA/05-final-review.md
                         -> docs/brain/B62-LaneA/06-deferred-backlog.md
```

---

## EXACT CHANGES REQUIRED (spec — not a suggestion)

### Change 1 — Replace `IsDedup` with persistent-only dedup (no time-based pruning)

**Current `IsDedup` body** (lines 1442–1459 of `src/PropTraderTools/CopyEngine.cs`):
```csharp
private bool IsDedup(string orderId)
{
    long now = DateTime.UtcNow.Ticks;
    long expiry = TimeSpan.FromSeconds(10).Ticks;

    // Prune expired entries
    foreach (var key in _dedupCache.Keys)
    {
        if (_dedupCache.TryGetValue(key, out long storedTicks) && now - storedTicks > expiry)
            _dedupCache.TryRemove(key, out _);
    }

    // Attempt add -- if TryAdd returns false, orderId already exists (duplicate)
    if (!_dedupCache.TryAdd(orderId, now))
        return true;

    return false;
}
```

**Required replacement** — remove time-based expiry entirely; eviction is now done by `EvictDedup`
called unconditionally on terminal states (see Change 2):
```csharp
// B62: dedup is now persistent (no time expiry). Eviction happens via EvictDedup on terminal state.
// CYC=2: TryAdd false-path (1) + early return.
// JS-025: ConcurrentDictionary.TryAdd is lock-free.
private bool IsDedup(string orderId)
{
    if (!_dedupCache.TryAdd(orderId, 0L))
        return true;

    return false;
}
```

The `long` value in `ConcurrentDictionary<string, long>` is now always `0L` — the timestamp is no
longer needed. The field declaration at line 112 stays unchanged (`ConcurrentDictionary<string, long>`).

### Change 2 — Add `EvictDedup` method (evict on terminal order state)

New `internal` method (add immediately after the new `IsDedup`):
```csharp
// B62: evict a dedup entry when the order reaches a terminal state.
// Called unconditionally from OnOrderUpdate before Gate 1 (after TryFirePositionState).
// CYC=2: terminal-state branch (1) + TryRemove (no branch).
// JS-025: ConcurrentDictionary.TryRemove is lock-free.
internal void EvictDedup(string orderId, OrderState state)
{
    if (state != OrderState.Filled && state != OrderState.Cancelled && state != OrderState.Rejected)
        return;

    _dedupCache.TryRemove(orderId, out _);
}
```

### Change 3 — Wire `EvictDedup` into `OnOrderUpdate` (line ~603, after `TryFirePositionState`)

Current lines 602–607:
```csharp
// Pre-gate: fire position state unconditionally (even when copy disabled)
TryFirePositionState(e);

// Gate 1: enabled check
if (!_isCopyEnabled)
    return;
```

Required — insert one line after `TryFirePositionState` call:
```csharp
// Pre-gate: fire position state unconditionally (even when copy disabled)
TryFirePositionState(e);
// B62: evict dedup cache on terminal states (Filled/Cancelled/Rejected) -- prevents drag re-entry.
EvictDedup(e.Order.OrderId.ToString(), e.Order.OrderState);

// Gate 1: enabled check
if (!_isCopyEnabled)
    return;
```

### Change 4 — Add `FindFollowerEntryOrder` method (mirror of `FindFollowerBracketOrder`)

Add after `FindFollowerBracketOrder` (currently ends at line 925):
```csharp
// B62: find a working PTT-Copy limit entry order on the follower account for the instrument.
// Mirror of FindFollowerBracketOrder but matches by Name=="PTT-Copy" + Limit + Working.
// CYC=3: foreach(1), instrument filter(2), state+name+type filter(3).
// JS-002: returns null when not found (caller must guard).
private static Order? FindFollowerEntryOrder(Account follower, Instrument instrument)
{
    foreach (var order in follower.Orders.ToList())                        // (1)
    {
        if (order.Instrument != instrument)                                // (2)
            continue;
        if (order.OrderState == OrderState.Working                         // (3)
            && order.OrderType == OrderType.Limit
            && order.Name == "PTT-Copy")
            return order;
    }
    return null;
}
```

### Change 5 — Add `HandleEntryChange` method (mirror of `HandleBracketChange`)

Add after `HandleBracketChange` (currently ends at line 900):
```csharp
// B62: sync a leader entry drag to all follower working PTT-Copy limit orders.
// Mirror of HandleBracketChange -- tick-rounds price, calls acc.Change() per follower.
// CYC=5: instr null(1), tickSize(2), rawPrice tick-round(3), foreach acc(4), fo null guard(5).
// JS-001: try/catch around acc.Change() -- no throw in hot path.
// JS-021: no lock -- _dedupCache is ConcurrentDictionary (lock-free).
private void HandleEntryChange(Order leaderOrder, CopyRule rule)
{
    var instrument = leaderOrder.Instrument;
    if (instrument == null)                                                    // (1)
        return;

    double tickSize = instrument.MasterInstrument?.TickSize ?? 0.0;           // (2)
    double rawPrice = leaderOrder.LimitPrice;                                  // (3)
    double newPrice = tickSize > 0
        ? Math.Round(rawPrice / tickSize) * tickSize
        : rawPrice;

    foreach (var acc in rule.FollowerAccounts)                                // (4)
    {
        if (acc == null)                                                       // (5)
            continue;

        var fo = FindFollowerEntryOrder(acc, instrument);
        if (fo == null)
            continue;

        double currentPrice = fo.LimitPrice;
        if (Math.Abs(newPrice - currentPrice) < tickSize)
            continue;

        try
        {
            fo.LimitPrice = newPrice;
            acc.Change(new Order[] { fo });
            StatusUpdate?.Invoke(acc.Name + ": entry dragged -> " + newPrice);
        }
        catch (Exception ex)
        {
            StatusUpdate?.Invoke(acc.Name + ": entry drag error: " + ex.Message);
        }
    }
}
```

### Change 6 — Add entry drag detection gate in `OnOrderUpdate` (after Gate B, before `DispatchCopy`)

Current lines 650–660:
```csharp
// Gate B: bracket drag detection -- divert to HandleBracketChange path
if (IsWorkingBracket(e.Order))
{
    if (e.Order.FromEntrySignal != null)
        PopulateOrderMap(e.Order.FromEntrySignal, e.Order.Account);
    HandleBracketChange(e.Order, matchedRule.Value);
    return;
}

// No bracket -- normal copy dispatch
DispatchCopy(e.Order, matchedRule.Value);
```

Required — add Gate C immediately after Gate B and before `DispatchCopy`:
```csharp
// Gate B: bracket drag detection -- divert to HandleBracketChange path
if (IsWorkingBracket(e.Order))
{
    if (e.Order.FromEntrySignal != null)
        PopulateOrderMap(e.Order.FromEntrySignal, e.Order.Account);
    HandleBracketChange(e.Order, matchedRule.Value);
    return;
}

// Gate C (B62): entry drag detection -- divert to HandleEntryChange when a known orderId drags
// Condition: order is Limit + Accepted/Working + orderId already in _dedupCache (seen before).
if (e.Order.OrderType == OrderType.Limit
    && (e.Order.OrderState == OrderState.Accepted || e.Order.OrderState == OrderState.Working)
    && _dedupCache.ContainsKey(e.Order.OrderId.ToString()))
{
    HandleEntryChange(e.Order, matchedRule.Value);
    return;
}

// No bracket, no entry drag -- normal copy dispatch
DispatchCopy(e.Order, matchedRule.Value);
```

---

## TESTS REQUIRED (5 new [Fact] tests, tag T_B62_01 through T_B62_05)

All tests use `xUnit [Fact]` only. All use `CopyEngine`'s `internal` or `static` methods.
No `lock()`, no `throw new`, no NUnit/MSTest.

**T_B62_01** — `IsDedup_FirstCall_ReturnsFalse`
- Arrange: new `CopyEngine` instance; arbitrary orderId "ord-001".
- Act: call `IsDedup` via reflection (private) with "ord-001".
- Assert: returns `false` (first time not a dup).

**T_B62_02** — `IsDedup_SecondCall_ReturnsTrue`
- Arrange: same engine; call `IsDedup("ord-002")` once (adds to cache).
- Act: call `IsDedup("ord-002")` a second time.
- Assert: returns `true` (duplicate).

**T_B62_03** — `EvictDedup_FilledState_RemovesEntry`
- Arrange: call `IsDedup("ord-003")` to seed the cache.
- Act: call `engine.EvictDedup("ord-003", OrderState.Filled)`.
- Assert: calling `IsDedup("ord-003")` again returns `false` (evicted, not a dup).

**T_B62_04** — `EvictDedup_WorkingState_DoesNotRemove`
- Arrange: call `IsDedup("ord-004")` to seed the cache.
- Act: call `engine.EvictDedup("ord-004", OrderState.Working)`.
- Assert: calling `IsDedup("ord-004")` again returns `true` (still in cache — not a terminal state).

**T_B62_05** — `EvictDedup_CancelledState_RemovesEntry`
- Arrange: call `IsDedup("ord-005")` to seed the cache.
- Act: call `engine.EvictDedup("ord-005", OrderState.Cancelled)`.
- Assert: calling `IsDedup("ord-005")` again returns `false` (evicted).

---

## DEFERRED WORK ITEMS TO DOCUMENT IN 06-deferred-backlog.md

Carry forward all open items from B61's backlog plus any new ones discovered during B62.

**DW-B62-01** (if not fully closed): Entry drag sync verified in sim — confirm no duplicate orders
accumulate in a live drag-and-release scenario over >10s.

**DW-B62-02** (new, if applicable): `HandleEntryChange` skips orders where `tickSize == 0.0`
(price-delta guard divides by zero risk) — NT8 instruments always have a valid TickSize in sim
but this should be confirmed for live accounts.

---

## JANE STREET / OKF COMPLIANCE CHECKLIST

Before Ph4a writes any code, the engineer MUST verify:

| Rule | Check |
|------|-------|
| JS-021 | No `lock()` added anywhere — all state via `ConcurrentDictionary` |
| JS-001 | No `throw new` in `HandleEntryChange` or `IsDedup` — `try/catch` only |
| JS-002 | `FindFollowerEntryOrder` returns `Order?` (nullable) with null guard at call site |
| CYC ≤ 8 | `HandleEntryChange` CYC=5, `IsDedup` CYC=2, `EvictDedup` CYC=2 — all ≤ 8 |
| ASCII-only | No Unicode/emoji in any new string literals |
| xUnit only | All 5 tests use `[Fact]` — no NUnit/MSTest |

---

## NT8 API CONSTRAINTS (from NT8_FULL_REFERENCE.md)

- `acc.Change(Order[])` — valid on `AddOnBase` for modifying a working order's price.
- `order.LimitPrice` — readable and writable on working Limit orders.
- `order.OrderState` — `Working` and `Accepted` are valid non-terminal states for entry limit orders.
- `order.Name` — `"PTT-Copy"` is the signal name assigned by `SendCopy` for all copy modes.
- Engineer MUST grep `docs/standards/NT8_FULL_REFERENCE.md` for `Change(` before writing any
  `acc.Change()` call to confirm the overload signature.

---

## BRAIN ARTIFACT CHECKLIST (all 8 files required for pipeline completion)

```
docs/brain/B62-LaneA/02-architecture-plan.md      <- Ph1 output
docs/brain/B62-LaneA/02-plan-review.md            <- Ph2 output  (must end: REVIEW_PASS)
docs/brain/B62-LaneA/04-tickets.md                <- Ph3 output
docs/brain/B62-LaneA/04-ticket-review.md          <- Ph3.5 output (must end: TICKET_REVIEW_PASS)
docs/brain/B62-LaneA/ticket-1-completion.md       <- Ph4a output (must contain git commit hash)
docs/brain/B62-LaneA/ticket-1-verification.md     <- Ph4b output (must end: VERIFY_PASS)
docs/brain/B62-LaneA/05-final-review.md           <- Ph5 output
docs/brain/B62-LaneA/06-deferred-backlog.md       <- Ph5 output
```

---

## SESSION STATE AT B62 START

| Item | Status |
|------|--------|
| B59 `IsExitSignalName` | ✅ CLOSED (commit `fac65246`) |
| B60 `TryDispatchLeaderFlat` wire-up + Rev prefix | ✅ CLOSED (commit `57b10313`) |
| B61 state guard + follower-only flatten | ✅ CLOSED (commit `8a097ac8`) |
| DW-B57-01 CreateOrder+Submit fix | ✅ CLOSED |
| DW-B58-01/02/03 | OPEN P2 (no action planned) |
| DW-B54-01 ATM auto-inject | OPEN P1 (blocked — StrategyBase-only) |
| DW-B62-01 entry drag | TARGET of this block |
| Total tests added B59–B61 | 14 new `[Fact]` tests |

---

## WORKSPACE RULES (copy verbatim into each phase prompt)

- SRC CODE BAN: ptt-architect and ptt-plan-reviewer MUST NOT edit any `.cs` file.
- ptt-engineer is the ONLY mode permitted to touch `.cs` files.
- Workspace: `C:\WSGTA\universal-or-strategy` (main branch only).
- After any `.cs` edit: run `powershell -File scripts\verify_links.ps1 -Fix`
  then commit: `git add src/PropTraderTools/ && git commit -m "fix(ptt): B62 -- ..."`
- NT8 API reference: grep `docs/standards/NT8_FULL_REFERENCE.md` before any NT8 API claim.
- Jane Street rules: JS-021 no lock(), JS-001 no throw in hot path, JS-002 no null return,
  CYC ≤ 8 per method, ASCII-only literals, xUnit [Fact] only.
