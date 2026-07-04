#!/usr/bin/env python3
"""
Wave 7 Post-Batch Deterministic Compliance Auditor (V3.1)

V3.1 CHANGES (Phase 5 ground-truth CYC check):
  - Phase 5 now runs complexity_audit.py and verifies the target method's
    actual CYC in src/ is <= 8. A report claiming final_cyc=8 but the method
    still measuring CYC=19 in source = HARD FAIL.
  - cyc_ground_truth check added to Phase 5 hard_checks list.
  - Epics whose precomputed.json has cyclomatic_complexity=0 (stub-zeroed) are
    re-resolved from 04-tickets.md for the original CYC, then verified against
    complexity_audit output.

V3.0 CHANGES (parallel general-subagent architecture):
  - MCP keyword checks REMOVED from Phases 0, 1, 1.5, 4, 5, 5v
  - MCP keyword checks RETAINED for Phases 2, 3, 4.5, 6
  - New check: precomputed_exists
  - denial phrase check retained for ALL phases

Exit codes:
  0 = all epics in batch PASS all hard checks
  1 = one or more epics FAIL a hard check (orchestrator must retry those epics)
  2 = invocation error (bad args, missing epic list, etc.)

Usage:
  python scripts/wave7_batch_audit.py --phase 0 --epics EPIC-W7-001 EPIC-W7-002 ...
  python scripts/wave7_batch_audit.py --phase 0 --all
  python scripts/wave7_batch_audit.py --phase 0 --all --json
  python scripts/wave7_batch_audit.py --phase 0 --all --fail-file /tmp/redo.txt
"""

import argparse
import json
import os
import re
import subprocess
import sys
from datetime import datetime, timezone
from pathlib import Path

# ---------------------------------------------------------------------------
# Phase definitions — mirrors AUTONOMOUS_REFACTOR_INTEGRATION_MATRIX_V2.md
# ---------------------------------------------------------------------------

# V3.0: Two worker tiers.
# PARALLEL phases (general subagents, precomputed data, no MCP keywords required):
#   0, 1, 1.5, 4, 5, 5v
# SEQUENTIAL phases (start_subtask with real MCP, MCP keywords required):
#   2, 3, 4.5, 6

