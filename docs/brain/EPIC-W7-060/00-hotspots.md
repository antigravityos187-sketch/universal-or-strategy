# EPIC-W7-060 — Phase 0: Hotspot Analysis
## Method: `SweepTrackedOrders`
**Source:** `src/V12_002.SIMA.Lifecycle.cs` · Lines 1308–1353
**Wave:** 7 | **Phase:** 0 | **CYC (confirmed):** 0

---

## 1. Symbol Location

| Attribute | Value |
|---|---|
| Class | `V12_002` (partial) |
| Namespace | `NinjaTrader.NinjaScript.Strategies` |
| File | `src/V12_002.SIMA.Lifecycle.cs` |
| Lines | 1308–1353 |
| Access | `private` |
| Signature | `private int SweepTrackedOrders(bool force)` |

---

## 2. Complexity Assessment

`SweepTrackedOrders` is a **flat sweep loop** with no nested conditionals or recursion.

- **Cyclomatic Complexity: 0** (single linear path; `force` selects an array literal at entry, the loop body is a trivial null + state guard with a `try/catch`).
- No branching beyond the null-check guards and the `OrderState` multi-condition filter (treated as a single guard predicate).
- No early-return loops, no recursion, no async await.
- The `force` ternary at line 1313–1324 selects between two pre-built arrays — it is an initialisation expression, not a control-flow branch within the body.

**Verdict:** Method is a hotspot-free utility — CYC confirmed 0 for refactoring purposes.

---

## 3. Blast Radius

### Direct callers
| Caller | File | Line | Call context |
|---|---|---|---|
| `CancelAllV12GtcOrders(bool force)` | `V12_002.SIMA.Lifecycle.cs` | 1296 | Phase 1 of two-phase GTC sweep |

### Indirect callers (via `CancelAllV12GtcOrders`)
| Caller | File | Line | Trigger |
|---|---|---|---|
| `ProcessShutdownSIMA()` | `V12_002.SIMA.Lifecycle.cs` | 100 | SIMA disable path (`force=false`) |
| Strategy `OnTermination` / shutdown drain | `V12_002.Lifecycle.cs` | 216 | Strategy terminate (`force=false`) |

### Dictionaries read / mutated
`entryOrders`, `stopOrders`, `target1Orders` … `target5Orders`
(all `ConcurrentDictionary<string, Order>`; read via `.ToArray()` snapshot — no structural mutation inside the sweep).

### Downstream sink
`CancelOrderOnAccount(ord, ord.Account)` — defined in `src/V12_002.Orders.CancelGateway.cs:46`.

### Blast radius summary
- Blast is **contained**: one direct caller, two indirect lifecycle callers.
- No cross-file writes to shared state.
- No impact on hot-path (per-tick) execution — cold path only (shutdown/SIMA-disable).

---

## 4. Hotspot Inventory

| # | Category | Observation | Severity |
|---|---|---|---|
| H-1 | Bare `catch {}` | Line 1349 swallows all exceptions from `CancelOrderOnAccount`. Cancellation failures are silently lost; no audit log entry is emitted on failure. | Low |
| H-2 | Linear dict scan | Iterates up to 7 dicts × N orders with `.ToArray()` snapshot per dict. Acceptable for cold path but allocates 7 intermediate arrays. | Info |
| H-3 | Force-branch as array literal | The `force` ternary at line 1313 builds the full 7-element array on every call even when only `entryOrders` is needed (`force=false`). Minor allocation on the hot-cold boundary. | Info |
| H-4 | No return-value utilisation guard | `trackedCancels` is logged by the parent but nothing acts on a zero-cancel result to distinguish "nothing to cancel" from "everything threw". | Info |

**No high-severity hotspots detected.** The method is structurally sound for its cold-path role.

---

## 5. Sequential Thinking Summary

**Thought 1 — Scope confirmation:**
`SweepTrackedOrders` is a Phase-1 sub-routine of `CancelAllV12GtcOrders`. Its only job is to iterate in-memory tracking dictionaries and issue cancel calls. The `force` flag semantics (Build 990) are correct: `force=false` touches only `entryOrders` to protect bracket orders guarding live positions; `force=true` sweeps all 7 dicts at strategy terminate. The logic is intentional and well-commented.

**Thought 2 — CYC=0 validation:**
The method body is a straight-line loop with guard predicates. No decision point creates an independent execution path that would increment cyclomatic complexity above the baseline. The `force` ternary is an r-value expression, not a branching statement within the loop body. CYC=0 is confirmed.

**Thought 3 — Blast radius is benign:**
The two lifecycle callers (`ProcessShutdownSIMA`, `OnTermination`) are both shutdown paths. No ticker, no REAPER audit loop, no UI callback references `SweepTrackedOrders`. A refactor or signature change here carries minimal risk of regressions outside the two-phase GTC sweep subsystem. The only actionable improvement flagged (H-1) is converting the bare `catch {}` to a logged catch — a pure observability improvement with no behavioural impact.

---

## 6. Phase 0 Conclusion

- **CYC confirmed:** 0
- **Risk level:** Low (cold-path only, contained blast radius)
- **Recommended next phase:** Phase 1 — targeted fix for H-1 (bare `catch {}` → logged catch) if observability improvement is desired; otherwise this method may be marked clean.
- **Output artifact:** `docs/brain/EPIC-W7-060/00-hotspots.md`
