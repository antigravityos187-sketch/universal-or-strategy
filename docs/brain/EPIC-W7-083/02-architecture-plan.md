# EPIC-W7-083 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:06:00Z
**Input:** docs/brain/EPIC-W7-083/01-scope-boundary.md

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-083 |
| **Method** | `AuditMaster_CheckExpectedActual` |
| **Source File** | `src/V12_002.REAPER.Audit.cs` |
| **Current CYC** | 13 |
| **Target CYC (parent)** | <= 8 |
| **Projected CYC (parent)** | 4 |
| **Extraction Count** | 3 |
| **Max CYC Projected (any symbol)** | 4 |
| **Boundary Verdict** | PASS (from Phase 1.5) |

---

## MCP Tools Used

- **get_context_bundle** — Retrieved full source of `AuditMaster_CheckExpectedActual` including
  imports and signature. Symbol ID: `src/V12_002.REAPER.Audit.cs::V12_002.AuditMaster_CheckExpectedActual#method`
- **get_call_hierarchy** — Confirmed callers: `AuditMaster_HandleDesyncFlatten` (line 582) and
  `AuditMaster_AccountIfNeeded` (line 684). Zero callees. Caller signatures are unchanged by this epic.
- **get_dependency_graph** — `src/V12_002.REAPER.Audit.cs` has zero external imports and zero importers.
  Blast radius is fully contained to the single file.
- **sequential** thinking (5 thoughts) — Used to design and validate the 3-helper extraction plan,
  verify CYC budgets, and confirm Jane Street alignment.

---

## Source Method (Current State)

```csharp
private bool AuditMaster_CheckExpectedActual(bool shouldLog, int masterActualQty, int masterExpectedQty)
{
    // REAP-01: Suppress critical-desync within ReaperFillGraceTicks of a fresh reservation.
    long stampTicks = Interlocked.Read(ref _lastExpectedPositionSetTicks);
    bool inFillGrace = stampTicks > 0 && (DateTime.UtcNow.Ticks - stampTicks) < ReaperFillGraceTicks;

    bool isCriticalDesync =
        !inFillGrace
        && (
            (masterActualQty != 0 && masterExpectedQty == 0)
            || (Math.Sign(masterActualQty) != Math.Sign(masterExpectedQty) && masterExpectedQty != 0)
        );

    if (inFillGrace && shouldLog)
    {
        Print($"[REAPER] {Account.Name} (Master): Fill grace active -- desync check suppressed.");
    }

    if (isCriticalDesync)
    {
        if (shouldLog)
            Print(
                $"[REAPER] CRITICAL DESYNC on {Account.Name} (Master): Expected={masterExpectedQty}, Actual={masterActualQty}"
            );
        if (AutoFlattenDesync)
        {
            return true;
        }
    }
    else if (shouldLog)
    {
        Print(
            $"[REAPER] Minor Desync on {Account.Name} (Master): Expected={masterExpectedQty}, Actual={masterActualQty}"
        );
    }

    return false;
}
```

**CYC breakdown (current):** base=1, `stampTicks > 0` branch=1, `(DateTime... < ReaperFillGraceTicks)` compound=1,
`masterActualQty != 0 && masterExpectedQty == 0` compound=2, `||` branch=1,
`Math.Sign...` compound=2, `if(inFillGrace && shouldLog)`=2, `if(isCriticalDesync)`=1,
`if(shouldLog)` nested=1, `if(AutoFlattenDesync)`=1, `else if(shouldLog)`=1 → **CYC ~13**

---

## Extraction Plan

### Helper 1: `AuditMaster_IsInFillGrace`

**Purpose:** Encapsulates the lock-free fill-grace time window check.