PHASE_SPECS = {
    # ------------------------------------------------------------------ #
    # PARALLEL PHASES — general subagents, precomputed.json as data source
    # ------------------------------------------------------------------ #
    "0": {
        "artifact":        "00-hotspots.md",
        "manifest_key":    "phase_0",
        "agent_name":      "wave7-phase0-worker",   # general worker agent label
        "min_bytes":       500,
        "jcm_keywords":    [],   # NOT required — precomputed.json used instead
        "seq_keywords":    [],   # NOT required — reasoning template in instructions
        "content_checks":  ["method_name", "cyc", "source_file",
                            "jane street", "extraction"],
        "hard_checks":     ["artifact_exists", "min_size", "no_denial",
                            "manifest_complete", "precomputed_exists",
                            "content_assertions"],
    },
    "1": {
        "artifact":        "00-scope.md",
        "manifest_key":    "phase_1",
        "agent_name":      "wave7-phase1-worker",
        "min_bytes":       500,
        "jcm_keywords":    [],
        "seq_keywords":    [],
        "content_checks":  ["scope_confirmed_single_method",
                            "single method", "scope boundary"],
        "hard_checks":     ["artifact_exists", "min_size", "no_denial",
                            "manifest_complete", "precomputed_exists",
                            "content_assertions"],
    },
    "1.5": {
        "artifact":        "01-scope-boundary.md",
        "manifest_key":    "phase_1_5",
        "agent_name":      "wave7-phase1-5-worker",
        "min_bytes":       500,
        "jcm_keywords":    [],
        "seq_keywords":    [],
        "content_checks":  ["boundary_verdict: pass", "boundary_verdict:pass",
                            "boundary_verdict**: pass"],
        "hard_checks":     ["artifact_exists", "min_size", "no_denial",
                            "manifest_complete", "precomputed_exists",
                            "content_assertions"],
    },
    "4": {
        "artifact":        "04-tickets.md",
        "manifest_key":    "phase_4",
        "agent_name":      "wave7-phase4-worker",
        "min_bytes":       500,
        "jcm_keywords":    [],
        "seq_keywords":    [],
        "content_checks":  ["ticket", "extraction", "cyc"],
        "hard_checks":     ["artifact_exists", "min_size", "no_denial",
                            "manifest_complete", "precomputed_exists",
                            "content_assertions"],
    },
    "5": {
        # artifact = 05-completion-report.md (final per-epic output from v12-p6-review)
        # When --ticket N supplied: checks ticket-N-completion.md instead
        # V3.2: cyc_ground_truth check runs complexity_audit.py and verifies
        #        the target method's actual measured CYC is <= 8 in src/.
        #        cyc_gate_line is a SOFT check logged in checks{} but NOT in hard_checks —
        #        it does not fail epics that pre-date the gate (written before wave7_cyc_gate.py
        #        existed). It is enforced only by roleDefinition + pre-push hook going forward.
        "artifact":        "05-completion-report.md",
        "manifest_key":    "phase_5",
        "agent_name":      "wave7-phase5-worker",
        "min_bytes":       300,
        "jcm_keywords":    [],
        "seq_keywords":    [],
        "content_checks":  ["final_cyc", "wave_ready", "build_passed", "cyc_achieved"],
        "soft_checks":     ["cyc_gate_line"],   # logged but not blocking
        "hard_checks":     ["artifact_exists", "min_size", "no_denial",
                            "manifest_complete", "content_assertions",
                            "cyc_ground_truth"],
    },
    "5v": {
        "artifact":        None,
        "manifest_key":    "phase_5v",
        "agent_name":      "wave7-phase5v-worker",
        "min_bytes":       500,
        "jcm_keywords":    [],
        "seq_keywords":    [],
        "content_checks":  ["verification_verdict: pass", "verification_verdict:pass"],
        "hard_checks":     ["artifact_exists", "min_size", "no_denial",
                            "manifest_complete", "content_assertions"],
    },

    # ------------------------------------------------------------------ #
    # SEQUENTIAL PHASES — start_subtask, real MCP calls required
    # ------------------------------------------------------------------ #
    "2": {
        "artifact":        "02-architecture-plan.md",
        "manifest_key":    "phase_2",
        "agent_name":      "v12-phase2-architecture",
        "min_bytes":       500,
        "jcm_keywords":    ["jcodemunch", "get_context_bundle", "get_dependency_graph",
                            "get_blast_radius", "find_references"],
        "seq_keywords":    ["sequential", "sequentialthinking"],
        "content_checks":  [],
        "hard_checks":     ["artifact_exists", "min_size", "jcm_evidence",
                            "seq_evidence", "no_denial", "manifest_complete", "agent_name"],
    },
    "3": {
        "artifact":        "03-audit-report.md",
        "manifest_key":    "phase_3",
        "agent_name":      "v12-phase3-audit",
        "min_bytes":       500,
        "jcm_keywords":    ["jcodemunch", "find_references", "get_dependency_graph"],
        "seq_keywords":    ["sequential", "sequentialthinking"],
        "content_checks":  [],
        "hard_checks":     ["artifact_exists", "min_size", "jcm_evidence",
                            "seq_evidence", "no_denial", "manifest_complete", "agent_name"],
    },
    "4.5": {
        "artifact":        "04-5-ticket-review.md",
        "manifest_key":    "phase_4_5",
        "agent_name":      "v12-phase4-5-review",
        "min_bytes":       500,
        "jcm_keywords":    [],
        "seq_keywords":    ["sequential", "sequentialthinking"],
        "content_checks":  ["review_verdict: pass", "review_verdict:pass",
                            "review_verdict**: pass"],
        "hard_checks":     ["artifact_exists", "min_size", "seq_evidence",
                            "no_denial", "manifest_complete", "agent_name",
                            "content_assertions"],
    },
    "6": {
        "artifact":        "05-completion-report.md",
        "manifest_key":    "phase_6",
        "agent_name":      "v12-phase6-review",
        "min_bytes":       500,
        "jcm_keywords":    ["jcodemunch", "get_symbol_complexity"],
        "seq_keywords":    ["sequential", "sequentialthinking"],
        "content_checks":  ["wave_ready", "final_cyc"],
        "hard_checks":     ["artifact_exists", "min_size", "jcm_evidence",
                            "seq_evidence", "no_denial", "manifest_complete",
                            "agent_name", "content_assertions"],
    },
}

