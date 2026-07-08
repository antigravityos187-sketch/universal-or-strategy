# EPIC-W7-059 — Phase 4: Ticket Definitions

**Agent Name:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Inputs:** docs/brain/EPIC-W7-059/02-architecture-plan.md + docs/brain/EPIC-W7-059/03-audit-report.md

---

## Summary

| Field                  | Value                                               |
|------------------------|-----------------------------------------------------|
| **Epic**               | EPIC-W7-059                                         |
| **Method**             | `AdoptMasterWorkingOrders`                          |
| **File**               | `src/V12_002.SIMA.Lifecycle.cs`                     |
| **CYC Baseline**       | 34 (cluster aggregate)                              |
| **CYC Target**         | <= 8 per method                                     |
| **max_cyc_projected**  | 4                                                   |
| **Ticket Count**       | 2                                                   |
| **DNA Verdict**        | PASS (Phase 3)                                      |
| **Risk Level**         | HIGH (26 points over Jane Street threshold 8)       |

---

## Sequential Thinking Evidence (Phase 4)

### Thought 1 — How Many Tickets?

The foreach body of `AdoptMasterWorkingOrders` mixes three concerns:
1. **Filtering** — instrument match guard + `IsOrderStateAdoptable` guard (predicate logic)
2. **Processing** — `ClassifyMasterOrderByPrefix` + null guard + dict write + counter increment (mutation)
3. **Logging** — `Print(string.Format(...))` diagnostics (observability, cold path)

The logging concern belongs inside the mutation ticket because the `Print` call immediately follows the successful `targetDict[key] = ord` assignment — observability is co-located with the side effect that caused it. Separating it would produce a 3-line helper with CYC=1, which adds indirection without reducing cognitive load.

**Decision: 2 extraction tickets — one per meaningful concern.**
- T-059-1: Extract `ShouldAdoptMasterOrder` (filter predicate)
- T-059-2: Extract `ProcessAdoptedMasterOrder` (classify + write + log)

### Thought 2 — Lines Moved, Helper Names, CYC After

**Ticket T-059-1 (ShouldAdoptMasterOrder):**
- Absorbs: `if (ord.Instrument?.FullName != Instrument?.FullName) continue;` + `if (!IsOrderStateAdoptable(...)) continue;`
- Converts early-continues to `return false`; returns `true` at end
- Projected CYC: base(1) + if(instrument)(1) + if(!adoptable)(1) = **3**

**Ticket T-059-2 (ProcessAdoptedMasterOrder):**
- Absorbs: `name` extraction, `ClassifyMasterOrderByPrefix` call, null-guard, `targetDict[key]=ord`, `adoptedCount++`, `Print(...)`
- Projected CYC: base(1) + if(null-guard with `||`)(1) = **2**

**Parent after both extractions:**
```csharp
private void AdoptMasterWorkingOrders(ref int adoptedCount)
{
    try
    {
        foreach (Order ord in Account.Orders.ToArray())
        {
            if (!ShouldAdoptMasterOrder(ord)) continue;
            ProcessAdoptedMasterOrder(ord, ref adoptedCount);
        }
    }
    catch (Exception ex)
    {
        Print(string.Format(
            "[SIMA HYDRATE] WARNING: Could not adopt orders for {0} (Master): {1}",
            Account.Name, ex.Message));
    }
}
```
- Parent projected CYC: base(1) + foreach(1) + if(!Should)(1) + catch(1) = **4**

### Thought 3 — CYC <= 8 Verification

| Method                       | CYC Branches                                   | Projected CYC | <= 8? |
|------------------------------|------------------------------------------------|---------------|-------|
| `AdoptMasterWorkingOrders`   | base+foreach+if(!Should)+catch                 | **4**         | ✓     |
| `ShouldAdoptMasterOrder`     | base+if(instrument!=)+if(!adoptable)           | **3**         | ✓     |
| `ProcessAdoptedMasterOrder`  | base+if(null-guard)                            | **2**         | ✓     |
| `IsOrderStateAdoptable`      | unchanged, CYC=7 (pre-existing)                | **7**         | ✓     |
| `ClassifyMasterOrderByPrefix`| unchanged, CYC=3 (pre-existing)               | **3**         | ✓     |

**All 5 methods satisfy CYC <= 8. max_cyc_projected = 4. ✓**

---

## MCP Evidence

### get_symbol_complexity
- **Tool:** `mcp__jcodemunch-mcp__get_symbol_complexity`
- **Query:** `AdoptMasterWorkingOrders` in `antigravityos187-sketch/universal-or-strategy`
- **Result:** Symbol not found in live index (method resides in `src-vm-backup/` path variant)
- **Fallback:** Architecture plan Phase 2 provides full complexity breakdown (lines 711–758, CYC cluster=34, raw method CYC=6–7). Used as authoritative input per V12 protocol.