**Signature:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private bool AuditMaster_IsInFillGrace()
```

**Body:**
```csharp
{
    long stampTicks = Interlocked.Read(ref _lastExpectedPositionSetTicks);
    return stampTicks > 0 && (DateTime.UtcNow.Ticks - stampTicks) < ReaperFillGraceTicks;
}
```

**Projected CYC:** 2
**Jane Street Alignment:**
- **gjengset**: `Interlocked.Read` preserves cache-coherent atomic read — no lock, no false-sharing,
  correct volatile semantics with MemoryBarrier-equivalent ordering.
- **carl_cook**: `[AggressiveInlining]` on hot-path predicate — zero-alloc, single atomic read per call.

---

### Helper 2: `AuditMaster_IsCriticalDesync`

**Purpose:** Pure predicate — evaluates whether qty mismatch constitutes a critical desync.

**Signature:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private bool AuditMaster_IsCriticalDesync(int masterActualQty, int masterExpectedQty)
```

**Body:**
```csharp
{
    return (masterActualQty != 0 && masterExpectedQty == 0)
        || (Math.Sign(masterActualQty) != Math.Sign(masterExpectedQty) && masterExpectedQty != 0);
}
```

**Projected CYC:** 3
**Jane Street Alignment:**
- **trading_billions**: Single responsibility — only computes the critical-desync predicate, no side effects.
  Defense in depth: each check is independently verifiable.
- **carl_cook**: Inlinable hot-path predicate with zero allocations.

---

### Helper 3: `AuditMaster_LogDesyncState`

**Purpose:** Cold-path logging sink for all desync state messages. Extracted out-of-line per
carl_cook's "extract cold logging out-of-line" pattern.

**Signature:**
```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private void AuditMaster_LogDesyncState(
    bool isCriticalDesync,
    bool inFillGrace,
    int masterExpectedQty,
    int masterActualQty)
```

**Body:**
```csharp
{
    if (inFillGrace)
    {
        Print($"[REAPER] {Account.Name} (Master): Fill grace active -- desync check suppressed.");
        return;
    }
    if (isCriticalDesync)
    {
        Print(
            $"[REAPER] CRITICAL DESYNC on {Account.Name} (Master): Expected={masterExpectedQty}, Actual={masterActualQty}"
        );
        return;
    }
    Print(
        $"[REAPER] Minor Desync on {Account.Name} (Master): Expected={masterExpectedQty}, Actual={masterActualQty}"
    );
}
```

**Projected CYC:** 3
**Jane Street Alignment:**
- **carl_cook**: `[NoInlining]` cold-path — logging is never on the hot path. Keeps the JIT from
  pulling logging bytecode into the hot inline cache.
- **trading_billions**: Single responsibility — this method only logs; it makes no decisions.

---

## Parent Method After Extraction

```csharp
private bool AuditMaster_CheckExpectedActual(bool shouldLog, int masterActualQty, int masterExpectedQty)
{
    bool inFillGrace = AuditMaster_IsInFillGrace();
    bool isCriticalDesync = !inFillGrace && AuditMaster_IsCriticalDesync(masterActualQty, masterExpectedQty);
    if (shouldLog)
    {
        AuditMaster_LogDesyncState(isCriticalDesync, inFillGrace, masterExpectedQty, masterActualQty);
    }
    if (isCriticalDesync && AutoFlattenDesync)
    {
        return true;
    }
    return false;
}
```

**Projected CYC:** 4
- base=1, `!inFillGrace &&` compound=1, `if(shouldLog)`=1, `if(isCriticalDesync && AutoFlattenDesync)`=2 → **CYC 4**

---

## CYC Budget Table

| Symbol | Current CYC | Projected CYC | Status |
|---|---|---|---|
| `AuditMaster_CheckExpectedActual` (parent) | 13 | 4 | PASS (<=8) |
| `AuditMaster_IsInFillGrace` | — | 2 | PASS (<=8) |
| `AuditMaster_IsCriticalDesync` | — | 3 | PASS (<=8) |
| `AuditMaster_LogDesyncState` | — | 3 | PASS (<=8) |
| **max_cyc_projected** | | **4** | PASS (<=8) |

