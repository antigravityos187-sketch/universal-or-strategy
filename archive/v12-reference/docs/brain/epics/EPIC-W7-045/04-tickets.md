# Phase 4: Tickets — EPIC-W7-045

## Header

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-045 |
| **Method** | `OnKeyDown` |
| **Source File** | `src/V12_002.UI.Callbacks.cs` |
| **Original CYC** | 9 (live measured via `get_symbol_complexity`; architecture plan reported 4 — live value used) |
| **Lines** | 391–426 (36 lines) |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 2 |
| **max_cyc_projected** | 7 |
| **dna_verdict** | PASS (from Phase 3 audit) |

---

## Ticket Summary

| Ticket ID | Helper Name | Concern | Projected Helper CYC | CYC Reduction in Parent |
|---|---|---|---|---|
| TICKET-045-1 | `ResolveModifierGroup` | Isolate all `Keyboard.IsKeyDown` modifier-key polling from dispatch | 7 | ~6 (3 two-branch `||` if-blocks removed) |
| TICKET-045-2 | `DispatchModifierAction` | Isolate action routing to `HandleTargetAction`/`HandleRunnerAction` from guard logic | 2 | ~3 (dispatch branches removed, parent stabilises at CYC 2) |

---

## TICKET-045-1

| Field | Value |
|---|---|
| **Ticket ID** | TICKET-045-1 |
| **Helper Name** | `ResolveModifierGroup` |
| **Signature** | `private static string? ResolveModifierGroup(KeyEventArgs e)` |
| **Concern** | Extract all `Keyboard.IsKeyDown(Key.D1) \|\| Keyboard.IsKeyDown(Key.NumPad1)` polling chains out of `OnKeyDown`. Centralises WPF keyboard-state coupling in one static helper. Enables unit testing without WPF dispatcher. |
| **Lines to Move** | ~403–420 (three sequential `if (Keyboard.IsKeyDown(...) \|\| Keyboard.IsKeyDown(...))` blocks that each conclude with an early `return` after dispatch — polling conditions only; dispatch calls remain for TICKET-045-2) |
| **CYC Reduction** | ~6 decisions removed from `OnKeyDown` (three two-arm `\|\|` branches = 6 decisions) |
| **Projected Helper CYC** | 7 (3 two-branch `\|\|` if-checks × 2 decisions = 6 + base 1 = 7; satisfies <=8 gate) |
| **Projected Parent CYC After This Ticket** | ~5 (original 9 − 6 decisions removed + 1 new null-guard call = ~5) |
| **Dependencies** | None — implement first |
| **Execution Mode** | `v12-engineer` (Bob CLI, src/ surgical refactoring) |

### Implementation Notes

1. Create `private static string? ResolveModifierGroup(KeyEventArgs e)` immediately after `OnKeyDown` in `src/V12_002.UI.Callbacks.cs`.
2. Body: three `if (Keyboard.IsKeyDown(Key.D1) || Keyboard.IsKeyDown(Key.NumPad1)) return "T1";` style guard returns for D1/NumPad1 → `"T1"`, D2/NumPad2 → `"T2"`, D3/NumPad3 → `"Runner"`.
3. Final line: `return null;` — making undefined-group dispatch structurally impossible (`string?` null return).
4. In `OnKeyDown`, replace the three if-blocks' condition expressions with: `var group = ResolveModifierGroup(e);`.
5. Zero heap allocations: returned string literals are compile-time interned; `Key` enum comparisons are value-type.
6. Do NOT move the `e.Handled = true; return;` dispatch calls yet — those remain for TICKET-045-2.

### Verification Criteria

- `OnKeyDown` compiles with no errors after this change.
- `ResolveModifierGroup` is callable (same partial class, same file).
- CYC of `OnKeyDown` measurably reduced (target ~5 or below before TICKET-045-2).
- No `lock()` blocks introduced.
- Only ASCII string literals: `"T1"`, `"T2"`, `"Runner"`.
- xUnit `[Fact]` test: `Assert.Equal("T1", ResolveModifierGroup(mockEventArgs_D1))` (via `InternalsVisibleTo`).

---

## TICKET-045-2

