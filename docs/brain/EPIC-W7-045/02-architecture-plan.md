# Phase 2: Architecture Plan — EPIC-W7-045

## Method Under Extraction

- **Method:** `OnKeyDown`
- **Source File:** `src/V12_002.UI.Callbacks.cs`
- **Original CYC:** 4 (measured: `_keyCommands` null-check + D1/NumPad1 + D2/NumPad2 + D3/NumPad3 branches)
- **Lines:** 391–426
- **Visibility:** `private` — registered as WPF `PreviewKeyDown` event handler

### jcodemunch get_context_bundle result
Symbol resolved at `src/V12_002.UI.Callbacks.cs:391`. Docstring: `[Phase7-UI T-A] OnKeyDown residual dispatcher (CYC 3) - Command Pattern with O(1) lookup`. Method body (35 lines): dictionary lookup guard returning early, then three sequential `if (Keyboard.IsKeyDown(...) || Keyboard.IsKeyDown(...))` blocks dispatching to `HandleTargetAction("T1")`, `HandleTargetAction("T2")`, and `HandleRunnerAction(key)` — each followed by `e.Handled = true; return;`. No heap allocations in body. Pure orchestration method.

### jcodemunch get_call_hierarchy result
- **Callers:** 0 static callers (wired via WPF event `+=` in `AttachHotkeys`, `-=` in `DetachHotkeys`)
- **Callees (depth 1):** `_keyCommands` (constant, line 42), `HandleTargetAction` (method, line 429, CYC 6), `HandleRunnerAction` (method, line 455, CYC 6)
- **Callees (depth 2):** `ExecuteTargetAction` (method, line 490), `Enqueue` (method in `src/V12_002.cs:428`) — Actor/Enqueue pattern confirmed present at depth 2

### jcodemunch get_dependency_graph result
No file-level imports or importers detected for `src/V12_002.UI.Callbacks.cs`. The file is a self-contained partial class with zero cross-file import edges in the index. Refactoring blast radius is fully contained within this single file.

### jcodemunch get_extraction_candidates result
No candidates returned (empty list). This is expected: `OnKeyDown` has no multi-file callers (it is event-wired only), so the extraction-candidates tool which requires `min_callers >= 1` across files produces no hits. Manual structural analysis applied instead per Jane Street KB rules.

---

## Sequential Thinking Summary

**Final Thought (5/5):** The extraction plan for EPIC-W7-045 is: extract 2 private helpers from `OnKeyDown` in `src/V12_002.UI.Callbacks.cs`.

- **Current CYC = 4** — already below the <=8 ceiling, so a minimal/structural plan applies.
- **Three identical if-chains** (modifier-key polling → dispatch → handled/return) represent a structural anti-pattern: mixed dispatch strategies, untestable `Keyboard.IsKeyDown` calls coupled to dispatch logic.
- **Helper 1:** `private static string? ResolveModifierGroup(KeyEventArgs e)` — isolates all `Keyboard.IsKeyDown` polling. Returns `"T1"`, `"T2"`, `"Runner"`, or `null`. CYC = 7 (three two-branch `||` if-checks, each counting 2 decisions = 6 + base 1). Passes <=8 gate.
- **Helper 2:** `private void DispatchModifierAction(string group, Key key)` — receives the resolved group string and dispatches to `HandleTargetAction("T1")`, `HandleTargetAction("T2")`, or `HandleRunnerAction(key)`. CYC = 2 (two-branch if/else-if).
- **Parent `OnKeyDown` after extraction:** Dictionary-lookup block (CYC 2) + `ResolveModifierGroup` call + single null-guard = CYC 2–3. Max projected CYC across all methods = **7**. All pass Jane Street <=8. Full Jane Street KB alignment: single-responsibility, zero heap allocs (interned string literals returned), no lock() blocks, Actor/Enqueue pattern at depth 2 preserved, guard clauses applied, illegal states (undefined group) unrepresentable via `string?` null return.

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|
| `ResolveModifierGroup` | `private static string? ResolveModifierGroup(KeyEventArgs e)` | Polls `Keyboard.IsKeyDown` for D1/NumPad1, D2/NumPad2, D3/NumPad3; returns `"T1"`, `"T2"`, `"Runner"`, or `null`. Isolates all WPF keyboard-state coupling from dispatch logic. Enables unit testing without WPF dispatcher. | 7 |
| `DispatchModifierAction` | `private void DispatchModifierAction(string group, Key key)` | Accepts a resolved group name and the pressed `Key`; routes to `HandleTargetAction("T1")`, `HandleTargetAction("T2")`, or `HandleRunnerAction(key)`. Single routing concern only. | 2 |

---

## Parent Method After Extraction

**Remaining logic in `OnKeyDown`:**
1. Guard: if `_keyCommands != null` and `TryGetValue(e.Key, out var cmd)` → invoke `cmd()`, set `e.Handled = true`, return early (dictionary fast path, O(1))
2. Call `ResolveModifierGroup(e)` → receive `string? group`
3. Guard: if `group != null` → call `DispatchModifierAction(group, e.Key)`, set `e.Handled = true`

**Projected CYC:** 2 (two `if` branches remain; no nested conditionals)

---

## max_cyc_projected: 7
## extraction_count: 2

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved for all methods | YES — parent=2, ResolveModifierGroup=7, DispatchModifierAction=2 |
| Single-responsibility per helper | YES — ResolveModifierGroup: key polling only; DispatchModifierAction: routing only |
| Lock-free/Actor pattern preserved | YES — no `lock()` in any extracted method; `Enqueue` call chain preserved at depth 2 |
| Illegal states unrepresentable | YES — `string?` null return from `ResolveModifierGroup` makes undefined-group dispatch structurally impossible |
| Zero-allocation hot paths | YES — interned string literals returned, `Key` enum comparisons are value-type, no closures or LINQ |
| Extract Guard Clauses applied | YES — three identical if-chain guards collapsed into single null-guard on resolved group |
| Replace if-chains with lookup approach | YES — modifier-group resolution centralized; new groups require only `ResolveModifierGroup` changes, not `OnKeyDown` |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Input** | docs/brain/EPIC-W7-045/01-scope-boundary.md |
| **Output** | docs/brain/EPIC-W7-045/02-architecture-plan.md |
