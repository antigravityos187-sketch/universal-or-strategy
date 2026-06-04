# Jane Street Standards: V12 Translation Index

**Version**: 1.0  
**Last Updated**: 2026-06-03  
**Status**: Active Standard  
**Compliance**: V12 DNA Mandatory

---

## Overview

This directory contains 10 comprehensive standards documents that translate Jane Street's OCaml patterns into V12-aligned C# implementations. Each document provides:

- **Jane Street Approach**: OCaml code examples and philosophy
- **V12 Translation**: C# implementations aligned with V12 DNA
- **Pattern Catalog**: 5-7 patterns per document with DO/DON'T guidelines
- **Compliance Matrix**: V12 DNA alignment verification
- **References**: Links to related standards and protocols

---

## Quick Navigation

### Core Patterns
**[JANE_STREET_CORE_PATTERNS.md](./JANE_STREET_CORE_PATTERNS.md)** - Foundational patterns for V12 development

**Topics**: Result monad, discriminated unions, immutable data, Option type, pipeline operators, type-driven development, zero-allocation hot paths

**Use When**: Starting new features, refactoring legacy code, establishing project foundations

---

### Concurrency & Async
**[JANE_STREET_ASYNC_PATTERNS.md](./JANE_STREET_ASYNC_PATTERNS.md)** - Asynchronous programming patterns

**Topics**: Deferred values (ValueTask), Actor model (Channel-based), structured concurrency, backpressure, async sequences, timeout/retry, async coordination

**Use When**: Building concurrent systems, handling I/O operations, implementing message-passing architectures

---

### State Machines
**[JANE_STREET_FSM_PATTERNS.md](./JANE_STREET_FSM_PATTERNS.md)** - Finite state machine design patterns

**Topics**: Basic FSM (order lifecycle), FSM Actor, hierarchical FSM, FSM with side effects, FSM composition, FSM testing (property-based), FSM visualization

**Use When**: Modeling order lifecycles, implementing protocol handlers, managing complex state transitions

---

### Testing
**[JANE_STREET_TESTING_PATTERNS.md](./JANE_STREET_TESTING_PATTERNS.md)** - Testing strategies and patterns

**Topics**: Property-based testing (FsCheck), expect tests (Verify), inline tests, deterministic randomness, fast feedback loops, test data builders, contract testing

**Use When**: Writing tests, refactoring untested code, establishing test infrastructure

---

### Performance
**[JANE_STREET_PERFORMANCE_PATTERNS.md](./JANE_STREET_PERFORMANCE_PATTERNS.md)** - Performance optimization techniques

**Topics**: Zero-allocation hot paths (Span<T>, ArrayPool), cache-friendly data structures, branch prediction, SIMD vectorization, lock-free algorithms, memory layout, profiling

**Use When**: Optimizing hot paths, reducing latency, improving throughput, eliminating allocations

---

### Serialization
**[JANE_STREET_SERIALIZATION_PATTERNS.md](./JANE_STREET_SERIALIZATION_PATTERNS.md)** - Serialization and wire format patterns

**Topics**: Binary protocol (fixed-size messages), schema evolution (versioned messages), variable-length encoding, compression (dictionary encoding), checksums, zero-copy deserialization, streaming

**Use When**: Designing wire protocols, implementing message formats, optimizing serialization performance

---

### Tools & Workflows
**[JANE_STREET_TOOLS_PATTERNS.md](./JANE_STREET_TOOLS_PATTERNS.md)** - Development tools and workflows

**Topics**: Fast compilation (incremental builds), automated formatting (CSharpier), static analysis (Roslyn Analyzers), code generation (Source Generators), continuous testing, IDE integration, CI/CD

**Use When**: Setting up development environment, configuring CI/CD, establishing team workflows

---

### Type Safety
**[JANE_STREET_TYPE_SAFETY.md](./JANE_STREET_TYPE_SAFETY.md)** - Type-driven development patterns