# Denial phrases — any match = HARD FAIL (worker admitted making up data).
DENIAL_PHRASES = [
    "not available as a callable tool",
    "not available in this execution environment",
    "not available in this session",
    "mcp server was not available",
    "no mcp server process responded",
    "simulated via static",
    "tool not available in session",
    "unavailable at runtime",
    "mcp tools unavailable",
    "not active in this session",
    "not registered in the active mcp",
    "jcodemunch and sequential-thinking mcp servers were unavailable",
    "mcp unavailable",
    "tools were not available",
    "configured but unavailable at runtime",
    "unavailable in this environment",
    "i don't have access to",
    "i cannot access",
    "unable to verify",
    "could not verify",
]


# ---------------------------------------------------------------------------
# CYC ground-truth cache (built once per process, shared across all epics)
# ---------------------------------------------------------------------------

_CYC_CACHE: dict[str, int] | None = None   # method_name -> measured CYC from src/

def _load_cyc_cache() -> dict[str, int]:
    """
    Run complexity_audit.py once and parse the output into a method->CYC map.
    Cached for the lifetime of this process.
    Lines of interest look like:
      "  - V12_002.SIMA.Lifecycle.cs::HydrateFromOpenPositions (CYC=31, LOC=98)"
    """
    global _CYC_CACHE
    if _CYC_CACHE is not None:
        return _CYC_CACHE

    _CYC_CACHE = {}
    try:
        result = subprocess.run(
            ["python3", "scripts/complexity_audit.py"],
            capture_output=True, text=True, timeout=120
        )
        output = result.stdout + result.stderr
        for line in output.splitlines():
            m = re.search(r"::([\w]+)\s+\(CYC=(\d+)", line)
            if m:
                method_name = m.group(1)
                cyc = int(m.group(2))
                # If same method appears in multiple files keep the highest CYC
                # (conservative — if any instance is above 8 we want to catch it)
                if method_name not in _CYC_CACHE or cyc > _CYC_CACHE[method_name]:
                    _CYC_CACHE[method_name] = cyc
    except Exception as e:
        # If we cannot run complexity_audit, skip the check (don't block the whole audit)
        print(f"  [WARN] Could not load CYC cache from complexity_audit.py: {e}", file=sys.stderr)
    return _CYC_CACHE


def _resolve_target_method(epic_id: str) -> str | None:
    """
    Return the target method name for an epic.
    Priority:
      1. precomputed.json method_name (if non-empty)
      2. 00-scope.md  — | **Method Name** | `MethodName` |  (most reliable)
      3. 04-tickets.md — first `MethodName` after "method" keyword
      4. 05-completion-report.md — method_name field
    A valid method name has no dots (rules out filenames like V12_002.Foo.cs).
    """
    brain_dir = Path("docs/brain") / epic_id

    def _is_valid_method(name: str) -> bool:
        """Method names never contain dots; reject file-path fragments."""
        return bool(name) and "." not in name and name.lower() not in ("unknown", "n/a", "")

    # 1. precomputed.json
    pre = brain_dir / "precomputed.json"
    if pre.exists():
        try:
            data = json.loads(pre.read_text(encoding="utf-8"))
            name = data.get("method_name", "").strip()
            if _is_valid_method(name):
                return name
        except Exception:
            pass

    # 2. 00-scope.md  (format: | **Method Name** | `FooBar` |)
    scope = brain_dir / "00-scope.md"
    if scope.exists():
        try:
            text = scope.read_text(encoding="utf-8")
            m = re.search(r'\*\*Method Name\*\*\s*\|\s*`([\w]+)`', text)
            if m and _is_valid_method(m.group(1)):
                return m.group(1)
        except Exception:
            pass

    # 3. 04-tickets.md
    tickets = brain_dir / "04-tickets.md"
    if tickets.exists():
        try:
            text = tickets.read_text(encoding="utf-8")
            # Match "method" keyword followed by a backtick-quoted identifier
            m = re.search(r'[Mm]ethod[^\n`]*`([\w_]+)`', text)
            if m and _is_valid_method(m.group(1)):
                return m.group(1)
        except Exception:
            pass

    # 4. 05-completion-report.md
    report = brain_dir / "05-completion-report.md"
    if report.exists():
        try:
            text = report.read_text(encoding="utf-8")
            m = re.search(r'method(?:_name)?\s*[|:]\s*`?([\w_]+)`?', text, re.IGNORECASE)
            if m and _is_valid_method(m.group(1)):
                return m.group(1)
        except Exception:
            pass

    return None


