#!/usr/bin/env python3
"""
wave7_prepush_gate.py -- Wave 7 pre-push quality gate

Checks performed (all BLOCKING unless marked WARNING):
  1. [BLOCKING]  ASCII-only in all modified src/ files
  2. [BLOCKING]  No DateTime.Now in modified src/ files (must use UtcNow)
  3. [BLOCKING]  No lock() in modified src/ files
  4. [BLOCKING]  No underscore-prefixed local variables in modified src/ files
  5. [BLOCKING]  Diff char count vs origin/main stays under SOURCERY_CHAR_LIMIT
  6. [WARNING]   Diff char count approaching limit (>= WARN_CHAR_LIMIT)

Usage:
    python scripts/wave7_prepush_gate.py
    python scripts/wave7_prepush_gate.py --base origin/main
    python scripts/wave7_prepush_gate.py --files src/Foo.cs src/Bar.cs  # explicit file list

Exit codes:
    0 = all blocking checks pass
    1 = one or more blocking checks failed
    2 = invocation error
"""

import argparse
import re
import subprocess
import sys
from pathlib import Path

# ---------------------------------------------------------------------------
# Thresholds
# ---------------------------------------------------------------------------
SOURCERY_CHAR_LIMIT = 150_000   # Sourcery skips review above this -- BLOCKING
WARN_CHAR_LIMIT = 120_000       # Early warning at 80% of limit


def run(cmd: list[str]) -> subprocess.CompletedProcess:
    return subprocess.run(cmd, capture_output=True, text=True)


def get_modified_src_files(base: str) -> list[str]:
    """Return list of src/*.cs files that differ between HEAD and base."""
    result = run(["git", "diff", "--name-only", f"{base}...HEAD", "--", "src/"])
    if result.returncode != 0:
        print(f"ERROR: git diff failed: {result.stderr.strip()}", file=sys.stderr)
        sys.exit(2)
    files = [f.strip() for f in result.stdout.splitlines() if f.strip().endswith(".cs")]
    return files


def get_diff_char_count(base: str) -> tuple[int, int]:
    """Return (raw_chars, stripped_chars) of src/ diff vs base.
    stripped_chars uses -w (ignore all whitespace changes) to exclude
    CSharpier/extraction reformatting from the Sourcery-limit calculation.
    """
    raw = run(["git", "diff", f"{base}...HEAD", "--", "src/"])
    stripped = run(["git", "diff", "-w", f"{base}...HEAD", "--", "src/"])
    if raw.returncode != 0:
        print(f"ERROR: git diff failed: {raw.stderr.strip()}", file=sys.stderr)
        sys.exit(2)
    return len(raw.stdout), len(stripped.stdout)


def check_ascii_only(files: list[str]) -> list[str]:
    """Return list of violation messages for non-ASCII bytes in source files."""
    violations = []
    for f in files:
        path = Path(f)
        if not path.exists():
            continue
        data = path.read_bytes()
        bad_positions = [i for i, b in enumerate(data) if b > 127]
        if bad_positions:
            # Show first offending line number and hex
            lines = data.split(b"\n")
            char_count = 0
            for lineno, line in enumerate(lines, 1):
                if any(b > 127 for b in line):
                    first_bad = next(b for b in line if b > 127)
                    violations.append(
                        f"  {f}:{lineno} -- non-ASCII byte 0x{first_bad:02x} found"
                    )
                    break
    return violations


def _scan_added_lines(base: str, pattern: re.Pattern, msg_template: str) -> list[str]:
    """Scan only added lines (+) in the diff for a pattern. Returns violation messages."""
    violations = []
    result = run(["git", "diff", f"{base}...HEAD", "--unified=0", "--", "src/"])
    if result.returncode != 0:
        return []
    current_file = ""
    current_line = 0
    for raw_line in result.stdout.splitlines():
        if raw_line.startswith("+++ b/"):
            current_file = raw_line[6:]
            continue
        if raw_line.startswith("@@"):
            m = re.search(r"\+(\d+)", raw_line)
            current_line = int(m.group(1)) - 1 if m else 0
            continue
        if raw_line.startswith("+"):
            current_line += 1
            content = raw_line[1:]
            stripped = content.lstrip()
            if stripped.startswith("//") or stripped.startswith("*"):
                continue
            if pattern.search(content):
                violations.append(f"  {current_file}:{current_line} -- {msg_template}")
        elif not raw_line.startswith("-"):
            current_line += 1
    return violations


def check_datetime_now(base: str) -> list[str]:
    """Return violations for DateTime.Now introduced in this diff."""
    return _scan_added_lines(
        base,
        re.compile(r"DateTime\.Now(?!\.Ticks)"),
        "DateTime.Now (use DateTime.UtcNow)",
    )


def check_lock_usage(base: str) -> list[str]:
    """Return violations for lock() introduced in this diff."""
    return _scan_added_lines(
        base,
        re.compile(r"\block\s*\("),
        "lock() found (use Actor/Enqueue pattern)",
    )


