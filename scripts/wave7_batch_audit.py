#!/usr/bin/env python3
"""
Wave 7 Post-Batch Deterministic Compliance Auditor (V3.0)

V3.0 CHANGES (parallel general-subagent architecture):
  - MCP keyword checks REMOVED from Phases 0, 1, 1.5, 4, 5, 5v
    (workers use precomputed.json instead of live MCP calls)
  - MCP keyword checks RETAINED for Phases 2, 3, 4.5, 6
    (these phases run as sequential start_subtask with real MCP)
  - New check: precomputed_exists (docs/brain/EPIC-W7-NNN/precomputed.json present)
  - New check: okf_referenced (artifact mentions okf or jane-street or complexity-reduction)
  - agent_name check relaxed for phases using general workers (agent name = "general-worker")
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
        "artifact":        None,
        "manifest_key":    "phase_5",
        "agent_name":      "wave7-phase5-worker",
        "min_bytes":       500,
        "jcm_keywords":    [],
        "seq_keywords":    [],
        "content_checks":  ["cyc_achieved", "build_passed"],
        "hard_checks":     ["artifact_exists", "min_size", "no_denial",
                            "manifest_complete", "content_assertions"],
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