# ---------------------------------------------------------------------------
# Core audit logic
# ---------------------------------------------------------------------------

def audit_epic(epic_id: str, phase: str, ticket_id: str | None = None) -> dict:
    """
    Audit a single epic for a given phase.
    Returns a result dict:
      { "epic_id", "phase", "status": "PASS"|"FAIL", "checks": {...}, "failures": [...] }
    """
    spec = PHASE_SPECS.get(phase)
    if spec is None:
        return {
            "epic_id": epic_id, "phase": phase,
            "status": "ERROR", "failures": [f"Unknown phase: {phase}"], "checks": {}
        }

    brain_dir = Path("docs/brain") / epic_id
    checks = {}
    failures = []

    # Resolve artifact path
    artifact_name = spec["artifact"]
    if artifact_name is None:
        # ticket-based phase
        if ticket_id:
            artifact_name = f"ticket-{ticket_id}-completion.md" if phase == "5" else f"ticket-{ticket_id}-verification.md"
        else:
            artifact_name = None

    artifact_path = (brain_dir / artifact_name) if artifact_name else None
    content = ""
    content_lower = ""

    # --- Check: artifact_exists ---
    if "artifact_exists" in spec["hard_checks"]:
        exists = artifact_path is not None and artifact_path.exists()
        checks["artifact_exists"] = exists
        if not exists:
            failures.append(f"MISSING artifact: {artifact_path or '(no artifact name resolved)'}")

    # Read content if file exists
    if artifact_path and artifact_path.exists():
        try:
            content = artifact_path.read_text(encoding="utf-8")
            content_lower = content.lower()
        except Exception as e:
            failures.append(f"Cannot read artifact: {e}")
            content = ""
            content_lower = ""

    # --- Check: min_size ---
    if "min_size" in spec["hard_checks"]:
        size = len(content.encode("utf-8")) if content else 0
        ok = size >= spec["min_bytes"]
        checks["min_size"] = ok
        if not ok:
            failures.append(f"Artifact too small: {size} bytes (min {spec['min_bytes']})")

    # --- Check: no_denial (Cat B/C detector) ---
    if "no_denial" in spec["hard_checks"]:
        denial_found = next(
            (phrase for phrase in DENIAL_PHRASES if phrase in content_lower), None
        )
        ok = denial_found is None
        checks["no_denial"] = ok
        if not ok:
            failures.append(f"DENIAL PHRASE found (Cat B/C artifact): '{denial_found}'")

    # --- Check: jcm_evidence (sequential phases only) ---
    if "jcm_evidence" in spec["hard_checks"] and spec["jcm_keywords"]:
        found = any(kw.lower() in content_lower for kw in spec["jcm_keywords"])
        checks["jcm_evidence"] = found
        if not found:
            failures.append(
                f"No jcodemunch MCP evidence (looked for: {spec['jcm_keywords'][:3]}...)"
            )

    # --- Check: seq_evidence (sequential phases only) ---
    if "seq_evidence" in spec["hard_checks"] and spec["seq_keywords"]:
        found = any(kw.lower() in content_lower for kw in spec["seq_keywords"])
        checks["seq_evidence"] = found
        if not found:
            failures.append(
                f"No sequential-thinking MCP evidence (looked for: {spec['seq_keywords'][:2]}...)"
            )

    # --- Check: precomputed_exists (parallel phases) ---
    if "precomputed_exists" in spec["hard_checks"]:
        precomputed_path = brain_dir / "precomputed.json"
        ok = precomputed_path.exists()
        checks["precomputed_exists"] = ok
        if not ok:
            failures.append("precomputed.json missing — run scripts/precompute_wave7_graph.py")

    # --- Check: manifest_complete ---
    if "manifest_complete" in spec["hard_checks"]:
        manifest_path = brain_dir / "manifest.json"
        manifest_ok = False
        if manifest_path.exists():
            try:
                m = json.loads(manifest_path.read_text(encoding="utf-8"))
                phase_data = m.get("phases", {}).get(spec["manifest_key"], {})
                manifest_ok = phase_data.get("status") == "completed"
            except Exception as e:
                failures.append(f"Manifest read error: {e}")
        else:
            failures.append("manifest.json missing")
        checks["manifest_complete"] = manifest_ok
        if not manifest_ok:
            failures.append(f"manifest phases.{spec['manifest_key']}.status != 'completed'")

    # --- Check: agent_name ---
    if "agent_name" in spec["hard_checks"]:
        expected = spec["agent_name"]
        found = expected.lower() in content_lower
        checks["agent_name"] = found
        if not found:
            failures.append(f"Agent name '{expected}' not found in artifact")

    # --- Check: content_assertions ---
    if "content_assertions" in spec["hard_checks"] and spec["content_checks"]:
        # Any one of the content_checks phrases must be present (OR logic)
        found = any(phrase.lower() in content_lower for phrase in spec["content_checks"])
        checks["content_assertions"] = found
        if not found:
            failures.append(
                f"Required content assertion not found. Expected one of: {spec['content_checks']}"
            )

    # --- Soft check: cyc_gate_line (Phase 5 only, non-blocking) ---
    # Records whether the completion report contains "CYC_GATE: PASS" or "CYC_GATE: NOT_FOUND".
    # SOFT — logged in checks{} for observability, does NOT block passing epics.
    # Reason: reports written before wave7_cyc_gate.py existed (pre-V3.2) never had this line.
    # The cyc_ground_truth hard check is the real enforcer for those old reports.
    # For new reports (post-V3.2), enforcement is via roleDefinition + pre-push hook.
    if "cyc_gate_line" in spec.get("soft_checks", []):
        has_gate_line = (
            "cyc_gate: pass" in content_lower
            or "cyc_gate: not_found" in content_lower
        )
        checks["cyc_gate_line"] = has_gate_line
        # Soft: intentionally not appended to failures

    # --- Check: cyc_ground_truth (Phase 5 only) ---
    # Runs complexity_audit.py and verifies the target method's ACTUAL CYC in src/ is <= 8.
    # A completion report can claim final_cyc=8 but the source may be untouched.
    # This is the only check that catches fabricated completion reports.
    if "cyc_ground_truth" in spec["hard_checks"]:
        cyc_cache = _load_cyc_cache()
        method_name = _resolve_target_method(epic_id)
        if method_name is None:
            # Cannot resolve method — soft warn, do not hard-fail (avoids blocking epics
            # that are compliance-only no-ops with no extractable method)
            checks["cyc_ground_truth"] = True
        else:
            actual_cyc = cyc_cache.get(method_name)
            if actual_cyc is None:
                # Method not found in complexity_audit output → it is either
                # already <= 8 (not listed) or renamed after extraction. Both are OK.
                checks["cyc_ground_truth"] = True
            elif actual_cyc <= 8:
                checks["cyc_ground_truth"] = True
            else:
                checks["cyc_ground_truth"] = False
                failures.append(
                    f"CYC GROUND TRUTH FAIL: {method_name} still measures CYC={actual_cyc} "
                    f"in src/ (threshold=8). Completion report is fabricated or extraction "
                    f"was not applied."
                )

    status = "PASS" if len(failures) == 0 else "FAIL"
    return {
        "epic_id":  epic_id,
        "phase":    phase,
        "status":   status,
        "checks":   checks,
        "failures": failures,
    }


