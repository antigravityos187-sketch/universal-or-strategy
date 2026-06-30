# EPIC-W7-130 — Phase 4: Ticket Definitions

## Agent Tracking

| Field              | Value                                                                                   |
|--------------------|-----------------------------------------------------------------------------------------|
| **Agent Name**     | v12-phase4-tickets                                                                      |
| **Wave**           | 7                                                                                       |
| **Phase**          | 4 — Ticket Generation                                                                   |
| **Epic**           | EPIC-W7-130                                                                             |
| **Generated**      | 2026-06-29T01:20:00Z                                                                    |
| **Lane**           | P4-L8                                                                                   |
| **MCP Tools Used** | jcodemunch resolve_repo, get_symbol_complexity; sequential-thinking sequentialthinking (3 thoughts) |

---

## Target Method

| Field             | Value                                              |
|-------------------|----------------------------------------------------|
| Method Name       | `SymmetryGuardCascadeFollowerCleanup`              |
| File              | `src/V12_002.Symmetry.Replace.cs`                  |
| Lines             | 198 – 243                                          |
| CYC (Phase 0)     | 0 (data artifact — parse miss on partial class)    |
| CYC (Phase 2 manual) | 7 (strict simplified) / 9 (true strict with `||`) |
| **CYC (MCP tool get_symbol_complexity)** | **11 (HIGH — index now resolves partial class)** |
| Threshold         | 8 (Jane Street standard)                           |
| Task Instruction  | CYC=7, compliant, Phase 5 SKIPPED (verification only) |

---

## ⚠️ CYC Discrepancy Notice

The MCP tool `get_symbol_complexity` now reports **CYC=11** (assessment: `high`) for this method.
This is **ABOVE the threshold of 8** and differs from the Phase 2 manual analysis (CYC=7).

| Source               | CYC  | Assessment    |
|----------------------|------|---------------|
| Phase 0 tooling      | 0    | Parse miss    |
| Phase 2 manual count | 7    | Compliant     |
| Phase 2 strict `||`  | 9    | Borderline    |
| **MCP get_symbol_complexity (current)** | **11** | **HIGH — above threshold** |

**Root cause**: The jCodemunch index was re-indexed and can now resolve the partial class.
The CYC=11 from the MCP tool is more likely accurate than the CYC=7 simplified manual count.

**Director Decision Point**: The orchestrator task instructs Phase 5 SKIPPED (verification only).
However, the MCP tool authoritative reading is CYC=11. The Phase 5 executor should:
1. Run `python scripts/complexity_audit.py` to get the local tool reading
2. If local tool confirms CYC > 8, escalate to director for Phase 5 extraction authorization
3. Phase 2 extraction plan (1 helper: `CancelFollowerEntryIfPending`) is ready to execute

---

## Ticket Summary

| Ticket         | Type             | Action                                          | Phase 5? |
|----------------|------------------|-------------------------------------------------|----------|
| TKT-130-01     | Verification     | Confirm CYC reading and compliance status       | NO (docs only) |

**Ticket count: 1** (verification only per task instructions)

---

## TKT-130-01 — Compliance Verification and CYC Discrepancy Resolution

### Metadata

| Field           | Value                                              |
|-----------------|----------------------------------------------------|
| **Ticket ID**   | TKT-130-01                                         |
| **Type**        | Verification (no extraction execution)             |
| **Priority**    | P2 — Requires director decision on CYC discrepancy |
| **Assignee**    | Phase 5 executor or Director review                |
| **Epic**        | EPIC-W7-130                                        |
| **Method**      | `SymmetryGuardCascadeFollowerCleanup`              |
| **File**        | `src/V12_002.Symmetry.Replace.cs`                  |

### Context

EPIC-W7-130 targets `SymmetryGuardCascadeFollowerCleanup` in `src/V12_002.Symmetry.Replace.cs`
(lines 198–243). This is a **DIFFERENT** instance from EPIC-W7-121, which covers a method of
the same name in a different partial class file with CYC=10.

Phase 2 manual analysis produced CYC=7 (strict simplified), concluding the method is compliant
below the Jane Street threshold of 8. Phase 3 DNA audit confirmed PASS with no violations.

However, Phase 4 MCP probe via `get_symbol_complexity` now returns **CYC=11 (high)** for this
method after the index was updated to resolve partial class boundaries. This triggers a
discrepancy flag and requires verification before Phase 5 disposition is finalized.

### Acceptance Criteria

