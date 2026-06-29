# EPIC-W7-017 — Phase 1: Scope Definition

## Scope Summary

This document defines the precise refactor scope for EPIC-W7-017 (Wave 7). The scope
boundary is drawn around a **single method** only. No other methods are included.

---

## Method in Scope

| Field            | Value                                                    |
|------------------|----------------------------------------------------------|
| **Method**       | `TryApplyConfigTarget_Value`                             |
| **File**         | `src/V12_002.UI.IPC.Commands.Config.cs`                  |
| **Lines**        | 209 – 297                                                |
| **Class**        | `V12_002` (partial class, `Strategy` subtype)            |
| **Current CYC**  | 22                                                       |
| **Target CYC**   | ≤ 8                                                      |
| **Wave**         | 7                                                        |

---

## Scope Boundary

The **scope boundary** is defined as: `TryApplyConfigTarget_Value` in
`src/V12_002.UI.IPC.Commands.Config.cs`, lines 209–297, and any private helpers
that are *newly extracted* from that method body during this epic. No pre-existing
methods, no callers, and no downstream consumers cross the scope boundary.

This is a **single method** refactor. The scope boundary is intentionally narrow
to contain blast radius, preserve the existing call contract, and allow full
validation without touching unrelated logic.

---

## Callers

| Caller                  | File                                        | Line |
|-------------------------|---------------------------------------------|------|
| `TryApplyConfigTargets` | `src/V12_002.UI.IPC.Commands.Config.cs`     | 198  |

**Callers count: 1**

`TryApplyConfigTarget_Value` has exactly one direct caller (`TryApplyConfigTargets`)
within the same file. The method signature `bool TryApplyConfigTarget_Value(string key,
string val)` must remain unchanged so that this single call site requires no
modification. Grep across `src/` confirms no other call sites exist.

---

## CYC Reduction Target

| Metric          | Value      |
|-----------------|------------|
| Current CYC     | 22         |
| Target CYC      | ≤ 8        |
| Required drop   | ≥ 14 points|
| Projected CYC   | 5 – 7      |

The Phase 0 hotspot analysis (see `00-hotspots.md`) identified three extraction
opportunities: a generic `TrySetValidatedTargetValue` helper, relocation of the `"CIT"`
key to its structurally correct handler, and a data-driven `Dictionary<string,
Action<double>>` dispatch table. Together these project CYC to 5–7 — a 68–77%
reduction.

---

## Why Other Methods Are NOT in Scope

V12.23 scoping rules prohibit multi-method or cross-file changes within a single epic
phase. The following related methods are explicitly **excluded**:

| Method                          | Reason Excluded                                                                     |
|---------------------------------|-------------------------------------------------------------------------------------|
| `TryApplyConfigTargets`         | Sole caller; its body changes only if signature of called method changes — it does not. Excluded by V12.23 single-method rule. |
| `TryApplyConfigTarget_Type`     | Sibling handler; structurally similar but a separate decision domain. Not flagged as a hotspot. Excluded by V12.23. |
| `TryApplyConfigTarget_Count`    | Sibling handler; handles COUNT/CIT count variants. CYC within threshold. Excluded by V12.23. |
| `ValidateIpcMultiplier`         | Pure utility in `src/V12_002.UI.IPC.cs:134`; no changes required to its body. Excluded by V12.23 cross-file rule. |
| All 13 downstream `TargetNValue` readers | Read-only consumers; refactor preserves assigned values exactly. Excluded by V12.23 blast-radius containment rule. |

**V12.23 rule summary:** Each epic phase targets the single highest-CYC method
identified in the hotspot analysis. Adjacent methods, callers, and downstream
consumers are out of scope unless a code change in the targeted method makes a
call-site update strictly unavoidable. In this case, the method signature is
preserved in full, so no adjacent method requires modification.

---

## Exclusions Checklist

- [ ] `TryApplyConfigTargets` — excluded (caller, signature unchanged)
- [ ] `TryApplyConfigTarget_Type` — excluded (sibling, separate domain)
- [ ] `TryApplyConfigTarget_Count` — excluded (sibling, CYC within threshold)
- [ ] `ValidateIpcMultiplier` — excluded (cross-file utility, no changes needed)
- [ ] All 13 downstream TargetNValue readers — excluded (consumers, values preserved)

---

## Inputs to This Phase

| Input           | Source                                      |
|-----------------|---------------------------------------------|
| CYC measurement | `00-hotspots.md` (Phase 0 output)           |
| Caller list     | grep of `src/` for `TryApplyConfigTarget_Value` |
| Blast radius    | `00-hotspots.md` Section: Blast Radius Summary |
| V12.23 rules    | Project scoping policy (single-method rule) |

---

## Agent Tracking

| Field              | Value                                               |
|--------------------|-----------------------------------------------------|
| **Agent Name**     | v12-phase1-scope                                    |
| **Epic**           | EPIC-W7-017                                         |
| **Wave**           | 7                                                   |
| **Phase**          | 1 — Scope Definition                                |
| **Method**         | `TryApplyConfigTarget_Value`                        |
| **Scope Confirmed**| Single method ✅                                    |
| **CYC Current**    | 22                                                  |
| **CYC Target**     | ≤ 8                                                 |
| **Completed**      | Phase 1 — Scope Definition ✅                       |