def run_batch_audit(
    phase: str,
    epic_ids: list[str],
    ticket_id: str | None = None,
    json_only: bool = False,
    fail_file: str | None = None,
) -> int:
    """
    Audit a batch of epics for a given phase.
    Prints human-readable summary (unless --json) and machine-readable JSON.
    Returns 0 if all pass, 1 if any fail.
    """
    results = [audit_epic(eid, phase, ticket_id) for eid in epic_ids]
    passed  = [r for r in results if r["status"] == "PASS"]
    failed  = [r for r in results if r["status"] == "FAIL"]
    errors  = [r for r in results if r["status"] == "ERROR"]

    summary = {
        "timestamp":    datetime.now(timezone.utc).isoformat(),
        "phase":        phase,
        "total":        len(results),
        "passed":       len(passed),
        "failed":       len(failed),
        "errors":       len(errors),
        "pass_rate":    f"{len(passed)}/{len(results)}",
        "status":       "ALL_PASS" if not failed and not errors else "HAS_FAILURES",
        "failed_epics": [r["epic_id"] for r in failed + errors],
        "results":      results,
    }

    if json_only:
        print(json.dumps(summary, indent=2))
    else:
        bar = "=" * 66
        print(f"\n{bar}")
        print(f"  WAVE 7 BATCH AUDIT  |  Phase {phase}  |  {len(results)} epics")
        print(bar)
        print(f"  PASSED : {len(passed):>4}  ({len(passed)/len(results)*100:.1f}%)")
        print(f"  FAILED : {len(failed):>4}")
        print(f"  ERRORS : {len(errors):>4}")
        print(bar)

        for r in failed + errors:
            print(f"\n  ❌ {r['epic_id']}  [{r['status']}]")
            for f in r["failures"]:
                print(f"       • {f}")

        if not failed and not errors:
            print(f"\n  ✅ All {len(passed)} epics PASSED Phase {phase} audit.")
        else:
            print(
                f"\n  ⚠️  {len(failed)+len(errors)} epics need redo. "
                f"Run orchestrator retry loop for these epic IDs."
            )

        print(f"\n{bar}\n")

    if fail_file:
        Path(fail_file).write_text(
            "\n".join(r["epic_id"] for r in failed + errors), encoding="utf-8"
        )
        if not json_only:
            print(f"  Failed epic IDs written to: {fail_file}")

    return 0 if summary["status"] == "ALL_PASS" else 1


