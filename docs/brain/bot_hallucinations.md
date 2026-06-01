# Bot Hallucination Log

Tracks false positives for pattern learning.

---

## PR #10 - Round 3 (2026-06-01)

### CodeFactor: Documentation Parser Bug (2 instances)

**Pattern**: CodeFactor's regex parser fails on numeric documentation patterns ending with period.

**Findings**:
1. Line 672: `/// Target CYC: <=5.` - Flagged as missing period (period IS present)
2. Line 709: `/// Target CYC: <=5.` - Flagged as missing period (period IS present)

**Suggested Fix**: `/// Target CYC:. <=5.` (nonsensical - adds period BEFORE "<=5")

**Root Cause**: CodeFactor's documentation linter uses regex that cannot parse `<=5.` correctly.

**Frequency**: 2/2 (100% false positive rate on this pattern)

**Mitigation**: Ignore CodeFactor documentation style warnings on numeric patterns.

---

## Valid Findings (Not Hallucinations)

### Gitar + Greptile: P1 Null-Handling Bug (VALID)

**Finding**: "Null stop silently stored at line 505"

**Status**: **VALID** - This is a REAL P1 bug that was missed in Round 1.

**Why This Is NOT a Hallucination**:
- Null guard exists in helper function (lines 698-702) ✅
- BUT caller does NOT check return value (line 505) ❌
- If `PublishPhoton_StopOrder` returns null, line 505 stores null in dictionary
- Downstream code expects valid Order → NullReferenceException

**Round 1 Gap**: Round 1 added null guard in helper but did NOT audit caller's handling of null return.

**Bot Consensus**: 2 bots (Gitar + Greptile) correctly identified this issue.

**Lesson Learned**: When adding null guards to helper functions, ALWAYS audit all callers to ensure they handle null returns correctly.

---

## Pattern Summary

| Pattern | Bot(s) | False Positive Rate | Persistence |
|---------|--------|---------------------|-------------|
| **Numeric doc patterns** | CodeFactor | 100% (2/2) | Single round |
| **Null guard with early return** | Gitar, Greptile | 0% (VALID) | N/A |

---

## Bot Reliability (Round 3)

| Bot | Findings | Valid | Accuracy | Notes |
|-----|----------|-------|----------|-------|
| **Gitar** | 1 | 1 | 100% | Correctly identified P1 bug |
| **Greptile** | 1 | 0 (dup) | N/A | Duplicate of Gitar (still valid) |
| **Codacy** | 5 | 5 | 100% | All complexity/style valid |
| **CodeFactor** | 2 | 0 | 0% | Parser bug on numeric docs |
| **CodeScene** | 0 | N/A | N/A | APPROVED (6 gates passed) |
| **SonarCloud** | 0 | N/A | N/A | Quality Gate PASSED |

**Overall Bot Accuracy**: 6/8 unique findings (75%)

---

## Lessons Learned

1. **CodeFactor Limitations**: Regex-based linters fail on complex patterns (e.g., `<=5.`).
2. **Bot Consensus ≠ Always Wrong**: In Round 3, 2 bots (Gitar + Greptile) agreed on a VALID P1 bug.
3. **Control Flow Analysis**: Gitar/Greptile correctly traced null return from helper to caller.
4. **Round 1 Gap**: Adding null guard in helper is insufficient - must audit all callers.

---

## Recommended Actions

1. **CodeFactor**: Ignore documentation style warnings on numeric patterns.
2. **Gitar/Greptile**: Trust null-handling warnings when bot consensus exists.
3. **Audit Callers**: When adding null guards, always audit all call sites.
4. **Jane Street Principle**: "Make illegal states unrepresentable" - helpers should throw exceptions instead of returning null.

---

## PR #10 - Round 5 (2026-06-01)

### CodeFactor: Documentation Parser Bug (2 instances) - PERSISTENT

**Pattern**: Same parser bug from Round 3/4 persists in Round 5.

**Findings**:
1. Line 350 in [`V12_002.Orders.Management.StopSync.cs`](../../src/V12_002.Orders.Management.StopSync.cs:350): `/// Target CYC: <=5.` - Flagged as missing period (period IS present)
2. Line 723 in [`V12_002.SIMA.Dispatch.cs`](../../src/V12_002.SIMA.Dispatch.cs:723): `/// Target CYC: <=5.` - Flagged as missing period (period IS present)

**Suggested Fix**: `/// Target CYC:. <=5.` (nonsensical - adds period BEFORE "<=5")

**Root Cause**: CodeFactor's documentation linter uses regex that cannot parse `<=5.` correctly.

**Frequency**: 4/4 across 3 rounds (100% false positive rate on this pattern)

**Persistence**: Round 3, Round 4, Round 5 (same files, same lines)

