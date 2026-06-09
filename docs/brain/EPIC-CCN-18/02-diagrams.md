# EPIC-CCN-18: Architecture Diagrams

## Diagram 1: Call Graph (Before Refactoring)

```mermaid
graph TD
    A[HandleFlatPositionUpdate CYC=37] --> B[Account Validation]
    A --> C[INLINE: Scan entryOrders CYC+8]
    C --> C1[foreach entryOrders.ToArray]
    C1 --> C2[5-way AND condition]
    C2 --> C3[IsOrderTerminal check]
    C2 --> C4[activePositions.TryGetValue]
    C2 --> C5[ExecutingAccount match]
    
    A --> D[INLINE: Scan activePositions CYC+6]
    D --> D1[foreach activePositions.ToArray]
    D1 --> D2[3-way AND condition]
    D2 --> D3[ExecutingAccount null check]
    D2 --> D4[Account name match]
    D2 --> D5[EntryFilled check]
    
    A --> E[Skip Logic / Clear expectedPositions CYC+3]
    E --> E1[Ternary operator]
    E1 --> E2[SetExpectedPositionLocked]
    
    A --> F[Early Return Check]
    F --> G[ReconcileOrphanedOrders]
    
    A --> H[INLINE: Position Cleanup Loop CYC+12]
    H --> H1[foreach activePositions.ToArray]
    H1 --> H2[ContainsKey guard]
    H1 --> H3[EntryFilled AND RemainingContracts]
    H3 --> H4[INLINE: Cancel stop order CYC+3]
    H4 --> H5[stopOrders.TryGetValue]
    H5 --> H6[2-way OR state check]
    H6 --> H7[CancelOrderSafe]
    
    H3 --> H8[INLINE: Cancel 5 targets CYC+6]
    H8 --> H9[for loop 1-5]
    H9 --> H10[GetTargetOrdersDictionary]
    H10 --> H11[TryGetValue]
    H11 --> H12[2-way OR state check]
    H12 --> H13[CancelOrderSafe]
    
    H3 --> H14[positionsToCleanup.Add]
    
    A --> I[Execute Cleanup]
    I --> I1[foreach positionsToCleanup]
    I1 --> I2[CleanupPosition]
    
    style A fill:#ff6b6b
    style C fill:#ffa07a
    style D fill:#ffa07a
    style H fill:#ffa07a
    style H4 fill:#ffcccb
    style H8 fill:#ffcccb
```

**Legend**:
- 🔴 Red: High complexity (CYC >30)
- 🟠 Orange: Medium complexity (CYC 10-30)
- 🟡 Yellow: Low complexity (CYC <10)

**Complexity Hotspots**:
- Main method: CYC 37 (2.5x Jane Street threshold)
- Entry order scan: +8 CYC (5-way AND condition)
- Active position scan: +6 CYC (3-way AND condition)
- Position cleanup loop: +12 CYC (nested stop + 5 targets)

---

## Diagram 2: Call Graph (After Refactoring)

```mermaid
graph TD
    A[HandleFlatPositionUpdate CYC=7] --> B[Account Validation]
    A --> C[HasPendingEntryForAccount CYC=6]
    C --> C1[foreach entryOrders.ToArray]
    C1 --> C2[5-way AND condition]
    C2 --> C3[return true/false]
    
    A --> D[HasActivePositionForAccount CYC=5]
    D --> D1[foreach activePositions.ToArray]
    D1 --> D2[3-way AND condition]
    D2 --> D3[return true/false]
    
    A --> E[Skip Logic / Clear expectedPositions CYC+3]
    E --> E1[Ternary operator]
    E1 --> E2[SetExpectedPositionLocked]
    
    A --> F[Early Return Check]
    F --> G[ReconcileOrphanedOrders]
    
    A --> H[CollectPositionsForCleanup CYC=7]
    H --> H1[foreach activePositions.ToArray]
    H1 --> H2[ContainsKey guard]
    H1 --> H3[EntryFilled AND RemainingContracts]
    H3 --> H4[CancelOrphanedOrdersForPosition CYC=10]
    H4 --> H5[Cancel stop order]
    H5 --> H6[stopOrders.TryGetValue]
    H6 --> H7[2-way OR state check]
    H7 --> H8[CancelOrderSafe]
    
    H4 --> H9[Cancel 5 targets]
    H9 --> H10[for loop 1-5]
    H10 --> H11[GetTargetOrdersDictionary]
    H11 --> H12[TryGetValue]
    H12 --> H13[2-way OR state check]
    H13 --> H14[CancelOrderSafe]
    
    H3 --> H15[return List of string]
    
    A --> I[Execute Cleanup]
    I --> I1[foreach positionsToCleanup]
    I1 --> I2[CleanupPosition]
    
    style A fill:#90ee90
    style C fill:#98fb98
    style D fill:#98fb98
    style H fill:#98fb98
    style H4 fill:#98fb98
```