---

## Call Graph (Post-Extraction)

```
AuditMaster_HandleDesyncFlatten (line 582)
  └─> AuditMaster_CheckExpectedActual [CYC 4]
        ├─> AuditMaster_IsInFillGrace [CYC 2]      (hot, inlined)
        ├─> AuditMaster_IsCriticalDesync [CYC 3]   (hot, inlined)
        └─> AuditMaster_LogDesyncState [CYC 3]     (cold, no-inline)

AuditMaster_AccountIfNeeded (line 684)
  └─> AuditMaster_HandleDesyncFlatten
        └─> AuditMaster_CheckExpectedActual  [unchanged signature]
```

---

## Dependency Graph Analysis

From **get_dependency_graph**: `src/V12_002.REAPER.Audit.cs` has:
- **imports:** 0 external file edges
- **importers:** 0 external file edges
- **node_count:** 1, **edge_count:** 0

The file is architecturally isolated. All helpers added as private methods within the same
partial class carry zero cross-file blast radius.

---

## Jane Street KB Alignment Summary

| KB Source | Pattern Applied | Where |
|---|---|---|
| **gjengset** | Lock-free atomic read (`Interlocked.Read`), cache-coherent ordering | `AuditMaster_IsInFillGrace` |
| **carl_cook** | Hot path `[AggressiveInlining]` on predicates | `AuditMaster_IsInFillGrace`, `AuditMaster_IsCriticalDesync` |
| **carl_cook** | Cold-path logging `[NoInlining]` extraction | `AuditMaster_LogDesyncState` |
| **carl_cook** | Zero-alloc on hot path (no string formatting in predicates) | All hot helpers |
| **trading_billions** | Single responsibility per helper | All 3 helpers |
| **trading_billions** | Defense in depth (each check independently verifiable) | `AuditMaster_IsCriticalDesync` |
| **trading_billions** | Circuit-breaker pattern (fill grace = rate-limit gate) | `AuditMaster_IsInFillGrace` |

---

## Sequential Thinking Validation

5 sequential thoughts executed:
1. **Thought 1** (probe): Confirmed repo indexed, CYC=13, target <=8.
2. **Thought 2** (analysis): Decomposed method into 3 logical zones — fill grace, desync predicate, logging response.
3. **Thought 3** (design): Specified 3 helper signatures with CYC budgets: IsInFillGrace(2), IsCriticalDesync(3), LogDesyncState(3). Parent reduces to CYC=4.
4. **Thought 4** (validation): Verified all constraints: CYC <=8 per symbol, signature unchanged, V12.23 scope compliance, Jane Street alignment, caller safety.
5. **Thought 5** (finalization): Confirmed extraction_count=3, max_cyc_projected=4. Plan ready.

---

## Ticket Preview (Phase 4 Input)

| Ticket | Work Item | File |
|---|---|---|
| T1 | Extract `AuditMaster_IsInFillGrace` private helper | `src/V12_002.REAPER.Audit.cs` |
| T2 | Extract `AuditMaster_IsCriticalDesync` private helper | `src/V12_002.REAPER.Audit.cs` |
| T3 | Extract `AuditMaster_LogDesyncState` cold-path helper | `src/V12_002.REAPER.Audit.cs` |
| T4 | Rewrite parent `AuditMaster_CheckExpectedActual` to delegate to 3 helpers | `src/V12_002.REAPER.Audit.cs` |

All 4 tickets target the single file. One PR. No cross-file changes.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:06:00Z |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-083 |
| **Method** | AuditMaster_CheckExpectedActual |
| **MCP Tools** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **Sequential Thoughts** | 5 |
| **Extraction Count** | 3 |
| **Max CYC Projected** | 4 |
| **Output** | docs/brain/EPIC-W7-083/02-architecture-plan.md |
