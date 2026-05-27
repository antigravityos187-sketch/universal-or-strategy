# Agent Bootstrap Context - Bob

**Task Type**: architecture
**File Scope**: All files
**Loaded**: 2026-05-25 00:55:45 UTC

## Jane Street Knowledge Base

### Distilled Intel: Hardware-Software Codesign (Oxide at Jane Street)
**Category**: N/A

**Key Takeaways**:
- Serious software engineering requires custom hardware integration and BIOS/BMC elimination.
- Hardware defects are frequently masked by software/BIOS workarounds (e.g., the 19-year double-reset NIC bug).
- Transient voltage sags (like the 12V to 8V dip on Jane Street's Sled 19) can cause selective component resets while the main processor remains active.

### Distilled Intel: Building Tools for Traders (Ian Henry)
**Category**: N/A

**Key Takeaways**:
- Trading tools require extreme information density and keyboard-first design over tutorials.
- Bonsai compiles UI as an incremental state DAG, enabling highly optimized, granular virtual DOM patching.
- OCaml isomorphic type sharing across frontend/backend eliminates API serialization boilerplate.

### Distilled Intel: How to Build an Exchange
**Category**: N/A

**Key Takeaways**:
- ECN matching engines operate as deterministic single-threaded state machines on commodity x86 hardware.
- UDP multicast is utilized for simultaneous, fair distribution of market data to all participants.
- State Machine Replication (SMR) allows any component to be rebuilt rapidly by replaying the transaction log.

### Distilled Intel: Production Engineering When Trading Billions
**Category**: N/A

**Key Takeaways**:
- Every order and its economic details are critical; adverse selection punishes bugs immediately.
- Banish average-based SLO alerts for core systems; implement event-based alerting for all edge cases.
- Implement orthogonal, epistemic alerts like 'Feel Too Good' (PnL exceeds expectations) to catch cross-stack issues.

### Distilled Intel: Making OCaml Safe for Performance Engineering
**Category**: N/A

**Key Takeaways**:
- Uniform value representation eases GC and polymorphism but forces boxing of floats/records on the heap.
- Kinds (layouts) track type shapes to specialized generics once per layout, minimizing binary bloat.
- Modes (Global vs Local) track escape behavior, enabling safe, compiler-checked stack allocation of closures.

## Graphify Knowledge Graph

- **Total nodes**: 13716
- **Relevant to scope**: 0

**God Nodes** (high coupling):
- `Unknown` (degree: 0)
- `Unknown` (degree: 0)
- `Unknown` (degree: 0)

## Compound Intelligence Learnings

*No learnings loaded*

## Previous Sessions

*No previous sessions*
