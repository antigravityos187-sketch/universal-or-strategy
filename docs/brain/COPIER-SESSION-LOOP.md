# PTT Copier Session Loop — Perpetual $continue Protocol

**Version**: 1.0
**Created**: 2026-09-01
**Purpose**: Every copier-spec session begins with a $continue prompt.
Each session validates pipeline output, runs SIM gates, updates the spec,
provides the next batch of ptt-orchestrator prompts, and ALWAYS ends by
providing the next $continue prompt — so the Director never has to ask for it.

---

## Loop Structure (every session does this in order)

```
SESSION START:   paste $continue prompt
  STEP 0:  Rules Catalog Gate (always mandatory)
  STEP 1:  Validate pipeline output from previous block (read source + verify docs)
  STEP 2:  Sync + F5 (ptt-sync-and-verify.ps1 + NinjaTrader recompile)
  STEP 3:  Update spec (close DW items, add pipeline section, update safety)
  STEP 4:  SIM gates (Director runs in NT8, reports results back)
  STEP 5:  Final spec updates (post-SIM gate results, deferred cards)
  STEP 6:  Provide next pipeline prompts (ptt-orchestrator prompts, parallel or serial)
  STEP 7:  Provide next $continue prompt  <-- always generated, never skipped
SESSION END:     Director has next $continue ready to paste when pipelines finish
```

---

## Loop Termination Criteria

The loop ends when ALL of the following are true:

1. **P0 + P1 defects**: zero open P0 or P1 DW items in the spec
2. **Deferred gates**: all deferred SIM gates resolved or explicitly archived
   (Director sign-off required to archive a deferred gate permanently)
3. **Complexity refactor wave**: autonomous-refactor wave complete —
   all CopyEngine.cs methods CYC <= 8 (Jane Street strict standard)
4. **Stable validation**: 5 consecutive live trading sessions with COPY ON,
   no new defects logged, no unexpected follower behavior observed
5. **Spec marked STABLE**: safety section updated to SAFE (no caveats),
   spec version bumped to STABLE in the header

Until all 5 conditions are true: the loop continues. A $continue prompt
is always provided at the end of every session.

---

## Loop Phase Map (current as of 2026-09-01)

```
PHASE 1 — Active defect fix (P1 items)         [CURRENT]
  B131: DW-B138 (Stop1 drag) + DW-B139 (multi PTT-TGT-Drag)
  B131 SIM: drag fix validation + DW-B134-OCO observation

PHASE 2 — Residual defect fix (conditional)    [NEXT]
  B132 LaneA: DW-B134-OCO fix (if orphan confirmed in B131 Test C)
  B132 LaneB: any new defects discovered in B131 SIM

PHASE 3 — Backlog P1 items (when copier stable) [QUEUED]
  DW-B115: ATM target-qty distribution mismatch
  DW-B120: partial ATM arm on late-filling followers
  DW-B126: BE/QX race condition
  DW-B127: QX fires on position entry transition
  DW-B131: bracket-event storm (P2 — lower priority)

PHASE 4 — Complexity refactor wave              [QUEUED — after Phase 2/3]
  autonomous-refactor: CopyEngine.cs all methods CYC <= 8
  Many methods above CYC 8: OnOrderUpdate, TryCopyEntry,
  SyncAtmFollowerBracket, HandleBracketChange, etc.

PHASE 5 — Stable validation                    [FINAL]
  5 consecutive live sessions, COPY ON, zero defects
  Spec marked STABLE. Loop ends.
```

---

## $continue Prompt Template

The $continue prompt is parameterized. The session that provides it fills in:
- {BLOCK_JUST_COMPLETED}: e.g. B131
- {LANES_COMPLETED}: e.g. "LaneA: DW-B138, LaneB: DW-B139"
- {DW_FIXED}: e.g. "DW-B138 P1, DW-B139 P1"
- {DW_OPEN_P0P1}: current open P0/P1 items remaining after this block
- {DEFERRED_CARRY}: deferred tests carrying forward
- {PHASE}: current loop phase (1/2/3/4/5)

See the bottom of every session output for the filled-in $continue prompt
for the next session.

---

## SRC CODE BAN (permanent — all sessions)

