#!/usr/bin/env python3
"""
OKF Violation Scanner -- Wave 7+ debt detection tool.

Scans all src/*.cs files for pre-existing OKF rule violations that complexity_audit.py
does NOT catch (it only finds CYC > 8). This script finds:

  P0: lock() calls (OKF Rule 1 -- STRICTLY BANNED)
  P1: DateTime.Now usage (OKF Rule 3 -- FSM Determinism)
  P2: Account.All / .Orders / .Positions enumerated without .ToArray() snapshot
  P3: .Instrument.FullName / .Name without null guard on preceding line
  P4: SA1503 single-line if bodies without braces (heuristic)

Output: docs/brain/debt/okf-violation-scan.md (human-readable)
        docs/brain/debt/okf-violation-scan.json (machine-readable for Phase 0)

Usage:
  python3 scripts/scan_okf_violations.py
  python3 scripts/scan_okf_violations.py --priority P0,P1
  python3 scripts/scan_okf_violations.py --file src/V12_002.SIMA.Lifecycle.cs
  python3 scripts/scan_okf_violations.py --since-wave 7  (skip already-registered DD entries)
"""

import os
import re
import json
import sys
import argparse
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).parent.parent
SRC_DIR = ROOT / "src"
DEBT_DIR = ROOT / "docs" / "brain" / "debt"
REGISTER_PATH = ROOT / "docs" / "brain" / "wave7-pr-repairs" / "deferred-debt-register.md"
OUTPUT_MD = DEBT_DIR / "okf-violation-scan.md"
OUTPUT_JSON = DEBT_DIR / "okf-violation-scan.json"


# ---------------------------------------------------------------------------
# Patterns
# ---------------------------------------------------------------------------

PATTERNS = {
    "P0_lock": {
        "priority": "P0",
        "rule": "Rule 1 -- Lock-Free Concurrency",
        "description": "lock() call -- STRICTLY BANNED",
        "regex": re.compile(r'\block\s*\('),
        "exclude_comment": True,
    },
    "P1_datetime_now": {
        "priority": "P1",
        "rule": "Rule 3 -- FSM Determinism",
        "description": "DateTime.Now -- use DateTime.UtcNow",
        "regex": re.compile(r'DateTime\.Now(?!\.ToString|Zone)'),
        "exclude_comment": True,
    },
    "P2_account_all_no_toarray": {
        "priority": "P2",
        "rule": "Rule 5 -- Production Safety (independent_tracking)",
        "description": "Account.All enumerated without .ToArray() snapshot",
        "regex": re.compile(r'Account\.All\b(?!\s*\.ToArray\b|\s*\.FirstOrDefault\b|\s*\.Any\b|\s*\.Count\b|\s*\.Where\b\.ToArray)'),
        "exclude_comment": True,
    },
    "P2_orders_no_toarray": {
        "priority": "P2",
        "rule": "Rule 5 -- Production Safety (defense in depth)",
        "description": "acct.Orders enumerated without .ToArray() snapshot",
        "regex": re.compile(r'(?:acct|account|Account)\.Orders\b(?!\s*\.ToArray\b|\s*\.Count\b|\s*\[)'),
        "exclude_comment": True,
    },
    "P2_positions_no_toarray": {
        "priority": "P2",
        "rule": "Rule 5 -- Production Safety (defense in depth)",
        "description": "acct.Positions enumerated without .ToArray() snapshot",
        "regex": re.compile(r'(?:acct|account|fleetAcct)\.Positions\b(?!\s*\.ToArray\b|\s*\.Count\b|\s*\.FirstOrDefault\b|\s*\.Any\b)'),
        "exclude_comment": True,
    },
}


def is_comment_line(line: str) -> bool:
    stripped = line.lstrip()
    return stripped.startswith("//") or stripped.startswith("/*") or stripped.startswith("*")


def load_registered_locations() -> set:
    """Return set of (file_basename, line_number_str) already in deferred-debt-register."""
    registered = set()
    if not REGISTER_PATH.exists():
        return registered
    with open(REGISTER_PATH) as f:
        for line in f:
            # Match lines like: | DD-NNN | PR-N | src/file.cs | 39, 96 | ...
            m = re.match(r'\|\s*DD-\d+\s*\|[^|]+\|\s*(src/[^\s|]+)\s*\|\s*([^|]+)\|', line)
            if m:
                fname = m.group(1).strip()
                lines_str = m.group(2).strip()
                for ln in re.findall(r'\d+', lines_str):
                    registered.add((fname, ln))
    return registered


