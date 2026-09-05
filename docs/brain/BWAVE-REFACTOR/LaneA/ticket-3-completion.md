# Ticket A-3 Completion Report

**Ticket**: A-3 — DW-C39-09 SaveRules not called after OnAddRule
**Engineer**: ptt-engineer
**Date**: 2026-08-26
**Scope**: TradeCopierWindow.cs — OnAddRule method body only

## Change Made

**File**: `src/PropTraderTools/TradeCopierWindow.cs`
**Method**: `OnAddRule` at line 902

**Before** (lines 902–906):
```csharp
private void OnAddRule(object sender, RoutedEventArgs e)
{
    _rulesPanel.Children.Add(BuildDynamicRuleRow());
    ApplyFeatureFlags(CopyEngine.Instance.Flags); // gate newly-added buttons
}
```

**After** (lines 902–907):
```csharp
private void OnAddRule(object sender, RoutedEventArgs e)
{
    _rulesPanel.Children.Add(BuildDynamicRuleRow());
    ApplyFeatureFlags(CopyEngine.Instance.Flags); // gate newly-added buttons
    CopyEngine.Instance.SaveRules();              // DW-C39-09: persist immediately
}
```

**Diff**: +1 line. No other lines modified. No other methods touched.

## Scan Results

| Scan | Command | Result | Status |
|------|---------|--------|--------|
| SCAN-01 | `Select-String -Pattern "lock\("` | 0 actual lock() calls (comment hits only) | PASS |
| SCAN-02 | Non-ASCII character scan | 0 results | PASS |
| SCAN-03 | `Select-String -Pattern "FontFamily"` | 0 results (comment hits only) | PASS |
| SCAN-04 | `Select-String -Pattern "#[0-9A-Fa-f]{6}"` | 0 results (comment annotation hits only) | PASS |
| SCAN-05 | CreateOrder PTT- prefix check | All CreateOrder calls use "PTT-" prefixed names | PASS |
| SCAN-06 | `Select-String -Pattern "DateTime\.Now[^U]"` | 0 results (comment hits only) | PASS |
| SCAN-07 | lizard CCN > 8 | 0 rows output — no method exceeds CCN 8 | PASS |

## Build Result

```
0 Warning(s)
0 Error(s)
```

**dotnet build src/PropTraderTools/**: 0 errors, 0 warnings.

## NT8 Sync Result

```
=== SYNC + VERIFY: PASS (18 files confirmed) ===
Copied: 1 | In-sync: 17 | Excluded: 71
All 18 files: OK (MD5 verified)
```

**18/18 OK — 0 MISMATCH lines.**

## CYC Report

| Method | CCN | Threshold | Status |
|--------|-----|-----------|--------|
| `OnAddRule` | 1 | ≤ 8 | PASS |

`lizard` output: `6,1,38,2,6,"AccountDisplayConverter::OnAddRule@902-907@src/PropTraderTools/TradeCopierWindow.cs"`

CCN = 1. Adding a method-call statement introduces no branch — CCN unchanged from pre-change value.

## Status: BUILD_PASS