### get_extraction_candidates
- **Tool:** `mcp__jcodemunch-mcp__get_extraction_candidates`
- **File:** `src/V12_002.SIMA.Lifecycle.cs`
- **Result:** `candidates=[]` (file variant not in live index)
- **Fallback:** Extraction plan from Phase 2 architecture used as authoritative input.

---

## Ticket Definitions

---

### T-059-1: Extract `ShouldAdoptMasterOrder` — Filter Predicate

| Field              | Value                                                          |
|--------------------|----------------------------------------------------------------|
| **Ticket ID**      | T-059-1                                                        |
| **Epic**           | EPIC-W7-059                                                    |
| **Type**           | extraction                                                     |
| **Priority**       | P1                                                             |
| **File**           | `src/V12_002.SIMA.Lifecycle.cs`                               |
| **Parent Method**  | `AdoptMasterWorkingOrders` (lines 711–758)                    |
| **Concern**        | Filter predicate — instrument match + state eligibility guard  |
| **Estimated CYC**  | 3                                                              |
| **Visibility**     | `private`                                                      |

#### New Helper Signature

```csharp
/// <summary>
/// Predicate: returns true if the order should be adopted into master tracking dictionaries.
/// Validates instrument match and order state eligibility.
/// </summary>
private bool ShouldAdoptMasterOrder(Order ord)
```

#### Implementation

```csharp
private bool ShouldAdoptMasterOrder(Order ord)
{
    if (ord.Instrument?.FullName != Instrument?.FullName)
        return false;
    if (!IsOrderStateAdoptable(ord.OrderState, includeMasterUnknown: true))
        return false;
    return true;
}
```

#### Lines Moved from Parent

Remove from `AdoptMasterWorkingOrders` foreach body:
```csharp
if (ord.Instrument?.FullName != Instrument?.FullName)
    continue;
if (!IsOrderStateAdoptable(ord.OrderState, includeMasterUnknown: true))
    continue;
```

Replace with:
```csharp
if (!ShouldAdoptMasterOrder(ord)) continue;
```

#### Acceptance Criteria

- [ ] `ShouldAdoptMasterOrder` added as `private bool` method in same class
- [ ] Returns `false` when instrument full name does not match
- [ ] Returns `false` when `IsOrderStateAdoptable` returns `false` (with `includeMasterUnknown: true`)
- [ ] Returns `true` when both checks pass
- [ ] Parent foreach body calls `if (!ShouldAdoptMasterOrder(ord)) continue;`
- [ ] `IsOrderStateAdoptable` signature unchanged
- [ ] Build passes (`dotnet build`)
- [ ] CYC of `ShouldAdoptMasterOrder` = 3 (verified via complexity audit)
- [ ] xUnit test: `[Fact]` asserting `Assert.False` / `Assert.True` for both guard branches

#### xUnit Test Stubs

```csharp
[Fact]
public void ShouldAdoptMasterOrder_ReturnsFalse_WhenInstrumentMismatch()
{
    // Arrange: ord.Instrument.FullName != Instrument.FullName
    // Act + Assert: Assert.False(sut.ShouldAdoptMasterOrder(ord));
}

[Fact]
public void ShouldAdoptMasterOrder_ReturnsFalse_WhenStateNotAdoptable()
{
    // Arrange: ord with non-adoptable OrderState
    // Act + Assert: Assert.False(sut.ShouldAdoptMasterOrder(ord));
}

[Fact]
public void ShouldAdoptMasterOrder_ReturnsTrue_WhenBothChecksPass()
{
    // Arrange: matching instrument + adoptable state
    // Act + Assert: Assert.True(sut.ShouldAdoptMasterOrder(ord));
}
```

---

### T-059-2: Extract `ProcessAdoptedMasterOrder` — Classify, Write, and Log

| Field              | Value                                                                   |
|--------------------|-------------------------------------------------------------------------|
| **Ticket ID**      | T-059-2                                                                 |
| **Epic**           | EPIC-W7-059                                                             |
| **Type**           | extraction                                                              |
| **Priority**       | P1                                                                      |
| **File**           | `src/V12_002.SIMA.Lifecycle.cs`                                        |
| **Parent Method**  | `AdoptMasterWorkingOrders` (lines 711–758)                             |
| **Concern**        | Classify + null guard + dict write + counter increment + diagnostic log |
| **Estimated CYC**  | 2                                                                       |
| **Visibility**     | `private`                                                               |

#### New Helper Signature

```csharp
/// <summary>
/// Classify, write, and log a single master order into its target tracking dictionary.
/// Increments adoptedCount on successful adoption.
/// </summary>
private void ProcessAdoptedMasterOrder(Order ord, ref int adoptedCount)
```