**Legend**:
- 🟢 Green: Jane Street aligned (CYC ≤15)
- 🟡 Yellow: Acceptable (CYC ≤12)

**Complexity Improvements**:
- Main method: CYC 37 → 7 (-30, 81% reduction)
- Helper 1: CYC 6 (pure function)
- Helper 2: CYC 5 (pure function)
- Helper 3: CYC 10 (actor-serialized)
- Helper 4: CYC 7 (orchestration)

---

## Diagram 3: Data Flow (Before vs After)

```mermaid
graph LR
    subgraph Before [Before Refactoring]
        A1[acctName] --> B1[Inline Logic CYC=37]
        B1 --> C1[entryOrders scan]
        B1 --> D1[activePositions scan]
        B1 --> E1[stop cancellation]
        B1 --> F1[5x target cancellation]
        B1 --> G1[cleanup execution]
    end
    
    subgraph After [After Refactoring]
        A2[acctName] --> B2[Main Method CYC=7]
        B2 --> C2[HasPendingEntryForAccount]
        C2 --> C3[bool]
        B2 --> D2[HasActivePositionForAccount]
        D2 --> D3[bool]
        B2 --> E2[CollectPositionsForCleanup]
        E2 --> F2[CancelOrphanedOrdersForPosition]
        F2 --> F3[void]
        E2 --> G2[List of string]
        B2 --> H2[cleanup execution]
    end
    
    style Before fill:#ffe6e6
    style After fill:#e6ffe6
```

**Data Flow Improvements**:
- Clear input/output contracts (bool, List<string>, void)
- Single responsibility per helper
- Orchestration pattern (Helper 4 calls Helper 3)
- Pure functions where possible (Helpers 1, 2, 4)

---

## Diagram 4: Ticket Dependencies

```mermaid
graph TD
    T0[Phase 0: Hotspot Analysis] --> T1[Phase 1: Scope Definition]
    T1 --> T1.5[Phase 1.5: Scope Boundary Validation]
    T1.5 --> T2[Phase 2: Architecture Planning]
    T2 --> T3[Phase 3: DNA & PR Audit]
    T3 --> T4[Phase 4: Ticket Generation]
    
    T4 --> T5.1[Ticket 1: Extract Boolean Helpers]
    T5.1 --> T5.1.1[HasPendingEntryForAccount]
    T5.1 --> T5.1.2[HasActivePositionForAccount]
    T5.1.1 --> T5.1.V[Verify: CYC 37→23]
    T5.1.2 --> T5.1.V
    
    T5.1.V --> T5.2[Ticket 2: Extract Cancellation Helper]
    T5.2 --> T5.2.1[CancelOrphanedOrdersForPosition]
    T5.2.1 --> T5.2.V[Verify: CYC 23→13]
    
    T5.2.V --> T5.3[Ticket 3: Extract Cleanup Helper]
    T5.3 --> T5.3.1[CollectPositionsForCleanup]
    T5.3.1 --> T5.3.V[Verify: CYC 13→7]
    
    T5.3.V --> T5.4{CYC ≤10?}
    T5.4 -->|YES| T6[Phase 6: Final Review]
    T5.4 -->|NO| T5.5[Ticket 4: Final Refactoring]
    T5.5 --> T5.5.V[Verify: CYC ≤10]
    T5.5.V --> T6
    
    T6 --> DONE[Epic Complete]
    
    style T0 fill:#e6f3ff
    style T1 fill:#e6f3ff
    style T1.5 fill:#e6f3ff
    style T2 fill:#e6f3ff
    style T3 fill:#fff3e6
    style T4 fill:#fff3e6
    style T5.1 fill:#e6ffe6
    style T5.2 fill:#e6ffe6
    style T5.3 fill:#e6ffe6
    style T5.4 fill:#ffffcc
    style T5.5 fill:#ffe6e6
    style T6 fill:#e6f3ff
    style DONE fill:#90ee90
```

