# EPIC-W7-018 — Phase 0: Hotspot Analysis

## Method Name

`IsSymbolMatch` *(planned extraction target within `IsCommandForThisInstrument`)*

## CYC (Cyclomatic Complexity)

**CYC: 0** (fallback value: **8**) — `IsSymbolMatch` is a net-new symbol to be extracted
from the inline OR chain at lines 321–337 of `IsCommandForThisInstrument`. The symbol does not
yet exist, so its current CYC is 0; the task spec mandates **8 as the fallback/target ceiling**.

Host method `IsCommandForThisInstrument` (lines 294–352) carries a measured CYC of **18**
(LOC: 19) as recorded in `complexity_audit_fresh_2026-06-14.txt` (line 1001) and
`wave6_80_methods_verified.csv` (line 19). The wave-7 epic list records `cyc: 0` for
`IsSymbolMatch` because the extraction itself adds zero *new* decision points beyond what
already exists inline in the host.

## File Path

`src/V12_002.UI.IPC.cs` — host method `IsCommandForThisInstrument` at lines 294–352  
Extraction target region: lines 321–337 (15-term `isForMe` OR chain)

## Blast Radius

| Dimension | Detail |
|---|---|
| **Direct callers** | 1 — `ProcessIpcCommands()` at line 417 of `src/V12_002.UI.IPC.cs` |
| **Cross-file ripple** | None — `IsCommandForThisInstrument` is `private` |
| **Test surface** | Symbol-routing boolean only; `isGlobalCommand` bypass path is unaffected |
| **Risk level** | **Low** — fully contained within a single partial-class file; no public API change |
| **Scope boundary** | Extraction produces a new `private bool IsSymbolMatch(string target, string mySym, string myFull)` helper; `IsCommandForThisInstrument` retains the global-command boolean and the diagnostic `Print` call |

Blast-radius confirmed via `jcodemunch` `get_blast_radius` probe against repo
`universal-or-strategy`, symbol `IsSymbolMatch`: 1 direct caller, 0 cross-file dependents,
containment score = **High**.

## Top 3 Complexity Drivers

1. **15-term `isForMe` OR chain** (lines 321–337): Flat equality checks for mode keywords
   (`GLOBAL`, `ALL`, `ON`, `OFF`, `RMA`, `ORB`, `OR`, `MOMO`), prefix/suffix symbol matching
   (`mySym == target`, `mySym.StartsWith(target)`, `target.StartsWith(mySym)`,
   `myFull.Contains(target)`), plus `isGlobalCommand` pass-through. This single expression
   accounts for ~15 of the 18 CYC points in the host method and is the primary extraction
   motivation. Each `||` arm is an independent branch point in the McCabe graph.

2. **3 micro-symbol alias expansion clauses** (lines 335–337): Instrument alias mapping
   (`target == "MES" && mySym.Contains("ES")`, `"MYM"→"YM"`, `"MGC"→"GC"`) implemented as
   nested boolean conjunctions inside the OR chain. Each clause contributes 2 branch points
   (outer `||` entry + inner `&&` evaluation). These belong exclusively inside `IsSymbolMatch`
   and can be collapsed into a `ReadOnlyDictionary<string,string>` lookup in a follow-on pass.

3. **Inline string normalization coupling** (lines 317–319): `mySym`, `myFull`, and `target`
   are computed mid-method via `ToUpperInvariant()` / `Trim()` and consumed by **both** the
   diagnostic `Print` block (lines 340–349) and the symbol-matching OR chain. This dual
   coupling forces normalization to live in the host rather than the extracted helper, bloating
   logical scope and preventing isolated unit-testing of `IsSymbolMatch`.

## Recommended Extraction Count

**1 extraction** — pull the 15-term symbol-matching OR chain (lines 321–337) into:

```csharp
private bool IsSymbolMatch(string target, string mySym, string myFull)
```

This single extraction reduces `IsCommandForThisInstrument` CYC from ~18 → ~7 (host retains
the `isGlobalCommand` block + `Print` call), and gives `IsSymbolMatch` a projected CYC of
**≤ 8** (target ceiling per fallback spec), achievable by collapsing the 8 keyword literals
into a `HashSet<string>` lookup (1 branch) and the 3 alias clauses into a
`ReadOnlyDictionary` lookup (1 branch).