| Field | Value |
|---|---|
| **Ticket ID** | TICKET-045-2 |
| **Helper Name** | `DispatchModifierAction` |
| **Signature** | `private void DispatchModifierAction(string group, Key key)` |
| **Concern** | Extract the dispatch routing (`HandleTargetAction("T1")`, `HandleTargetAction("T2")`, `HandleRunnerAction(key)`) and `e.Handled = true` bookkeeping from `OnKeyDown`, leaving the parent as a pure orchestrator: dictionary fast-path guard → resolve group → dispatch. |
| **Lines to Move** | ~403–426 (the dispatch branch bodies: `HandleTargetAction("T1")`, `HandleTargetAction("T2")`, `HandleRunnerAction(key)`, each with `e.Handled = true; return;` — approximately 9 lines of routing logic) |
| **CYC Reduction** | ~3 decisions removed from `OnKeyDown` (if/else-if dispatch routing removed; replaces with single null-guard + one call) |
| **Projected Helper CYC** | 2 (if/else-if with 2 branches: T1 vs T2 vs Runner — one `if`, one `else if` = 2 decisions + base 1 = 2) |
| **Projected Parent CYC After This Ticket** | 2 (only two `if` branches remain: `_keyCommands` dict guard + `group != null` guard) |
| **Dependencies** | TICKET-045-1 must be complete (requires `ResolveModifierGroup` to already exist and return the `group` string) |
| **Execution Mode** | `v12-engineer` (Bob CLI, src/ surgical refactoring) |

### Implementation Notes

1. Create `private void DispatchModifierAction(string group, Key key)` after `ResolveModifierGroup` in `src/V12_002.UI.Callbacks.cs`.
2. Body: `if (group == "T1") { HandleTargetAction("T1", key); } else if (group == "T2") { HandleTargetAction("T2", key); } else { HandleRunnerAction(key); }`.
3. Note: `DispatchModifierAction` does NOT set `e.Handled` — the caller (`OnKeyDown`) sets `e.Handled = true` after the call returns. This preserves separation between WPF event management (in `OnKeyDown`) and business routing (in `DispatchModifierAction`).
4. In `OnKeyDown`, replace the remaining dispatch bodies with: `if (group != null) { DispatchModifierAction(group, e.Key); e.Handled = true; }`.
5. Final `OnKeyDown` shape: (a) `_keyCommands` null+TryGetValue guard → `cmd()` + `e.Handled = true; return;`; (b) `var group = ResolveModifierGroup(e);`; (c) `if (group != null) { DispatchModifierAction(group, e.Key); e.Handled = true; }`.
6. No `lock()` blocks. Actor/Enqueue pattern preserved at depth 2 via unchanged `HandleTargetAction` and `HandleRunnerAction` call chains.

### Verification Criteria

- `OnKeyDown` compiles with no errors after this change.
- `DispatchModifierAction` is callable (same partial class, same file).
- `get_symbol_complexity` for `OnKeyDown` returns CYC <= 2 post-extraction.
- `get_symbol_complexity` for `DispatchModifierAction` returns CYC <= 2.
- `get_symbol_complexity` for `ResolveModifierGroup` returns CYC <= 7.
- Zero new `lock()` blocks.
- Build passes: `powershell -File .\scripts\build_readiness.ps1`.
- `deploy-sync.ps1` executed to re-synchronize NinjaTrader hard links.

---

## CYC Projection After All Tickets

| Method | CYC Before | CYC After | Status |
|---|---|---|---|
| `OnKeyDown` (parent) | 9 | 2 | Reduced — PASS (<=8) |
| `ResolveModifierGroup` (new) | N/A | 7 | New helper — PASS (<=8) |
| `DispatchModifierAction` (new) | N/A | 2 | New helper — PASS (<=8) |
| `HandleTargetAction` (unchanged) | 6 | 6 | No change — PASS |
| `HandleRunnerAction` (unchanged) | 6 | 6 | No change — PASS |
| **projected_parent_cyc_after_all** | — | **2** | **PASS** |
| **max_cyc_projected** | — | **7** | **PASS (<=8 gate)** |

---

## Sequential Thinking Validation Summary

- **Thought 1:** Live CYC=9 confirmed via `get_symbol_complexity`; architecture plan (CYC=4) was pre-extraction estimate — live value drives ticket sizing.
- **Thought 2:** 2-ticket structure validated: TICKET-1 moves keyboard-polling (~6 CYC decisions), TICKET-2 moves dispatch routing (~3 CYC decisions); parent reaches CYC=2 after both.
- **Thought 3:** Ticket dependency order confirmed (TICKET-1 before TICKET-2); all projected CYC values satisfy Jane Street <=8 gate.
- **Thought 4:** Final plan validated — 2 tickets, CYC=7 max, parent CYC=2, no violations, ready to write.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T01:25:00Z |
| **jCodemunch Tools Called** | resolve_repo, search_symbols, get_symbol_complexity, get_extraction_candidates |
| **Sequential Thinking Calls** | 5 (1 probe + 4 breakdown thoughts) |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Input** | `docs/brain/EPIC-W7-045/02-architecture-plan.md`, `docs/brain/EPIC-W7-045/03-audit-report.md` |
| **Output** | `docs/brain/EPIC-W7-045/04-tickets.md` |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 2 |
| **max_cyc_projected** | 7 |
| **dna_verdict** | PASS (inherited from Phase 3) |
| **Lane** | P4-L3 |
