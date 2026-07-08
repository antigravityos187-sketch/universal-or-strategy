# EPIC-W7-069 — Phase 1: Scope Definition

**Wave:** 7 | **Phase:** 1 | **Method:** `GetFsmExpectedPosition` | **Epic:** EPIC-W7-069

---

## 1. Single Method in Scope

This epic targets exactly one **single method**:

| Field          | Value                                                         |
|----------------|---------------------------------------------------------------|
| Method         | `GetFsmExpectedPosition(string accountName)`                  |
| File           | `src/V12_002.Symmetry.BracketFSM.cs`                         |
| Declaration    | line 422                                                      |
| Visibility     | `private`                                                     |
| Class          | `V12_002 : Strategy` (partial)                               |
| Return type    | `int` (signed net expected contracts for one named account)  |

The **scope boundary** is drawn at this single method. No other methods, classes,
or files are included in the refactor target for this epic.

---

## 2. Cyclomatic Complexity

| Metric        | Value                                                                 |
|---------------|-----------------------------------------------------------------------|
| Current CYC   | **0** (linear `foreach` scan + guard `continue` clauses + one data-selection `if/else if`; no independent termination paths) |
| Target CYC    | **≤ 8**                                                               |
| Gap           | CYC is already below the target ceiling; work focuses on observability and correctness, not structural decomposition |

The CYC of **0** is accurate: the method is a pure aggregation kernel. Its
complexity resides in the state space it queries (`FollowerBracketState` × N FSMs),
not in its own control flow. The target of ≤ 8 is the ceiling that must not be
breached by any refactor undertaken in later phases.

---

## 3. Callers

A `grep` of the entire `src/` tree for `GetFsmExpectedPosition` returned
**3 grep hits across 3 files**:

| File                                      | Line | Role                                                    |
|-------------------------------------------|------|---------------------------------------------------------|
| `src/V12_002.REAPER.Audit.cs`             | 404  | **1 direct call site** — sole runtime caller            |
| `src/V12_002.Symmetry.BracketFSM.cs`      | 422  | Method declaration (not a caller)                       |
| `src/V12_002.cs`                          | 661  | XML doc comment reference only (not a caller)           |

**Total callers: 1** (`V12_002.REAPER.Audit.cs` line 404 is the sole call site).

Because there is only one caller, any signature change or behavioral adjustment
has a contained blast radius limited to that single call site.

---

## 4. Scope Boundary

The **scope boundary** for EPIC-W7-069 is defined as follows:

- **In scope:** `GetFsmExpectedPosition` in `src/V12_002.Symmetry.BracketFSM.cs`
  (declaration, body, inline behavior).
- **Out of scope:** all other methods in the same file, all callers, all
  `_followerBrackets` mutation sites, and all downstream REAPER orchestration
  logic.

This is a **single method** epic. Changes proposed in subsequent phases must
not touch code outside this boundary without a separate epic authorization.

---

## 5. Why Other Methods Are NOT in Scope (V12.23 Policy)

Per **V12.23** (single-method epic isolation rule), each Wave-7 epic targets
exactly one method per scope document. The following methods were considered and
explicitly excluded:

| Method / Symbol                    | Reason Excluded                                                                    |
|------------------------------------|------------------------------------------------------------------------------------|
| `DrainAccountMailbox`              | FSM event pump — distinct responsibility; mutates state this method only reads     |
| `ProcessBracketEvent`              | State-transition dispatcher — separate FSM lifecycle concern                       |
| `TryTerminateFollowerBracket`      | Post-call REAPER action — acts on the *result* of this method, not inside it       |
| `REAPER.Audit` orchestration logic | Caller context — V12.23 prohibits scope creep into callers without separate epic   |
| `expectedPositions` (master path)  | Legacy master-account authority — intentionally parallel, excluded by design       |

V12.23 mandates that expanding scope to adjacent methods requires a new epic
charter with its own hotspot analysis and manifest entry. Any cross-method
refactor discovered during Phase 2+ must be deferred to a linked child epic.

---

## 6. Key Invariants Carried Forward

The following invariants from Phase 0 (hotspot analysis) bind all later phases:

1. `GetFsmExpectedPosition` **must remain read-only** — no FSM state mutations permitted.
2. The six non-terminal states (`Active`, `Accepted`, `Submitted`, `PendingSubmit`,
   `Replacing`, `Modifying`) are **load-bearing** and must not be reduced.
3. `Disconnected` is intentionally excluded and must remain excluded.
4. Master accounts must **never** be routed through this method.
5. CYC must not exceed **8** after any refactor.

---

## 7. Agent Tracking

```
Agent Name:  v12-phase1-scope
Epic:        EPIC-W7-069
Wave:        7
Phase:       1 — Scope Definition (REDO)
Method:      GetFsmExpectedPosition
Source:      src/V12_002.Symmetry.BracketFSM.cs
Current CYC: 0
Target CYC:  <=8
Callers:     1 (src/V12_002.REAPER.Audit.cs line 404)
Status:      completed
Output:      docs/brain/EPIC-W7-069/00-scope.md
```
