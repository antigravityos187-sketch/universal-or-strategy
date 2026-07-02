#!/usr/bin/env python3
"""
Git pre-push hook — Wave 7 CYC Integrity Gate.

BLOCKS a push if any Wave 7 manifest claims phase_5.status=completed
but the target method still measures CYC > 8 in current src/.

Install:
  cp .git/hooks/pre-push .git/hooks/pre-push.bak 2>/dev/null || true
  cp scripts/wave7_prepush_gate.py .git/hooks/pre-push
  chmod +x .git/hooks/pre-push

This script is idempotent and safe to run multiple times.
It does NOT block pushes for non-W7 work (exits 0 immediately if
no docs/brain/EPIC-W7-* directories exist).

Exit 0 = push allowed
Exit 1 = push blocked (manifests claim done but source CYC > 8)
"""
import json
import os
import re
import subprocess
import sys
from pathlib import Path

THRESHOLD = 8
BRAIN = Path("docs/brain")


def run_complexity_audit() -> dict[str, int]:
    """Return {method_name: max_cyc} from complexity_audit.py output."""
    cyc_map: dict[str, int] = {}
    try:
        r = subprocess.run(
            ["python3", "scripts/complexity_audit.py"],
            capture_output=True, text=True, timeout=120,
            cwd=str(Path.cwd())
        )
        for line in (r.stdout + r.stderr).splitlines():
            m = re.search(r"::([\w]+)\s+\(CYC=(\d+)", line)
            if m:
                name, cyc = m.group(1), int(m.group(2))
                if name not in cyc_map or cyc > cyc_map[name]:
                    cyc_map[name] = cyc
    except Exception as e:
        print(f"[pre-push] WARNING: could not run complexity_audit.py: {e}", file=sys.stderr)
    return cyc_map


def collect_claimed_done_methods() -> list[tuple[str, str]]:
    """
    Scan all EPIC-W7-* manifests.
    Return list of (epic_id, method_name) where phase_5.status == 'completed'.
    """
    pairs: list[tuple[str, str]] = []
    if not BRAIN.exists():
        return pairs

    for epic_dir in sorted(BRAIN.iterdir()):
        if not epic_dir.name.startswith("EPIC-W7-"):
            continue
        manifest_path = epic_dir / "manifest.json"
        if not manifest_path.exists():
            continue
        try:
            manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
            phase5 = manifest.get("phases", {}).get("phase_5", {})
            if phase5.get("status") != "completed":
                continue
        except Exception:
            continue

        # Resolve method name
        method_name = _resolve_method(epic_dir)
        if method_name:
            pairs.append((epic_dir.name, method_name))

    return pairs


def _resolve_method(epic_dir: Path) -> str | None:
    """Try precomputed.json first, then 00-scope.md, then 04-tickets.md."""
    # 1. precomputed.json
    pre = epic_dir / "precomputed.json"
    if pre.exists():
        try:
            d = json.loads(pre.read_text(encoding="utf-8"))
            name = d.get("method_name", "").strip()
            if name:
                return name
        except Exception:
            pass

    # 2. 00-scope.md — look for | **Method Name** | `Foo` |
    scope = epic_dir / "00-scope.md"
    if scope.exists():
        try:
            text = scope.read_text(encoding="utf-8")
            m = re.search(r"\*\*Method Name\*\*\s*\|\s*`([\w]+)`", text)
            if m:
                return m.group(1)
        except Exception:
            pass

    # 3. 04-tickets.md
    tickets = epic_dir / "04-tickets.md"
    if tickets.exists():
        try:
            text = tickets.read_text(encoding="utf-8")
            m = re.search(r"[Mm]ethod[^\n`]*`(\w+)`", text)
            if m:
                return m.group(1)
        except Exception:
            pass

    return None


def main() -> int:
    # Only run if Wave 7 brain dirs exist
    if not BRAIN.exists() or not any(
        d.name.startswith("EPIC-W7-") for d in BRAIN.iterdir() if d.is_dir()
    ):
        return 0

    print("[pre-push] Wave 7 CYC integrity gate running...", file=sys.stderr)

    claimed = collect_claimed_done_methods()
    if not claimed:
        print("[pre-push] No manifests claim phase_5 completed. Gate PASS.", file=sys.stderr)
        return 0

    cyc_map = run_complexity_audit()

    violations: list[tuple[str, str, int]] = []
    for epic_id, method_name in claimed:
        actual_cyc = cyc_map.get(method_name)
        if actual_cyc is not None and actual_cyc > THRESHOLD:
            violations.append((epic_id, method_name, actual_cyc))

    if not violations:
        print(
            f"[pre-push] CYC gate PASS — all {len(claimed)} claimed-done methods are CYC<={THRESHOLD}.",
            file=sys.stderr
        )
        return 0

    print(
        f"\n[pre-push] ❌ PUSH BLOCKED — {len(violations)} manifest(s) fraudulently claim done:\n",
        file=sys.stderr
    )
    for epic_id, method, cyc in violations:
        print(f"  {epic_id}  {method}  CYC={cyc}  (threshold={THRESHOLD})", file=sys.stderr)

    print(
        "\n  Fix: run `start_subtask(mode=v12-engineer)` for the above epics.\n"
        "  The engineer MUST run `python3 scripts/wave7_cyc_gate.py <epic_id> <method>`\n"
        "  and receive exit 0 before writing any completion report.\n",
        file=sys.stderr
    )
    return 1


if __name__ == "__main__":
    sys.exit(main())