#### Implementation

```csharp
private void ProcessAdoptedMasterOrder(Order ord, ref int adoptedCount)
{
    string name = ord.Name ?? string.Empty;
    string key, dictName;
    ConcurrentDictionary<string, Order> targetDict = ClassifyMasterOrderByPrefix(
        name, out key, out dictName
    );
    if (targetDict == null || key == null)
        return;
    targetDict[key] = ord;
    adoptedCount++;
    Print(string.Format(
        "[SIMA HYDRATE] {0} (Master): Adopted {1} -> {2}[{3}]",
        Account.Name, name, dictName, key
    ));
}
```

#### Lines Moved from Parent

Remove from `AdoptMasterWorkingOrders` foreach body:
```csharp
string name = ord.Name ?? string.Empty;
string key, dictName;
ConcurrentDictionary<string, Order> targetDict = ClassifyMasterOrderByPrefix(
    name, out key, out dictName
);
if (targetDict == null || key == null)
    continue;
targetDict[key] = ord;
adoptedCount++;
Print(string.Format(
    "[SIMA HYDRATE] {0} (Master): Adopted {1} -> {2}[{3}]",
    Account.Name, name, dictName, key
));
```

Replace with:
```csharp
ProcessAdoptedMasterOrder(ord, ref adoptedCount);
```

#### Acceptance Criteria

- [ ] `ProcessAdoptedMasterOrder` added as `private void` method in same class
- [ ] Uses `ref int adoptedCount` parameter (no boxing; zero-alloc hot path per Jane Street)
- [ ] Returns early (no increment, no Print) when `ClassifyMasterOrderByPrefix` returns null dict or null key
- [ ] Increments `adoptedCount` only on successful dict write
- [ ] `Print` call uses ASCII-only format string `[SIMA HYDRATE]`
- [ ] `ClassifyMasterOrderByPrefix` signature unchanged
- [ ] Build passes (`dotnet build`)
- [ ] CYC of `ProcessAdoptedMasterOrder` = 2 (verified via complexity audit)
- [ ] xUnit test: `[Fact]` verifying `adoptedCount` incremented on success and unchanged on null-guard early return

#### xUnit Test Stubs

```csharp
[Fact]
public void ProcessAdoptedMasterOrder_IncrementsCount_WhenClassificationSucceeds()
{
    // Arrange: ClassifyMasterOrderByPrefix returns valid dict + key
    int count = 0;
    // Act: sut.ProcessAdoptedMasterOrder(ord, ref count);
    // Assert: Assert.Equal(1, count);
}

[Fact]
public void ProcessAdoptedMasterOrder_DoesNotIncrement_WhenTargetDictNull()
{
    // Arrange: ClassifyMasterOrderByPrefix returns null dict
    int count = 0;
    // Act: sut.ProcessAdoptedMasterOrder(ord, ref count);
    // Assert: Assert.Equal(0, count);
}
```

---

## Execution Order

| Step | Ticket    | Action                                       | Dependency  |
|------|-----------|----------------------------------------------|-------------|
| 1    | T-059-1   | Extract `ShouldAdoptMasterOrder`             | None        |
| 2    | T-059-2   | Extract `ProcessAdoptedMasterOrder`          | After T-059-1 (parent method in clean state) |

Both tickets target `src/V12_002.SIMA.Lifecycle.cs` only. No cross-file changes. No caller modifications.

---

## CYC Reduction Summary

| Method                       | CYC Before | CYC After | Delta  |
|------------------------------|-----------|-----------|--------|
| `AdoptMasterWorkingOrders`   | ~6–7      | 4         | -2–3   |
| `ShouldAdoptMasterOrder`     | (new)     | 3         | n/a    |
| `ProcessAdoptedMasterOrder`  | (new)     | 2         | n/a    |
| Cluster aggregate            | 34        | ~14       | -20    |

**All individual methods <= 8 post-extraction. Jane Street threshold satisfied. ✓**

---

## Agent Tracking

| Field                         | Value                                                                            |
|-------------------------------|----------------------------------------------------------------------------------|
| **Agent Name**                | v12-phase4-tickets                                                               |
| **Wave**                      | 7                                                                                |
| **Phase**                     | 4                                                                                |
| **Bobcoins Used**             | 0.7                                                                              |
| **Execution Time**            | batch                                                                            |
| **MCP Tools Used**            | resolve_repo, sequentialthinking (×3), get_symbol_complexity, get_extraction_candidates |
| **Sequential Thinking Thoughts** | 3 (ticket count + line mapping + CYC verification)                           |
| **Ticket Count**              | 2                                                                                |
| **max_cyc_projected**         | 4                                                                                |
| **dna_verdict (Phase 3)**     | PASS                                                                             |