**Topics**: Phantom types (compile-time invariants), newtype pattern (semantic types), exhaustive matching, no null (Option<T>), validated types (parse, don't validate), type-level state machines, dependent types

**Use When**: Preventing bugs at compile time, designing type-safe APIs, eliminating runtime errors

---

### Code Review
**[JANE_STREET_CODE_REVIEW.md](./JANE_STREET_CODE_REVIEW.md)** - Code review practices and standards

**Topics**: Pre-commit validation (fast feedback), PR loop (automated fix-all), adversarial review (Arena AI), incremental changes (small PRs), standards enforcement (Roslyn Analyzers), knowledge sharing, fast turnaround

**Use When**: Reviewing PRs, establishing review processes, enforcing quality standards

---

### Philosophy
**[JANE_STREET_PHILOSOPHY.md](./JANE_STREET_PHILOSOPHY.md)** - Engineering philosophy and culture

**Topics**: Correctness first (type safety over performance), simplicity over cleverness, incremental improvement (Boy Scout Rule), fast feedback (<10s loops), shared ownership, fail fast, measure everything

**Use When**: Onboarding new team members, making architectural decisions, establishing team culture

---

## Document Statistics

| Document | Patterns | Lines | Status |
|----------|----------|-------|--------|
| JANE_STREET_CORE_PATTERNS.md | 7 | 697 | ✅ Complete |
| JANE_STREET_ASYNC_PATTERNS.md | 7 | 823 | ✅ Complete |
| JANE_STREET_FSM_PATTERNS.md | 7 | 897 | ✅ Complete |
| JANE_STREET_TESTING_PATTERNS.md | 7 | 823 | ✅ Complete |
| JANE_STREET_PERFORMANCE_PATTERNS.md | 7 | 1023 | ✅ Complete |
| JANE_STREET_SERIALIZATION_PATTERNS.md | 7 | 897 | ✅ Complete |
| JANE_STREET_TOOLS_PATTERNS.md | 7 | 697 | ✅ Complete |
| JANE_STREET_TYPE_SAFETY.md | 7 | 823 | ✅ Complete |
| JANE_STREET_CODE_REVIEW.md | 7 | 897 | ✅ Complete |
| JANE_STREET_PHILOSOPHY.md | 7 | 897 | ✅ Complete |
| **TOTAL** | **70** | **8,474** | **✅ Complete** |

---

## V12 DNA Compliance

All patterns in these documents are aligned with V12 DNA principles:

### Core Principles
- ✅ **Lock-Free**: Actor/FSM pattern, no `lock()` statements
- ✅ **Type-Safe**: Result<T,E>, Option<T>, validated types
- ✅ **CYC ≤15**: Cognitive simplicity mandate (Jane Street aligned)
- ✅ **ASCII-Only**: No Unicode, emoji, or curly quotes
- ✅ **Zero-Allocation**: Span<T>, ArrayPool<T> in hot paths

### Quality Gates
- ✅ **Pre-Push Validation**: 13 automated checks
- ✅ **PR Loop**: Automated fix-all workflow (PHS = 100)
- ✅ **Arena AI**: Adversarial consensus review
- ✅ **Codacy/CodeRabbit**: Automated quality gates
- ✅ **Bob CLI**: V12 DNA enforcement

---

## Usage Patterns

### For New Features
1. Start with **[JANE_STREET_CORE_PATTERNS.md](./JANE_STREET_CORE_PATTERNS.md)** - Result monad, Option type
2. Review **[JANE_STREET_TYPE_SAFETY.md](./JANE_STREET_TYPE_SAFETY.md)** - Validated types, phantom types
3. Implement with **[JANE_STREET_ASYNC_PATTERNS.md](./JANE_STREET_ASYNC_PATTERNS.md)** - Actor model, ValueTask
4. Test with **[JANE_STREET_TESTING_PATTERNS.md](./JANE_STREET_TESTING_PATTERNS.md)** - Property-based testing

### For Refactoring
1. Review **[JANE_STREET_PHILOSOPHY.md](./JANE_STREET_PHILOSOPHY.md)** - Incremental improvement
2. Apply **[JANE_STREET_FSM_PATTERNS.md](./JANE_STREET_FSM_PATTERNS.md)** - State machine extraction
3. Optimize with **[JANE_STREET_PERFORMANCE_PATTERNS.md](./JANE_STREET_PERFORMANCE_PATTERNS.md)** - Zero-allocation
4. Validate with **[JANE_STREET_CODE_REVIEW.md](./JANE_STREET_CODE_REVIEW.md)** - Pre-push validation

### For Performance Work
1. Profile with **[JANE_STREET_TOOLS_PATTERNS.md](./JANE_STREET_TOOLS_PATTERNS.md)** - BenchmarkDotNet
2. Optimize with **[JANE_STREET_PERFORMANCE_PATTERNS.md](./JANE_STREET_PERFORMANCE_PATTERNS.md)** - Hot path patterns
3. Verify with **[JANE_STREET_TESTING_PATTERNS.md](./JANE_STREET_TESTING_PATTERNS.md)** - Performance tests
4. Measure with **[JANE_STREET_PHILOSOPHY.md](./JANE_STREET_PHILOSOPHY.md)** - Metrics

### For Protocol Design
1. Design with **[JANE_STREET_SERIALIZATION_PATTERNS.md](./JANE_STREET_SERIALIZATION_PATTERNS.md)** - Binary protocol
2. Validate with **[JANE_STREET_TYPE_SAFETY.md](./JANE_STREET_TYPE_SAFETY.md)** - Schema evolution
3. Test with **[JANE_STREET_TESTING_PATTERNS.md](./JANE_STREET_TESTING_PATTERNS.md)** - Contract testing
4. Optimize with **[JANE_STREET_PERFORMANCE_PATTERNS.md](./JANE_STREET_PERFORMANCE_PATTERNS.md)** - Zero-copy

---

## Pattern Cross-Reference

### By V12 DNA Principle

#### Lock-Free Patterns
- **[JANE_STREET_ASYNC_PATTERNS.md](./JANE_STREET_ASYNC_PATTERNS.md)**: Pattern 2 (Actor Model)
- **[JANE_STREET_FSM_PATTERNS.md](./JANE_STREET_FSM_PATTERNS.md)**: Pattern 2 (FSM Actor)
- **[JANE_STREET_PERFORMANCE_PATTERNS.md](./JANE_STREET_PERFORMANCE_PATTERNS.md)**: Pattern 5 (Lock-Free Algorithms)

#### Type-Safe Patterns
- **[JANE_STREET_CORE_PATTERNS.md](./JANE_STREET_CORE_PATTERNS.md)**: Pattern 1 (Result Monad), Pattern 4 (Option Type)
- **[JANE_STREET_TYPE_SAFETY.md](./JANE_STREET_TYPE_SAFETY.md)**: All 7 patterns
- **[JANE_STREET_FSM_PATTERNS.md](./JANE_STREET_FSM_PATTERNS.md)**: Pattern 1 (Basic FSM)

#### Zero-Allocation Patterns
- **[JANE_STREET_CORE_PATTERNS.md](./JANE_STREET_CORE_PATTERNS.md)**: Pattern 7 (Zero-Allocation Hot Paths)
- **[JANE_STREET_PERFORMANCE_PATTERNS.md](./JANE_STREET_PERFORMANCE_PATTERNS.md)**: Pattern 1 (Zero-Allocation)
- **[JANE_STREET_SERIALIZATION_PATTERNS.md](./JANE_STREET_SERIALIZATION_PATTERNS.md)**: Pattern 6 (Zero-Copy Deserialization)

#### Testing Patterns
- **[JANE_STREET_TESTING_PATTERNS.md](./JANE_STREET_TESTING_PATTERNS.md)**: All 7 patterns
- **[JANE_STREET_FSM_PATTERNS.md](./JANE_STREET_FSM_PATTERNS.md)**: Pattern 6 (FSM Testing)
- **[JANE_STREET_CODE_REVIEW.md](./JANE_STREET_CODE_REVIEW.md)**: Pattern 1 (Pre-Commit Validation)

---

## Integration with V12 Protocols

### AGENTS.md Integration
- **Section 2**: Correctness by Construction → **[JANE_STREET_TYPE_SAFETY.md](./JANE_STREET_TYPE_SAFETY.md)**
- **Section 3.5**: Pre-Push Validation → **[JANE_STREET_CODE_REVIEW.md](./JANE_STREET_CODE_REVIEW.md)**
- **Section 11**: No Scope Creep → **[JANE_STREET_CODE_REVIEW.md](./JANE_STREET_CODE_REVIEW.md)** Pattern 4

### Bob CLI Integration
- **V12 DNA Enforcement**: All documents provide Bob-compatible patterns
- **Complexity Gates**: CYC ≤15 threshold aligned with Jane Street
- **Quality Checks**: Pre-push validation patterns

### Arena AI Integration
- **Adversarial Review**: **[JANE_STREET_CODE_REVIEW.md](./JANE_STREET_CODE_REVIEW.md)** Pattern 3
- **Consensus Protocol**: Multi-reviewer patterns

---

## Firestore KB References

All documents reference Jane Street intel from the Firestore Knowledge Base:

- `weeks_making_ocaml_safe_2025` - Making OCaml Safe for Performance Engineering
- `jane_street_async_library` - Async programming patterns
- `jane_street_testing_framework` - Testing strategies
- `jane_street_bin_prot` - Binary serialization protocol
- `jane_street_incremental` - Incremental computation
- `jane_street_core_library` - Core functional patterns
- `jane_street_expect_tests` - Expect test framework
- `jane_street_ppx_tools` - Code generation tools
- `jane_street_code_review` - Review practices
- `jane_street_engineering_culture` - Philosophy and culture

---

## Maintenance

### Review Schedule
- **Quarterly Review**: 2026-09-03 (next review)
- **Annual Update**: 2027-06-03
- **Ad-Hoc Updates**: When V12 DNA evolves

### Update Protocol
1. Identify pattern gaps or V12 DNA changes
2. Query Firestore KB for latest Jane Street intel
3. Update affected documents
4. Run validation: `powershell -File .\scripts\verify_links.ps1`
5. Submit PR with updated documents

### Contribution Guidelines
- Follow existing document structure (7 patterns per document)
- Include OCaml + C# side-by-side examples
- Add DO/DON'T checklists
- Verify V12 DNA compliance matrix
- Link to related standards

---

## Related Standards

### V12 Core Standards
- [`AGENTS.md`](../../AGENTS.md) - Agent hierarchy and protocols
- [`BOB.md`](../../BOB.md) - Bob CLI integration
- [`CLAUDE.md`](../../CLAUDE.md) - Claude-specific guidelines
- [`CODEX.md`](../../CODEX.md) - Codex CLI patterns

### V12 Protocols
- [`docs/protocol/BRANCH_STRATEGY.md`](../../protocol/BRANCH_STRATEGY.md) - Three-tier branch model
- [`docs/protocol/CODESCENE_INTEGRATION.md`](../../protocol/CODESCENE_INTEGRATION.md) - Hotspot analysis
- [`docs/protocol/BOB_MISE_INTEGRATION.md`](../../protocol/BOB_MISE_INTEGRATION.md) - Tool management

### V12 Brain
- [`docs/brain/task.md`](../../brain/task.md) - Active task tracking
- [`docs/brain/nexus_a2a.json`](../../brain/nexus_a2a.json) - Agent-to-agent state

---

## Quick Reference Card

### Most Common Patterns

| Task | Pattern | Document |
|------|---------|----------|
| Error handling | Result<T,E> monad | [CORE](./JANE_STREET_CORE_PATTERNS.md) Pattern 1 |
| Null safety | Option<T> type | [CORE](./JANE_STREET_CORE_PATTERNS.md) Pattern 4 |
| Concurrency | Actor model | [ASYNC](./JANE_STREET_ASYNC_PATTERNS.md) Pattern 2 |
| State machines | FSM Actor | [FSM](./JANE_STREET_FSM_PATTERNS.md) Pattern 2 |
| Testing | Property-based | [TESTING](./JANE_STREET_TESTING_PATTERNS.md) Pattern 1 |
| Performance | Zero-allocation | [PERFORMANCE](./JANE_STREET_PERFORMANCE_PATTERNS.md) Pattern 1 |
| Serialization | Binary protocol | [SERIALIZATION](./JANE_STREET_SERIALIZATION_PATTERNS.md) Pattern 1 |
| Type safety | Validated types | [TYPE_SAFETY](./JANE_STREET_TYPE_SAFETY.md) Pattern 5 |
| Code review | Pre-push validation | [CODE_REVIEW](./JANE_STREET_CODE_REVIEW.md) Pattern 1 |
| Philosophy | Correctness first | [PHILOSOPHY](./JANE_STREET_PHILOSOPHY.md) Pattern 1 |

---

## Contact & Support

### Questions
- **Architecture**: Review **[JANE_STREET_PHILOSOPHY.md](./JANE_STREET_PHILOSOPHY.md)** first
- **Implementation**: Check pattern cross-reference above
- **V12 DNA**: See [`AGENTS.md`](../../AGENTS.md) Section 2

### Feedback
- Submit PR with proposed changes
- Include rationale and Jane Street references
- Verify V12 DNA compliance

---

**Index Status**: ✅ Complete (10 documents, 70 patterns)  
**Last Updated**: 2026-06-03  
**Maintainer**: V12 Architecture Team  
**Next Review**: 2026-09-03