No .cs edits in any copier-spec session.
All .cs changes go through the FULL 5-PHASE PTT PIPELINE:
  Ph1  ptt-architect       -> 02-architecture-plan.md
  Ph2  ptt-plan-reviewer   -> 02-plan-review.md       (REVIEW_PASS gate)
  Ph3  ptt-architect       -> 04-tickets.md
  Ph3.5 ptt-ticket-reviewer -> 04-ticket-review.md   (TICKET_REVIEW_PASS gate)
  Ph4a ptt-engineer        -> src .cs edits + ticket-N-completion.md
  Ph4b ptt-verifier        -> ticket-N-verification.md (VERIFY_PASS gate)
  Ph5  ptt-plan-reviewer   -> 05-final-review.md + 06-deferred-backlog.md

"add it" = add to spec HTML only. Never edit src code.

---

## Key Source Locations (stable reference — update if methods move)

```
CopyEngine.cs
  OnOrderUpdate            ~L1296  : entry point for all order events
  TryHandleDrag            ~L1734  : Gate B+C drag routing
  TryHandleBracketDrag     ~L1720  : Gate B — bracket drag
  IsWorkingBracket         ~L2083  : state guard for bracket detection
  IsBracketLegStatic       ~L3800  : name predicate for bracket orders
  IsAtmSTPOrder            ~L2107  : name predicate for ATM STP/Stop/Target
  HandleBracketChange      ~L2315  : fan-out to followers for bracket drag
  SyncFollowerBracket      ~L2131  : per-follower bracket sync
  FindFollowerBracketOrder ~L2339  : locate follower bracket by FromEntrySignal
  SyncAtmFollowerBracket   ~L2202  : cancel+resubmit for ATM stop brackets
  SyncAtmFollowerTarget    ~L2262  : cancel+resubmit for ATM target brackets
  TryCancelFollowerEntries ~L1638  : leader cancel propagation (scoped by order ID)
  MoveStopToBreakEven      ~L3922  : BE-ALL step A — calls SnapshotBeTargets
  SnapshotBeTargets        (new)   : filters stale PTT-BE-Target-* orders

specs/002-trade-copier-spec.html  : primary spec
docs/brain/B{N}/                  : per-block pipeline artifacts
docs/standards/NT8_FULL_REFERENCE.md     : NT8 API reference (102.7 KB)
docs/standards/NT8_ADDON_KNOWLEDGE.md   : AddOn-specific constraints (37.3 KB)
docs/standards/jane-street/RULES_CATALOG.md : JS-001..JS-110
```

---

## NT8 Key Facts (do not re-investigate)

- AtmStrategyChangeStopTarget() — StrategyBase-only. NOT AddOnBase.
- AtmStrategyCreate() — StrategyBase-only. NOT AddOnBase.
- Account.Change() — AddOnBase-available but silent no-op on ATM-owned brackets.
- Account.Cancel() + Account.CreateOrder() + Submit() — AddOnBase correct pattern.
- ATM bracket Order.Name = template name ("Stop1", "Target3" etc.).
- ATM bracket drag fires OnOrderUpdate with OrderState.ChangeSubmitted -> Working.
- NT8 Close button fires Order.Name = "Close".
- Copier resets to COPY OFF on every NT8 restart (by design, not a bug).

---

## Dev Setup (stable reference)

```
%NT8UserDataDir%\PropTraderTools\dev_mode.txt
%NT8UserDataDir%\PropTraderTools\license.txt       (key = "DEV")
%NT8UserDataDir%\PropTraderTools\license_cache.json (Elite features cached)
Session startup: select chart -> check follower boxes -> click COPY ON
SIM accounts: Sim101 (leader), Sim102/103/104 (followers)
ATM template: MES $200 SL 6 (bracket names: Stop1/Stop2/Stop3, Target1/Target2/Target3)
```

---

## Sync + F5 Gate (mandatory after every pipeline)

```powershell
powershell -File scripts\ptt-sync-and-verify.ps1
# Gate: 0 MISMATCH
# Then: F5 in NinjaTrader 8 -> confirm green compile
# Then: dotnet test --filter B{N} -> all tests green
```