**Mitigation**: Ignore CodeFactor documentation style warnings on numeric patterns.

---

## Valid Findings (Not Hallucinations)

### Round 5: Greptile P1 Race Condition (VALID)

**Finding**: "Behavioral regression: silent stop-resize abort when TryRemove loses a race"

**Lines**: 335-365 in [`V12_002.Orders.Management.StopSync.cs`](../../src/V12_002.Orders.Management.StopSync.cs:335-365)

**Status**: **VALID P1 CRITICAL** - Race condition handling bug introduced during extraction.

**Why This Is NOT a Hallucination**:
- Helper returns `false` in two distinct cases:
  1. Fresh pending (age < threshold) → Update quantity, early exit ✅ CORRECT
  2. TryRemove race (removal failed) → Should retry, NOT abort ❌ WRONG
- When `TryRemove` fails (concurrent purge), caller aborts instead of retrying
- Position's stop quantity left unsynced, position over-exposed

**Root Cause**: Extraction collapsed two control flows into single boolean return.

**Jane Street Violation**: Fail-Fast Isolation - race condition silently swallowed.

**Bot Confidence**: High - Detailed behavioral analysis with race condition walkthrough.

**Lesson Learned**: Boolean returns can suffer from "boolean blindness" - use enums for multi-case control flow.

---

### Round 3: Gitar + Greptile: P1 Null-Handling Bug (VALID)

**Finding**: "Null stop silently stored at line 505"

**Status**: **VALID** - This is a REAL P1 bug that was missed in Round 1.

**Why This Is NOT a Hallucination**:
- Null guard exists in helper function (lines 698-702) ✅
- BUT caller does NOT check return value (line 505) ❌
- If `PublishPhoton_StopOrder` returns null, line 505 stores null in dictionary
- Downstream code expects valid Order → NullReferenceException

**Round 1 Gap**: Round 1 added null guard in helper but did NOT audit caller's handling of null return.

**Bot Consensus**: 2 bots (Gitar + Greptile) correctly identified this issue.

**Lesson Learned**: When adding null guards to helper functions, ALWAYS audit all callers to ensure they handle null returns correctly.

---

## Pattern Summary

| Pattern | Bot(s) | False Positive Rate | Persistence |
|---------|--------|---------------------|-------------|
| **Numeric doc patterns** | CodeFactor | 100% (4/4) | 3 rounds (R3, R4, R5) |
| **Null guard with early return** | Gitar, Greptile | 0% (VALID) | N/A |
| **Race condition handling** | Greptile | 0% (VALID) | N/A |

---

## Bot Reliability (Cumulative)

| Bot | Total Findings | Valid | Hallucinations | Accuracy |
|-----|----------------|-------|----------------|----------|
| **Greptile** | 2 | 2 | 0 | 100% |
| **Gitar** | 1 | 1 | 0 | 100% |
| **Codacy** | 5 | 5 | 0 | 100% |
| **CodeFactor** | 4 | 0 | 4 | 0% |
| **CodeScene** | 0 | N/A | N/A | N/A (APPROVED) |
| **SonarCloud** | 0 | N/A | N/A | N/A (Quality Gate PASSED) |

**Overall Bot Accuracy**: 8/12 unique findings (67%)

**Greptile Reliability**: 100% (2/2 valid, detailed analysis)

---

## Lessons Learned

1. **CodeFactor Limitations**: Regex-based linters fail on complex patterns (e.g., `<=5.`). Ignore documentation warnings.
2. **Bot Consensus ≠ Always Wrong**: In Round 3, 2 bots (Gitar + Greptile) agreed on a VALID P1 bug.
3. **Solo Findings Can Be Valid**: Greptile's Round 5 solo finding is high-confidence due to detailed behavioral analysis.
4. **Control Flow Analysis**: Greptile correctly traced race condition through extraction refactoring.
5. **Boolean Blindness**: Single boolean returns cannot distinguish multiple control flow cases - use enums.
6. **Extraction Hazards**: Collapsing control flows into boolean returns loses semantic information.

---

## Recommended Actions

1. **CodeFactor**: Ignore documentation style warnings on numeric patterns (persistent bug).
2. **Greptile**: Trust behavioral analysis warnings, especially with detailed race condition walkthroughs.
3. **Audit Callers**: When adding null guards, always audit all call sites.
4. **Type Safety**: Use enums instead of booleans for multi-case control flow (Jane Street: "Make illegal states unrepresentable").
5. **Extraction Reviews**: When extracting helpers, verify all control flow paths are preserved.

---

**Last Updated**: 2026-06-01
**Total Hallucinations Logged**: 4 (CodeFactor parser bugs across 3 rounds)
**Valid Findings Confirmed**: 8 (2 P1 critical + 1 P1 race + 5 Codacy)
