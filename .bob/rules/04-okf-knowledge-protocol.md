# OKF Knowledge Protocol (V12.34)

## Mandatory Rule — All Modes, All Agents

The Jane Street Intelligence Wiki at `docs/intel/jane-street/` is the project's
**Open Knowledge Format (OKF)** local knowledge base. It contains distilled architectural
patterns from Jane Street engineering talks, applied directly to V12 C# patterns.

**Status**: MANDATORY — these are architectural constraints, not suggestions.
**Replaces**: Firebase Firestore `jane_street_knowledge_base` (credential revoked 2026-06).

## What It Is

13 OKF documents covering:
- CYC <= 8 reduction patterns (the primary wave 7 goal)
- Lock-free Actor/Enqueue mandate (DNA enforcement)
- xUnit-only testing standard
- FSM determinism and sidecar lifecycle
- Zero-alloc hot path, cache alignment, JIT warmup
- Data race freedom via type-system patterns
- CPU front-end DSB cache (why CYC <= 8 is also a performance win)

## MANDATORY Reading Triggers

You MUST read the relevant OKF document before:

| Trigger | Documents to Read |
|---------|------------------|
| Phase 2 (Architecture Planning) | `complexity-reduction.md`, `how-to-build-an-exchange.md` |
| Phase 3 (DNA Audit) | `lock-free-patterns.md` |
| Phase 5 (Ticket Execution) | `complexity-reduction.md` + task-specific doc |
| Phase 5.V (Verification) | `lock-free-patterns.md`, `testing-strategies.md` |
| Any `lock()` found in src/ | `lock-free-patterns.md` immediately |
| Any complexity question | `complexity-reduction.md`, `advanced-skylake-deep-dive.md` |

## How to Read

**Index first** (always):
```bash
read_file("docs/intel/jane-street/index.md")
```

**Then the specific document**:
```bash
read_file("docs/intel/jane-street/complexity-reduction.md")
```

**Or query by keyword** (falls back to local wiki):
```bash
python scripts/query_kb.py "complexity reduction"
python scripts/query_kb.py "FSM extraction"
python scripts/query_kb.py "lock-free"
```

## Document Map

| File | Tags | Key Patterns |
|------|------|-------------|
| `complexity-reduction.md` | cyc, extraction | Extract guards, helper methods, FSM decomposition |
| `lock-free-patterns.md` | lock-free, actor | Actor/Enqueue, Interlocked, ConcurrentQueue |
| `testing-strategies.md` | xunit, testing | [Fact], Assert.Equal, never NUnit/MSTest |
| `how-to-build-an-exchange.md` | fsm, determinism | one_in_flight, sidecar_lifecycle, determinism |
| `microsecond-eternity.md` | hot-path, zero-alloc | jit_warmup, cache_alignment, inlining, zero_alloc |
| `ocaml-performance-engineering.md` | structs, cache | struct_cache_locality, ref_struct_escape_prevention |
| `concurrency-coordination.md` | cache-coherency | false sharing, Left-Right double-buffering |
| `advanced-skylake-deep-dive.md` | cpu, dsp | DSB cache fit, denormal protection, lock_free_execution |
| `building-tools-for-traders.md` | ui, keyboard | keyboard_first_ui, exhaustive_pattern_matching |
| `hardware-software-codesign.md` | resilience | defensive_initialization, infrastructure_telemetry |
| `production-engineering-billions.md` | safety | staleness_guard, independent_tracking, rate_limiting |
| `lab-to-trading-floor.md` | ux, config | serializable_configurations, keyboard_driven_execution |
| `why-testing-is-hard.md` | testing | test isolation, FSM testability |

## Enforcement

- ❌ NEVER make an architectural claim without reading the relevant OKF document first
- ❌ NEVER use `lock()` — `lock-free-patterns.md` is the authoritative reference
- ❌ NEVER use NUnit or MSTest — `testing-strategies.md` mandates xUnit only
- ✅ Query the wiki BEFORE Phase 2 and Phase 5 — not after
- ✅ The `pre_task_jane_street_kb.py` hook auto-queries on keyword-matching tasks
