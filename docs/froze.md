Previous conversation summary:

Goal
Build and maintain the PTT Trade Copier spec (specs/002-trade-copier-spec.html) for NinjaTrader 8, and design/orchestrate new features through the PTT pipeline. Current active goals:

B50 — Three parallel lanes ready to run (prompts not yet written — user asked for them but session ended before delivery)
Clone mode — new third copy mode: auto-inherits leader's live ATM template at fill time + bracket drag sync (HandleBracketChange) like Signal mode
BE button color fix — BE +1 and BE ALL should use same teal color as Quick buttons
CopyEngineTests.cs cleanup — DW-B48-01: 60 compile errors blocking dotnet test runner
Live F5 acceptance test — DW-B46-01: Director needs to F5 with B49 binary and run D1–D5 test
Instructions
SRC CODE BAN: Strategy-director mode. BANNED from editing any .cs file directly. All C# src edits go through ptt-orchestrator → ptt-architect → ptt-engineer. "Add it" = spec HTML only.
Jane Street rules apply to all coding decisions (CYC ≤ 8, no lock(), no async void, no return null, xUnit only)
NT8 compiler rules: docs/standards/NT8_COMPILER_RULES.md — must be read before any .cs work
Wave workspace: C:\WSGTA\universal-or-strategy\src\PropTraderTools\
Spec file: specs/002-trade-copier-spec.html
Brain dirs: docs/brain/B##-Lane#/
MCP routing: ctx_read not read_file, ctx_shell not execute_command for output-bearing commands
Tests must go in src/PropTraderTools/Tests/ subfolder (never flat root) — NT8-054 convention per B48
Discoveries
Competitive Analysis (Affordable Indicators)
Their Executions Mode = copy on fill → market order on follower. No brackets. ≈ PTT Mirror mode
Their Orders Mode = copy order structure + ATM spawns on follower. BUT: all followers locked to same bracket prices as leader at all times. One fixed ATM template for all followers.
PTT advantage: per-follower independent ATM template (Signal mode). Clone mode (incoming) = their Orders Mode + auto template inheritance — doesn't exist in their product.
Mode Matrix (FINAL — locked this session)
Signal	Mirror	Clone (new)
Entry copy	✅ on Submitted	✅ on Submitted	✅ on Submitted
ATM	✅ per-follower dropdown	✅ per-follower dropdown	❌ hidden — auto leader template
Stop/target drag sync	✅ HandleBracketChange	❌ no	✅ HandleBracketChange
Exit	Own bracket fills	Market order on leader bracket fill	Own bracket fills
Multiplier	❌ removed	❌ removed	❌ removed
Multiplier removed from all modes — sizing handled by ATM template per account
ATM combo hidden in Clone mode (auto), visible in Signal + Mirror
Clone = Option B — drag sync included (Director confirmed)
Button Colors (exact values from source)
BE +1: Background = BrushInactive (grey fill) — wrong, needs teal border like Quick
BE ALL: BorderBrush = BrushPurple RGB(168,85,247) — wrong, needs teal
Quick +4 / Quick ALL: BorderBrush = MakeBrush(13,148,136) teal, Foreground same, no background fill — this is the target style for BE buttons too
Fix = give BE +1 and BE ALL the same BorderBrush + Foreground = MakeBrush(13,148,136), BorderThickness = new Thickness(2), remove filled background
Clone Mode Architecture
New CopyMode.Clone = 2 in enum
Dispatch path: same as Signal (DispatchCopy + HandleBracketChange)
ATM injection: cache GetLeaderAtmTemplateName() on UI thread into volatile string field → DispatchCopy injects as Named(cachedTemplate) for all followers when Clone mode active
GetLeaderAtmTemplateName() already exists in TradeCopierPanel.cs — reads ChartTrader visual tree
PTTFollowerStrategy.CallAtmStrategyCreate already handles Named mode via FillSignalEventArgs.AtmTemplateName
ATM combo: hide (Visibility.Collapsed) per-follower when _copyModeValue == Clone; show when Signal/Mirror
HandleBracketChange — already works
Signal mode already syncs leader bracket drag → all follower brackets move to same price
Mirror mode does NOT sync bracket drags (intentional — market order exit only)
Clone mode will use same HandleBracketChange path as Signal (Option B)
CopyEngineTests.cs 60 errors (DW-B48-01)
Errors: CS0246 CopyRule (private nested type), CS0234 System.Collections.Immutable, CS0433 Globals ambiguous, CS0246 DisarmTrailBe
File lives at flat root: src/PropTraderTools/CopyEngineTests.cs — stays at root (private type access requirement)
Does NOT affect NT8 F5 (file is in $DeployExcludes)
Blocks dotnet test runner for all B42–B49 tests
B49 completed this session
Layout reorder: BE/Quick rows top → Copier (Mode row inside) → status → Position Tools bottom
PttBuild.Tag = "PTT-COPIER B49 | layout-reorder | 2026-08-08"
All 7 scans PASS, FINAL_PASS confirmed
B48 compiled error (fixed this session)
B46Tests.cs was hard-linked in NT8 bin — manually deleted. verify_links.ps1 confirms DESYNC=0 MISSING=0 SKIPPED=7
Accomplished
Completed this session
 Diagnosed 2026-08-07 03:25 PM compilation errors — B46Tests.cs stale hard-link, deleted it
 Confirmed B47-LaneC FINAL_PASS (9 xUnit tests), B48-LaneA FINAL_PASS (test isolation)
 Confirmed B49-LaneA FINAL_PASS (layout reorder + Clone radio button layout spec)
 Updated spec: B47 pills → green PIPELINE_COMPLETE
 Added B48 block to spec (DW-B44-01 NT8 F5 closure, 2026-08-07 forensics)
 Added B49 block to spec (layout reorder table, files changed)
 Full competitive analysis of Affordable Indicators copier
 Designed Clone mode architecture (engine + UI)
 Locked final mode matrix (Signal/Mirror/Clone, multiplier removed)
 Identified BE button color fix needed
 Read full deferred backlog from B49-LaneA/06-deferred-backlog.md