- [ ] Run `python scripts/complexity_audit.py` on `src/V12_002.Symmetry.Replace.cs`
- [ ] Confirm local CYC reading for `SymmetryGuardCascadeFollowerCleanup`
- [ ] If local CYC <= 8: Mark epic as compliant, no extraction required, update manifest
- [ ] If local CYC > 8: Escalate to director; Phase 2 extraction plan (1 helper) is pre-approved
- [ ] Document final CYC reading in `ticket-1-verification.md`
- [ ] Verify the method is the correct instance (lines 198–243, NOT the EPIC-W7-121 instance)
- [ ] Confirm no lock() blocks in `src/V12_002.Symmetry.Replace.cs` (search_text validated in Phase 3)
- [ ] Build passes: `dotnet build` — zero errors

### MCP Evidence

```json
{
  "tool": "get_symbol_complexity",
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "symbol_id": "src/V12_002.Symmetry.Replace.cs::V12_002.SymmetryGuardCascadeFollowerCleanup#method",
  "name": "SymmetryGuardCascadeFollowerCleanup",
  "kind": "method",
  "file": "src/V12_002.Symmetry.Replace.cs",
  "line": 198,
  "cyclomatic": 11,
  "max_nesting": 6,
  "param_count": 1,
  "lines": 46,
  "assessment": "high"
}
```

### Phase 2 Extraction Plan (Pre-Approved — Execute if CYC > 8)

If local tool confirms CYC > 8, the Phase 5 executor should apply the Phase 2 extraction plan
without requiring a new architecture cycle. The plan is already fully specified:

| # | Helper Name                    | Responsibility                                            | Signature                                                  | Max CYC Projected |
|---|--------------------------------|-----------------------------------------------------------|------------------------------------------------------------|-------------------|
| 1 | `CancelFollowerEntryIfPending` | Resolve pos+order for one follower, guard nulls, cancel pending entry | `private void CancelFollowerEntryIfPending(string followerName)` | **7** |

**Extraction steps** (from Phase 2 `02-architecture-plan.md`):
1. Read `src/V12_002.Symmetry.Replace.cs` lines 198–243
2. Extract the `foreach` body (lines ~218–241) into `private void CancelFollowerEntryIfPending(string followerName)`
3. Replace foreach body in parent with: `CancelFollowerEntryIfPending(followerName);`
4. Optionally simplify: `pos.ExecutingAccount != null ? pos.ExecutingAccount.Name : "Master"` → `pos.ExecutingAccount?.Name ?? "Master"`
5. Verify CYC of parent <= 8 and CYC of helper <= 8 via `python scripts/complexity_audit.py`
6. Run `dotnet build` — zero errors
7. Run `dotnet csharpier check src/` — zero formatting issues
8. Run `powershell -File .\deploy-sync.ps1` — sync hard links

**Projected CYC after extraction:**
- Parent `SymmetryGuardCascadeFollowerCleanup`: **4** (base + 3 guards/loop)
- Helper `CancelFollowerEntryIfPending`: **7** (base + 6 branches including `||` conditions)
- Max CYC projected: **7** — compliant with threshold 8

### xUnit Test Requirements (if extraction performed)

Per V12 protocol (xUnit only — NO NUnit, NO MSTest):

```csharp
[Fact]
public void CancelFollowerEntryIfPending_NullOrder_DoesNotThrow() { ... }

[Fact]
public void CancelFollowerEntryIfPending_WorkingOrderState_CallsCancelOrderSafe() { ... }

[Fact]
public void SymmetryGuardCascadeFollowerCleanup_NoDispatchId_ReturnsEarly() { ... }
```

### DNA Compliance (from Phase 3)

| Check                        | Status |
|------------------------------|--------|
| Zero `lock()` blocks         | PASS   |
| ASCII-only string literals   | PASS   |
| No scope creep               | PASS   |
| xUnit tests planned          | PASS   |
| Max CYC projected <= 8       | PASS (projected 7) |

---

## Sequential Thinking Validation Summary

| Thought | Decision                                                                                  |
|---------|-------------------------------------------------------------------------------------------|
| 1       | Task says CYC=7 compliant, Phase 5 SKIPPED — generate verification-only ticket           |
| 2       | MCP tool reports CYC=11 (high) — ABOVE threshold — flags discrepancy; document both      |
| 3       | Final: 1 verification ticket with CYC discrepancy notice and pre-approved extraction plan |

---

## Phase 5 Disposition

Per task instructions: **Phase 5 SKIPPED** (method reported as compliant at CYC=7).

However, MCP `get_symbol_complexity` reports CYC=11. The Phase 5 executor must:
1. Run local `complexity_audit.py` as the tiebreaker
2. If CYC <= 8 locally: mark complete, no extraction
3. If CYC > 8 locally: apply Phase 2 extraction plan (1 helper, pre-approved) and re-verify

This ticket is the sole deliverable for EPIC-W7-130 Phase 4.
