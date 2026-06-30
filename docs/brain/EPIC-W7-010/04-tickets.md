# Phase 4: Tickets — EPIC-W7-010

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Inputs:** docs/brain/EPIC-W7-010/02-architecture-plan.md + docs/brain/EPIC-W7-010/03-audit-report.md

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `ShowModeSpecificControls` |
| **Source File** | `src/V12_002.UI.Panel.Handlers.cs` |
| **Lines** | 690–719 |
| **Original CYC** | 8 |
| **Pattern Applied** | Replace Switch/If-Chains with Lookup Tables + Extract Named Helper Methods |
| **DNA Verdict** | PASS |

---

## ticket_count: 1

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | 1 |
| **helper_name** | `InitializeModeControlMap` |
| **concern** | Build and populate the private `Dictionary<string, Action>` that maps mode strings to their corresponding `ShowXxxControls` delegate; called once during class initialization |
| **lines_to_move** | The 7 switch-case arms from `ShowModeSpecificControls` (lines ~693–718): `case "ORB": ShowOrbControls(); break;` through `default: ShowOrbControls(); break;` — these 7 action-to-delegate bindings become the object initializer body of `_modeControlMap` in the new helper |
| **cyc_reduction** | 6 (parent drops from CYC 8 to CYC 2: removes 7 switch branches, retains 1 if-branch for TryGetValue miss) |
| **projected_helper_cyc** | 1 (linear dictionary object initializer — no branches, no loops) |

### New Field

```csharp
private Dictionary<string, Action> _modeControlMap;
```

### New Helper Body

```csharp
private void InitializeModeControlMap()
{
    _modeControlMap = new Dictionary<string, Action>
    {
        { "ORB",    ShowOrbControls    },
        { "RMA",    ShowRmaControls    },
        { "RETEST", ShowRetestControls },
        { "MOMO",   ShowMomoControls   },
        { "FFMA",   ShowFfmaControls   },
        { "TREND",  ShowTrendControls  },
        { "MNL",    ShowMnlControls    }
    };
}
```

### Refactored Parent Body

```csharp
// [EPIC-W7-010] Dictionary-dispatch replaces 8-arm switch (CYC 8 -> 2)
private void ShowModeSpecificControls(string mode)
{
    if (!_modeControlMap.TryGetValue(mode, out var show))
        show = ShowOrbControls;
    show();
}
```

### Call-Site Requirement

`InitializeModeControlMap()` MUST be called from the class initialization path (e.g., `OnStateChange` or constructor) before `ShowModeSpecificControls` is first invoked.

---

## projected_parent_cyc_after_all: 2

---

## CYC Verification

| Symbol | Pre-Extraction CYC | Post-Extraction CYC | <= 8? |
|---|---|---|---|
| `ShowModeSpecificControls` (parent) | 8 | 2 | YES |
| `InitializeModeControlMap` (new helper) | N/A | 1 | YES |
| **max_cyc_projected** | — | **2** | YES |

---

## Jane Street Alignment

| Rule | Status | Notes |
|---|---|---|
| CYC<=8 achieved | YES | Parent CYC 2, Helper CYC 1; max=2; 75% headroom |
| Single-responsibility per helper | YES | `InitializeModeControlMap` does exactly one thing: build the action map |
| Lock-free / Actor pattern preserved | YES | Pure UI visibility dispatch; no lock() blocks; no state mutations |
| Illegal states unrepresentable | YES (improved) | TryGetValue with explicit ORB fallback makes default case visible |
| Zero-allocation hot paths | YES | Dictionary built once at init; TryGetValue is O(1) hash — no heap allocation per dispatch |

---

## Sequential Thinking Evidence

### Thought 1 — Ticket Count
Method is a pure dispatch switch (CYC=8, at ceiling). Architecture plan confirms 1 extraction: `InitializeModeControlMap`. One ticket = one extracted helper = one concern. `ticket_count = 1`.

### Thought 2 — Lines to Move and CYC Projections
The 7 switch-case arms (lines ~693–718) move into the dictionary object initializer in `InitializeModeControlMap`. New helper CYC = 1 (no branches). Parent retains TryGetValue + 1 if-branch = CYC 2. `cyc_reduction = 6`.

### Thought 3 — Post-Extraction CYC Verification
- `ShowModeSpecificControls` after: CYC = 2 (1 base + 1 if) — 2 <= 8 PASS
- `InitializeModeControlMap`: CYC = 1 — 1 <= 8 PASS
- max_cyc_projected = 2. All Jane Street rules verified. Scope confined to `src/V12_002.UI.Panel.Handlers.cs`. No caller modifications.

---

## jcodemunch MCP Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | found=true, indexed=true, repo="antigravityos187-sketch/universal-or-strategy" |
| `get_symbol_complexity(ShowModeSpecificControls)` | Not in index (symbol resolved at compile time via partial class) |
| `get_extraction_candidates(V12_002.UI.Panel.Handlers.cs)` | 0 candidates (all helpers pre-extracted; CYC data confirms no residual hotspots) |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket-breakdown thoughts) |
| **Epic** | EPIC-W7-010 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Method** | `ShowModeSpecificControls` |
| **File** | `src/V12_002.UI.Panel.Handlers.cs` |
| **CYC Before** | 8 |
| **CYC After (max projected)** | 2 |
| **ticket_count** | 1 |
| **Pattern Applied** | Replace Switch/If-Chains with Lookup Tables + Extract Named Helper Methods |