In progress (session ended before delivery)
 B50 orchestrator prompts NOT YET WRITTEN — user asked, session ended
 $continue validation prompt NOT YET WRITTEN
B50 lanes planned (prompts needed)
B50-LaneA — Clone mode (engine + UI, complex)

Add CopyMode.Clone = 2 to enum
Cache GetLeaderAtmTemplateName() as _cloneAtmCache volatile string on UI thread
DispatchCopy: inject Named(_cloneAtmCache) when Clone mode
OnOrderUpdate Clone path: same as Signal (DispatchCopy + HandleBracketChange)
BuildModeRow: add Clone radio button (_cloneModeBtn)
OnCloneModeClick: CopyEngine.Instance.SetCopyMode(CopyMode.Clone) + show/hide ATM combos
ATM combo visibility: hide when Clone, show when Signal/Mirror
Mode switch handlers (Signal/Mirror click): restore ATM combo visibility
New B50Tests.cs in Tests\ subfolder
B50-LaneB — BE button color fix (UI only, simple)

_beBtn2: remove Background = BrushInactive, add BorderBrush = MakeBrush(13,148,136), Foreground = MakeBrush(13,148,136), BorderThickness = new Thickness(2)
_globalBeBtn2: change BorderBrush and Foreground from BrushPurple → MakeBrush(13,148,136) teal
UpdateButtonColors + UpdateBeVisuals: update BE state color logic to use border/foreground changes (not background fills)
Build tag update
B50-LaneC — CopyEngineTests.cs 60-error fix (DW-B48-01)

Fix CS0246 CopyRule private type access
Fix CS0234 System.Collections.Immutable (NT8-004: use Dictionary instead)
Fix CS0433 Globals ambiguity
Fix CS0246 DisarmTrailBe
Confirm dotnet test runs green after fix
B50-LaneD — Small cleanup pass (P2, optional parallel)

DW-B42-04: PttContracts.cs line 254 // NT8-NEW → // NT8-005
DW-B47-04: Add T_B47_05 (IsFollowerAccount_ReturnsFalse_WhenNoRules) to B47Tests.cs
DW-B47-05: FindRule return null → JS-002 compliant Option type or named sentinel
Relevant files / directories
Spec (director workspace)
specs/002-trade-copier-spec.html — main spec, updated through B49 this session
docs/brain/B49-LaneA/06-deferred-backlog.md — authoritative open items list
Wave workspace source
C:\WSGTA\universal-or-strategy\src\PropTraderTools\CopyEngine.cs — core engine: CopyMode enum (line 87), _copyModeValue (line 108), SetCopyMode (line 337), DispatchCopy (line 556), HandleBracketChange (line 694), SendCopy (line 815), FollowerAtmMode (line 73), GetLeaderAtmTemplateName exists in Panel
C:\WSGTA\universal-or-strategy\src\PropTraderTools\TradeCopierPanel.cs — UI: BuildModeRow (line 1386), BuildCopierSection (line 1691), GetLeaderAtmTemplateName (line 1987), BuildCheckItemTemplate (line 1806), _signalModeBtn/_mirrorModeBtn (line 195-196), brush definitions (lines 234-273), UpdateButtonColors (line 553), _beBtn2 construction (line 939), _globalBeBtn2 construction (line 962), Quick buttons (lines 995-1030)
C:\WSGTA\universal-or-strategy\src\PropTraderTools\Features\PttFollowerStrategy.cs — OnFillSignal, CallAtmStrategyCreate
C:\WSGTA\universal-or-strategy\src\PropTraderTools\Features\PttBreakEven.cs — Execute(IPttHostContext ctx), reads ctx.BeBuffer (NOT ATR engine)
C:\WSGTA\universal-or-strategy\src\PropTraderTools\Features\PttGlobalBreakEven.cs
C:\WSGTA\universal-or-strategy\src\PropTraderTools\Features\PttGlobalQuickExit.cs
C:\WSGTA\universal-or-strategy\src\PropTraderTools\Tests\B42–B47Tests.cs — all in Tests\ subfolder
C:\WSGTA\universal-or-strategy\src\PropTraderTools\CopyEngineTests.cs — flat root, 60 errors (DW-B48-01)
C:\WSGTA\universal-or-strategy\scripts\verify_links.ps1 — $DeployExcludes array + Layer 1 Tests\ skip
Standards / knowledge
docs/standards/NT8_COMPILER_RULES.md
docs/standards/NT8_ADDON_KNOWLEDGE.md — B48 section (Tests\ convention)
docs/standards/jane-street/RULES_CATALOG.md
docs/affordable indicators/ — competitor analysis (read this session)