# Node Description Batch 39 of 61

Graphify is running in assistant/skill mode (no API key). You are the host
assistant (Claude Code / Codex / Gemini CLI). Read the prompt below and write
your JSON answer to the answer file.

## Prompt

You are documenting nodes in a knowledge graph.
For each entry below, write ONE concise factual plain-language sentence
describing what it is or does. Use only the provided context.
For a code symbol (kind=code-symbol — a function, class, or constant),
describe what the function/symbol does based on its name, source location
and neighbors — e.g. "Resolves the configured ontology profile from graphify.yaml.".
No marketing language.
Respond ONLY with a JSON object mapping each node id (as a string) to its
one-sentence description — no prose, no markdown fences.

- "scripts_fix_phase_modes_main": "main()" | kind=code-symbol | source=scripts/fix_phase_modes.py:L72 | neighbors=[fix_phase_modes.py, fix_manifest()]
- "scripts_fix_phase0_status_fix_epic": "fix_epic()" | kind=code-symbol | source=scripts/fix_phase0_status.py:L12 | neighbors=[fix_phase0_status.py, main()]
- "scripts_fix_phase0_status_main": "main()" | kind=code-symbol | source=scripts/fix_phase0_status.py:L52 | neighbors=[fix_phase0_status.py, fix_epic()]
- "scripts_fix_phase1_outputs_main": "main()" | kind=code-symbol | source=scripts/fix_phase1_outputs.py:L51 | neighbors=[fix_phase1_outputs.py, fix_manifest()]
- "scripts_fix_synthetic_events_main": "main()" | kind=code-symbol | source=scripts/fix_synthetic_events.py:L40 | neighbors=[fix_synthetic_events.py, fix_event_log()]
- "scripts_generate_phase2_scripts_with_real_keys_main": "main()" | kind=code-symbol | source=scripts/generate_phase2_scripts_with_real_keys.py:L84 | neighbors=[generate_phase2_scripts_with_real_keys.…, generate_phase2_script()]
- "scripts_generate_report_main": "main()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/generate_report.py:L304 | neighbors=[generate_report.py, generate_html()]
- "scripts_get_linear_team_id": "get_linear_team_id.py" | kind=code-symbol | source=scripts/get_linear_team_id.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…]
- "scripts_get_next_epics_get_next_pending_epics": "get_next_pending_epics()" | kind=code-symbol | source=scripts/get_next_epics.py:L6 | neighbors=[get_next_epics.py, Get next N pending epics from epic_road…]
- "scripts_harden_agents": "harden_agents.py" | kind=code-symbol | source=scripts/harden_agents.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…]
- "scripts_improve_description_main": "main()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/improve_description.py:L194 | neighbors=[improve_description.py, improve_description()]
- "scripts_init": "__init__.py" | kind=code-symbol | source=scripts/__init__.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "scripts_lamport_clock_deterministicworkflow_init": ".__init__()" | kind=code-symbol | source=scripts/lamport_clock.py:L35 | neighbors=[DeterministicWorkflow, ._load_global_clock()]
- "scripts_langsmith_bridge_main": "main()" | kind=code-symbol | source=scripts/langsmith_bridge.py:L46 | neighbors=[langsmith_bridge.py, trace_agent_handoff()]
- "scripts_langsmith_bridge_trace_forensic_run": "trace_forensic_run()" | kind=code-symbol | source=scripts/langsmith_bridge.py:L32 | neighbors=[langsmith_bridge.py, Traces an AMAL forensic run and attache…]
- "scripts_linear_sync_v2_linearissue": "LinearIssue" | kind=code-symbol | source=scripts/linear_sync_v2.py:L33 | neighbors=[linear_sync_v2.py, Represents a Linear issue to be created…]
- "scripts_nexus_relay": "nexus_relay.py" | kind=code-symbol | source=scripts/nexus_relay.py:L1 | neighbors=[main(), relay_to_agent()]
- "scripts_nexus_relay_main": "main()" | kind=code-symbol | source=scripts/nexus_relay.py:L40 | neighbors=[nexus_relay.py, relay_to_agent()]
- "scripts_orchestrate_full_epic_execution_epicwaveorchestrator_load_roadmap": "._load_roadmap()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L118 | neighbors=[EpicWaveOrchestrator, .__init__()]
- "scripts_orchestrate_phase_execution_phaseorchestrator_init": ".__init__()" | kind=code-symbol | source=scripts/orchestrate_phase_execution.py:L32 | neighbors=[PhaseOrchestrator, ._load_roadmap()]
- "scripts_orchestrate_phase_execution_phaseorchestrator_load_roadmap": "._load_roadmap()" | kind=code-symbol | source=scripts/orchestrate_phase_execution.py:L35 | neighbors=[PhaseOrchestrator, .__init__()]
- "scripts_package_skill_main": "main()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/package_skill.py:L111 | neighbors=[package_skill.py, package_skill()]
- "scripts_phase_0_hotspot_mcp_fastmcp_execute_phase_0": "execute_phase_0()" | kind=code-symbol | source=scripts/phase_0_hotspot_mcp_fastmcp.py:L22 | neighbors=[phase_0_hotspot_mcp_fastmcp.py, Execute Phase 0 (Hotspot Analysis) for …]
- "scripts_phase_0_hotspot_mcp_list_tools": "list_tools()" | kind=code-symbol | source=scripts/phase_0_hotspot_mcp.py:L24 | neighbors=[phase_0_hotspot_mcp.py, List available MCP tools]
- "scripts_phase_1_5_boundary_mcp_execute_phase_1_5": "execute_phase_1_5()" | kind=code-symbol | source=scripts/phase_1_5_boundary_mcp.py:L13 | neighbors=[phase_1_5_boundary_mcp.py, Execute Phase 1.5 (Scope Boundary Valid…]
- "scripts_phase_1_scope_mcp_call_tool": "call_tool()" | kind=code-symbol | source=scripts/phase_1_scope_mcp.py:L52 | neighbors=[phase_1_scope_mcp.py, execute_phase_1_tool()]
- "scripts_phase_1_scope_mcp_fastmcp_execute_phase_1": "execute_phase_1()" | kind=code-symbol | source=scripts/phase_1_scope_mcp_fastmcp.py:L14 | neighbors=[phase_1_scope_mcp_fastmcp.py, Execute Phase 1 (Scope Definition) for …]
- "scripts_phase_1_scope_mcp_list_tools": "list_tools()" | kind=code-symbol | source=scripts/phase_1_scope_mcp.py:L27 | neighbors=[phase_1_scope_mcp.py, List available Phase 1 tools.]
- "scripts_phase_2_architecture_mcp_execute_phase_2": "execute_phase_2()" | kind=code-symbol | source=scripts/phase_2_architecture_mcp.py:L13 | neighbors=[phase_2_architecture_mcp.py, Execute Phase 2 (Architecture Planning)…]
- "scripts_phase_3_audit_mcp_execute_phase_3": "execute_phase_3()" | kind=code-symbol | source=scripts/phase_3_audit_mcp.py:L13 | neighbors=[phase_3_audit_mcp.py, Execute Phase 3 (DNA & PR Audit) for an…]
- "scripts_phase_4_5_ticket_review_mcp_validate_complexity_targets": "validate_complexity_targets()" | kind=code-symbol | source=scripts/phase_4_5_ticket_review_mcp.py:L96 | neighbors=[phase_4_5_ticket_review_mcp.py, Validate that complexity targets are re…]
- "scripts_phase_4_5_ticket_review_mcp_validate_ticket_scope": "validate_ticket_scope()" | kind=code-symbol | source=scripts/phase_4_5_ticket_review_mcp.py:L71 | neighbors=[phase_4_5_ticket_review_mcp.py, Validate that tickets respect single-me…]
- "scripts_phase_4_tickets_mcp_execute_phase_4": "execute_phase_4()" | kind=code-symbol | source=scripts/phase_4_tickets_mcp.py:L13 | neighbors=[phase_4_tickets_mcp.py, Execute Phase 4 (Ticket Generation) for…]
- "scripts_phase_5_execute_mcp_execute_phase_5": "execute_phase_5()" | kind=code-symbol | source=scripts/phase_5_execute_mcp.py:L13 | neighbors=[phase_5_execute_mcp.py, Execute Phase 5 (Ticket Execution) for …]
- "scripts_phase_5_verify_mcp_execute_phase_5_verify": "execute_phase_5_verify()" | kind=code-symbol | source=scripts/phase_5_verify_mcp.py:L13 | neighbors=[phase_5_verify_mcp.py, Execute Phase 5.V (Verification) for an…]
- "scripts_phase_6_review_mcp_execute_phase_6": "execute_phase_6()" | kind=code-symbol | source=scripts/phase_6_review_mcp.py:L13 | neighbors=[phase_6_review_mcp.py, Execute Phase 6 (Final Review) for an e…]
- "scripts_precompute_wave7_graph_load_epic_list": "load_epic_list()" | kind=code-symbol | source=scripts/precompute_wave7_graph.py:L34 | neighbors=[precompute_wave7_graph.py, main()]
- "scripts_query_kb_init_firestore": "init_firestore()" | kind=code-symbol | source=scripts/query_kb.py:L60 | neighbors=[query_kb.py, Initializes Firebase using local servic…]
- "scripts_quick_validate_validate_skill": "validate_skill()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/quick_validate.py:L12 | neighbors=[quick_validate.py, Basic validation of a skill]
- "scripts_reaper_split_extract": "extract()" | kind=code-symbol | source=scripts/reaper_split.py:L65 | neighbors=[reaper_split.py, main()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-038.json

Keep each description factual and concise (one sentence). No markdown, no prose
outside the JSON object. It is acceptable to omit a node if context is
insufficient — but include every node you can ground confidently.

Example answer format:
```json
{
  "node_id_1": "Resolves the configured ontology profile from graphify.yaml.",
  "node_id_2": "Colonel James Barclay, an antagonist in The Crooked Man."
}
```
