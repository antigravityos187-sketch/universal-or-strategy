#!/usr/bin/env python3
"""
Wave 7 Graph Pre-computation Script (V3.0)

Runs ONCE at wave start. Uses jcodemunch MCP data already cached in
wave7-epic-list.json and complexity_audit.py output. Dumps per-epic
static JSON to docs/brain/EPIC-W7-NNN/precomputed.json so that
general subagent workers can load it without any MCP calls.

Also reads all 14 OKF Jane Street wiki files and dumps a single
docs/brain/wave7-okf-cache.json with the full text — workers load
this instead of calling query_kb.py.

Usage:
    python3 scripts/precompute_wave7_graph.py
    python3 scripts/precompute_wave7_graph.py --force   # overwrite existing
"""

import json
import os
import sys
import subprocess
import argparse
from pathlib import Path

WORKSPACE = Path(__file__).parent.parent
EPIC_LIST = WORKSPACE / "docs/brain/wave7-epic-list.json"
BRAIN_DIR = WORKSPACE / "docs/brain"
OKF_DIR   = WORKSPACE / "docs/intel/jane-street"
OKF_CACHE = WORKSPACE / "docs/brain/wave7-okf-cache.json"
COMPLEXITY_SCRIPT = WORKSPACE / "scripts/complexity_audit.py"


def load_epic_list():
    with open(EPIC_LIST, encoding="utf-8") as f:
        return json.load(f)


def run_complexity_audit():
    """Run complexity_audit.py and return dict keyed by method_name."""
    if not COMPLEXITY_SCRIPT.exists():
        print("  [WARN] complexity_audit.py not found — using epic list CYC values only")
        return {}
    try:
        result = subprocess.run(
            [sys.executable, str(COMPLEXITY_SCRIPT), "--json"],
            capture_output=True, text=True, cwd=WORKSPACE, timeout=120
        )
        if result.returncode == 0 and result.stdout.strip():
            data = json.loads(result.stdout)
            # Normalise to dict keyed by method_name
            if isinstance(data, list):
                return {item["name"]: item for item in data if "name" in item}
            if isinstance(data, dict):
                return data
    except Exception as e:
        print(f"  [WARN] complexity_audit.py failed: {e}")
    return {}


def build_okf_cache():
    """Read all 14 OKF .md files and write a single cache JSON."""
    if OKF_CACHE.exists():
        print(f"  [SKIP] OKF cache already exists: {OKF_CACHE.name}")
        return

    okf_data = {}
    if not OKF_DIR.exists():
        print(f"  [WARN] OKF dir not found: {OKF_DIR}")
        return

    for md_file in sorted(OKF_DIR.glob("*.md")):
        if md_file.name == "index.md":
            continue
        content = md_file.read_text(encoding="utf-8")
        okf_data[md_file.stem] = {
            "filename": md_file.name,
            "content": content
        }

    OKF_CACHE.write_text(json.dumps(okf_data, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"  [OK] OKF cache written: {len(okf_data)} files -> {OKF_CACHE.name}")


def build_precomputed(epic: dict, complexity_map: dict, force: bool) -> bool:
    """Build precomputed.json for one epic. Returns True if written."""
    epic_id    = epic["epic_id"]
    method     = epic["method_name"]
    cyc        = epic["cyc"]
    source_file = epic["source_file"]

    out_dir  = BRAIN_DIR / epic_id
    out_file = out_dir / "precomputed.json"

    if out_file.exists() and not force:
        return False  # already done

    out_dir.mkdir(parents=True, exist_ok=True)

    # Enrich with complexity_audit data if available
    complexity_entry = complexity_map.get(method, {})
    confirmed_cyc  = complexity_entry.get("cyclomatic_complexity", cyc)
    max_nesting    = complexity_entry.get("max_nesting_depth",    None)
    line_count     = complexity_entry.get("line_count",           None)
    param_count    = complexity_entry.get("parameter_count",      None)

    # Determine risk level
    if confirmed_cyc >= 20:
        risk = "HIGH"
    elif confirmed_cyc >= 15:
        risk = "MEDIUM-HIGH"
    elif confirmed_cyc >= 10:
        risk = "MEDIUM"
    else:
        risk = "LOW"

    # Normalise source_file path
    src = source_file
    if src and not src.startswith("src/") and not src.startswith("/") and "backup" not in src:
        # Some entries missing src/ prefix
        src = f"src/{src}" if not src.startswith("V12_") else f"src/{src}"

    precomputed = {
        "schema_version":  "3.0",
        "epic_id":         epic_id,
        "method_name":     method,
        "source_file":     src,
        "cyc":             confirmed_cyc,
        "cyc_raw_list":    cyc,
        "max_nesting":     max_nesting,
        "line_count":      line_count,
        "param_count":     param_count,
        "risk_level":      risk,
        "jane_street_threshold": 8,
        "cyc_over_threshold": max(0, confirmed_cyc - 8),
        # Blast radius: conservatively UNKNOWN until Phase 1.5 runs real check
        "blast_radius": {
            "status":      "pending_phase_1_5",
            "note":        "Will be confirmed in Phase 1.5 via get_blast_radius"
        },
        # Extraction estimate: rough formula
        "estimated_extractions": max(1, (confirmed_cyc - 8) // 4),
        "okf_cache_path": "docs/brain/wave7-okf-cache.json",
        "epic_list_path": "docs/brain/wave7-epic-list.json",
        "precomputed_by": "precompute_wave7_graph.py v3.0",
    }

    out_file.write_text(json.dumps(precomputed, indent=2, ensure_ascii=False), encoding="utf-8")
    return True


def main():
    parser = argparse.ArgumentParser(description="Wave 7 graph pre-computation (V3.0)")
    parser.add_argument("--force", action="store_true", help="Overwrite existing precomputed.json files")
    args = parser.parse_args()

    print("\n=== Wave 7 Graph Pre-computation V3.0 ===\n")

    # 1. OKF cache
    print("[1/3] Building OKF Jane Street KB cache...")
    build_okf_cache()

    # 2. Complexity audit
    print("\n[2/3] Loading complexity audit data...")
    complexity_map = run_complexity_audit()
    print(f"  Complexity entries loaded: {len(complexity_map)}")

    # 3. Per-epic precomputed.json
    print("\n[3/3] Writing per-epic precomputed.json files...")
    epics = load_epic_list()
    written = 0
    skipped = 0
    for epic in epics:
        ok = build_precomputed(epic, complexity_map, force=args.force)
        if ok:
            written += 1
        else:
            skipped += 1

    print(f"\n  Written : {written}")
    print(f"  Skipped : {skipped} (already existed, use --force to overwrite)")
    print(f"\n=== Done. {written + skipped}/161 epics have precomputed.json ===\n")


if __name__ == "__main__":
    main()
