# EPIC-W7-018 — Phase 1: Scope Definition

## Method in Scope

**Single method:** `IsSymbolMatch`

`IsSymbolMatch` is a **net-new** symbol to be extracted from the 15-term `isForMe` OR chain
at lines 321–337 of `IsCommandForThisInstrument` in `src/V12_002.UI.IPC.cs`.
It does not yet exist in the codebase (confirmed: zero symbol occurrences found via grep
across `src/`), so its **current CYC = 0** (fallback value from spec).

Planned signature after extraction:

```csharp
private bool IsSymbolMatch(string target, string mySym, string myFull)
```

---

## Complexity Baseline

| Field                         | Value                                                         |
|-------------------------------|---------------------------------------------------------------|
| **Method**                    | `IsSymbolMatch`                                               |
| **Current CYC**               | 0 (net-new; symbol does not exist yet)                        |
| **Fallback / Spec CYC**       | 8 (per hotspot spec — wave-7 epic entry)                      |
| **Target CYC ceiling**        | ≤ 8                                                           |
| **Host method**               | `IsCommandForThisInstrument` (lines 294–352)                  |
| **Host CYC (pre-extraction)** | 18 (LOC: 19, per `complexity_audit_fresh_2026-06-14.txt`)     |
| **Host CYC (post-extraction)**| ~7 (retains `isGlobalCommand` block + diagnostic `Print`)     |
| **Extraction region**         | Lines 321–337 — 15-term `isForMe` OR chain                    |

The projected CYC of `IsSymbolMatch` is **≤ 8**, achievable by:
- Collapsing the 8 keyword literals (`GLOBAL`, `ALL`, `ON`, `OFF`, `RMA`, `ORB`, `OR`,
  `MOMO`) into a `HashSet<string>` lookup — 1 branch point.
- Collapsing the 3 micro-alias clauses (`MES→ES`, `MYM→YM`, `MGC→GC`) into a
  `ReadOnlyDictionary<string,string>` lookup — 1 branch point.
- Retaining the 4 string-comparison arms (`==`, `StartsWith` ×2, `Contains`) — 4 branch points.

Total projected CYC for `IsSymbolMatch`: **≤ 6**, well within the ≤ 8 target ceiling.

---

## File

**`src/V12_002.UI.IPC.cs`**

- Host method `IsCommandForThisInstrument`: lines 294–352
- Extraction target region: lines 321–337 (the `isForMe` boolean assignment)

The file is a single C# partial-class file. `IsCommandForThisInstrument` is declared
`private`, confining the entire extraction within this one file.

---

## Callers

**Callers count: 1**

| Caller              | Location                              | How it calls                                  |
|---------------------|---------------------------------------|-----------------------------------------------|
| `ProcessIpcCommands`| `src/V12_002.UI.IPC.cs`, line 417     | `if (!IsCommandForThisInstrument(action, targetSymbol))` |

After extraction, `IsCommandForThisInstrument` will call `IsSymbolMatch` internally.
The external call site at line 417 is **unchanged** — `ProcessIpcCommands` continues to
call `IsCommandForThisInstrument` with no signature or behavioural difference visible to
it. Net new callers of `IsSymbolMatch` post-extraction: **1**
(only `IsCommandForThisInstrument` itself).

Cross-file ripple: **none** — `IsCommandForThisInstrument` is `private`.
Blast-radius containment score: **High**.

---

## Scope Boundary

This is a **single method** extraction epic. The **scope boundary** is defined as follows:

> Extract exactly the 15-term `isForMe` boolean expression at lines 321–337 of
> `IsCommandForThisInstrument` into a new `private bool IsSymbolMatch(string target,
> string mySym, string myFull)` helper. Nothing else is touched in this ticket.

The **scope boundary** is enforced by three explicit exclusion rules (see next section).

---

## Why Other Methods Are NOT in Scope (V12.23)

Per **V12.23** scoping policy, each wave-7 epic covers exactly **one extraction target**.
The following methods are explicitly excluded from this ticket:

### `isGlobalCommand` block (lines 297–314)

The `isGlobalCommand` OR chain is a logically distinct concern: it tests the **action**
string against a list of fleet-wide command verbs, with no dependency on the symbol/
instrument identity. Extracting it is a separate architectural decision tracked by the
global-command registry design in **ticket-03**. Including it here would mix two
orthogonal concerns and violate the single-responsibility boundary of this epic.

### Diagnostic `Print` block (lines 339–349)

The `Print` call references `isGlobalCommand`, `isForMe`, `action`, `target`, and `mySym`
— values produced by both the global-command block and the symbol-match block. Extracting
the print logic requires resolving this dual coupling first (a follow-on pass), and it
carries no CYC contribution to `IsCommandForThisInstrument`. It is out of scope for this
ticket.

### `ProcessIpcCommands` (line 417 caller)

`ProcessIpcCommands` is the caller — it is not a complexity hotspot and has no extraction
candidate within its own body that is related to symbol matching. It is explicitly outside
the **scope boundary** of this epic.

### All other methods in `src/V12_002.UI.IPC.cs`

No other method in the file is referenced in the wave-7 epic list entry for EPIC-W7-018.
The V12.23 policy mandates that scope remain a **single method**: adding any other symbol
to this ticket would require a separate epic, separate hotspot analysis, and separate
blast-radius evaluation.

---

## Extraction Summary

| Item                          | Value                                                         |
|-------------------------------|---------------------------------------------------------------|
| **Epic**                      | EPIC-W7-018                                                   |
| **Wave**                      | 7                                                             |
| **Phase**                     | 1 — Scope Definition                                          |
| **Single method in scope**    | `IsSymbolMatch`                                               |
| **Scope confirmed**           | Yes — **single method**, scope boundary enforced by V12.23   |
| **Source file**               | `src/V12_002.UI.IPC.cs`                                       |
| **Extraction lines**          | 321–337 (from `IsCommandForThisInstrument`)                   |
| **Current CYC**               | 0 (net-new)                                                   |
| **Target CYC**                | ≤ 8                                                           |
| **Callers count**             | 1 (`ProcessIpcCommands` → `IsCommandForThisInstrument`)       |
| **Cross-file ripple**         | None                                                          |
| **Risk**                      | Low                                                           |

---

## Agent Tracking

| Field              | Value                          |
|--------------------|--------------------------------|
| **Agent Name**     | v12-phase1-scope               |
| **Bobcoins Used**  | 1.0                            |
| **Wave**           | 7                              |
| **Phase**          | 1 — Scope Definition           |
| **Epic**           | EPIC-W7-018                    |
| **Output**         | `docs/brain/EPIC-W7-018/00-scope.md` |
| **CYC Confirmed**  | 0 (net-new; fallback = 8; target ≤ 8) |
| **Scope Policy**   | V12.23 — single method per epic |
| **Sources Used**   | `00-hotspots.md`, `manifest.json`, `src/V12_002.UI.IPC.cs` grep, direct source read lines 294–352, 410–425 |
