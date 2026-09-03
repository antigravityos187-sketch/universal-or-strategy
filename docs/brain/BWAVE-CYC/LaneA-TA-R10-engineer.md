# BWAVE-CYC Lane-A — TA-R10 Engineer Completion

**Ticket**: TA-R10
**Engineer**: ptt-engineer
**Status**: BUILD_PASS

---

## Methods Modified

### 1. `RuleToDto` (CopyEngine.cs, ~L6186)
- **CCN before**: 9
- **CCN after**: 7
- **Change**: Extracted multiplier lookup into `GetFollowerMultiplier(CopyRule rule, int i)`; replaced 2-branch inline expression `(rule.FollowerMultipliers != null && i < rule.FollowerMultipliers.Length) ? rule.FollowerMultipliers[i] : 1` with single call.

### 2. `DtoToRule` (CopyEngine.cs, ~L6221)
- **CCN before**: 11
- **CCN after**: 7
- **Change**: Extracted ATM mode map building into `BuildAtmModeMap(CopyRuleDto dto)`; replaced 3-branch inline block (null check + for loop + IsNullOrEmpty guard) with single call.

---

## Helpers Extracted

### `GetFollowerMultiplier(CopyRule rule, int i)` — private static
- **Location**: CopyEngine.cs ~L6276
- **CCN**: 3 (1 base + 1 `&&` + 1 `?:`)
- **Purpose**: Returns `rule.FollowerMultipliers[i]` when array is present and long enough, otherwise returns 1.
- **JS-002**: No return null — returns int with default value 1.
- **JS-021**: Pure computation, no lock().

### `BuildAtmModeMap(CopyRuleDto dto)` — private static
- **Location**: CopyEngine.cs ~L6285
- **CCN**: 4 (1 base + 1 if-null-guard + 1 for + 1 if-IsNullOrEmpty)
- **Purpose**: Builds `Dictionary<string, FollowerAtmMode>` from `dto.FollowerAtmModeNames`. Returns empty dictionary when `FollowerAtmModeNames` is null (backward compat with B6/B7 XML).
- **JS-002**: Returns empty dictionary, never null.
- **JS-021**: No lock().

---

## Tests Added (CopyEngineTests.cs)

All 5 new `[Fact]` tests added at end of test class:

| Test | Helper Covered |
|------|----------------|
| `GetFollowerMultiplier_ShouldReturnStoredValue_WhenIndexValid` | `GetFollowerMultiplier` |
| `GetFollowerMultiplier_ShouldReturnOne_WhenMultipliersIsNull` | `GetFollowerMultiplier` |
| `GetFollowerMultiplier_ShouldReturnOne_WhenIndexOutOfRange` | `GetFollowerMultiplier` |
| `BuildAtmModeMap_ShouldReturnEmptyDictionary_WhenFollowerAtmModeNamesIsNull` | `BuildAtmModeMap` |
| `BuildAtmModeMap_ShouldPopulateDictionary_WhenValidAtmModeNamesProvided` | `BuildAtmModeMap` |

---

## Lizard Results

```
lizard src/PropTraderTools/CopyEngine.cs --CCN 8
```

| Method | CCN Before | CCN After | Warning? |
|--------|-----------|----------|----------|
| `RuleToDto` | 9 | 7 | NONE |
| `DtoToRule` | 11 | 7 | NONE |
| `GetFollowerMultiplier` | NEW | 3 | NONE |
| `BuildAtmModeMap` | NEW | 5 | NONE |

**!!!! Warnings count: 0 for ticket methods !!!**

---

## Build Result

```
dotnet build src/PropTraderTools/
Build succeeded.
0 Error(s)
```

---

## 7-Scan Results

| Scan | Command | Result |
|------|---------|--------|
| SCAN-01 | `Select-String "lock(" src/PropTraderTools/*.cs` | 0 actual lock() calls (17 matches are comments only) |
| SCAN-02 | Non-ASCII chars in CopyEngine.cs | 0 |
| SCAN-03 | `Select-String "FontFamily" src/PropTraderTools/*.cs` | 0 actual usage (4 matches are comments only) |
| SCAN-04 | `Select-String "#[0-9A-Fa-f]{6}" src/PropTraderTools/*.cs` | 0 actual usage (9 matches are comments only) |
| SCAN-05 | CreateOrder calls with "PTT-" prefix | 0 violations |
| SCAN-06 | `Select-String "DateTime\.Now[^U]"` | 0 actual usage (1 match is comment only) |
| SCAN-07 | `Select-String "\block\s*\("` | 0 actual lock() calls (17 matches are comments only) |

---

## CS Delta

`cs delta` output:
- `src/PropTraderTools/CopyEngine.cs`: `[X] Fixed issue: Overall Code Complexity` — Code health IMPROVED.
- No new issues introduced in ticket target file.

---

## JS Rule Compliance

- **JS-021** (no lock()): Zero lock() calls in any new code. ✓
- **JS-002** (no return null): `BuildAtmModeMap` returns empty Dictionary, never null. `GetFollowerMultiplier` returns int. ✓
- **JS-033** (no async void): All helpers are synchronous. ✓
