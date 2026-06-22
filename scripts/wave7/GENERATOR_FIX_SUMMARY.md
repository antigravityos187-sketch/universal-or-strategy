# Generator Fix - 20 API Keys with Even Distribution

**Date**: 2026-06-22  
**Issue**: Generator was using 15 keys instead of 19, causing uneven distribution  
**Fix**: Updated to use all 20 keys (19 existing + 2 new - 1 removed)

## Changes Made

### 1. Updated API_FILES List (Line 39-48)

**Before** (15 keys):
```python
API_FILES = [
    "bob.json", "bob (1).json", "bob (2).json", "pepeescobar.json",
    "bob (4).json", "bob (5).json", "bob (6).json",
    "b.json", "b (3).json",  # b.json exhausted
    "jessica.json", "mikethelife.json", "sammy96.json",
    "sean.carter.jr@atomicmail.io.json", "tory.json", "iyanajackson.json"
]
```

**After** (20 keys):
```python
API_FILES = [
    "bob.json", "bob (1).json", "bob (2).json", "pepeescobar.json",
    "bob (4).json", "bob (5).json", "bob (6).json",
    "fresh1.json", "fresh2.json",  # Replaced exhausted b.json
    "b (3).json",
    "jessica.json", "mikethelife.json", "sammy96.json",
    "sean.carter.jr@atomicmail.io.json", "tory.json", "iyanajackson.json",
    "alprofit.json", "rakaarababa.json", "ranirabah (1).json", "jimmydore.json"
]
```

### 2. Updated Validation (Line 149)

**Before**: `if len(api_keys) != 15:`  
**After**: `if len(api_keys) != 20:`

### 3. Updated Round-Robin (Line 172)

**Before**: `api_index = i % 15`  
**After**: `api_index = i % 20`

### 4. Updated Distribution Report (Line 199-203)

**Before**: Showed 15 keys without names  
**After**: Shows 20 keys with names and even distribution

## Distribution Analysis

### Old Distribution (15 keys)
- 161 epics ÷ 15 keys = 10.7 epics per key
- Uneven: Some keys got 18 epics (b.json), others got 10
- Result: b.json exhausted (18 × 15 = 270 > 160 limit)

### New Distribution (20 keys)
- 161 epics ÷ 20 keys = 8.05 epics per key
- Even: Each key gets 8-9 epics
- Result: No key exceeds 135 bobcoins (9 × 15 = 135 < 160 limit)

## Expected Output

When generator runs with 20 keys:
```
[*] Loaded 20 API keys
[*] Found 161 pending epics for Wave 7

[OK] Generated 161 Phase 0 scripts

[*] API Distribution (20 keys, ~8 epics each):
    API  1 (bob                          ): 8 epics
    API  2 (bob (1)                      ): 8 epics
    API  3 (bob (2)                      ): 8 epics
    API  4 (pepeescobar                  ): 8 epics
    API  5 (bob (4)                      ): 8 epics
    API  6 (bob (5)                      ): 8 epics
    API  7 (bob (6)                      ): 8 epics
    API  8 (fresh1                       ): 8 epics
    API  9 (fresh2                       ): 8 epics
    API 10 (b (3)                        ): 8 epics
    API 11 (jessica                      ): 8 epics
    API 12 (mikethelife                  ): 8 epics
    API 13 (sammy96                      ): 8 epics
    API 14 (sean.carter.jr@atomicmail.io ): 8 epics
    API 15 (tory                         ): 8 epics
    API 16 (iyanajackson                 ): 8 epics
    API 17 (alprofit                     ): 8 epics
    API 18 (rakaarababa                  ): 8 epics
    API 19 (ranirabah (1)                ): 8 epics
    API 20 (jimmydore                    ): 9 epics
```

## Waiting for User Action

**Status**: Generator updated and ready  
**Blocker**: Need 2 fresh API keys uploaded as `fresh1.json` and `fresh2.json`

**User must**:
1. Create 2 fresh Bob CLI API keys at https://myibm.ibm.com/dashboard
2. Save as `fresh1.json` and `fresh2.json`
3. Upload to `docs/API/` directory
4. Push to GitHub: `git add docs/API/fresh*.json && git commit -m "feat: Add fresh API keys" && git push`

**Then agent will**:
1. Pull changes on VM
2. Regenerate 18 failed epic scripts with new distribution
3. Re-launch failed epics
4. Monitor to 161/161 completion

## Benefits of 20-Key Distribution

1. **Even Load**: Each key handles 8-9 epics (vs 10-18 with 15 keys)
2. **No Exhaustion**: Max usage 135 bobcoins (vs 270 with old distribution)
3. **Safety Buffer**: 25 bobcoins per key (160 - 135 = 25)
4. **Future-Proof**: Can handle up to 200 epics (20 × 10 = 200)

## Files Modified

- `scripts/wave7/generate_phase0_scripts_fixed.py` (lines 39-48, 149, 172, 199-203)

## Next Wave Phases

After 161/161 completion, the same 20-key distribution will be used for:
- Phase 1 (Scope Definition)
- Phase 1.5 (Boundary Validation)
- Phase 2 (Architecture Planning)
- Phase 3 (DNA Audit)
- Phase 4 (Ticket Generation)
- Phase 5 (Ticket Execution)
- Phase 5.V (Verification)
- Phase 6 (Final Review)

All phase generators will use the same 20-key pattern for consistency.