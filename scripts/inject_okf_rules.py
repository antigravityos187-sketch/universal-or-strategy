#!/usr/bin/env python3
"""
inject_okf_rules.py — Inject the full Jane Street OKF rule corpus into every
custom mode's roleDefinition in .bob/custom_modes.yaml.

Strategy:
  - For each mode, find the roleDefinition string value.
  - If it already contains the OKF_MARKER, skip (idempotent).
  - Otherwise prepend the OKF block right before the mode's first
    protocol/step/job description line (after the identity sentence).

The OKF block is injected as the SECOND paragraph in roleDefinition,
immediately after the "You are the ... " opening sentence(s), before
any protocol steps. This way every agent reads the rules as part of
its core identity before seeing any task-specific instructions.
"""

import re
import sys

YAML_PATH = ".bob/custom_modes.yaml"
OKF_MARKER = "JANE STREET OKF RULES (V12 — live guidance"

# The full OKF rule corpus — embedded inline so every agent has it
# regardless of file-read availability.
OKF_BLOCK = """
      ═══════════════════════════════════════════════════════════════
      JANE STREET OKF RULES (V12 — live guidance for every decision)
      Source: docs/intel/jane-street/  (OKF wiki — replaces Firebase KB)
      Status: MANDATORY CONSTRAINTS — not suggestions.
      Apply these rules during planning, execution, and verification.
      ═══════════════════════════════════════════════════════════════

      ── 1. LOCK-FREE CONCURRENCY (lock-free-patterns.md) ─────────────────
        RULE: lock() is STRICTLY BANNED everywhere in src/. Zero tolerance.
              grep -r "lock(" src/ must return 0 results at all times.
        RULE: All state mutations use ONE of: Interlocked.Add/Exchange/
              CompareExchange, volatile field, ConcurrentQueue<T>,
              ConcurrentDictionary<K,V>, ImmutableArray<T>, FSM Enqueue(msg).
        RULE: NEVER use Monitor, Mutex, SemaphoreSlim, ReaderWriterLockSlim
              for state mutation — these all acquire locks.
        RULE: OnStateChange — no lock(). Use _actor.Enqueue(StateChangeMsg).
        RULE: ATM state flags — Interlocked.CompareExchange only.
        RULE: Fleet/Symmetry coordination — ConcurrentQueue message passing.
        RULE: static readonly collections are safe (immutable after init).
        WHY: lock() triggers cache-line invalidation broadcast (~30-60ns/op).
             The Actor/Enqueue model eliminates ALL coordination overhead.
             Data race freedom via types, not runtime mutual exclusion.
        ACTION: Any lock() found → HARD STOP. Escalate. Never auto-fix.

      ── 2. CACHE COHERENCY & FALSE SHARING (concurrency-coordination.md) ──
        RULE: Align independent thread-local variables to separate 64-byte
              cache line boundaries to prevent false sharing.
        RULE: Replace ReaderWriterLockSlim with immutable snapshot + Volatile.
              Write (single writer, zero coordination for readers).
        RULE: Use Volatile.Write/Read for memory barriers between threads.
        RULE: Reader-writer locks perform WORSE than mutexes under contention
              (readers write a shared counter — cache ping-pong). Ban both.

      ── 3. FSM DETERMINISM (how-to-build-an-exchange.md) ──────────────────
        RULE: DateTime.Now is BANNED. Use DateTime.UtcNow or bar-based ticks.
        RULE: All time comparisons must use the SAME clock source (UTC only).
        RULE: one_in_flight — only ONE order per instrument in flight at any
              time (PendingNew → Live → PendingCancel — no overlap, no ghosts).
        RULE: FSM state resets must be intentional and auditable. Accidental
              resets on pre-heartbeat startup = regression bug.
        RULE: sidecar_lifecycle — separate core matching logic from temporal
              rules. Validation sidecars process ONLY known/allowlisted commands.
        RULE: Allowlist check BEFORE rate limiter. Unknown commands rejected
              before consuming rate-limit budget.
        RULE: State Machine Replication — all FSM state must be replayable from
              the transaction log (no non-deterministic side effects in FSM).

      ── 4. DETERMINISTIC TIME & TESTING (why-testing-is-hard.md) ──────────
        RULE: Inject IClock / use bar-based time for testability. Never bind
              tests to system wall clock.
        RULE: Add state_invariants checks (Debug.Assert) at end of every FSM
              state transaction to catch corruption immediately.
        RULE: Phase 5 workers must NOT reduce CYC by deleting logic or
              disabling conditions — extraction must be behavior-preserving.
        RULE: "Evil genie" anti-pattern: never make tests pass by destroying
              architectural constraints. Phase 5.V exists to catch this.

      ── 5. PRODUCTION SAFETY (production-engineering-billions.md) ─────────
        RULE: independent_tracking — each account/position tracked in its own
              variable. NEVER proxy through master/this.Account for fleet checks.
              "Each account tracked independently, never proxied through master."
        RULE: rate_limiting — circuit breaker must fire AFTER allowlist check,
              never on garbage/unknown input.
        RULE: staleness_guard — detect stale feeds by comparing machine time
              vs last tick time; halt strategy on threshold exceeded.
        RULE: manifest_logging — log BUILD_TAG + all parameters at startup
              for deployment roll audits.
        RULE: Defense in depth — independent enforcement gates, separate
              codebases per gate. Never share state between safety layers.

      ── 6. COMPLEXITY REDUCTION (complexity-reduction.md) ────────────────
        RULE: CYC <= 8 for EVERY method. This is the Jane Street strict
              standard (not Codacy's 15). No exceptions.
        RULE: CYC <= 8 is ALSO a CPU optimization — small methods fit in the
              DSB micro-op cache (1536 ops). God methods (CYC > 20) fall back
              to full decode = 2-4x slower (advanced-skylake-deep-dive.md).
        RULE: Reduction strategies (in priority order):
              1. Extract guard clauses (early returns, flat structure).
              2. Named private helper methods — single concern, CYC <= 8 each.
              3. Lookup table / Dictionary dispatch (replaces switch+N cases).
              4. FSM decomposition — each transition = one small method.
              5. Loop body extraction — foreach body → ProcessSingleItem().
        RULE: switch expressions (not statements) for enum dispatch — CYC+1
              vs CYC+N. Always prefer switch expression.
        RULE: ONE method per epic. Helpers go in the SAME class. Never widen
              scope (private stays private).
        RULE: Every extracted helper gets at minimum 1 xUnit [Fact] test.

      ── 7. HOT PATH PERFORMANCE (microsecond-eternity.md) ────────────────
        RULE: Hot path = zero allocations per call. No LINQ, no new T() per
              call, no string concatenation per call.
        RULE: Use struct ref/in/out parameters. Preallocate all buffers.
        RULE: [MethodImpl(AggressiveInlining)] on hot-path methods.
        RULE: [MethodImpl(NoInlining)] on cold-path loggers/diagnostics.
        RULE: JIT warmup: feed dummy events before market open to force
              compilation of all hot paths.
        RULE: StructLayout(Explicit) + FieldOffset for 64-byte cache-line
              padding of hot-path state structs.
        ACTION: If a fix introduces new allocation on a hot-path method
                (OnBarUpdate, signal evaluation, log buffer) → reject plan.

      ── 8. CPU MICRO-ARCHITECTURE (advanced-skylake-deep-dive.md) ─────────
        RULE: denormal_protection — flush doubles near zero to 0.0 to avoid
              CPU pipeline microcode assists (100-200x slowdown).
              if (Math.Abs(value) < 1e-300) value = 0.0;
        RULE: Avoid locked instructions (Interlocked.*) inside tight loops —
              plain increments safe in single-threaded FSM actor thread.
        RULE: Small methods (CYC <= 8) fit entirely in DSB micro-op cache.
              This is why CYC <= 8 is a performance target, not just style.

      ── 9. DATA STRUCTURES (ocaml-performance-engineering.md) ─────────────
        RULE: Prefer struct arrays over class arrays (zero GC scans, L1/L2
              cache-friendly contiguous layout).
        RULE: ref struct for stack-only hot-path snapshots (no heap escape,
              compiler-enforced).
        RULE: Contention mode = Interlocked only. Portability mode = readonly
              fields + immutable snapshots. Global = static readonly.
        RULE: Never box value types on hot path.

      ── 10. TESTING (testing-strategies.md) ──────────────────────────────
        RULE: ALL tests use xUnit ONLY. Framework: [Fact], Assert.Equal().
        RULE: STRICTLY BANNED test attributes:
              [TestFixture] [Test] [TestCase] Assert.That()  ← NUnit
              [TestClass]   [TestMethod]                     ← MSTest
        RULE: Test naming: MethodName_WhenCondition_ThenExpectedBehavior.
        RULE: Test project: tests/V12_Performance.Tests/.
        RULE: Every extracted helper → minimum 1 [Fact] happy-path test.
        RULE: Expect tests for FSM paths: Assert.Equal(
              File.ReadAllText("expected_trace.txt"), fsm.Trace(inputs)).
        ACTION: Any NUnit/MSTest pattern found in a diff → reject, re-plan.

      ── 11. ASCII / ENCODING ──────────────────────────────────────────────
        RULE: ALL C# source (strings, comments, identifiers) = ASCII only.
        RULE: BANNED characters: em dash —(U+2014), en dash –(U+2013),
              curly quotes ""''(U+2018-201D), non-breaking space (U+00A0),
              any character > U+007F.
        RULE: Use -- (double hyphen ASCII) instead of — (em dash) in comments.
        RULE: File encoding: UTF-8, no BOM.
        ACTION: wave7_prepush_gate.py ascii_only section must report 0.

      ── 12. NAMING CONVENTIONS ───────────────────────────────────────────
        RULE: Local variables → camelCase. NEVER _underscore prefix for locals.
        RULE: Private instance fields → _camelCase (underscore prefix only here).
        RULE: Methods → PascalCase. NO underscores in method names.
        RULE: No abbreviations that obscure intent (oqDepth not _oqDepth).

      ── 13. DEFENSIVE INIT (hardware-software-codesign.md) ───────────────
        RULE: OnStateChange must be idempotent — safe to call multiple times.
        RULE: Use EnsureInitialized() guard pattern:
              if (_initialized) return; _initialized = true; InitFsm();
        RULE: Log GC/memory/thread diagnostics alongside trade events for
              infrastructure observability (infrastructure_telemetry).

      ── 14. UI & CONFIGURATION (building-tools-for-traders.md + lab-to-trading-floor.md) ─
        RULE: Keyboard-first UI. No mouse-only workflows in trading panels.
        RULE: Hotkey dispatch via Dictionary<Key, Action> (CYC+1 vs CYC+N).
        RULE: Serializable configurations — save parameters to JSON at startup
              for Git auditing (serializable_configurations).
        RULE: Exhaustive enum dispatch → switch expressions, not statements.

      ═══════════════════════════════════════════════════════════════
      OKF QUICK REFERENCE (use before every planning/coding/review decision):
        lock()       → BANNED. Escalate.
        DateTime.Now → BANNED. Use UtcNow or bar ticks.
        NUnit/MSTest → BANNED. xUnit [Fact] only.
        Unicode      → BANNED. ASCII-only.
        _localVar    → BANNED. camelCase locals.
        CYC > 8      → MUST reduce. Extract helpers.
        New alloc in hot path → MUST avoid. Pre-allocate.
        this.Account for fleet → BANNED. Account.All lookup.
        switch stmt for enum  → Prefer switch expression.
        OnStateChange lock()  → BANNED. Actor.Enqueue().
      ═══════════════════════════════════════════════════════════════
"""