def check_underscore_locals(base: str) -> list[str]:
    """Return violations for underscore-prefixed local variables introduced in this PR's diff."""
    violations = []
    # Match added lines: type _varName = or var _varName, etc.
    # Field declarations (private/protected/public/internal/static/readonly) are excluded.
    local_pattern = re.compile(
        r"^\+\s+(?!private|protected|public|internal|static|readonly|const|override|virtual|abstract|extern|partial|sealed|new\s)"
        r"(?:[A-Za-z][A-Za-z0-9.<>\[\]?,\s]*?\s+)(_[a-z][A-Za-z0-9]*)\s*[=;,)]"
    )
    result = run(["git", "diff", f"{base}...HEAD", "--unified=0", "--", "src/"])
    if result.returncode != 0:
        return []
    current_file = ""
    current_line = 0
    for raw_line in result.stdout.splitlines():
        if raw_line.startswith("+++ b/"):
            current_file = raw_line[6:]
            continue
        if raw_line.startswith("@@"):
            # Parse new-file line number: @@ -a,b +c,d @@
            m = re.search(r"\+(\d+)", raw_line)
            current_line = int(m.group(1)) - 1 if m else 0
            continue
        if raw_line.startswith("+"):
            current_line += 1
            stripped = raw_line[1:].lstrip()
            if stripped.startswith("//") or stripped.startswith("*"):
                continue
            m = local_pattern.match(raw_line)
            if m:
                violations.append(
                    f"  {current_file}:{current_line} -- underscore local '{m.group(1)}' (use camelCase)"
                )
        elif not raw_line.startswith("-"):
            current_line += 1
    return violations


def check_diff_char_count(base: str) -> tuple[int, int, list[str], list[str]]:
    """
    Returns (raw_chars, stripped_chars, blocking_violations, warnings).
    BLOCKING if stripped_chars >= SOURCERY_CHAR_LIMIT (whitespace-only diffs
    from CSharpier/extraction should not trigger the gate).
    WARNING  if raw_chars >= WARN_CHAR_LIMIT (surface total size for visibility).
    """
    raw_count, stripped_count = get_diff_char_count(base)
    blocking = []
    warnings = []
    if stripped_count >= SOURCERY_CHAR_LIMIT:
        pct = stripped_count / SOURCERY_CHAR_LIMIT * 100
        blocking.append(
            f"  Diff (stripped) is {stripped_count:,} chars ({pct:.0f}% of {SOURCERY_CHAR_LIMIT:,} limit) -- "
            f"Sourcery will SKIP this PR. Split into smaller PRs or remove unreachable code changes."
        )
    elif raw_count >= WARN_CHAR_LIMIT:
        pct = raw_count / SOURCERY_CHAR_LIMIT * 100
        warnings.append(
            f"  Diff is {raw_count:,} raw chars ({pct:.0f}% of {SOURCERY_CHAR_LIMIT:,} Sourcery limit, "
            f"{stripped_count:,} stripped) -- consider splitting if whitespace is significant."
        )
    return raw_count, stripped_count, blocking, warnings


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
def main() -> int:
    parser = argparse.ArgumentParser(description="Wave 7 pre-push quality gate")
    parser.add_argument("--base", default="origin/main", help="Base ref to diff against")
    parser.add_argument("--files", nargs="*", help="Explicit file list (skips git diff for file detection)")
    args = parser.parse_args()

    base = args.base
    files = args.files if args.files else get_modified_src_files(base)

    if not files:
        print("wave7_prepush_gate: no src/ changes detected vs " + base)
        return 0

    print(f"wave7_prepush_gate: checking {len(files)} modified src/ file(s) vs {base}\n")

    all_blocking: list[str] = []
    all_warnings: list[str] = []

    # -- Check 1: ASCII only --
    v = check_ascii_only(files)
    if v:
        print("[FAIL] Check 1 -- ASCII-only:")
        all_blocking.extend(v)
        for msg in v:
            print(msg)
    else:
        print("[PASS] Check 1 -- ASCII-only")

    # -- Check 2: DateTime.Now (diff-only) --
    v = check_datetime_now(base)
    if v:
        print("[FAIL] Check 2 -- DateTime.Now usage (introduced in this diff):")
        all_blocking.extend(v)
        for msg in v:
            print(msg)
    else:
        print("[PASS] Check 2 -- DateTime.Now (none introduced)")

    # -- Check 3: lock() (diff-only) --
    v = check_lock_usage(base)
    if v:
        print("[FAIL] Check 3 -- lock() usage:")
        all_blocking.extend(v)
        for msg in v:
            print(msg)
    else:
        print("[PASS] Check 3 -- lock() (none found)")

    # -- Check 4: underscore locals (diff-only, not pre-existing) --
    v = check_underscore_locals(base)
    if v:
        print("[FAIL] Check 4 -- underscore local variables:")
        all_blocking.extend(v)
        for msg in v:
            print(msg)
    else:
        print("[PASS] Check 4 -- underscore locals (none found)")

    # -- Check 5+6: diff char count --
    raw_chars, stripped_chars, blocking, warnings = check_diff_char_count(base)
    if blocking:
        print(f"[FAIL] Check 5 -- diff size ({raw_chars:,} raw / {stripped_chars:,} stripped chars):")
        all_blocking.extend(blocking)
        for msg in blocking:
            print(msg)
    elif warnings:
        print(f"[WARN] Check 5 -- diff size ({raw_chars:,} raw / {stripped_chars:,} stripped chars):")
        all_warnings.extend(warnings)
        for msg in warnings:
            print(msg)
    else:
        print(
            f"[PASS] Check 5 -- diff size "
            f"({raw_chars:,} raw / {stripped_chars:,} stripped, under {SOURCERY_CHAR_LIMIT:,} limit)"
        )

    # -- Summary --
    print()
    if all_warnings:
        print("WARNINGS:")
        for w in all_warnings:
            print(w)
        print()

    if all_blocking:
        print(f"GATE FAILED -- {len(all_blocking)} blocking violation(s). Fix before pushing.")
        return 1

    print("GATE PASSED. Ready to push.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