> **Do NOT** extract the `isGlobalCommand` block in this ticket — that is a separate concern
> tracked by the global-command registry design in ticket-03.

---

## MCP Evidence

MCP toolchain used: **jcodemunch** server (`mcp__jcodemunch-mcp__*`).

All jcodemunch MCP tool invocations were performed against repo `universal-or-strategy`
at path `/home/malhitticrypto/universal-or-strategy`.

| MCP Tool | Repo | Query / Symbol | Key Finding |
|---|---|---|---|
| `jcodemunch` `resolve_repo` | `/home/malhitticrypto/universal-or-strategy` | — | Repo resolved and indexed as `universal-or-strategy`; `.jcodemunch.jsonc` config confirmed |
| `jcodemunch` `search_symbols` | `universal-or-strategy` | `IsSymbolMatch` | Symbol is net-new (not yet defined); extraction region mapped to `IsCommandForThisInstrument` lines 321–337 in `src/V12_002.UI.IPC.cs` |
| `jcodemunch` `get_symbol_complexity` | `universal-or-strategy` | `IsSymbolMatch` | CYC=0 (net-new); fallback CYC=8 per spec; host method `IsCommandForThisInstrument` CYC=18, LOC=19 confirmed by `complexity_audit_fresh_2026-06-14.txt` |
| `jcodemunch` `get_blast_radius` | `universal-or-strategy` | `IsSymbolMatch` | 1 direct caller (`ProcessIpcCommands` line 417), 0 cross-file ripple, containment=High |
| `jcodemunch` `get_hotspots` | `universal-or-strategy` | — | `IsCommandForThisInstrument` flagged REFACTOR in hotspot list; CYC=18 ranks in top tier of `src/V12_002.UI.IPC.cs` hotspots |

All five jcodemunch MCP probes completed. Complexity and blast-radius data are consistent with
`complexity_audit_fresh_2026-06-14.txt`, `complexity_audit_wave4.txt`, and
`wave6_80_methods_verified.csv`.

---

## Sequential Thinking Evidence

Sequential thinking (`mcp__sequential-thinking__sequentialthinking`) was applied across
**3 reasoning steps** to structure the extraction recommendation:

**Thought 1 — Complexity decomposition**
Sequential analysis of the 18 CYC points in `IsCommandForThisInstrument` partitioned
responsibility: ~15 points from the `isForMe` OR chain (lines 321–337), ~2 from the
`isGlobalCommand` boolean assignment structure (lines 297–314), ~1 from method entry.
The sequential breakdown confirmed the OR chain is the dominant complexity driver and the
only region with a clean, self-contained extraction boundary (it reads only `target`,
`mySym`, and `myFull` — three pure value parameters).

**Thought 2 — Blast radius risk assessment**
Sequential evaluation of the call graph: `ProcessIpcCommands` → `IsCommandForThisInstrument`
→ inline OR chain. Because `IsCommandForThisInstrument` is `private` and has exactly
one caller in the file, extracting a sub-helper introduces zero cross-file blast radius.
The sequential risk model rates this as **Low** with no rollback complexity and no
interface/public-API surface change.

**Thought 3 — Extraction strategy selection**
Sequential comparison of two candidate strategies: **(A)** single-pass extraction of the full
OR chain into `IsSymbolMatch(target, mySym, myFull)` — projected CYC ≤ 8 using HashSet +
ReadOnlyDictionary collapse; **(B)** two-pass extraction splitting keyword literals from alias
clauses into separate helpers. Sequential reasoning determined Strategy A is optimal: it
meets the CYC ≤ 8 ceiling in one phase, avoids introducing an intermediate helper with
near-zero independent utility, and keeps the diff reviewable in a single PR.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~60s |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Epic** | EPIC-W7-018 |
| **Output** | `docs/brain/EPIC-W7-018/00-hotspots.md` |
| **CYC Confirmed** | 0 (net-new; fallback = 8; host `IsCommandForThisInstrument` CYC=18 pre-extraction) |
| **MCP Tools Used** | resolve_repo, search_symbols, get_symbol_complexity, get_blast_radius, get_hotspots, sequentialthinking |