def inject_okf(content: str) -> str:
    """
    Inject OKF block into every roleDefinition that doesn't already have it.
    Strategy: find each `roleDefinition: >` or `roleDefinition: >-` block,
    locate its content, and prepend the OKF block after the first paragraph
    (identity sentence), before any protocol/step text.
    """
    lines = content.split("\n")
    result = []
    i = 0
    injections = 0

    while i < len(lines):
        line = lines[i]
        result.append(line)

        # Detect start of a roleDefinition block
        stripped = line.strip()
        if not (stripped.startswith("roleDefinition: >") or
                stripped == "roleDefinition: |"):
            i += 1
            continue

        # Determine indentation of roleDefinition key
        role_indent = len(line) - len(line.lstrip())
        content_indent = role_indent + 2  # YAML block scalar content indent

        # Collect all lines of the block scalar
        block_lines = []
        j = i + 1
        while j < len(lines):
            bl = lines[j]
            # Block scalar ends when we hit a line with same or less indentation
            # that is non-empty (or end of file)
            if bl.strip() == "":
                # Blank lines are part of the block
                block_lines.append((j, bl))
                j += 1
                continue
            bl_indent = len(bl) - len(bl.lstrip())
            if bl_indent <= role_indent:
                break
            block_lines.append((j, bl))
            j += 1

        if not block_lines:
            i += 1
            continue

        # Check if OKF already injected
        block_text = "\n".join(bl for _, bl in block_lines)
        if OKF_MARKER in block_text:
            # Already has it — consume block lines into result and continue
            for _, bl in block_lines:
                result.append(bl)
            i = j
            continue

        # Find the insertion point: after the first non-empty paragraph
        # (the "You are..." opening), before any ═══ or STEP or PROTOCOL line.
        # We insert after the first blank line that follows text content.
        insert_after_idx = None  # index within block_lines
        found_text = False
        for k, (_, bl) in enumerate(block_lines):
            bls = bl.strip()
            if bls:
                found_text = True
            elif found_text:
                # First blank line after text content = insert point
                insert_after_idx = k
                break

        # If no blank line found, insert before the last protocol block
        if insert_after_idx is None:
            # Find first ═══ or STEP or MANDATORY or PROTOCOL line
            for k, (_, bl) in enumerate(block_lines):
                bls = bl.strip()
                if (bls.startswith("═") or bls.startswith("STEP ") or
                        bls.startswith("MANDATORY") or bls.startswith("YOUR PROTOCOL") or
                        bls.startswith("EXECUTION MODEL") or bls.startswith("STARTUP")):
                    insert_after_idx = k
                    break
            if insert_after_idx is None:
                insert_after_idx = 0  # fallback: prepend

        # Build the OKF injection with correct indentation
        indent_str = " " * content_indent
        okf_indented = "\n".join(
            (indent_str + l if l.strip() else "")
            for l in OKF_BLOCK.split("\n")
        )

        # Write block lines with OKF injected
        for k, (_, bl) in enumerate(block_lines):
            result.append(bl)
            if k == insert_after_idx:
                # Inject OKF block
                for okf_line in okf_indented.split("\n"):
                    result.append(okf_line)

        injections += 1
        i = j

    print(f"Injected OKF block into {injections} roleDefinition(s).", file=sys.stderr)
    return "\n".join(result)


def main():
    with open(YAML_PATH, "r", encoding="utf-8") as f:
        content = f.read()

    new_content = inject_okf(content)

    # Validate YAML parses
    import yaml
    try:
        yaml.safe_load(new_content)
        print("YAML validation: PASS", file=sys.stderr)
    except yaml.YAMLError as e:
        print(f"YAML validation: FAIL — {e}", file=sys.stderr)
        sys.exit(1)

    with open(YAML_PATH, "w", encoding="utf-8") as f:
        f.write(new_content)

    print(f"Done. Written to {YAML_PATH}.", file=sys.stderr)


if __name__ == "__main__":
    main()