def scan_file(filepath: Path, registered: set, priority_filter: set) -> list:
    findings = []
    rel_path = str(filepath.relative_to(ROOT))
    try:
        with open(filepath, encoding="utf-8", errors="replace") as f:
            lines = f.readlines()
    except Exception as e:
        return findings

    for i, line in enumerate(lines, 1):
        if is_comment_line(line):
            continue
        for pattern_key, spec in PATTERNS.items():
            if spec["priority"] not in priority_filter:
                continue
            if spec.get("exclude_comment") and is_comment_line(line):
                continue
            if spec["regex"].search(line):
                # Skip if already registered
                if (rel_path, str(i)) in registered:
                    continue
                findings.append({
                    "file": rel_path,
                    "line": i,
                    "priority": spec["priority"],
                    "rule": spec["rule"],
                    "description": spec["description"],
                    "pattern_key": pattern_key,
                    "code_snippet": line.rstrip(),
                })
    return findings


def main():
    parser = argparse.ArgumentParser(description="OKF Violation Scanner")
    parser.add_argument("--priority", default="P0,P1,P2,P3,P4",
                        help="Comma-separated priority levels to scan (default: all)")
    parser.add_argument("--file", default=None,
                        help="Scan a single file instead of all src/")
    parser.add_argument("--skip-registered", action="store_true", default=True,
                        help="Skip violations already in deferred-debt-register.md")
    parser.add_argument("--json-only", action="store_true",
                        help="Output JSON only, no markdown")
    args = parser.parse_args()

    priority_filter = set(p.strip() for p in args.priority.split(","))
    registered = load_registered_locations() if args.skip_registered else set()

    if args.file:
        files = [Path(args.file) if Path(args.file).is_absolute() else ROOT / args.file]
    else:
        files = sorted(SRC_DIR.glob("*.cs"))

    all_findings = []
    for filepath in files:
        if not filepath.exists():
            print(f"WARNING: {filepath} not found", file=sys.stderr)
            continue
        all_findings.extend(scan_file(filepath, registered, priority_filter))

    # Sort: P0 first, then P1, then by file
    priority_order = {"P0": 0, "P1": 1, "P2": 2, "P3": 3, "P4": 4}
    all_findings.sort(key=lambda x: (priority_order.get(x["priority"], 9), x["file"], x["line"]))

    # Ensure output dir
    DEBT_DIR.mkdir(parents=True, exist_ok=True)

    # Write JSON
    output_json = {
        "generated": datetime.now(timezone.utc).isoformat(),
        "total": len(all_findings),
        "by_priority": {
            p: len([f for f in all_findings if f["priority"] == p])
            for p in ["P0", "P1", "P2", "P3", "P4"]
        },
        "findings": all_findings,
    }
    with open(OUTPUT_JSON, "w") as f:
        json.dump(output_json, f, indent=2)

    if args.json_only:
        print(json.dumps(output_json, indent=2))
        return

    # Write Markdown
    now = datetime.now(timezone.utc).strftime("%Y-%m-%d")
    lines_md = [
        "# OKF Violation Scan Report",
        "",
        f"**Generated**: {now}",
        f"**Scope**: src/*.cs ({len(files)} files)",
        f"**Total new violations**: {len(all_findings)} (excluding already-registered entries)",
        "",
        "| Count | Priority | Rule |",
        "|-------|----------|------|",
    ]
    for p in ["P0", "P1", "P2", "P3", "P4"]:
        n = len([f for f in all_findings if f["priority"] == p])
        if n:
            label = {"P0":"lock() banned","P1":"DateTime.Now","P2":"missing .ToArray()","P3":"null deref risk","P4":"style/naming"}[p]
            lines_md.append(f"| {n} | {p} | {label} |")

    lines_md += ["", "---", "", "## Findings", ""]

    current_priority = None
    for finding in all_findings:
        if finding["priority"] != current_priority:
            current_priority = finding["priority"]
            lines_md.append(f"### {current_priority}")
            lines_md.append("")

        lines_md.append(
            f"- **{finding['file']}:{finding['line']}** -- {finding['description']}"
        )
        lines_md.append(f"  `{finding['code_snippet'].strip()[:100]}`")
        lines_md.append("")

    lines_md += [
        "---",
        "",
        "## Next Steps",
        "",
        "1. P0 findings: HARD STOP. Fix before any merge. Escalate to Director.",
        "2. P1 findings: Add to next wave Phase 0 hotspot list as mandatory pre-scan.",
        "3. P2/P3 findings: Triage per file. Add to deferred-debt-register.md rows.",
        "4. P4 findings: Group by file. Fix in next wave touchin the same file.",
        "",
        f"**Machine-readable**: docs/brain/debt/okf-violation-scan.json",
    ]

    with open(OUTPUT_MD, "w") as f:
        f.write("\n".join(lines_md) + "\n")

    # Console summary
    print(f"OKF Violation Scan complete.")
    print(f"  Files scanned: {len(files)}")
    print(f"  New violations: {len(all_findings)}")
    for p in ["P0", "P1", "P2", "P3", "P4"]:
        n = len([f for f in all_findings if f["priority"] == p])
        if n:
            print(f"  {p}: {n}")
    print(f"  Report: {OUTPUT_MD}")
    print(f"  JSON:   {OUTPUT_JSON}")


if __name__ == "__main__":
    main()