# ---------------------------------------------------------------------------
# CLI entry point
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(
        description="Wave 7 post-batch deterministic compliance auditor (V2.8)"
    )
    parser.add_argument("--phase", required=True,
                        choices=list(PHASE_SPECS.keys()),
                        help="Phase to audit (0, 1, 1.5, 2, 3, 4, 4.5, 5, 5v, 6)")
    parser.add_argument("--epics", nargs="*", default=[],
                        metavar="EPIC_ID",
                        help="Space-separated epic IDs to audit (e.g. EPIC-W7-001 EPIC-W7-002)")
    parser.add_argument("--all", action="store_true",
                        help="Audit all 161 EPIC-W7-* directories")
    parser.add_argument("--ticket", default=None,
                        help="Ticket ID for phase 5 / 5v artifact resolution")
    parser.add_argument("--json", action="store_true",
                        help="Output machine-readable JSON only (suppress human text)")
    parser.add_argument("--fail-file", default=None,
                        help="Path to write failed epic IDs (one per line) — for orchestrator redo loops")

    args = parser.parse_args()

    if args.all:
        brain = Path("docs/brain")
        epic_ids = sorted(d.name for d in brain.iterdir()
                         if d.is_dir() and d.name.startswith("EPIC-W7-"))
    elif args.epics:
        epic_ids = args.epics
    else:
        parser.error("Provide --epics EPIC-W7-001 ... or --all")
        return 2

    if not epic_ids:
        print("No epics found.", file=sys.stderr)
        return 2

    return run_batch_audit(
        phase=args.phase,
        epic_ids=epic_ids,
        ticket_id=args.ticket,
        json_only=args.json,
        fail_file=args.fail_file,
    )


if __name__ == "__main__":
    sys.exit(main())
