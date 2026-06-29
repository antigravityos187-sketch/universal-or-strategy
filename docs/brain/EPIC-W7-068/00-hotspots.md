# EPIC-W7-068 | Phase 0 — Hotspot Analysis

## Target Symbol

| Field | Value |
|---|---|
| Method | `TryParseTargetMode` |
| Source file | `src/V12_002.UI.IPC.cs` |
| Lines | 97–128 |
| Reported CYC | **0** (tool artefact — see below) |

---

## CYC Discrepancy Finding

The static-analysis tool reported **CYC = 0** for `TryParseTargetMode`.  
This is a **phantom reading caused by partial-class fragmentation**: the analyser
resolved the `partial class V12_002` only over a single translation unit and
produced a zero-branch result instead of failing loudly.

Manual McCabe count for the actual method body:

| Branch source | Count |
|---|---|
| Base path | +1 |
| `if (string.IsNullOrWhiteSpace(raw))` | +1 |
| `case "ATR"/"A"` | +1 |
| `case "TICKS"/"TICK"/"T"` | +1 |
| `case "POINTS"/"POINT"/"PTS"/"P"` | +1 |
| `case "RUNNER"/"R"` | +1 |
| `default:` | +1 |
| **Total (real CYC)** | **7** |

CYC 7 is within acceptable range (threshold typically 10), so this method does
**not** require immediate structural refactoring. The priority is fixing the
measurement gap so future runs report accurately.

---

## Blast Radius

```
TryParseTargetMode  (src/V12_002.UI.IPC.cs:97)
  └─ called 5×  by  TryApplyConfigTarget_Type  (src/V12_002.UI.IPC.Commands.Config.cs:299–341)
        T1TYPE → T1Type
        T2TYPE → T2Type
        T3TYPE → T3Type
        T4TYPE → T4Type
        T5TYPE → T5Type
  └─ depends on  TargetMode enum  (src/V12_002.Properties.cs:41–47)
        members: ATR | Ticks | Points | Runner
```

Blast radius is **narrow**: one caller, five assignment sites.  
No other files reference this helper.

---

## Silent-Failure Risk

`TryApplyConfigTarget_Type` pattern:

```csharp
if (TryParseTargetMode(val, out var parsed))
{
    T1Type = parsed;
}
return true;          // ← always returns true even on parse failure
```

A parse failure (unknown or misspelled mode string arriving over IPC) is
**silently swallowed** — no log line, no NACK, strategy property unchanged.
The outer caller returns `true` regardless, so the operator gets no feedback
that their `CONFIG|ES|T1TYPE=WRONGVALUE` command was ignored.

---

## Hotspot Summary

| # | Hotspot | Severity | Action |
|---|---|---|---|
| H1 | CYC reported as 0 — partial-class analyser gap | Medium | Fix analysis config to resolve `partial` across all `.cs` shards |
| H2 | `default` arm in switch is silent (no log) | Low | Add `Print` warning in `default:` for observability |
| H3 | `TryApplyConfigTarget_Type` returns `true` on parse failure | Low | Log parse failure at call site or propagate `false` to caller |
| H4 | `TargetMode` enum / switch parity — no guard | Low | Add `Debug.Assert` or unit-test enum coverage |

---

## Recommended Phase 1 Scope

- **Phase 1a**: Fix analysis tooling to honour C# `partial` class boundaries.
- **Phase 1b**: Add a `Print` diagnostic in the `default:` arm of
  `TryParseTargetMode` (one-line change, zero CYC impact).
- **Phase 1c**: Log or return meaningful error from `TryApplyConfigTarget_Type`
  when `TryParseTargetMode` returns `false`.

---

*Generated: Wave 7 | Phase 0 | EPIC-W7-068*
