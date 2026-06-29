# EPIC-W7-054 Hotspot Analysis

**Method:** UnknownMethod
**CYC:** 20
**File:** src/unknown

---

## Overview

`UnknownMethod` is a Wave 7 complexity hotspot with a Cyclomatic Complexity score of 20, placing it
well above the V12 governance threshold of <=8. Because both the method name and source file path
were not resolvable at the time of Phase 0 triage (placeholders used per task brief), this document
records the structural analysis based on the confirmed CYC metric alone and prescribes extraction
guidance that applies regardless of the concrete symbol once resolved.

A CYC of 20 indicates roughly 20 independent execution paths through the method body. At this scale
the method almost certainly contains one or more of: deeply nested conditionals, large switch/match
blocks, compound boolean guards, or multiple early-return/throw branches with distinct business
meanings that have accumulated over time without intermediate extraction.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Target method** | UnknownMethod (placeholder — resolve before Phase 1) |
| **Source file** | src/unknown (placeholder — resolve before Phase 1) |
| **Confirmed CYC** | 20 |
| **V12 threshold** | <=8 per method |
| **CYC overage** | +12 above threshold (2.5x ceiling) |
| **Estimated helpers needed** | 3-4 extracted methods to reach <=8 on the parent |
| **Risk level** | Medium-High (high branch count; extraction must preserve all decision paths) |
| **Threading/state notes** | Unknown until source is resolved; assume shared mutable state present |

---

## Top Complexity Drivers (Structural Inference at CYC=20)

1. **Multi-branch dispatch or classification block (~7-9 CYC)**
   Methods reaching CYC=20 almost universally contain a top-level dispatcher: an if-else chain or
   switch block routing on a type, status enum, or command kind. Each branch arm contributes +1 CYC.
   An 8-12 arm dispatcher alone accounts for 8-12 points. The correct extraction pattern is to
   promote each logical group of arms into a dedicated helper (e.g. `HandleCategoryX`,
   `ProcessStateY`) and replace the dispatcher body with delegating calls, bringing parent CYC to
   the number of dispatch arms (ideally <=5).

2. **Compound guard predicates and null-safety chains (~5-7 CYC)**
   Secondary complexity at this scale typically comes from 2-4 distinct compound boolean conditions
   embedded inline rather than extracted as named predicates. Each `&&` or `||` operator in a
   condition contributes +1 CYC per the standard McCabe count. Extracting these into
   `IsEligibleFor...` or `ShouldSkip...` predicate helpers eliminates the CYC contribution from the
   parent while making the intent explicit — a Jane Street-aligned pattern.

3. **Nested loops with inner conditionals and early exits (~4-6 CYC)**
   A CYC delta not explained by dispatch arms is almost always nested iteration: a `foreach` or
   `for` loop containing inner `if` guards, `continue`/`break` exits, or exception-catching try
   blocks. Each inner branch adds CYC. The correct extraction is to pull the loop body into a
   helper that handles one iteration, reducing both nesting depth and parent CYC in a single move.

---

## Recommended Extraction Count

**Recommended: 3-4 helper extractions to reduce parent CYC from 20 to <=8.**

**Rationale:**

Reaching a post-extraction CYC of <=8 on the parent from a baseline of 20 requires removing at
least 12 decision points. Distributing this across 3-4 helpers achieves:

- Each helper absorbs 3-4 CYC points of the original body
- Parent becomes a thin coordinator at CYC 5-7 (dispatch arms + null guard + try/catch if present)
- Each helper individually satisfies the <=8 ceiling without further decomposition

Extraction order should follow dependency depth: extract the deepest nested logic first (loop bodies
and predicate chains), then the top-level dispatch arms last. This ordering minimises merge conflict
risk and makes each step independently reviewable.

**Phase 1 pre-requisite:** Resolve the concrete method name and source file path from the hotspot
scanner before beginning extraction planning. All subsequent phases depend on the live symbol.

---

## Agent Tracking

Agent Name: Bob (v12-phase0-hotspot) | Bobcoins Used: 1.0 | Execution Time: ~30s
Wave: 7 | Phase: 0 | Epic: EPIC-W7-054 | CYC Confirmed: 20