**Legend**:
- 🔵 Blue: Planning phases (0, 1, 1.5, 2, 6)
- 🟠 Orange: Audit phases (3, 4)
- 🟢 Green: Execution phases (5.1, 5.2, 5.3)
- 🟡 Yellow: Decision point (5.4)
- 🔴 Red: Conditional phase (5.5)

**Critical Path**:
1. Phase 0-2: Planning (COMPLETE)
2. Phase 3: DNA & PR Audit (NEXT)
3. Phase 4: Ticket Generation
4. Phase 5.1-5.3: Sequential execution (MANDATORY)
5. Phase 5.4: Decision gate (CYC ≤10?)
6. Phase 5.5: Conditional (only if CYC >10)
7. Phase 6: Final review

---

## Diagram 5: Complexity Reduction Timeline

```mermaid
gantt
    title EPIC-CCN-18 Complexity Reduction Timeline
    dateFormat YYYY-MM-DD
    section Planning
    Phase 0 Hotspot Analysis       :done, p0, 2026-06-09, 1h
    Phase 1 Scope Definition        :done, p1, after p0, 1h
    Phase 1.5 Scope Boundary        :done, p1.5, after p1, 1h
    Phase 2 Architecture Planning   :active, p2, after p1.5, 2h
    section Audit
    Phase 3 DNA & PR Audit          :p3, after p2, 1h
    Phase 4 Ticket Generation       :p4, after p3, 1h
    section Execution
    Ticket 1 Boolean Helpers        :t1, after p4, 3h
    Ticket 2 Cancellation Helper    :t2, after t1, 4h
    Ticket 3 Cleanup Helper         :t3, after t2, 3h
    Ticket 4 Final Refactoring      :crit, t4, after t3, 2h
    section Review
    Phase 6 Final Review            :p6, after t3, 1h
```

**Timeline Summary**:
- **Planning**: 5 hours (Phases 0-2)
- **Audit**: 2 hours (Phases 3-4)
- **Execution**: 10-12 hours (Tickets 1-4)
- **Review**: 1 hour (Phase 6)
- **Total**: 18-20 hours (2.5 days)

---

## Diagram 6: Complexity Metrics (Before vs After)

```mermaid
graph LR
    subgraph Before [Before Refactoring]
        A1[Main Method] --> B1[CYC: 37]
        A1 --> C1[Nesting: 6]
        A1 --> D1[LOC: 108]
        A1 --> E1[Helpers: 0]
    end
    
    subgraph After [After Refactoring]
        A2[Main Method] --> B2[CYC: 7]
        A2 --> C2[Nesting: 3]
        A2 --> D2[LOC: 40]
        A2 --> E2[Helpers: 4]
        
        F2[Helper 1] --> G2[CYC: 6]
        H2[Helper 2] --> I2[CYC: 5]
        J2[Helper 3] --> K2[CYC: 10]
        L2[Helper 4] --> M2[CYC: 7]
    end
    
    style B1 fill:#ff6b6b
    style C1 fill:#ff6b6b
    style D1 fill:#ff6b6b
    style B2 fill:#90ee90
    style C2 fill:#90ee90
    style D2 fill:#90ee90
    style G2 fill:#90ee90
    style I2 fill:#90ee90
    style K2 fill:#90ee90
    style M2 fill:#90ee90
```

**Metrics Comparison**:

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| **Main CYC** | 37 | 7 | -30 (81%) |
| **Max Nesting** | 6 | 3 | -3 (50%) |
| **Main LOC** | 108 | 40 | -68 (63%) |
| **Helper Count** | 0 | 4 | +4 |
| **Max Helper CYC** | N/A | 10 | Jane Street aligned |
| **Total CYC** | 37 | 28 | -9 (24%) |

**Jane Street Alignment**:
- ✅ Main method CYC ≤10 (target: 7)
- ✅ All helpers CYC ≤12 (max: 10)
- ✅ Nesting depth ≤4 (achieved: 3)
- ✅ Functions fit in working memory (<50 LOC)

---

## Diagram 7: Risk Mitigation Strategy

