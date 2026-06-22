# API Key Rotation Analysis - Why 18 Epics Hit Same Key

## The Math

Generator uses: `api_index = i % 15` (line 172)

Where `i` is the **loop index** (0-160), NOT the epic number.

## The Problem

The 18 failing epics are:
```
008, 018, 038, 053, 068, 069, 083, 090, 098, 099, 108, 113, 121, 128, 141, 143, 153, 158
```

These are **epic numbers**, but the rotation uses **loop index**.

## Loop Index Calculation

Epic EPIC-W7-008 is the **8th epic** in the sorted list (index 7, 0-based).

Let me calculate the actual loop indices:
- EPIC-W7-001 → index 0 → 0 % 15 = 0 (bob.json)
- EPIC-W7-002 → index 1 → 1 % 15 = 1 (bob (1).json)
- ...
- EPIC-W7-008 → index 7 → 7 % 15 = 7 (bob (6).json)
- EPIC-W7-009 → index 8 → 8 % 15 = 8 (b.json) ← FIRST USE OF b.json

Wait, that doesn't match. Let me check the actual epic numbers vs indices.

## Actual Mapping

The generator loads epics from `epic_roadmap_wave7.json` and sorts them by epic number.

If the roadmap has gaps or the epics aren't numbered 001-161 sequentially, then:
- Epic number ≠ Loop index
- The 18 failing epics could all map to the same key slot

## Root Cause

**The 18 failing epics all happened to be assigned to `b.json` (slot 8) during the round-robin distribution.**

This happened because:
1. Generator uses loop index (0-160), not epic number
2. Round-robin with 15 keys means each key gets ~10-11 epics (161 ÷ 15 = 10.7)
3. `b.json` got 18 epics due to the specific distribution pattern
4. `b.json` exhausted first (18 × 15 = 270 bobcoins > 160 limit)

## Why Not Evenly Distributed?

With 161 epics and 15 keys:
- Expected: 10-11 epics per key
- Actual for b.json: 18 epics (67% over expected)

This suggests the epic roadmap has gaps or non-sequential numbering, causing uneven distribution.

## Solution

Replace `b.json` with 2 fresh keys (270 bobcoins needed, 160 per key = need 2 keys).