```mermaid
graph TD
    A[Risk Identification] --> B[Risk 1: Triple Nested Loops]
    A --> C[Risk 2: Multi-Condition Guards]
    A --> D[Risk 3: Position State Management]
    
    B --> B1[Mitigation: Extract Outer Loop First]
    B1 --> B2[Ticket 3: CollectPositionsForCleanup]
    B2 --> B3[Ticket 2: CancelOrphanedOrdersForPosition]
    B3 --> B4[Validation: TDD Tests + F5]
    
    C --> C1[Mitigation: Preserve Original Logic]
    C1 --> C2[No Boolean Simplification]
    C2 --> C3[Test All Condition Combinations]
    C3 --> C4[Validation: Complexity Audit]
    
    D --> D1[Mitigation: ZERO TOLERANCE for Drift]
    D1 --> D2[TDD Tests BEFORE Extraction]
    D2 --> D3[F5 Verification After Each Ticket]
    D3 --> D4[Manual Testing Protocol]
    D4 --> D5[Rollback Protocol Ready]
    
    style A fill:#fff3e6
    style B fill:#ffe6e6
    style C fill:#ffe6e6
    style D fill:#ff6b6b
    style B4 fill:#90ee90
    style C4 fill:#90ee90
    style D5 fill:#90ee90
```

**Risk Severity**:
- 🔴 HIGH: Position state management (Risk 3)
- 🟠 MEDIUM: Triple nested loops (Risk 1)
- 🟠 MEDIUM: Multi-condition guards (Risk 2)

**Mitigation Success Criteria**:
- ✅ All risks have concrete mitigation strategies
- ✅ Validation gates defined for each risk
- ✅ Rollback protocol documented
- ✅ TDD tests capture current behavior

---

## Diagram 8: Test Coverage Strategy

```mermaid
graph TD
    A[Test Strategy] --> B[Helper 1: 6 Tests]
    A --> C[Helper 2: 5 Tests]
    A --> D[Helper 3: 8 Tests]
    A --> E[Helper 4: 6 Tests]
    
    B --> B1[Empty entryOrders]
    B --> B2[No account match]
    B --> B3[Pending entry exists]
    B --> B4[Terminal order state]
    B --> B5[Position not in activePositions]
    B --> B6[ExecutingAccount null]
    
    C --> C1[Empty activePositions]
    C --> C2[No account match]
    C --> C3[Active unfilled position]
    C --> C4[Position already filled]
    C --> C5[ExecutingAccount null]
    
    D --> D1[Cancel stop Working]
    D --> D2[Cancel stop Accepted]
    D --> D3[Skip stop Filled]
    D --> D4[Missing stop order]
    D --> D5[Cancel 5 targets Working]
    D --> D6[Cancel 5 targets Accepted]
    D --> D7[Skip targets Filled]
    D --> D8[Missing target orders]
    
    E --> E1[Empty activePositions]
    E --> E2[No cleanup criteria]
    E --> E3[EntryFilled + RemainingContracts]
    E --> E4[Skip if not EntryFilled]
    E --> E5[Skip if RemainingContracts = 0]
    E --> E6[Concurrent modification]
    
    style A fill:#e6f3ff
    style B fill:#e6ffe6
    style C fill:#e6ffe6
    style D fill:#e6ffe6
    style E fill:#e6ffe6
```

**Test Coverage Summary**:
- **Total Tests**: 25 tests (6 + 5 + 8 + 6)
- **Coverage**: 100% (all code paths)
- **Test Types**: Unit tests (pure functions + actor-serialized)
- **TDD Protocol**: Tests written BEFORE extraction

---

## Notes

All diagrams use Mermaid syntax for rendering in Markdown viewers that support Mermaid (GitHub, GitLab, VS Code with extensions, etc.).

**Rendering Instructions**:
1. View in GitHub/GitLab (native Mermaid support)
2. VS Code: Install "Markdown Preview Mermaid Support" extension
3. Online: Copy to https://mermaid.live for interactive editing

**Diagram Purpose**:
- **Diagram 1-2**: Visual complexity comparison (before/after)
- **Diagram 3**: Data flow and parameter contracts
- **Diagram 4**: Ticket execution sequence
- **Diagram 5**: Timeline and effort estimation
- **Diagram 6**: Quantitative metrics comparison
- **Diagram 7**: Risk mitigation strategy
- **Diagram 8**: Test coverage strategy

---

**[DIAGRAMS-COMPLETE]** All architecture diagrams generated. Ready for Phase 3 (DNA & PR Audit).